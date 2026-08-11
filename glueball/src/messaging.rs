use crate::EventType;
use crate::logging::{LogDestination, LogSender};
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::state::{ClientId, ClientSender, State};
use crate::util::{deserialize_messagepack, server_sent_msg, trim_uuid};
use crate::wire::{Delivery, Outbound, read_message, write_message};

use anyhow::{Result, bail};
use bytes::Bytes;
use chrono::Utc;
use tokio::sync::mpsc::{self};
use tokio::time::timeout;
use wtransport::VarInt;
use wtransport::endpoint::IncomingSession;
use wtransport::error::SendDatagramError;
use wtransport::Connection;

use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;

/// How long to wait for a client's next message before assuming it is gone.
/// Clients ping every five seconds, so a silent client is a dead one even while
/// its datagrams keep arriving.
const TIMEOUT: Duration = Duration::from_secs(30);

/// Number of messages that may be queued for one client before writes to it
/// block (streams) or are dropped (datagrams).
const OUTBOUND_CAPACITY: usize = 64;

/// Application error code sent to a client whose session the server terminates.
const CLOSED_BY_SERVER: VarInt = VarInt::from_u32(0);

/// Completes the `WebTransport` handshake for an incoming QUIC connection, then
/// hands the session to [`handle_connection`].
pub async fn handle_session(state: Arc<State>, session: IncomingSession, logging_tx: LogSender) {
    let addr = session.remote_address();

    let request = match session.await {
        Ok(request) => request,
        Err(e) => {
            error_global!(logging_tx, "QUIC handshake with {addr} failed: {e}");
            return;
        }
    };

    let connection = match request.accept().await {
        Ok(connection) => connection,
        Err(e) => {
            error_global!(logging_tx, "WebTransport handshake with {addr} failed: {e}");
            return;
        }
    };

    handle_connection(state, connection, addr, logging_tx).await;
}

pub async fn handle_connection(
    state: Arc<State>,
    connection: Connection,
    addr: SocketAddr,
    logging_tx: LogSender,
) {
    info_global!(logging_tx, "WT session established with {addr}");

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client by the writer task
    let (tx, rx) = mpsc::channel::<Outbound>(OUTBOUND_CAPACITY);

    // Order of messages sent from a new client to the server:
    // 1-n. Any number of `RequestRooms` messages -> server will return a list of rooms
    // n..n+1. An `InitializationMessage`, indicating whether the client wishes to create or join a room -> server will return a room and client id
    // n+1..m. Any number of messages that will be forwarded to every other client in their room -> server will not respond, instead forwarding
    let Some(client_id) =
        wait_for_initialization(&state, &connection, tx.clone(), addr, &logging_tx).await
    else {
        return;
    };

    spawn_writer(connection.clone(), rx, logging_tx.clone());
    spawn_datagram_reader(
        connection.clone(),
        state.clone(),
        client_id,
        logging_tx.clone(),
    );

    // Listen for and pass along messages to other client channels in the same room
    loop {
        // Messages are taken one at a time rather than concurrently: streams give
        // no ordering guarantees between each other, so draining them in arrival
        // order is the closest thing to the ordering clients used to rely on
        let message = match timeout(TIMEOUT, accept_message(&connection)).await {
            Ok(Ok(message)) => message,
            Ok(Err(e)) => {
                warn_global!(logging_tx, "{} disconnected: {e}", trim_uuid(&client_id));
                break;
            }
            Err(_) => {
                warn_global!(logging_tx, "{} timed out", trim_uuid(&client_id));
                break;
            }
        };

        handle_client_message(message, Delivery::Stream, &state, client_id, &logging_tx).await;
    }

    let _ = handle_client_close(client_id, &state, logging_tx).await;
}

/// Waits for the client's next stream and reads the message off it.
///
/// Only unidirectional streams count as messages. Replies are sent as their own
/// stream rather than written back onto the request's stream, so there is nothing
/// a bidirectional stream would buy, and ignoring them means a client that opens
/// one and leaves it empty cannot stall this loop.
async fn accept_message(connection: &Connection) -> Result<Bytes> {
    let read = connection.accept_uni().await?;

    read_message(read).await
}

