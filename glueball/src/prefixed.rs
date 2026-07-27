use crate::{EventType, room::State};
use tokio::io::{AsyncRead, AsyncReadExt, AsyncWrite, AsyncWriteExt, ReadBuf};

use std::net::SocketAddr;
use std::sync::{Arc, Mutex};
use std::task::{Context, Poll};
use std::{io::Cursor, pin::Pin};

/// Replays a buffer of already-read ("peeked") bytes before continuing to read
/// from the underlying stream. Writes pass straight through. Used to "un-read"
/// bytes we inspected so `accept_async` still sees the full request.
pub struct Prefixed<S> {
    prefix: Cursor<Vec<u8>>,
    inner: S,
}

impl<S> Prefixed<S> {
    pub const fn new(prefix: Vec<u8>, inner: S) -> Self {
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
        let pos = usize::try_from(self.prefix.position()).expect("32-bit machines not supported");
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

pub trait SynthesisStream: AsyncRead + AsyncWrite + Unpin + Send + 'static {}
impl<S: AsyncRead + AsyncWrite + Unpin + Send + 'static> SynthesisStream for S {}

pub enum ConnectionStatus<S> {
    HungUp,
    Error,
    Http,
    Ws(Prefixed<S>),
}

pub async fn into_prefixed_or_respond<S>(
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
