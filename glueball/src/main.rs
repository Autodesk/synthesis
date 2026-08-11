mod cert;
mod cleanup;
mod config;
mod connection;
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

use crate::cert::{build_hashes, build_identity};
use crate::cleanup::Cleanup;
use crate::config::retrieve_config;
use crate::connection::spawn_webtransport_responder;
use crate::http::spawn_http_responder;
use crate::kick::setup_user_action_system;
use crate::logging::{
    EventType, LogDestination, LogRequest, Logger, MAX_LOG_LINES, create_logging_channel,
    print_global, print_room, spawn_log_receiver,
};
use crate::state::State;
use crate::tui::start_tui_thread;
use anyhow::Result;
use std::sync::{Arc, Mutex, OnceLock};
use tokio::sync::mpsc::{Receiver, Sender};

/// Please copy out of this once lock when you want to log
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

    if let Some(room_id) = config.permanent_room {
        state.new_permanent_room(room_id);
    }

    let identity = build_identity(&config.cert_dir).await?;
    let hashes = build_hashes(&identity);

    spawn_http_responder(config.port, hashes).await?;

    spawn_webtransport_responder(state, config.port, identity).await?;

    Ok(())
}

fn setup_cli_logging(logging_rx: Receiver<LogRequest>) {
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

fn setup_tui_with_logger(state: &Arc<State>, logging_rx: Receiver<LogRequest>) {
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