/// Drains `rx` onto the client's session.
fn spawn_writer(connection: Connection, mut rx: mpsc::Receiver<Outbound>, logging_tx: LogSender) {
    tokio::spawn(async move {
        // Undeliverable datagrams come in floods rather than one at a time, so the
        // reason is worth saying once and then never again for this session
        let mut warned_undeliverable = false;

        while let Some(message) = rx.recv().await {
            match message {
                Outbound::Stream(payload) => {
                    if write_message(&connection, &payload).await.is_err() {
                        break;
                    }
                }

                // Datagrams are best-effort, so one that cannot be sent is dropped
                // rather than retried. It is still worth saying so once: a payload
                // that never fits looks exactly like a peer that has gone quiet,
                // which is a miserable thing to debug
                Outbound::Datagram(payload) => {
                    let size = payload.len();

                    match connection.send_datagram(payload) {
                        Ok(()) => {}
                        Err(SendDatagramError::NotConnected) => break,
                        Err(e) if warned_undeliverable => {
                            let _ = e;
                        }
                        Err(SendDatagramError::TooLarge) => {
                            warned_undeliverable = true;
                            warn_global!(
                                logging_tx,
                                "Dropping datagrams: {size} bytes exceeds the {} the path allows. Send these over a stream instead",
                                connection.max_datagram_size().unwrap_or_default()
                            );
                        }
                        Err(SendDatagramError::UnsupportedByPeer) => {
                            warned_undeliverable = true;
                            warn_global!(
                                logging_tx,
                                "Dropping datagrams: the client does not accept them"
                            );
                        }
                    }
                }

                Outbound::Close => {
                    connection.close(CLOSED_BY_SERVER, b"Closed by server");
                    break;
                }
            }
        }
    });
}

