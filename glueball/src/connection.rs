//! Module for handling all connections with a client
//! Heavily utilizes methods from the `messaging` module

use crate::messaging::{
    CLOSED_BY_SERVER, accept_message, handle_client_close, handle_client_message_datagram,
    handle_client_message_stream, handle_first_message, handle_room_list_request,
};
use crate::model::{ClientToServerMessage, ServerToClientMessage};
use crate::state::{ClientId, ClientSender};
use crate::util::{server_sent_msg, trim_uuid};
use crate::wire::send_message;
use crate::{
    config::build_server_config, error_global, info_global, state::State, util::get_local_ip,
    warn_global, wire::Outbound,
};
use anyhow::{Result, bail};
use futures_util::never::Never;
use std::net::SocketAddr;
use std::{sync::Arc, time::Duration};
use tokio::sync::mpsc::Receiver;
use tokio::{
    sync::mpsc::{self},
    time::timeout,
};
use wtransport::error::SendDatagramError;
use wtransport::{Connection, Endpoint, Identity, endpoint::IncomingSession};

/// How long to wait for a client's next message before assuming it is gone.
/// Clients ping every five seconds, so a silent client is a dead one even while
/// its datagrams keep arriving.
pub const TIMEOUT: Duration = Duration::from_secs(30);

/// Number of messages that may be queued for one client before writes to it
/// block (streams) or are dropped (datagrams).
const OUTBOUND_CAPACITY: usize = 64;

pub async fn spawn_webtransport_responder(
    state: Arc<State>,
    port: u16,
    identity: Identity,
) -> Result<Never> {
    let server_config = build_server_config(port, identity);

    let Ok(wt_endpoint) = Endpoint::server(server_config) else {
        bail!("Could not create UDP listener (the port is likely in use)");
    };

    let local_ip = get_local_ip().unwrap_or_else(|| String::from("0.0.0.0"));

    info_global!(
        "Server hosted on {local_ip} listening at port {} (UDP), certificate at /cert (TCP)",
        port
    );

    loop {
        tokio::spawn(accept_incoming_session(
            state.clone(),
            wt_endpoint.accept().await,
        ));
    }
}

/// Completes the `WebTransport` handshake for an incoming QUIC connection
///
/// Then hands the session to [`handle_connection`].
pub async fn accept_incoming_session(state: Arc<State>, session: IncomingSession) {
    let addr = session.remote_address();

    let request = match session.await {
        Ok(request) => request,
        Err(e) => {
            error_global!("QUIC handshake with {addr} failed: {e}");
            return;
        }
    };

    let connection = match request.accept().await {
        Ok(connection) => connection,
        Err(e) => {
            error_global!("WebTransport handshake with {addr} failed: {e}");
            return;
        }
    };

    handle_connection(state, connection, addr).await;
}

async fn handle_connection(state: Arc<State>, connection: Connection, addr: SocketAddr) {
    info_global!("WT session established with {addr}");

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client by the writer task
    let (tx, rx) = mpsc::channel::<Outbound>(OUTBOUND_CAPACITY);

    // Order of messages sent from a new client to the server:
    // 1-n. Any number of `RequestRooms` messages -> server will return a list of rooms
    // n..n+1. An `InitializationMessage`, indicating whether the client wishes to create or join a room -> server will return a room and client id
    // n+1..m. Any number of messages that will be forwarded to every other client in their room -> server will not respond, instead forwarding
    let Some(client_id) = wait_for_initialization(&state, &connection, tx.clone(), addr).await
    else {
        return;
    };

    spawn_client_sink(connection.clone(), rx);
    spawn_datagram_listener(connection.clone(), state.clone(), client_id);

    // Listen for and pass along meskesages to other client channels in the same room
    loop {
        // Messages are taken one at a time rather than concurrently: streams give
        // no ordering guarantees between each other, so draining them in arrival
        // order is the closest thing to the ordering clients used to rely on
        let message = match timeout(TIMEOUT, accept_message(&connection)).await {
            Ok(Ok(message)) => message,
            Ok(Err(e)) => {
                warn_global!("{} disconnected: {e}", trim_uuid(&client_id));
                break;
            }
            Err(_) => {
                warn_global!("{} timed out", trim_uuid(&client_id));
                break;
            }
        };

        handle_client_message_stream(message, &state, &client_id).await;
    }

    let _ = handle_client_close(client_id, &state).await;
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
) -> Option<ClientId> {
    loop {
        match handle_first_message(connection, addr).await {
            Some(ClientToServerMessage::RequestRooms) => {
                handle_room_list_request(state, connection).await;
            }

            // When they ask to initialize a connection
            // we add them to that room or create one for them
            Some(ClientToServerMessage::InitializeConnection { room_id, name }) => {
                let (client_id, room_id) = state.initialize_client_in_room(tx, room_id, &name)?;

                let message = server_sent_msg(ServerToClientMessage::SendInfo {
                    room_id,
                    client_id: client_id.to_string(),
                });

                if send_message(connection, &message).await.is_err() {
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

/// Client sink passes all messages from `rx` down the client's `connection`,
/// back to the client machine.
fn spawn_client_sink(connection: Connection, mut rx: Receiver<Outbound>) {
    tokio::spawn(async move {
        // Undeliverable datagrams come in floods rather than one at a time, so the
        // reason is worth saying once and then never again for this session
        let mut warned_undeliverable = false;

        while let Some(message) = rx.recv().await {
            match message {
                Outbound::Stream(payload) => {
                    if send_message(&connection, &payload).await.is_err() {
                        break;
                    }
                }

                Outbound::Datagram(ref payload) => match connection.send_datagram(payload) {
                    Ok(()) => {}
                    Err(SendDatagramError::NotConnected) => break,
                    Err(e) if warned_undeliverable => {
                        let _ = e;
                    }

                    Err(SendDatagramError::TooLarge) => {
                        warned_undeliverable = true;
                        warn_global!(
                            "Dropping datagrams: {} bytes exceeds the {} the path allows. Send these over a stream instead",
                            payload.len(),
                            connection.max_datagram_size().unwrap_or_default()
                        );
                    }

                    Err(SendDatagramError::UnsupportedByPeer) => {
                        warned_undeliverable = true;
                        warn_global!("Dropping datagrams: the client does not accept them");
                    }
                },

                Outbound::Close => {
                    connection.close(CLOSED_BY_SERVER, b"Closed by server");
                    break;
                }
            }
        }
    });
}

/// Reads datagrams, extracting their payload and sending them to [`handle_client_message`]
fn spawn_datagram_listener(connection: Connection, state: Arc<State>, client_id: ClientId) {
    tokio::spawn(async move {
        while let Ok(datagram) = connection.receive_datagram().await {
            handle_client_message_datagram(datagram.payload(), &state, &client_id).await;
        }
    });
}
