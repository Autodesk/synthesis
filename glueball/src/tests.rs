/// The purpose of these tests is to test the `handle_connection` function
/// In the future, more shared functionality (such as spawning an insecure server) extracted
use std::sync::Arc;

use futures_util::{SinkExt, StreamExt};
use tokio::net::TcpListener;
use tokio::time::{Duration, timeout};
use tokio_tungstenite::{connect_async, tungstenite::Message};

use crate::logging::create_logging_channel;
use crate::messaging::handle_connection;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::state::State;
use crate::util::{deserialize_messagepack, serialize_and_prefix};

const RECV_TIMEOUT: Duration = Duration::from_secs(3);

/// Binds a server on a random port and returns the `ws://` URL.
async fn spawn_server_insecure() -> String {
    let _log_rx = create_logging_channel();

    let state = Arc::new(State::new());
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let port = listener.local_addr().unwrap().port();

    tokio::spawn(async move {
        while let Ok((stream, addr)) = listener.accept().await {
            tokio::spawn(handle_connection(state.clone(), stream, addr));
        }
    });

    format!("ws://127.0.0.1:{port}")
}

fn create_msg_to_server(msg: ClientToServerMessage) -> Message {
    serialize_and_prefix(msg, MessagePrefix::Server)
}

fn parse_msg_to_server(msg: Message) -> ServerToClientMessage {
    let Message::Binary(bytes) = msg else {
        panic!("Expected binary message from server");
    };
    deserialize_messagepack::<ServerToClientMessage>(&bytes[1..]).unwrap()
}

/// Receive and parse a message from the server over a websocket connections
async fn recv(
    ws: &mut tokio_tungstenite::WebSocketStream<
        tokio_tungstenite::MaybeTlsStream<tokio::net::TcpStream>,
    >,
) -> ServerToClientMessage {
    let msg = timeout(RECV_TIMEOUT, ws.next())
        .await
        .expect("Timed out")
        .expect("Stream ended")
        .expect("WS error");
    parse_msg_to_server(msg)
}

/// Connect and initialize, returning the WebSocket and the assigned room/client IDs.
async fn connect_and_init(
    url: &str,
    name: &str,
    room_id: Option<String>,
) -> (
    tokio_tungstenite::WebSocketStream<tokio_tungstenite::MaybeTlsStream<tokio::net::TcpStream>>,
    String,
    String,
) {
    let (mut ws, _) = connect_async(url).await.unwrap();
    ws.send(create_msg_to_server(
        ClientToServerMessage::InitializeConnection {
            room_id,
            name: name.to_string(),
        },
    ))
    .await
    .unwrap();

    let ServerToClientMessage::SendInfo { room_id, client_id } = recv(&mut ws).await else {
        panic!("Expected SendInfo");
    };

    (ws, room_id, client_id)
}

#[tokio::test]
async fn request_rooms_returns_empty_list() {
    let url = spawn_server_insecure().await;
    let (mut ws, _) = connect_async(&url).await.unwrap();

    ws.send(create_msg_to_server(ClientToServerMessage::RequestRooms))
        .await
        .unwrap();

    let ServerToClientMessage::RoomList { rooms } = recv(&mut ws).await else {
        panic!("Expected RoomList");
    };
    assert!(rooms.is_empty());
}

#[tokio::test]
async fn initialize_creates_room_and_returns_send_info() {
    let url = spawn_server_insecure().await;
    let (mut ws, _) = connect_async(&url).await.unwrap();

    ws.send(create_msg_to_server(
        ClientToServerMessage::InitializeConnection {
            room_id: None,
            name: "Alice".to_string(),
        },
    ))
    .await
    .unwrap();

    assert!(matches!(
        recv(&mut ws).await,
        ServerToClientMessage::SendInfo { .. }
    ));
}

#[tokio::test]
async fn second_client_joins_existing_room() {
    let url = spawn_server_insecure().await;
    let (_ws_a, alices_room, _) = connect_and_init(&url, "Alice", None).await;
    let (_, bobs_room, _) = connect_and_init(&url, "Bob", Some(alices_room.clone())).await;
    assert_eq!(bobs_room, alices_room);
}

#[tokio::test]
async fn messages_forwarded_to_peers_in_room() {
    let url = spawn_server_insecure().await;

    let (mut ws_a, alices_room, _) = connect_and_init(&url, "Alice", None).await;
    let (mut ws_b, _, _) = connect_and_init(&url, "Bob", Some(alices_room)).await;

    // Alice sends a peer-to-peer message (CLIENT_PREFIX byte + arbitrary payload)
    let payload: Vec<u8> = vec![MessagePrefix::Client as u8, 0xDA, 0x15, 0x7];
    ws_a.send(Message::Binary(payload.clone().into()))
        .await
        .unwrap();

    // Bob receives it unchanged
    let received = timeout(RECV_TIMEOUT, ws_b.next())
        .await
        .unwrap()
        .unwrap()
        .unwrap();
    assert_eq!(received, Message::Binary(payload.into()));
}

#[tokio::test]
async fn ping_returns_pong_with_matching_timestamp() {
    let url = spawn_server_insecure().await;
    let (mut ws, _, _) = connect_and_init(&url, "Alice", None).await;

    let ts = 987_654_321_u64;
    ws.send(create_msg_to_server(ClientToServerMessage::Ping {
        timestamp: ts,
    }))
    .await
    .unwrap();

    let ServerToClientMessage::Pong { client_send_ts, .. } = recv(&mut ws).await else {
        panic!("Expected Pong");
    };
    assert_eq!(client_send_ts, ts);
}

#[tokio::test]
async fn client_disconnect_notifies_peers() {
    let url = spawn_server_insecure().await;

    let (mut ws_a, alices_room, alice_id) = connect_and_init(&url, "Alice", None).await;
    let (mut ws_b, _, _) = connect_and_init(&url, "Bob", Some(alices_room)).await;

    ws_a.close(None).await.unwrap();

    let ServerToClientMessage::Kick { client_id } = recv(&mut ws_b).await else {
        panic!("Expected Kick");
    };
    assert_eq!(client_id, alice_id);
}
