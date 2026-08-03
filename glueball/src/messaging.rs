use crate::logging::LogDestination;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::prefixed::{ConnectionStatus, Prefixed, SynthesisStream, into_prefixed_or_respond};
use crate::room::{ClientId, ClientSender, State};
use crate::util::{deserialize_messagepack, serialize_and_prefix, trim_uuid};
use crate::{EventType, lock};

use anyhow::{Result, bail};
use bytes::Bytes;
use chrono::Utc;
use futures_util::stream::{SplitSink, SplitStream};
use futures_util::{SinkExt, StreamExt};
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::sync::mpsc::{self, Sender};
use tokio::time::timeout;
use tokio_tungstenite::{WebSocketStream, tungstenite::Message};

use std::net::SocketAddr;
use std::sync::{Arc, Mutex};
use std::time::Duration;

type WsStream<S> = WebSocketStream<Prefixed<S>>;

const TIMEOUT: Duration = Duration::from_secs(30);

pub async fn handle_connection<S>(
    state: Arc<Mutex<State>>,
    raw_stream: S,
    addr: SocketAddr,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    let ConnectionStatus::Ws(stream) =
        into_prefixed_or_respond(raw_stream, addr, logging_tx.clone()).await
    else {
        return;
    };

    let ws_stream = match tokio_tungstenite::accept_async(stream).await {
        Ok(ws_stream) => ws_stream,
        Err(e) => {
            error_global!(logging_tx, "Websocket handshake with {addr} failed: {e}");
            return;
        }
    };

    info_global!(logging_tx, "WS connection established with {addr}");

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client through the `write` sink
    let (tx, mut rx) = mpsc::channel::<Message>(64);

    // Order of messages sent from a new client to the server:
    // 1-n. Any number of `RequestRooms` messages.
    // n..n+1. An `InitializationMessage`, indicating whether the client wishes to create or join a room
    // n+1..m. Any number of messages that will be forwarded to every other client in their room
    let (mut write, mut read) = ws_stream.split();

    let Some(client_id) = wait_for_initialization(
        state.clone(),
        &mut read,
        &mut write,
        tx.clone(),
        addr,
        logging_tx.clone(),
    )
    .await
    else {
        return;
    };

    // This task listens for messages to the channel and sends them down the sink to the client
    tokio::spawn(async move {
        while let Some(msg) = rx.recv().await {
            if write.send(msg.clone()).await.is_err() {
                break;
            }
        }
    });

    // Listen for and pass along messages to other client channels in the same room
    loop {
        let result = timeout(TIMEOUT, read.next()).await;

        // If it's a timeout error, we print such
        if result.is_err() {
            warn_global!(logging_tx, "{} timed out", trim_uuid(&client_id));
        }

        // If it's anything but a correct response, we disconnect
        let Ok(Some(Ok(message))) = result else { break };

        if !handle_client_message(message, state.clone(), client_id, logging_tx.clone()).await {
            // We don't break here, to avoid double closing the connection
            return;
        }
    }

    let _ = handle_client_close(client_id, &state, logging_tx).await;
}

/// Waits for and handles messages from the client that are intended for the server.
///
/// If the message is `ClientToServerMessage::RequestRooms`,
/// the function handles the request and keeps listening
///
/// If the message is `ClientToServerMessage::InitializeConnection`,
/// the function handles the request by generating a client id, and putting
/// the client in the correct room. This may involve creating a new room depending on the request
/// The function then returns the generated `ClientId`
///
/// If any message is unable to be parse, the function returns `None`.
async fn wait_for_initialization<S>(
    state: Arc<Mutex<State>>,
    read: &mut SplitStream<WsStream<S>>,
    write: &mut SplitSink<WsStream<S>, Message>,
    tx: ClientSender,
    addr: SocketAddr,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) -> Option<ClientId>
where
    S: SynthesisStream,
{
    loop {
        match parse_first_message(read, addr, logging_tx.clone()).await {
            Some(ClientToServerMessage::RequestRooms) => {
                handle_room_list_request(state.clone(), write).await;
            }

            // When they ask to initialize a connection, then we add them to a room
            // Or create a room for them
            Some(ClientToServerMessage::InitializeConnection { room_id, name }) => {
                let (client_id, room_id) = {
                    // The lock is relinquished at the end of this expression
                    let info = lock!(state).initialize_client_in_room(tx, room_id, &name);
                    match info {
                        Some(info) => info,
                        None => return None,
                    }
                };

                let response = ServerToClientMessage::SendInfo {
                    room_id,
                    client_id: client_id.to_string(),
                };
                let message = serialize_and_prefix(response, MessagePrefix::Server);

                if write.send(message).await.is_err() {
                    error_global!(logging_tx, "Failed to send back initial response");

                    return None;
                }

                break Some(client_id);
            }
            Some(ClientToServerMessage::Ping { timestamp: _ }) => {
                error_global!(
                    logging_tx,
                    "Received ping from client during initialization"
                );
                return None;
            }
            None => return None,
        }
    }
}

