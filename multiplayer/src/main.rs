mod messaging;
mod room;
mod tui;
#[macro_use]
mod logging;

use crate::messaging::InitialResponse;
use crate::room::{ClientSender, State};
use crate::{logging::EventType, messaging::InitialMessage};

use std::error::Error;
use std::fs::File;
use std::io::BufReader;
use std::net::SocketAddr;
use std::sync::{Arc, Mutex};
use std::{env, fs, process, thread};

use futures_util::{SinkExt, StreamExt};
use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio::io::{AsyncRead, AsyncWrite};
use tokio::net::TcpListener;
use tokio::sync::mpsc;
use tokio_rustls::TlsAcceptor;
use tokio_rustls::rustls::ServerConfig;
use tokio_rustls::rustls::pki_types::{CertificateDer, PrivateKeyDer};
use tokio_tungstenite::tungstenite::{Message, Utf8Bytes};

const DEFAULT_PORT: u32 = 9001;

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
            let parsed = value.map(|n| n.parse().ok()).flatten();
            if let Some(p) = parsed {
                port = p;
            }
        }
    });

    // `listener` will be used regardless of the security level specified
    let listener = TcpListener::bind(format!("127.0.0.1:{port}")).await?;

    if env::args().any(|a| a == "--insecure") {
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
                Err(e) => {
                    error_lock!(state, "Secure connection with client failed {}", e);
                    return;
                }
            }
        });
    }

    Ok(())
}

async fn handle_connection<S>(state: Arc<Mutex<State>>, raw_stream: S, addr: SocketAddr)
where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    let ws_stream = match tokio_tungstenite::accept_async(raw_stream).await {
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

    // Parse initial message, then user in correct room
    let Some(Ok(Message::Text(initial_message_string))) = read.next().await else {
        warn_lock!(
            state,
            "Client disconnected before handshake (probably a test)"
        );
        return;
    };

    let Ok(initial_message) = serde_json::from_str::<InitialMessage>(&initial_message_string)
    else {
        error_lock!(state, "{addr} sent an invalid initial message");
        return;
    };

    let (client_id, room_id) = {
        // The lock is relinquished after this match statement
        let mut guard = state.lock().unwrap();
        match initial_message.room_id {
            None => guard.add_room_and_authority(initial_message.name, tx),
            Some(room_id) => match guard.add_client_to_room(initial_message.name, tx, room_id) {
                Some(client_id) => (client_id, room_id),
                None => return,
            },
        }
    };

    let response = InitialResponse {
        room_id,
        client_id: client_id.to_string(),
    };
    let bytes = Utf8Bytes::from(serde_json::to_string(&response).unwrap());
    if write.send(Message::Text(bytes)).await.is_err() {
        error_lock!(state, "Failed to send back initial response");

        return;
    }

    tokio::spawn(async move {
        while let Some(msg) = rx.recv().await {
            if write.send(msg).await.is_err() {
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
            Message::Text(_) | Message::Binary(_) => {
                let senders: Vec<ClientSender> = {
                    // The lock is relinquished after senders are retreived
                    let mut guard = state.lock().unwrap();
                    guard.get_senders_from_user_room(client_id)
                };

                for tx in senders {
                    tx.send(message.clone()).await.ok();
                }
            }

            Message::Close(_) => {
                let mut guard = state.lock().unwrap();
                warn!(guard, "Connection with {client_id} closed");
                guard.remove_client(client_id);

                return;
            }
            _ => todo!("Handle Ping/Pong"),
        }
    }
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
