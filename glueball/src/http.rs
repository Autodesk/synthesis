//! A minimal HTTP/1.1 responder sharing the server's port over TCP.
//!
//! `WebTransport` is carried over QUIC, which is UDP.
//!
//! The TCP half of the port would otherwise sit unused. Its job is
//! to answer the plain HTTP requests a browser makes before it connects:
//!
//! * `GET /cert` returns this server's certificate digests as JSON, which a
//!   client passes to `serverCertificateHashes` to reach a self-signed server.
//! * Anything else gets a short body, so hitting the port in a browser says
//!   something useful instead of hanging.
//!
//! Responses are plain HTTP rather than HTTPS on purpose:
//! serving them over TLS with the very certificate the client
//! is trying to learn about would be circular!
//!
//! Browsers treat `localhost` and `127.0.0.1` as trustworthy origins,
//! so mixed-content rules do not block this for the local servers it exists for.

use crate::model::CertificateHashes;

use anyhow::{Result, bail};
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::{TcpListener, TcpStream};

use std::net::SocketAddr;
use std::sync::Arc;

/// Largest request head we will read. Anything a browser sends us fits well
/// inside this, and we have no use for a body.
const MAX_REQUEST_SIZE: usize = 1024;

/// These responses are read by a page served from a different origin, so they
/// have to opt in to being read cross-origin.
const CORS_HEADERS: &str =
    "Access-Control-Allow-Origin: *\r\nAccess-Control-Allow-Methods: GET, OPTIONS\r\n";

/// Binds the TCP half of `port` and starts answering HTTP requests on it.
///
/// Binding happens before returning so a port conflict is reported to the caller
/// rather than swallowed by a background task.
pub async fn spawn_http_responder(port: u16, hashes: CertificateHashes) -> Result<()> {
    let Ok(listener) = TcpListener::bind(format!("0.0.0.0:{port}")).await else {
        bail!("Could not create TCP listener (the port is likely in use)");
    };

    let certificate_response = Arc::new(json_response(&serde_json::to_string(&hashes)?));

    tokio::spawn(async move {
        while let Ok((stream, addr)) = listener.accept().await {
            let certificate_response = certificate_response.clone();

            // Each request gets a task so a slow client cannot hold up the others
            tokio::spawn(async move {
                handle_request(stream, addr, &certificate_response).await;
            });
        }
    });

    Ok(())
}

/// Reads one request and writes one response.
///
/// Connections are never kept alive, so there is no need to find the end of the
/// request head: the first read tells us the method and path, which is all that
/// distinguishes the handful of responses we serve.
async fn handle_request(mut stream: TcpStream, addr: SocketAddr, certificate_response: &str) {
    let mut buf = [0u8; MAX_REQUEST_SIZE];

    let count = match stream.read(&mut buf).await {
        Ok(0) => return,
        Ok(count) => count,
        Err(e) => {
            error_global!("Failed to read from {addr}: {e}");
            return;
        }
    };

    let truncated = &buf[0..count];
    let request = String::from_utf8_lossy(truncated).to_ascii_lowercase();

    let response = if request.starts_with("get /cert ") {
        certificate_response.to_string()
    } else if request.starts_with("options ") {
        // A plain `GET` should not provoke a preflight, but answering one costs
        // nothing and saves a confusing failure if a client adds a header
        text_response("")
    } else {
        text_response("Synthesis")
    };

    let _ = stream.write_all(response.as_bytes()).await;
    let _ = stream.flush().await;
}

fn json_response(body: &str) -> String {
    build_response("application/json", body)
}

fn text_response(body: &str) -> String {
    build_response("text/plain", body)
}

fn build_response(content_type: &str, body: &str) -> String {
    format!(
        "HTTP/1.1 200 OK\r\n\
         Content-Type: {content_type}\r\n\
         Content-Length: {}\r\n\
         {CORS_HEADERS}\
         Connection: close\r\n\
         \r\n\
         {body}",
        body.len()
    )
}
