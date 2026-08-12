//! This module is responsible for handling connections with users.
//!
//! It mostly provides functions to be used by the [`connection`] module

use crate::connection::handle_client_close;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::prefixed::{Prefixed, SynthesisStream};
use crate::room::{ClientId, ClientSender};
use crate::state::State;
use crate::util::{deserialize_messagepack, server_sent_msg};

use bytes::Bytes;
use chrono::Utc;
use futures_util::stream::{SplitSink, SplitStream};
use futures_util::{SinkExt, StreamExt};
use tokio_tungstenite::{WebSocketStream, tungstenite::Message};

use std::net::SocketAddr;
use std::ops;
use std::sync::Arc;

type WsStream<S> = WebSocketStream<Prefixed<S>>;

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
pub async fn wait_for_initialization<S>(
    state: Arc<State>,
    read: &mut SplitStream<WsStream<S>>,
    write: &mut SplitSink<WsStream<S>, Message>,
    tx: ClientSender,
    addr: SocketAddr,
) -> Option<ClientId>
where
    S: SynthesisStream,
{
    loop {
        match parse_first_message(read, addr).await {
            Some(ClientToServerMessage::RequestRooms) => {
                handle_room_list_request(state.clone(), write).await;
            }

            // When they ask to initialize a connection, then we add them to a room
            // Or create a room for them
            Some(ClientToServerMessage::InitializeConnection { room_id, name }) => {
                let (client_id, room_id) = {
                    // The lock is relinquished at the end of this expression
                    let info = state.initialize_client_in_room(tx, room_id, &name);
                    match info {
                        Some(info) => info,
                        None => return None,
                    }
                };

                let message = server_sent_msg(ServerToClientMessage::SendInfo {
                    room_id,
                    client_id: client_id.to_string(),
                });

                if write.send(message).await.is_err() {
                    error_global!("Failed to send back initial response");

                    return None;
                }

                break Some(client_id);
            }
            Some(ClientToServerMessage::Ping { timestamp: _ }) => {
                error_global!("Received ping from client during initialization");
                return None;
            }
            None => return None,
        }
    }
}

async fn parse_first_message<S>(
    read: &mut SplitStream<WebSocketStream<Prefixed<S>>>,
    addr: SocketAddr,
) -> Option<ClientToServerMessage>
where
    S: SynthesisStream,
{
    // Parse initial message, then user in correct room
    let Some(Ok(Message::Binary(message_data))) = read.next().await else {
        warn_global!("Client disconnected before handshake (probably a test)");
        return None;
    };

    let Ok(message) = deserialize_messagepack::<ClientToServerMessage>(&message_data[1..]) else {
        error_global!("{addr} sent an invalid initial message");
        return None;
    };

    Some(message)
}

async fn handle_room_list_request<S>(
    state: Arc<State>,
    write: &mut SplitSink<WebSocketStream<Prefixed<S>>, Message>,
) where
    S: SynthesisStream,
{
    let message = server_sent_msg(ServerToClientMessage::RoomList {
        rooms: state.list_rooms(),
    });

    write.send(message).await.ok();
}

pub async fn handle_client_message(
    message: Message,
    state: Arc<State>,
    client_id: ClientId,
) -> ops::ControlFlow<(), ()> {
    match message {
        Message::Binary(ref bytes) => {
            if bytes.len() <= 1 {
                return ops::ControlFlow::Continue(());
            }

            if bytes[0] == MessagePrefix::Server as u8 {
                handle_client_ping(bytes, &client_id, &state).await;
                return ops::ControlFlow::Continue(());
            }

            // If we're here, that means the message has a client-client prefix
            // which we want anyway, so there's no need to prefix the message
            // we can just forward it!
            let senders: Vec<ClientSender> = { state.get_senders_from_user_room(client_id) };

            let tasks = senders.iter().map(|tx| tx.send(message.clone()));
            let _ = futures_util::future::join_all(tasks).await;

            ops::ControlFlow::Continue(())
        }

        Message::Close(_) => {
            let _ = handle_client_close(client_id, &state).await;

            ops::ControlFlow::Break(())
        }
        _ => ops::ControlFlow::Continue(()),
    }
}

async fn handle_client_ping(bytes: &Bytes, client_id: &ClientId, state: &Arc<State>) {
    let Ok(ClientToServerMessage::Ping { timestamp }) =
        deserialize_messagepack::<ClientToServerMessage>(&bytes[1..])
    else {
        error_global!("Got invalid client to server message while client was in room");
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
            error_global!("Received client-server message from client not in room");
            return;
        };

        tx
    };

    let _ = tx.send(message).await;
}
