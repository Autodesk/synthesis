mod messaging;
mod prefixed;
mod room;
mod tui;
#[macro_use]
mod logging;

use crate::logging::EventType;
use crate::messaging::{
    ClientToServerMessage, MessagePrefix, ServerMessage, deserialize_messagepack,
    serialize_messagepack,
};
use crate::prefixed::{Prefixed, SynthesisStream};
use crate::room::{ClientId, ClientSender, State};

use std::ops::Deref;
use std::sync::{Arc, Mutex};
use std::{env, fs, process, thread};
use std::{error::Error, fs::File};
use std::{io::BufReader, net::SocketAddr};

use futures_util::stream::{SplitSink, SplitStream};
use futures_util::{SinkExt, StreamExt};
use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio::io::{AsyncRead, AsyncReadExt, AsyncWrite, AsyncWriteExt};
use tokio::{net::TcpListener, sync::mpsc};
use tokio_rustls::TlsAcceptor;
use tokio_rustls::rustls::ServerConfig;
use tokio_rustls::rustls::pki_types::{CertificateDer, PrivateKeyDer};
use tokio_tungstenite::WebSocketStream;
use tokio_tungstenite::tungstenite::Message;

const DEFAULT_PORT: u32 = 2610;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let state = Arc::new(Mutex::new(State::new()));

    // `--headless` is passed in to not open the dashboard
    if !env::args().any(|a| a == "--headless") {
        let tui_state_handle = state.clone();

        // On an OS thread because crossterm (and thus ratatui) will block on user input
        // So it wouldn't play nice with tokio's runtime, which expects yielding
        thread::spawn(move || {
            if let Err(e) = tui::run(tui_state_handle) {
                eprintln!("TUI error: {e}");
            }

            process::exit(0);
        });
    }

    // Queries and sets the port given by the cli, or the [`DEFAULT_PORT`] if one was not passed
    let mut port: u32 = DEFAULT_PORT;
    env::args().enumerate().for_each(|(i, arg)| {
        if arg == "--port" {
            let value = env::args().nth(i + 1);
            let parsed = value.and_then(|n| n.parse().ok());
            if let Some(p) = parsed {
                port = p;
            }
        }
    });

    // Queries and sets the port given by the cli, or the [`DEFAULT_PORT`] if one was not passed
    env::args().enumerate().for_each(|(i, arg)| {
        if arg == "--permanentRoom" {
            let Some(room_id) = env::args().nth(i + 1) else {
                return;
            };
            let mut guard = state.lock().unwrap();
            guard.new_permanent_room(room_id);
        }
    });

    // `listener` will be used regardless of the security level specified
    let listener = TcpListener::bind(format!("127.0.0.1:{port}")).await?;

    if !env::args().any(|a| a == "--secure") {
        while let Ok((stream, addr)) = listener.accept().await {
            tokio::spawn(handle_connection(state.clone(), stream, addr));
        }

        return Ok(());
    }

    let config = build_tls_config()?;
    let acceptor = TlsAcceptor::from(Arc::new(config));

    info_lock!(state, "Server listening on port {port} (secure)");

    while let Ok((stream, addr)) = listener.accept().await {
        let acceptor = acceptor.clone();
        let state = state.clone();

        // TLS handshake happens in task to avoid being held up by a slow client
        tokio::spawn(async move {
            match acceptor.accept(stream).await {
                Ok(tls_stream) => handle_connection(state, tls_stream, addr).await,
                Err(e) => error_lock!(state, "Secure connection with client failed {}", e),
            }
        });
    }

    Ok(())
}

async fn handle_connection<S>(state: Arc<Mutex<State>>, raw_stream: S, addr: SocketAddr)
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
    // 1. Initialization. An instance of the `InitializationMessage` structure.
    //    Indicating whether the client wishes to create or join a room
    // 2..n. Any number of messages that will be forwarded to every other client in their room
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
    while let Some(maybe_message) = read.next().await {
        let Ok(message) = maybe_message else {
            continue;
        };

        match message {
            Message::Binary(bytes) => {
                let senders: Vec<ClientSender> = {
                    // The lock is relinquished after senders are retreived
                    let mut guard = state.lock().unwrap();
                    guard.get_senders_from_user_room(client_id)
                };

                let message = prefix_message(bytes, MessagePrefix::Client);
                for tx in senders {
                    tx.send(message.clone()).await.ok();
                }
            }

            Message::Close(_) => {
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
                        return;
                    };

                    let senders = room.get_senders(Some(&client_id)).clone();
                    guard.remove_client(client_id);

                    senders
                };

                for tx in senders {
                    let _ = tx.clone().send(message.clone()).await;
                }

                return;
            }
            _ => todo!("Handle Ping/Pong"),
        }
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
    read: &mut SplitStream<WebSocketStream<Prefixed<S>>>,
    write: &mut SplitSink<WebSocketStream<Prefixed<S>>, Message>,
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
                    match room_id {
                        None => guard.add_room_and_authority(name, tx),
                        Some(room_id) => match guard.add_client_to_room(name, tx, &room_id) {
                            Some(client_id) => (client_id, room_id),
                            None => return None,
                        },
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
            None => return None,
        }
    }
}