/// Reads datagrams for the lifetime of the session.
///
/// Datagrams arrive outside of any stream, so they need a reader of their own.
fn spawn_datagram_reader(
    connection: Connection,
    state: Arc<State>,
    client_id: ClientId,
    logging_tx: LogSender,
) {
    tokio::spawn(async move {
        while let Ok(datagram) = connection.receive_datagram().await {
            handle_client_message(
                datagram.payload(),
                Delivery::Datagram,
                &state,
                client_id,
                &logging_tx,
            )
            .await;
        }
    });
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
async fn wait_for_initialization(
    state: &Arc<State>,
    connection: &Connection,
    tx: ClientSender,
    addr: SocketAddr,
    logging_tx: &LogSender,
) -> Option<ClientId> {
    loop {
        match parse_first_message(connection, addr, logging_tx).await {
            Some(ClientToServerMessage::RequestRooms) => {
                handle_room_list_request(state, connection).await;
            }

            // When they ask to initialize a connection, then we add them to a room
            // Or create a room for them
            Some(ClientToServerMessage::InitializeConnection { room_id, name }) => {
                let (client_id, room_id) = state.initialize_client_in_room(tx, room_id, &name)?;

                let message = server_sent_msg(ServerToClientMessage::SendInfo {
                    room_id,
                    client_id: client_id.to_string(),
                });

                if write_message(connection, &message).await.is_err() {
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

async fn parse_first_message(
    connection: &Connection,
    addr: SocketAddr,
    logging_tx: &LogSender,
) -> Option<ClientToServerMessage> {
    // Parse initial message, then user in correct room
    let message_data = match timeout(TIMEOUT, accept_message(connection)).await {
        Ok(Ok(message_data)) => message_data,
        Ok(Err(e)) => {
            warn_global!(logging_tx, "{addr} disconnected before handshake: {e}");
            return None;
        }
        Err(_) => {
            warn_global!(
                logging_tx,
                "{addr} did not complete a message before the handshake timed out. A client must finish each stream it writes, since that is what ends the message"
            );
            return None;
        }
    };

    if message_data.is_empty() {
        error_global!(logging_tx, "{addr} sent an empty initial message");
        return None;
    }

    let Ok(message) = deserialize_messagepack::<ClientToServerMessage>(&message_data[1..]) else {
        error_global!(logging_tx, "{addr} sent an invalid initial message");
        return None;
    };

    Some(message)
}

async fn handle_room_list_request(state: &Arc<State>, connection: &Connection) {
    let message = server_sent_msg(ServerToClientMessage::RoomList {
        rooms: state.list_rooms(),
    });

    let _ = write_message(connection, &message).await;
}

/// Routes one message from a client.
///
/// Messages carrying [`MessagePrefix::Server`] are for the server to answer;
/// everything else is forwarded verbatim to the client's roommates over the same
/// kind of channel it arrived on.
async fn handle_client_message(
    payload: Bytes,
    delivery: Delivery,
    state: &Arc<State>,
    client_id: ClientId,
    logging_tx: &LogSender,
) {
    // A prefix byte on its own carries nothing
    if payload.len() <= 1 {
        warn_global!(
            logging_tx,
            "Discarding {} byte message from {}",
            payload.len(),
            trim_uuid(&client_id)
        );
        return;
    }

    if payload[0] == MessagePrefix::Server as u8 {
        if delivery == Delivery::Datagram {
            // Answering still works, but the client should not be risking a
            // dropped client-server message in the first place
            warn_global!(
                logging_tx,
                "{} sent a client-server message as a datagram",
                trim_uuid(&client_id)
            );
        }

        handle_client_ping(&payload, &client_id, state, logging_tx.clone()).await;
        return;
    }

    // If we're here, that means the message has a client-client prefix
    // which we want anyway, so there's no need to prefix the message
    // we can just forward it!
    let senders: Vec<ClientSender> = { state.get_senders_from_user_room(client_id) };

    match delivery {
        // Guaranteed traffic waits for room in each peer's queue
        Delivery::Stream => {
            let tasks = senders
                .iter()
                .map(|tx| tx.send(delivery.queue(payload.clone())));
            let _ = futures_util::future::join_all(tasks).await;
        }

        // Unreliable traffic is dropped instead of queued: one peer that cannot
        // keep up must not stall every other peer's updates, and a stale physics
        // update is worth less than the one behind it
        Delivery::Datagram => {
            for tx in &senders {
                let _ = tx.try_send(delivery.queue(payload.clone()));
            }
        }
    }
}

async fn handle_client_ping(
    bytes: &Bytes,
    client_id: &ClientId,
    state: &Arc<State>,
    logging_tx: LogSender,
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

    #[allow(clippy::expect_used)]
    let current_server_timestamp = u64::try_from(Utc::now().timestamp_millis())
        .expect("Negative timestamps (before 1970) are invalid. Please fix your system clock.");

    let message = server_sent_msg(ServerToClientMessage::Pong {
        client_send_ts: timestamp,
        server_ts: current_server_timestamp,
    });

    // Scope hack to avoid holding the guard while sending a message
    // Because Mutex locks are not Send
    let tx = {
        let Some(tx) = state.get_client_tx(client_id) else {
            error_global!(
                logging_tx,
                "Received client-server message from client not in room"
            );
            return;
        };

        tx
    };

    let _ = tx.send(Outbound::Stream(message)).await;
}

async fn handle_client_close(
    client_id: ClientId,
    state: &Arc<State>,
    logging_tx: LogSender,
) -> Result<()> {
    // Send message to all other clients telling them `client_id` has been kicked
    let message = server_sent_msg(ServerToClientMessage::Kick {
        client_id: client_id.to_string(),
    });

    let Some(room) = state.get_room_of_client_mut(&client_id) else {
        let err = "Client attempted to leave when they were not in a room ";

        error_global!(logging_tx, "{}", err);
        bail!(err);
    };

    let client_name = room.get_client_name(&client_id)?;
    warn_global!(logging_tx, "Connection with {client_name} closed");

    let senders = room.get_peer_senders(&client_id);
    drop(room);

    state.remove_client(&client_id);

    let tasks = senders
        .iter()
        .map(|tx| tx.send(Outbound::Stream(message.clone())));
    let _ = futures_util::future::join_all(tasks).await;

    Ok(())
}
