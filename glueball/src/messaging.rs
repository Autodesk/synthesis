use crate::connection::TIMEOUT;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::state::{ClientId, ClientSender, State};
use crate::util::{deserialize_messagepack, server_sent_msg, trim_uuid};
use crate::wire::{Outbound, read_message, write_message};

use anyhow::{Result, bail};
use bytes::Bytes;
use chrono::Utc;
use tokio::time::timeout;
use wtransport::Connection;
use wtransport::VarInt;

use std::net::SocketAddr;
use std::ops::ControlFlow;
use std::sync::Arc;

/// Application error code sent to a client whose session the server terminates.
pub const CLOSED_BY_SERVER: VarInt = VarInt::from_u32(0);

/// Waits for the client's next stream and reads the message off it.
///
/// Only unidirectional streams count as messages. Replies are sent as their own
/// stream rather than written back onto the request's stream, so there is nothing
/// a bidirectional stream would buy, and ignoring them means a client that opens
/// one and leaves it empty cannot stall this loop.
pub async fn accept_message(connection: &Connection) -> Result<Bytes> {
    let read = connection.accept_uni().await?;

    read_message(read).await
}

/// Passes all messages sent down the corresponding `tx` to the client
pub async fn handle_first_message(
    connection: &Connection,
    addr: SocketAddr,
) -> Option<ClientToServerMessage> {
    let maybe_timed_out_response = timeout(TIMEOUT, accept_message(connection)).await;

    let message_data = match maybe_timed_out_response {
        Ok(Ok(message_data)) => message_data,

        Ok(Err(e)) => {
            warn_global!("{addr} disconnected before handshake: {e}");
            return None;
        }

        Err(_) => {
            warn_global!(
                "{addr} did not complete a message before the handshake timed out. A client must finish each stream it writes, since that is what ends the message"
            );
            return None;
        }
    };

    if message_data.is_empty() {
        error_global!("{addr} sent an empty initial message");
        return None;
    }

    let Ok(message) = deserialize_messagepack::<ClientToServerMessage>(&message_data[1..]) else {
        error_global!("{addr} sent an invalid initial message");
        return None;
    };

    Some(message)
}

pub async fn handle_room_list_request(state: &Arc<State>, connection: &Connection) {
    let message = server_sent_msg(ServerToClientMessage::RoomList {
        rooms: state.list_rooms(),
    });

    let _ = write_message(connection, &message).await;
}

pub async fn handle_client_message_datagram(
    payload: Bytes,
    state: &Arc<State>,
    client_id: &ClientId,
) {
    let handle_datagram = async |payload: Bytes, senders: Vec<ClientSender>| {
        for tx in &senders {
            let _ = tx.try_send(Outbound::Datagram(payload.clone()));
        }
    };

    handle_client_message_generic(payload, state, client_id, handle_datagram).await;
}

pub async fn handle_client_message_stream(
    payload: Bytes,
    state: &Arc<State>,
    client_id: &ClientId,
) {
    let handle_datagram = async |payload: Bytes, senders: Vec<ClientSender>| {
        let responses = senders
            .iter()
            .map(|tx| tx.send(Outbound::Stream(payload.clone())));
        let _ = futures_util::future::join_all(responses).await;
    };

    handle_client_message_generic(payload, state, client_id, handle_datagram).await;
}

/// Handles a message from the client.
///
/// All messages carry a prefix byte ([`MessagePrefix`]) that indicates whether the message was
/// meant as a client-client message ([`MessagePrefix::Client`]) or as a server-client message ([`MessagePrefix::Server`])
///
/// If it's a client-client message, it gets sent down the appropriate client's sinks (handled by [`spawn_client_sink`])
/// If it's a client-server message, an appropriate response is sent.
///
/// Client-client messages can be sent either via datagrams or streams
/// Client-server messages are expected to be sent via streams
async fn handle_client_message_generic<F, Fut>(
    payload: Bytes,
    state: &Arc<State>,
    client_id: &ClientId,
    payload_handler: F,
) where
    F: Fn(Bytes, Vec<ClientSender>) -> Fut,
    Fut: Future<Output = ()>,
{
    let result = check_message_prefix(state, &payload, client_id).await;

    if result.is_break() {
        return;
    }

    // If we're here, that means the message has a client-client prefix
    // which we want anyway, so there's no need to prefix the message:
    // we can just forward it!
    let senders: Vec<ClientSender> = { state.get_senders_from_user_room(*client_id) };

    payload_handler(payload, senders).await;
}

/// Checks the message prefix
///
/// If its a client-server message, passes it off to [`handle_client_ping`]
async fn check_message_prefix(
    state: &Arc<State>,
    payload: &Bytes,
    client_id: &ClientId,
) -> ControlFlow<()> {
    if payload.len() <= 1 {
        warn_global!(
            "Discarding {} byte message from {}",
            payload.len(),
            trim_uuid(client_id)
        );
        return ControlFlow::Break(());
    }

    let prefix = payload[0];

    if prefix == MessagePrefix::Server as u8 {
        handle_client_ping(payload, client_id, state).await;

        return ControlFlow::Break(());
    }

    ControlFlow::Continue(())
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

    let _ = tx.send(Outbound::Stream(message)).await;
}

pub async fn handle_client_close(client_id: ClientId, state: &Arc<State>) -> Result<()> {
    // Send message to all other clients telling them `client_id` has been kicked
    let message = server_sent_msg(ServerToClientMessage::Kick {
        client_id: client_id.to_string(),
    });

    let Some(room) = state.get_room_of_client_mut(&client_id) else {
        let err = "Client attempted to leave when they were not in a room ";

        error_global!("{}", err);
        bail!(err);
    };

    let client_name = room.get_client_name(&client_id)?;
    warn_global!("Connection with {client_name} closed");

    let senders = room.get_peer_senders(&client_id);
    drop(room);

    state.remove_client(&client_id);

    let tasks = senders
        .iter()
        .map(|tx| tx.send(Outbound::Stream(message.clone())));
    let _ = futures_util::future::join_all(tasks).await;

    Ok(())
}
