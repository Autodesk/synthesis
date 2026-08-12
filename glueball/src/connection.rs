//! This module is responsible for handling connections with users.
//!
//! It makes heavy use of functions provided in the [`messaging`] module.

use crate::{
    error_global, info_global,
    messaging::{handle_client_message, wait_for_initialization},
    model::ServerToClientMessage,
    prefixed::{ConnectionStatus, into_prefixed_or_respond},
    state::{ClientId, State},
    util::{server_sent_msg, trim_uuid},
    warn_global,
};
use anyhow::{Result, bail};
use futures_util::{SinkExt, StreamExt};
use std::{net::SocketAddr, sync::Arc, time::Duration};
use tokio::{
    io::{AsyncRead, AsyncWrite},
    sync::mpsc,
    time::timeout,
};
use tokio_tungstenite::tungstenite::Message;

pub const TIMEOUT: Duration = Duration::from_secs(30);

pub async fn handle_connection<S>(state: Arc<State>, raw_stream: S, addr: SocketAddr)
where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    let ConnectionStatus::Ws(stream) = into_prefixed_or_respond(raw_stream, addr).await else {
        return;
    };

    let ws_stream = match tokio_tungstenite::accept_async(stream).await {
        Ok(ws_stream) => ws_stream,
        Err(e) => {
            error_global!("Websocket handshake with {addr} failed: {e}");
            return;
        }
    };

    info_global!("WS connection established with {addr}");

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client through the `write` sink
    let (tx, mut rx) = mpsc::channel::<Message>(64);

    // Order of messages sent from a new client to the server:
    // 1-n. Any number of `RequestRooms` messages -> server will return a list of rooms
    // n..n+1. An `InitializationMessage`, indicating whether the client wishes to create or join a room -> server will return a room and client id
    // n+1..m. Any number of messages that will be forwarded to every other client in their room -> server will not respond, instead forwarding
    let (mut write, mut read) = ws_stream.split();

    let Some(client_id) =
        wait_for_initialization(state.clone(), &mut read, &mut write, tx.clone(), addr).await
    else {
        return;
    };

    // This task listens for messages to the channel and sends them down the sink to the client
    tokio::spawn(async move {
        while let Some(msg) = rx.recv().await {
            if write.send(msg).await.is_err() {
                break;
            }
        }
    });

    // Listen for and pass along messages to other client channels in the same room
    loop {
        let result = timeout(TIMEOUT, read.next()).await;

        // If it's a timeout error, we print such
        if result.is_err() {
            warn_global!("{} timed out", trim_uuid(&client_id));
        }

        // If it's anything but a correct response, we disconnect
        let Ok(Some(Ok(message))) = result else { break };

        let message_result = handle_client_message(message, state.clone(), client_id).await;

        if message_result.is_break() {
            // We don't break here, to avoid double closing the connection
            return;
        }
    }

    let _ = handle_client_close(client_id, &state).await;
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

    let tasks = senders.iter().map(|tx| tx.send(message.clone()));
    let _ = futures_util::future::join_all(tasks).await;

    Ok(())
}
