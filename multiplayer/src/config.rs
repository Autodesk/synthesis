use crate::{
    DEFAULT_PORT, EventType, error, error_lock,
    room::{State, is_valid_room_id},
    tui,
};
use std::{
    env, process,
    sync::{Arc, Mutex},
    thread,
};

pub struct Config {
    pub port: u32,
}

pub fn parse_arguments(state: &Arc<Mutex<State>>) -> Config {
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
            let parsed = value.and_then(|n| n.parse().ok());
            if let Some(p) = parsed {
                port = p;
            }
        }
    });

    // Queries and sets the port given by the cli, or the [`DEFAULT_PORT`] if one was not passed
    env::args().enumerate().for_each(|(i, arg)| {
        if arg == "--permanentRoom" {
            let Some(room_id) = env::args().nth(i + 1) else {
                return;
            };
            if !is_valid_room_id(&room_id) {
                error_lock!(
                    state,
                    "Invalid permanent room id: {room_id}, must be 6 characters and each character must match `[0-9A-Z]`"
                );
                return;
            }
            let mut guard = state.lock().unwrap();
            guard.new_permanent_room(room_id);
        }
    });

    Config { port }
}
