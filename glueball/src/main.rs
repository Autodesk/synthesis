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
use crate::config::Config;
use crate::logging::EventType;
use crate::messaging::handle_connection;
use crate::room::State;
use crate::tui::start_tui_thread;

use std::error::Error;
use std::sync::{Arc, Mutex};

use tokio::net::TcpListener;
use tokio_rustls::TlsAcceptor;

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error>> {
    let state = Arc::new(Mutex::new(State::new()));

    // Parse and handle initial configuration
    let config: Config = argh::from_env();
    if !config.headless {
        start_tui_thread(&state);
    }
    if let Some(room_id) = config.permanent_room {
        state.lock().unwrap().new_permanent_room(room_id);
    }

    // `listener` will be used regardless of the security level specified
    let listener = TcpListener::bind(format!("127.0.0.1:{}", config.port)).await?;

    if !config.secure {
        while let Ok((stream, addr)) = listener.accept().await {
            tokio::spawn(handle_connection(state.clone(), stream, addr));
        }

        return Ok(());
    }

    // Run secure server
    let tls_config = build_tls_config()?;
    let acceptor = TlsAcceptor::from(Arc::new(tls_config));

    info_lock!(state, "Server listening on port {} (secure)", config.port);

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
