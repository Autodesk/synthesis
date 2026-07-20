mod messaging;
mod room;

use std::collections::HashMap;
use std::sync::Arc;
use std::sync::Mutex;

use futures_util::StreamExt;
use futures_util::TryStreamExt;
use futures_util::future;
use log::error;
use log::info;
use tokio::net::{TcpListener, TcpStream};
use tokio_tungstenite::tungstenite::Message;

use crate::room::RoomMap;

const PORT: u32 = 9002;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let rooms: RoomMap = Arc::new(Mutex::new(HashMap::new()));

    let addr = format!("127.0.0.1:{PORT}");
    let listener = TcpListener::bind(&addr).await?;
    println!("Server listening on port {PORT}");

    while let Ok((stream, _)) = listener.accept().await {
        tokio::spawn(handle_connection(rooms.clone(), stream));
    }

    Ok(())
}

async fn handle_connection(rooms: RoomMap, raw_stream: TcpStream) {
    let addr = raw_stream
        .peer_addr()
        .expect("Connected stream missing peer address");
    info!("Peer Address: {addr}");

    let ws_stream = tokio_tungstenite::accept_async(raw_stream)
        .await
        .expect("Error during websocket handshake");

    info!("New WebSoccket connection: {addr}");

    let (write, read) = ws_stream.split();
    while let Some(maybe_message) = read.next().await {
        let Ok(message) = maybe_message else {
            continue;
        };

        match message {
            Message::Text(text) => {
                if text.trim().is_empty() {
                    continue;
                }

                match serde_json::from_str(&text) {
                    Ok(json) => {
                        println!("Parsed: {:?}", json);
                    }
                    Err(e) => {
                        error!("Failed to parse json");
                    }
                }
            }
            Message::Binary(_) => {
                info!("Received Binary, skipping");
            }
            Message::Close(_) => {
                info!("Connection with {addr} closed")
            }
            // TODO
            // Handle Ping/Pong
            _ => {}
        }
    }
}
