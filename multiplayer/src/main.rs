mod cert;
mod config;
#[macro_use]
mod logging;
mod messaging;
mod model;
mod prefixed;
mod room;
mod tui;
mod util;

use crate::cert::build_tls_config;
use crate::config::{Config, parse_arguments};
use crate::logging::EventType;
use crate::messaging::handle_connection;
use crate::room::State;

use std::env;
use std::error::Error;
use std::sync::{Arc, Mutex};

use tokio::net::TcpListener;
use tokio_rustls::TlsAcceptor;

const DEFAULT_PORT: u32 = 2610;

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error>> {
    let state = Arc::new(Mutex::new(State::new()));

    let Config { port } = parse_arguments(&state);

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
