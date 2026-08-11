mod cert;
mod cleanup;
mod config;
#[macro_use]
mod logging;
mod http;
mod kick;
mod messaging;
mod model;
mod state;
#[cfg(test)]
mod tests;
mod tui;
#[macro_use]
mod util;
mod wire;

use crate::cert::build_identity;
use crate::cleanup::Cleanup;
use crate::config::retrieve_config;
use crate::http::spawn_http_responder;
use crate::kick::setup_user_action_system;
use crate::logging::{
    EventType, LogDestination, LogRequest, Logger, MAX_LOG_LINES, print_global, print_room,
    spawn_log_receiver,
};
use crate::messaging::handle_session;
use crate::model::{CertificateHash, CertificateHashes};
use crate::state::State;
use crate::tui::start_tui_thread;
use crate::util::get_local_ip;
use anyhow::{Result, bail};
use std::sync::{Arc, Mutex};
use std::time::Duration;
use tokio::sync::mpsc;
use wtransport::{Endpoint, ServerConfig};

/// How often to poke an otherwise idle connection so QUIC doesn't time it out.
const KEEP_ALIVE_INTERVAL: Duration = Duration::from_secs(10);

#[tokio::main]
async fn main() -> Result<()> {
    let _cleanup_trigger = Cleanup;

    // # Parse and create defaults for the application configuration
    let config = retrieve_config()?;

    // # Setup logging, state, channels and listeners, etc

    // In my mind, this is the best way to handling an interface that could be sending message to a
    // tui running on a different OS thread or just printing them
    let (logging_tx, logging_rx) = mpsc::channel::<LogRequest>(MAX_LOG_LINES);

    let state = Arc::new(State::new(logging_tx.clone()));

    let user_action_tx = setup_user_action_system(&state);

    if config.headless {
        // Read the logging channel and immediantly print result
        let print_to_terminal =
            move |message: String, kind: EventType, log_destination: LogDestination| {
                match log_destination {
                    LogDestination::Global => print_global(&message, &kind),
                    LogDestination::Room(id) => print_room(&message, &kind, &id),
                    // This can be a no-op, because no room is ever created,
                    // as the logger isn't used
                    LogDestination::RemoveRoom(_) => {}
                }
            };

        spawn_log_receiver(logging_rx, print_to_terminal);
    } else {
        // Only use the `logger` in tui mode
        // `logger` will be jointly owned by the main thread and the tui thread
        // Tokio tasks will pass their log messages down the logging channel
        // instead of having to take a lock to log
        let logger = Arc::new(Mutex::new(Logger::new(MAX_LOG_LINES)));

        start_tui_thread(&state, user_action_tx, logger.clone());

        // Read the logging channel and write every message to the `logger`
        let send_to_logger =
            move |message: String, kind: EventType, log_destination: LogDestination| {
                match log_destination {
                    LogDestination::Global => lock!(logger).push_global(message, kind),
                    LogDestination::Room(id) => lock!(logger).push_room(message, kind, id),
                    LogDestination::RemoveRoom(id) => lock!(logger).remove_room(&id),
                }
            };

        spawn_log_receiver(logging_rx, send_to_logger);
    }

    if let Some(room_id) = config.permanent_room {
        state.new_permanent_room(room_id);
    }

    if !config.secure {
        warn_global!(
            logging_tx,
            "Ignoring the insecure setting: WebTransport traffic is always encrypted"
        );
    }

    // # Setup the WebTransport endpoint

    let identity = build_identity(&config.cert_dir).await?;

    // Browsers will not offer to trust a self-signed WebTransport certificate the
    // way they do for HTTPS, so clients pin these digests with
    // `serverCertificateHashes` instead. They are served over `GET /cert`
    let hashes = CertificateHashes {
        hashes: identity
            .certificate_chain()
            .as_slice()
            .iter()
            .map(|certificate| CertificateHash::sha256(certificate.hash().as_ref()))
            .collect(),
    };

    let server_config = ServerConfig::builder()
        .with_bind_default(config.port)
        .with_identity(identity)
        .keep_alive_interval(Some(KEEP_ALIVE_INTERVAL))
        .build();

    let Ok(endpoint) = Endpoint::server(server_config) else {
        bail!("Could not create UDP listener (the port is likely in use)");
    };

    // Serves the certificate digests over the TCP half of the same port
    spawn_http_responder(config.port, hashes, logging_tx.clone()).await?;

    let local_ip = get_local_ip().unwrap_or_else(|| String::from("0.0.0.0"));

    info_global!(
        logging_tx,
        "Server hosted on {local_ip} listening at port {} (UDP), certificate at /cert (TCP)",
        config.port
    );

    loop {
        let session = endpoint.accept().await;

        // The handshake happens in a task to avoid being held up by a slow client
        tokio::spawn(handle_session(state.clone(), session, logging_tx.clone()));
    }
}
