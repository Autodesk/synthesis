mod messaging;
mod room;
mod tui;
#[macro_use]
mod util;

use futures_util::SinkExt;
use futures_util::StreamExt;
use std::env;
use std::process;
use std::sync::Arc;
use std::sync::Mutex;
use std::thread;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;

use crate::messaging::InitialMessage;
use crate::room::ClientSender;
use crate::room::State;

const PORT: u32 = 9002;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let state = Arc::new(Mutex::new(State::new()));

    let addr = format!("127.0.0.1:{PORT}");
    let listener = TcpListener::bind(&addr).await?;
    println!("Server listening on port {PORT}");

    if env::args().any(|a| a == "--tui") {
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

    while let Ok((stream, _)) = listener.accept().await {
        tokio::spawn(handle_connection(state.clone(), stream));
    }

    Ok(())
}

async fn handle_connection(state: Arc<Mutex<State>>, raw_stream: TcpStream) {
    let addr = raw_stream
        .peer_addr()
        .expect("Connected stream missing peer address");

    let ws_stream = tokio_tungstenite::accept_async(raw_stream)
        .await
        .expect("Error during websocket handshake");

    {
        let mut guard = state.lock().unwrap();
        log!(guard, "connection from {addr}");
    }

    // Each client gets an mpsc channel
    // Other client threads on the server can write to it
    // Everything written gets dumped back to its client through the `write` sink
    let (tx, mut rx) = mpsc::channel::<Message>(64);

    // Order of messages sent from a new client to the server:
    // 1. Initialization. An instance of the `InitializationMessage` structure.
    //    Indicating whether the client wishes to create or join a room
    // 2..n. Any number of messages that will be forwarded to every other client in their room
    let (mut write, mut read) = ws_stream.split();
    tokio::spawn(async move {
        while let Some(msg) = rx.recv().await {
            if write.send(msg).await.is_err() {
                break;
            }
        }
    });

    // Parse initial message, then user in correct room
    let Some(Ok(Message::Text(initial_message_string))) = read.next().await else {
        let mut guard = state.lock().unwrap();
        log!(guard, "{addr} disconnected before handshake");

        return;
    };

    let Ok(initial_message) = serde_json::from_str::<InitialMessage>(&initial_message_string)
    else {
        let mut guard = state.lock().unwrap();
        log!(guard, "{addr} sent an invalid initial message");

        return;
    };

    {
        // The lock is relinquished after this match statement
        let mut guard = state.lock().unwrap();
        match initial_message {
            InitialMessage::Create => guard.add_room_and_host(addr, tx),
            InitialMessage::Join(room_id) => guard.add_client_to_room(addr, tx, room_id),
        }
    }

    // Listen for and pass along messages to other client channels in the same room
    while let Some(maybe_message) = read.next().await {
        let Ok(message) = maybe_message else {
            continue;
        };

        match message {
            Message::Text(ref text) => {
                if text.trim().is_empty() {
                    continue;
                }

                let senders: Vec<ClientSender> = {
                    // The lock is relinquished after senders are retreived
                    let mut guard = state.lock().unwrap();
                    guard.get_senders_from_user_room(addr)
                };

                for tx in senders {
                    tx.send(message.clone()).await.ok();
                }
            }

            Message::Binary(_) => {
                let mut guard = state.lock().unwrap();
                log!(guard, "Received Binary, skipping");
            }
            Message::Close(_) => {
                let mut guard = state.lock().unwrap();
                log!(guard, "Connection with {addr} closed");
                guard.remove_client(addr);

                return;
            }
            // TODO
            // Handle Ping/Pong
            _ => {}
        }
    }
}
