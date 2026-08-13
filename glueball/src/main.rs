mod cert;
mod cleanup;
mod config;
mod connection;
#[macro_use]
mod logging;
mod action;
mod messaging;
mod model;
mod prefixed;
mod room;
mod state;
#[cfg(test)]
mod tests;
mod tui;
#[macro_use]
mod util;

use crate::action::setup_user_action_system;
use crate::cert::build_tls_config;
use crate::cleanup::Cleanup;
use crate::config::{AppConfig, retrieve_config};
use crate::connection::handle_connection;
use crate::logging::{
    EventType, LogDestination, LogRequest, Logger, MAX_LOG_LINES, create_logging_channel,
    print_global, print_room, spawn_log_receiver,
};
use crate::state::State;
use crate::tui::start_tui_thread;
use crate::util::get_local_ip;
use anyhow::{Result, bail};
use std::sync::{Arc, Mutex, OnceLock};
use tokio::net::TcpListener;
use tokio::sync::mpsc::{self, Sender};
use tokio_rustls::TlsAcceptor;

static LOG_TX: OnceLock<Sender<LogRequest>> = OnceLock::new();

#[tokio::main]
async fn main() -> Result<()> {
    let _cleanup_trigger = Cleanup;

    let config = retrieve_config()?;

    let logging_rx = create_logging_channel();

    let state = Arc::new(State::new());

    if config.headless {
        setup_cli_logging(logging_rx);
    } else {
        setup_tui_with_logger(&state, logging_rx);
    }

    if let Some(ref room_id) = config.permanent_room {
        state.new_permanent_room(&room_id);
    }

    // `listener` will be used regardless of the security level specified
    let Ok(listener) = TcpListener::bind(format!("0.0.0.0:{}", config.port)).await else {
        bail!("Could not create TCP listener (the port is likely in use)");
    };

    let local_ip = get_local_ip().unwrap_or_else(|| String::from("0.0.0.0"));

    if config.secure {
        run_secure_server(state, listener, config, local_ip).await
    } else {
        run_insecure_server(state, listener, config, local_ip).await
    }
}

async fn run_insecure_server(
    state: Arc<State>,
    listener: TcpListener,
    config: AppConfig,
    local_ip: String,
) -> Result<()> {
    info_global!(
        "Server hosted on {local_ip} listening at port {} (insecure)",
        config.port
    );

    while let Ok((stream, addr)) = listener.accept().await {
        tokio::spawn(handle_connection(state.clone(), stream, addr));
    }

    Ok(())
}

async fn run_secure_server(
    state: Arc<State>,
    listener: TcpListener,
    config: AppConfig,
    local_ip: String,
) -> Result<()> {
    let tls_config = build_tls_config(&config.cert_dir)?;
    let acceptor = TlsAcceptor::from(Arc::new(tls_config));

    info_global!(
        "Server hosted on {local_ip} listening at port {} (secure)",
        config.port
    );

    while let Ok((stream, addr)) = listener.accept().await {
        let acceptor = acceptor.clone();
        let state = state.clone();

        // TLS handshake happens in task to avoid being held up by a slow client
        tokio::spawn(async move {
            match acceptor.accept(stream).await {
                Ok(tls_stream) => handle_connection(state, tls_stream, addr).await,
                Err(e) => {
                    error_global!("Secure connection with client failed {}", e);
                }
            }
        });
    }

    Ok(())
}

fn setup_cli_logging(logging_rx: mpsc::Receiver<LogRequest>) {
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
}

fn setup_tui_with_logger(state: &Arc<State>, logging_rx: mpsc::Receiver<LogRequest>) {
    // Only use the `logger` in tui mode
    // `logger` will be jointly owned by the main thread and the tui thread
    // Tokio tasks will pass their log messages down the logging channel
    // instead of having to take a lock to log
    let logger = Arc::new(Mutex::new(Logger::new(MAX_LOG_LINES)));

    let user_action_tx = setup_user_action_system(state);

    start_tui_thread(state, user_action_tx, logger.clone());

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
