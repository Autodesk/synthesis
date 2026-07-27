use crate::EventType;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerMessage};
use crate::prefixed::{ConnectionStatus, Prefixed, SynthesisStream, into_prefixed_or_respond};
use crate::room::{ClientId, ClientSender, State};
use crate::util::{deserialize_messagepack, prefix_message, serialize_messagepack};

use bytes::Bytes;
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::sync::mpsc;
use tokio::time::timeout;
use tokio_tungstenite::{WebSocketStream, tungstenite::Message};

use futures_util::stream::{SplitSink, SplitStream};
use futures_util::{SinkExt, StreamExt};
use std::net::SocketAddr;
use std::sync::{Arc, Mutex};
use std::time::Duration;

type WsStream<S> = WebSocketStream<Prefixed<S>>;

const TIMEOUT: Duration = Duration::from_mins(1);

pub async fn handle_connection<S>(state: Arc<Mutex<State>>, raw_stream: S, addr: SocketAddr)
where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    let ConnectionStatus::Ws(stream) =
        into_prefixed_or_respond(state.clone(), raw_stream, addr).await
    else {
        return;
    };

    let ws_stream = match tokio_tungstenite::accept_async(stream).await {
        Ok(ws_stream) => ws_stream,
        Err(e) => {
            error_lock!(state, "Websocket handshake with {addr} failed: {e}");
            return;
        }
    };

    info_lock!(state, "WS connection established with {addr}");

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client through the `write` sink
    let (tx, mut rx) = mpsc::channel::<Message>(64);

    // Order of messages sent from a new client to the server:
    // 1-n. Any number of `RequestRooms` messages.
    // n..n+1. An `InitializationMessage`, indicating whether the client wishes to create or join a room
    // n+1..m. Any number of messages that will be forwarded to every other client in their room
    let (mut write, mut read) = ws_stream.split();

    let Some(client_id) =
        wait_for_initializtion(state.clone(), &mut read, &mut write, tx, addr).await
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
    while let Some(maybe_message) = timeout(TIMEOUT, read.next()).await.ok().flatten() {
        let Ok(message) = maybe_message else {
            return;
        };

        handle_client_message(message, state.clone(), client_id).await;
    }
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
async fn wait_for_initializtion<S>(
    state: Arc<Mutex<State>>,
    read: &mut SplitStream<WsStream<S>>,
    write: &mut SplitSink<WsStream<S>, Message>,
    tx: ClientSender,
    addr: SocketAddr,
) -> Option<ClientId>
where
    S: SynthesisStream,
{
    loop {
        match parse_first_message(state.clone(), read, addr).await {
            Some(ClientToServerMessage::RequestRooms) => {
                handle_room_list_request(state.clone(), write).await;
            }

            // When they ask to initialize a connection, then we add them to a room
            // Or create a room for them
            Some(ClientToServerMessage::InitializeConnection { room_id, name }) => {
                let (client_id, room_id) = {
                    // The lock is relinquished at the end of this expression
                    let mut guard = state.lock().unwrap();
                    match guard.initialize_client_in_room(tx, room_id, name) {
                        Some(info) => info,
                        None => return None,
                    }
                };

                let response = ServerMessage::SendInfo {
                    room_id,
                    client_id: client_id.to_string(),
                };
                let bytes = serialize_messagepack(&response);

                let message = prefix_message(bytes, MessagePrefix::Server);

                if write.send(message).await.is_err() {
                    error_lock!(state, "Failed to send back initial response");

                    return None;
                }

                break Some(client_id);
            }
            Some(ClientToServerMessage::Ping { timestamp: _ }) => {
                error_lock!(state, "Received ping from client during initialization");
                return None;
            }
            None => return None,
        }
    }
}

async fn parse_first_message<S>(
    state: Arc<Mutex<State>>,
    read: &mut SplitStream<WebSocketStream<Prefixed<S>>>,
    addr: SocketAddr,
) -> Option<ClientToServerMessage>
where
    S: SynthesisStream,
{
    // Parse initial message, then user in correct room
    let Some(Ok(Message::Binary(message_data))) = read.next().await else {
        warn_lock!(
            state,
            "Client disconnected before handshake (probably a test)"
        );
        return None;
    };

    let Ok(message) = deserialize_messagepack::<ClientToServerMessage>(&message_data[1..]) else {
        error_lock!(state, "{addr} sent an invalid initial message");
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
        ServerMessage::RoomList {
            rooms: state.lock().unwrap().list_rooms(),
        }
    };

    let bytes = serialize_messagepack(message);
    let message = prefix_message(bytes, MessagePrefix::Server);

    write.send(message).await.ok();
}

async fn handle_client_message(message: Message, state: Arc<Mutex<State>>, client_id: ClientId) {
    match message {
        Message::Binary(ref bytes) => {
            if bytes[0] == MessagePrefix::Server as u8 {
                handle_client_ping(bytes, &client_id, &state).await;
                return;
            }

            assert_eq!(bytes[0], MessagePrefix::Client as u8);
            // If we're here, that means the message has a client-client prefix
            // which we want anyway, so there's no need to prefix the message
            // we can just forward it!

            let senders: Vec<ClientSender> =
                { state.lock().unwrap().get_senders_from_user_room(client_id) };

            for tx in senders {
                tx.send(message.clone()).await.ok();
            }
        }

        Message::Close(_) => handle_client_close(client_id, &state).await,
        _ => todo!("Handle ws protocol ping/pong and text messagaes"),
    }
}

async fn handle_client_ping(bytes: &Bytes, client_id: &ClientId, state: &Arc<Mutex<State>>) {
    let Ok(ClientToServerMessage::Ping { timestamp }) =
        deserialize_messagepack::<ClientToServerMessage>(&bytes[1..])
    else {
        error_lock!(
            state,
            "Got invalid client to server message while client was in room"
        );
        return;
    };

    let message = serialize_messagepack(ServerMessage::Pong { timestamp });
    let message = prefix_message(message, MessagePrefix::Server);

    // Scope hack to avoid holding the guard while sending a message
    // Because Mutex locks are not Send
    let tx = {
        let mut guard = state.lock().unwrap();
        let Some(tx) = guard.get_client_tx(client_id) else {
            error!(
                guard,
                "Received client-server message from client not in room"
            );
            return;
        };
        drop(guard);

        tx
    };

    let _ = tx.send(message).await;
}

async fn handle_client_close(client_id: ClientId, state: &Arc<Mutex<State>>) {
    // Send message toa ll other clients telling them `client_id` has been kicked
    let message = ServerMessage::Kick {
        client_id: client_id.to_string(),
    };

    let message_buffer_no_prefix = serialize_messagepack(message);
    let message = prefix_message(message_buffer_no_prefix, MessagePrefix::Server);

    let senders = {
        let mut guard = state.lock().unwrap();
        warn!(guard, "Connection with {client_id} closed");

        let Some(room) = guard.get_room_of_client(&client_id).map(|a| a.1) else {
            error!(
                guard,
                "Client attempted to leave when they were not in a room "
            );
            return;
        };

        let senders = room.get_senders(Some(&client_id));
        guard.remove_client(client_id);

        senders
    };

    for tx in senders {
        let _ = tx.clone().send(message.clone()).await;
    }
}
