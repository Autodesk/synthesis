mod cert;
mod config;
#[macro_use]
mod logging;
mod messaging;
mod model;
mod panic;
mod prefixed;
mod room;
mod tui;
#[macro_use]
mod util;

use crate::cert::build_tls_config;
use crate::config::{CliConfig, config_or_default, parse_config_file};
use crate::logging::{
    EventType, LogDestination, Logger, MAX_LOG_LINES, print_global, print_room, spawn_log_receiver,
};
use crate::messaging::handle_connection;
use crate::room::State;
use crate::tui::start_tui_thread;

use std::sync::{Arc, Mutex};

use anyhow::{Result, bail};
use tokio::net::TcpListener;
use tokio::sync::mpsc;
use tokio_rustls::TlsAcceptor;

const _: () = assert!(std::mem::size_of::<usize>() >= std::mem::size_of::<u64>());

#[tokio::main]
async fn main() -> Result<()> {
    // Setup logging
    // In my mind, this is the best way to handling an interface that could be sending message to a
    // tui running on a different OS thread or just printing them

    // Always use the logging chanel
    let (logging_tx, logging_rx) =
        mpsc::channel::<(String, EventType, LogDestination)>(MAX_LOG_LINES);

    // Only use the `logger` in tui mode
    // `logger` will be jointly owned by the main thread and the tui thread
    // Tokio tasks will pass their log messages down the logging channel
    // instead of having to take a lock to log
    let logger = Arc::new(Mutex::new(Logger::new(MAX_LOG_LINES)));

    let state = Arc::new(Mutex::new(State::new(logging_tx.clone())));

    // Parse and create defaults for the application configuration
    let mut config: CliConfig = argh::from_env();
    if let Some(config_file) = config.config_file.clone() {
        parse_config_file(config_file, &mut config)?;
    }
    let (cert_dir, port) = config_or_default(&config)?;

    // `listener` will be used regardless of the security level specified
    let Ok(listener) = TcpListener::bind(format!("127.0.0.1:{port}")).await else {
        bail!("Could not create TCP listener (the port is likely in use)");
    };

    if config.headless {
        // Read the logging channel and immediantly print result
        let print_to_terminal =
            |message: String, kind: EventType, log_destination: LogDestination| {
                match log_destination {
                    LogDestination::Global => print_global(&message, &kind),
                    LogDestination::Room(id) => print_room(&message, &kind, &id),
                }
            };

        spawn_log_receiver(logging_rx, print_to_terminal);
    } else {
        start_tui_thread(&state, logger.clone());

        // Read the logging channel and write every message to the `logger`
        let send_to_logger =
            move |message: String, kind: EventType, log_destination: LogDestination| {
                match log_destination {
                    LogDestination::Global => lock!(logger).push_global(message, kind),
                    LogDestination::Room(id) => lock!(logger).push_room(message, kind, id),
                }
            };

        spawn_log_receiver(logging_rx, send_to_logger);
    }

    if let Some(room_id) = config.permanent_room {
        lock!(state).new_permanent_room(room_id);
    }

    if !config.secure {
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
    let tls_config = build_tls_config(&cert_dir)?;
    let acceptor = TlsAcceptor::from(Arc::new(tls_config));

    info_global!(logging_tx, "Server listening on port {} (secure)", port);

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