async fn parse_first_message<S>(
    read: &mut SplitStream<WebSocketStream<Prefixed<S>>>,
    addr: SocketAddr,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) -> Option<ClientToServerMessage>
where
    S: SynthesisStream,
{
    // Parse initial message, then user in correct room
    let Some(Ok(Message::Binary(message_data))) = read.next().await else {
        warn_global!(
            logging_tx,
            "Client disconnected before handshake (probably a test)"
        );
        return None;
    };

    let Ok(message) = deserialize_messagepack::<ClientToServerMessage>(&message_data[1..]) else {
        error_global!(logging_tx, "{addr} sent an invalid initial message");
        return None;
    };

    Some(message)
}

async fn handle_room_list_request<S>(
    state: Arc<Mutex<State>>,
    write: &mut SplitSink<WebSocketStream<Prefixed<S>>, Message>,
) where
    S: SynthesisStream,
{
    let message = {
        ServerToClientMessage::RoomList {
            rooms: lock!(state).list_rooms(),
        }
    };
    let message = serialize_and_prefix(message, MessagePrefix::Server);

    write.send(message).await.ok();
}

async fn handle_client_message(
    message: Message,
    state: Arc<Mutex<State>>,
    client_id: ClientId,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) -> bool {
    match message {
        Message::Binary(ref bytes) => {
            if bytes[0] == MessagePrefix::Server as u8 {
                handle_client_ping(bytes, &client_id, &state, logging_tx).await;
                return true;
            }

            assert_eq!(bytes[0], MessagePrefix::Client as u8);
            // If we're here, that means the message has a client-client prefix
            // which we want anyway, so there's no need to prefix the message
            // we can just forward it!

            let senders: Vec<ClientSender> = { lock!(state).get_senders_from_user_room(client_id) };

            for tx in senders {
                tx.send(message.clone()).await.ok();
            }

            true
        }

        Message::Close(_) => {
            let _ = handle_client_close(client_id, &state, logging_tx).await;
            false
        }
        _ => todo!("Handle ws protocol ping/pong and text messagaes"),
    }
}

async fn handle_client_ping(
    bytes: &Bytes,
    client_id: &ClientId,
    state: &Arc<Mutex<State>>,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) {
    let Ok(ClientToServerMessage::Ping { timestamp }) =
        deserialize_messagepack::<ClientToServerMessage>(&bytes[1..])
    else {
        error_global!(
            logging_tx,
            "Got invalid client to server message while client was in room"
        );
        return;
    };

    let current_server_timestamp = Utc::now().timestamp_millis() as u64;

    let message = ServerToClientMessage::Pong {
        last_ping_timestamp: timestamp,
        current_server_timestamp,
    };
    let message = serialize_and_prefix(message, MessagePrefix::Server);

    // Scope hack to avoid holding the guard while sending a message
    // Because Mutex locks are not Send
    let tx = {
        let Some(tx) = lock!(state).get_client_tx(client_id) else {
            error_global!(
                logging_tx,
                "Received client-server message from client not in room"
            );
            return;
        };

        tx
    };

    let _ = tx.send(message).await;
}

async fn handle_client_close(
    client_id: ClientId,
    state: &Arc<Mutex<State>>,
    logging_tx: Sender<(String, EventType, LogDestination)>,
) -> Result<()> {
    // Send message toa ll other clients telling them `client_id` has been kicked
    let message = ServerToClientMessage::Kick {
        client_id: client_id.to_string(),
    };
    let message = serialize_and_prefix(message, MessagePrefix::Server);

    let senders = {
        let mut guard = lock!(state);

        let Some(room) = guard.get_room_of_client_mut(&client_id).map(|a| a.1) else {
            let err = "Client attempted to leave when they were not in a room ";

            error_global!(logging_tx, "{}", err);
            bail!(err);
        };

        let client_name = room.get_client_name(&client_id)?;
        warn_global!(logging_tx, "Connection with {client_name} closed");

        let senders = room.get_senders(Some(&client_id));
        guard.remove_client(client_id);

        senders
    };

    for tx in senders {
        let _ = tx.clone().send(message.clone()).await;
    }

    Ok(())
}
