mod messaging;
mod room;
mod tui;
#[macro_use]
mod logging;

use crate::messaging::{InitialResponse, MessagePrefix};
use crate::room::{ClientSender, State};
use crate::{logging::EventType, messaging::InitialMessage};

use std::io::Cursor;
use std::pin::Pin;
use std::sync::{Arc, Mutex};
use std::task::{Context, Poll};
use std::{env, fs, process, thread};
use std::{error::Error, fs::File};
use std::{io::BufReader, net::SocketAddr};

use futures_util::{SinkExt, StreamExt};
use rcgen::{CertifiedKey, generate_simple_self_signed};
use tokio::io::{AsyncRead, AsyncReadExt, AsyncWrite, AsyncWriteExt, ReadBuf};
use tokio::{net::TcpListener, sync::mpsc};
use tokio_rustls::TlsAcceptor;
use tokio_rustls::rustls::ServerConfig;
use tokio_rustls::rustls::pki_types::{CertificateDer, PrivateKeyDer};
use tokio_tungstenite::tungstenite::{Message, Utf8Bytes};

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
            let parsed = value.map(|n| n.parse().ok()).flatten();
            if let Some(p) = parsed {
                port = p;
            }
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
                Err(e) => {
                    error_lock!(state, "Secure connection with client failed {}", e);
                    return;
                }
            }
        });
    }

    Ok(())
}

/// Replays a buffer of already-read ("peeked") bytes before continuing to read
/// from the underlying stream. Writes pass straight through. Used to "un-read"
/// bytes we inspected so `accept_async` still sees the full request.
struct Prefixed<S> {
    prefix: Cursor<Vec<u8>>,
    inner: S,
}

impl<S> Prefixed<S> {
    fn new(prefix: Vec<u8>, inner: S) -> Self {
        Self {
            prefix: Cursor::new(prefix),
            inner,
        }
    }
}

impl<S: AsyncRead + Unpin> AsyncRead for Prefixed<S> {
    fn poll_read(
        mut self: Pin<&mut Self>,
        cx: &mut Context<'_>,
        buf: &mut ReadBuf<'_>,
    ) -> Poll<std::io::Result<()>> {
        let pos = self.prefix.position() as usize;
        let data = self.prefix.get_ref();
        if pos < data.len() {
            let n = (data.len() - pos).min(buf.remaining());
            buf.put_slice(&data[pos..pos + n]);
            self.prefix.set_position((pos + n) as u64);
            return Poll::Ready(Ok(()));
        }
        Pin::new(&mut self.inner).poll_read(cx, buf)
    }
}

impl<S: AsyncWrite + Unpin> AsyncWrite for Prefixed<S> {
    fn poll_write(
        mut self: Pin<&mut Self>,
        cx: &mut Context<'_>,
        buf: &[u8],
    ) -> Poll<std::io::Result<usize>> {
        Pin::new(&mut self.inner).poll_write(cx, buf)
    }

    fn poll_flush(mut self: Pin<&mut Self>, cx: &mut Context<'_>) -> Poll<std::io::Result<()>> {
        Pin::new(&mut self.inner).poll_flush(cx)
    }

    fn poll_shutdown(mut self: Pin<&mut Self>, cx: &mut Context<'_>) -> Poll<std::io::Result<()>> {
        Pin::new(&mut self.inner).poll_shutdown(cx)
    }
}

async fn handle_connection<S>(state: Arc<Mutex<State>>, mut raw_stream: S, addr: SocketAddr)
where
    S: AsyncRead + AsyncWrite + Unpin + Send + 'static,
{
    // "Peek" at the request: read the first chunk, then replay it in front of
    // the stream. `peek()` is an inherent method on `TcpStream` (not a trait),
    // so it can't be called generically or on a `TlsStream` — this replays the
    // bytes instead, letting us both inspect the request and recover the stream.
    let mut buf = vec![0u8; 1024];
    let n = match raw_stream.read(&mut buf).await {
        Ok(0) => return, // client hung up
        Ok(n) => n,
        Err(e) => {
            error_lock!(state, "Failed to read from {addr}: {e}");
            return;
        }
    };
    buf.truncate(n);

    let message = String::from_utf8_lossy(&buf).to_ascii_lowercase();

    let is_ws = message.contains("upgrade: websocket");

    let mut stream = Prefixed::new(buf, raw_stream);

    // Respond to plain HTTP requests properly, rather than failing the handshake.
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
        return;
    }

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
            if write.send(msg.clone()).await.is_err() {
                break;
            }

            if let Message::Close(_) = msg {
                // NOTE
                // I believe that the `rx` automatically closes whene all transmitters are dropped.
                // Which they are when a close message is sent because we remove the client
                // from the `ClientMap`

                // Not sure if this is needed
                let _ = write.close();
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

                let Ok(old_message) = message.into_text() else {
                    continue;
                };
                let bytes = old_message.as_bytes();

                let mut buf = bytes::BytesMut::with_capacity(bytes.len());
                buf[1..].copy_from_slice(bytes);
                buf[0] = MessagePrefix::Client as u8;

                let message = Message::Binary(buf.into());
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
