mod cert;
mod config;
#[macro_use]
mod logging;
mod kick;
mod messaging;
mod model;
mod panic;
mod prefixed;
mod room;
#[cfg(test)]
mod tests;
mod tui;
#[macro_use]
mod util;

use crate::cert::build_tls_config;
use crate::config::retrieve_config;
use crate::kick::setup_kick_system;
use crate::logging::{
    EventType, LogDestination, LogRequest, Logger, MAX_LOG_LINES, print_global, print_room,
    spawn_log_receiver,
};
use crate::messaging::handle_connection;
use crate::room::State;
use crate::tui::start_tui_thread;
use crate::util::get_local_ip;

use std::sync::{Arc, Mutex};

use anyhow::{Result, bail};
use tokio::net::TcpListener;
use tokio::sync::mpsc;
use tokio_rustls::TlsAcceptor;

#[tokio::main]
async fn main() -> Result<()> {
    // # Parse and create defaults for the application configuration
    let config = retrieve_config()?;

    // # Setup logging, state, channels and listeners, etc

    // In my mind, this is the best way to handling an interface that could be sending message to a
    // tui running on a different OS thread or just printing them
    let (logging_tx, logging_rx) = mpsc::channel::<LogRequest>(MAX_LOG_LINES);

    let state = Arc::new(Mutex::new(State::new(logging_tx.clone())));

    let kick_tx = setup_kick_system(&state);

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

        start_tui_thread(&state, kick_tx, logger.clone());

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
        lock!(state).new_permanent_room(room_id);
    }

    // # Setup socket listener

    // `listener` will be used regardless of the security level specified
    let Ok(listener) = TcpListener::bind(format!("0.0.0.0:{}", config.port)).await else {
        bail!("Could not create TCP listener (the port is likely in use)");
    };

    let local_ip = get_local_ip().unwrap_or_else(|| String::from("0.0.0.0"));

    // Run insecure server
    if !config.secure {
        info_global!(
            logging_tx,
            "Server hosted on {local_ip} listening at port {} (insecure)",
            config.port
        );

        while let Ok((stream, addr)) = listener.accept().await {
            tokio::spawn(handle_connection(
                state.clone(),
                stream,
                addr,
                logging_tx.clone(),
            ));
        }

        return Ok(());
    }

    // Run secure server
    let tls_config = build_tls_config(&config.cert_dir)?;
    let acceptor = TlsAcceptor::from(Arc::new(tls_config));

    info_global!(
        logging_tx,
        "Server hosted on {local_ip} listening at port {} (secure)",
        config.port
    );

    while let Ok((stream, addr)) = listener.accept().await {
        let acceptor = acceptor.clone();
        let state = state.clone();

        // TLS handshake happens in task to avoid being held up by a slow client
        let logging_tx = logging_tx.clone();
        tokio::spawn(async move {
            match acceptor.accept(stream).await {
                Ok(tls_stream) => handle_connection(state, tls_stream, addr, logging_tx).await,
                Err(e) => {
                    error_global!(logging_tx, "Secure connection with client failed {}", e);
                }
            }
        });
    }

    Ok(())
}