async fn handle_room_list_request<S>(
    state: Arc<Mutex<State>>,
    write: &mut SplitSink<WebSocketStream<Prefixed<S>>, Message>,
) where
    S: SynthesisStream,
{
    let message = {
        let guard = state.lock().unwrap();
        ServerMessage::RoomList {
            rooms: guard.list_rooms(),
        }
    };

    let bytes = serialize_messagepack(message);
    let message = prefix_message(bytes, MessagePrefix::Server);

    write.send(message).await.ok();
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

    let Some(message) =
        deserialize_messagepack::<ClientToServerMessage, bytes::Bytes>(&message_data)
    else {
        error_lock!(state, "{addr} sent an invalid initial message");
        return None;
    };

    Some(message)
}

enum ConnectionStatus<S> {
    HungUp,
    Error,
    Http,
    Ws(Prefixed<S>),
}

async fn into_prefixed_or_respond<S>(
    state: Arc<Mutex<State>>,
    mut raw_stream: S,
    addr: SocketAddr,
) -> ConnectionStatus<S>
where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    // "Peek" at the request: read the first chunk, then replay it in front of
    // the stream. `peek()` is an inherent method on `TcpStream` (not a trait),
    // so it can't be called generically or on a `TlsStream` — this replays the
    // bytes instead, letting us both inspect the request and recover the stream.
    let mut buf = vec![0u8; 1024];
    let n = match raw_stream.read(&mut buf).await {
        Ok(0) => return ConnectionStatus::HungUp,
        Ok(n) => n,
        Err(e) => {
            error_lock!(state, "Failed to read from {addr}: {e}");
            return ConnectionStatus::Error;
        }
    };
    buf.truncate(n);

    let message = String::from_utf8_lossy(&buf).to_ascii_lowercase();
    let mut stream = Prefixed::new(buf, raw_stream);

    // Respond to plain HTTP requests properly, rather than failing the handshake.
    let is_ws = message.contains("upgrade: websocket");
    if !is_ws {
        let resp = if message[0..10] == *"get /cert " {
            let body = "<script>window.close()</script>You may now close this page.";
            format!(
                "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nContent-Length: {}\r\nConnection: close\r\n\r\n{body}",
                body.len()
            )
        } else {
            let body = "Synthesis";
            format!(
                "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {}\r\nConnection: close\r\n\r\n{body}",
                body.len(),
            )
        };

        let _ = stream.write_all(resp.as_bytes()).await;
        let _ = stream.flush().await;
        return ConnectionStatus::Http;
    }

    ConnectionStatus::Ws(stream)
}

/// Creates a new `Message::Binary` containing `bytes`,
/// prefixed with the byte value of `MessagePrefix`
pub fn prefix_message<M>(bytes: M, prefix: MessagePrefix) -> Message
where
    M: Deref<Target = [u8]>,
{
    let mut buf = vec![0u8; bytes.len() + 1];
    buf[1..].copy_from_slice(&bytes);
    buf[0] = prefix as u8;

    Message::Binary(buf.into())
}

/// Creates a TLS config for the server
/// Generates a certificate if one does not exist
fn build_tls_config() -> Result<ServerConfig, Box<dyn Error>> {
    ensure_certificate()?;

    let mut cert_reader = BufReader::new(File::open("./secrets/cert.pem")?);
    let cert_chain: Vec<CertificateDer> =
        rustls_pemfile::certs(&mut cert_reader).collect::<Result<_, _>>()?;

    let mut key_reader = BufReader::new(File::open("./secrets/key.pem")?);
    let key = rustls_pemfile::pkcs8_private_keys(&mut key_reader)
        .next()
        .ok_or("Invalid PKCS#8 private key found in ./secrets/key.pem")??;

    let config = ServerConfig::builder()
        .with_no_client_auth()
        .with_single_cert(cert_chain, PrivateKeyDer::Pkcs8(key))?;

    Ok(config)
}

/// Writes a self-signed certificate and keypair to `./secrets` if one isn't already present.
fn ensure_certificate() -> Result<(), Box<dyn Error>> {
    if !fs::exists("./secrets")? {
        fs::create_dir("./secrets")?;
    }

    if fs::exists("./secrets/cert.pem")? {
        return Ok(());
    }

    let subject_alt_names = vec!["localhost".to_string(), "127.0.0.1".to_string()];
    let CertifiedKey { cert, signing_key } = generate_simple_self_signed(subject_alt_names)?;

    fs::write("./secrets/cert.pem", cert.pem())?;
    fs::write("./secrets/key.pem", signing_key.serialize_pem())?;

    Ok(())
}
