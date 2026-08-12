/// The purpose of these tests is to test the `handle_session` function
/// In the future, more shared functionality (such as spawning a server) extracted
use std::sync::Arc;

use bytes::Bytes;
use tokio::time::{Duration, timeout};
use wtransport::{ClientConfig, Connection, Endpoint, Identity, ServerConfig, VarInt};

use crate::connection::accept_incoming_session;
use crate::kick::{UserAction, setup_user_action_system};
use crate::logging::create_logging_channel;
use crate::model::{ClientToServerMessage, MessagePrefix, ServerToClientMessage};
use crate::state::State;
use crate::util::{deserialize_messagepack, serialize_and_prefix};
use crate::wire::{read_message, send_message};

const RECV_TIMEOUT: Duration = Duration::from_secs(3);

/// Binds a server on a random port and returns its URL alongside the SHA-256
/// digest of its certificate, which clients pin instead of validating a chain.
async fn spawn_server() -> (String, wtransport::tls::Sha256Digest) {
    let (url, hash, _state) = spawn_server_with_state().await;
    (url, hash)
}

/// As [`spawn_server`], but also hands back the server's state so a test can act
/// on it the way the TUI does.
async fn spawn_server_with_state() -> (String, wtransport::tls::Sha256Digest, Arc<State>) {
    let _log_rx = create_logging_channel();

    let state = Arc::new(State::new());

    let identity = Identity::self_signed(["localhost", "127.0.0.1", "::1"]).unwrap();
    let hash = identity.certificate_chain().as_slice()[0].hash();

    let config = ServerConfig::builder()
        .with_bind_default(0)
        .with_identity(identity)
        .build();

    let endpoint = Endpoint::server(config).unwrap();
    let port = endpoint.local_addr().unwrap().port();

    let accept_state = state.clone();
    tokio::spawn(async move {
        loop {
            let session = endpoint.accept().await;
            tokio::spawn(accept_incoming_session(accept_state.clone(), session));
        }
    });

    (format!("https://127.0.0.1:{port}"), hash, state)
}

/// A `WebTransport` client speaking the same wire protocol as the browser client:
/// one unidirectional stream per message, plus datagrams.
struct TestClient {
    /// Held so the endpoint outlives the connection it created
    _endpoint: Endpoint<wtransport::endpoint::endpoint_side::Client>,
    connection: Connection,
}

impl TestClient {
    async fn connect(url: &str, hash: wtransport::tls::Sha256Digest) -> Self {
        let config = ClientConfig::builder()
            .with_bind_default()
            .with_server_certificate_hashes([hash])
            .build();

        let endpoint = Endpoint::client(config).unwrap();
        let connection = endpoint.connect(url).await.unwrap();

        Self {
            _endpoint: endpoint,
            connection,
        }
    }

    async fn send_server(&self, message: ClientToServerMessage) {
        let payload = serialize_and_prefix(message, MessagePrefix::Server);
        send_message(&self.connection, &payload).await.unwrap();
    }

    async fn send_peer_stream(&self, payload: &[u8]) {
        send_message(&self.connection, payload).await.unwrap();
    }

    fn send_peer_datagram(&self, payload: &[u8]) {
        self.connection.send_datagram(payload).unwrap();
    }

    /// Receives the next message the server sends on a stream
    async fn recv_stream(&self) -> Bytes {
        let read = timeout(RECV_TIMEOUT, self.connection.accept_uni())
            .await
            .expect("Timed out")
            .expect("Session ended");

        read_message(read).await.expect("Stream error")
    }

    async fn recv_server(&self) -> ServerToClientMessage {
        let payload = self.recv_stream().await;
        deserialize_messagepack::<ServerToClientMessage>(&payload[1..]).unwrap()
    }

    async fn recv_datagram(&self) -> Bytes {
        timeout(RECV_TIMEOUT, self.connection.receive_datagram())
            .await
            .expect("Timed out")
            .expect("Session ended")
            .payload()
    }
}

/// Connect and initialize, returning the client and the assigned room/client IDs.
async fn connect_and_init(
    url: &str,
    hash: wtransport::tls::Sha256Digest,
    name: &str,
    room_id: Option<String>,
) -> (TestClient, String, String) {
    let client = TestClient::connect(url, hash).await;

    client
        .send_server(ClientToServerMessage::InitializeConnection {
            room_id,
            name: name.to_string(),
        })
        .await;

    let ServerToClientMessage::SendInfo { room_id, client_id } = client.recv_server().await else {
        panic!("Expected SendInfo");
    };

    (client, room_id, client_id)
}

#[tokio::test]
async fn request_rooms_returns_empty_list() {
    let (url, hash) = spawn_server().await;
    let client = TestClient::connect(&url, hash).await;

    client
        .send_server(ClientToServerMessage::RequestRooms)
        .await;

    let ServerToClientMessage::RoomList { rooms } = client.recv_server().await else {
        panic!("Expected RoomList");
    };
    assert!(rooms.is_empty());
}

#[tokio::test]
async fn initialize_creates_room_and_returns_send_info() {
    let (url, hash) = spawn_server().await;
    let client = TestClient::connect(&url, hash).await;

    client
        .send_server(ClientToServerMessage::InitializeConnection {
            room_id: None,
            name: "Alice".to_string(),
        })
        .await;

    assert!(matches!(
        client.recv_server().await,
        ServerToClientMessage::SendInfo { .. }
    ));
}

#[tokio::test]
async fn second_client_joins_existing_room() {
    let (url, hash) = spawn_server().await;
    let (_alice, alices_room, _) = connect_and_init(&url, hash.clone(), "Alice", None).await;
    let (_, bobs_room, _) = connect_and_init(&url, hash, "Bob", Some(alices_room.clone())).await;
    assert_eq!(bobs_room, alices_room);
}

#[tokio::test]
async fn stream_messages_forwarded_to_peers_in_room() {
    let (url, hash) = spawn_server().await;

    let (alice, alices_room, _) = connect_and_init(&url, hash.clone(), "Alice", None).await;
    let (bob, _, _) = connect_and_init(&url, hash, "Bob", Some(alices_room)).await;

    // Alice sends a peer-to-peer message (CLIENT_PREFIX byte + arbitrary payload)
    let payload: Vec<u8> = vec![MessagePrefix::Client as u8, 0xDA, 0x15, 0x7];
    alice.send_peer_stream(&payload).await;

    // Bob receives it unchanged, and on a stream rather than as a datagram
    assert_eq!(bob.recv_stream().await, Bytes::from(payload));
}

#[tokio::test]
async fn datagram_messages_forwarded_to_peers_as_datagrams() {
    let (url, hash) = spawn_server().await;

    let (alice, alices_room, _) = connect_and_init(&url, hash.clone(), "Alice", None).await;
    let (bob, _, _) = connect_and_init(&url, hash, "Bob", Some(alices_room)).await;

    let payload: Vec<u8> = vec![MessagePrefix::Client as u8, 0xDA, 0x15, 0x7];

    // Datagrams are unreliable, so keep resending until one lands
    let received = loop {
        alice.send_peer_datagram(&payload);

        if let Ok(datagram) = timeout(Duration::from_millis(200), bob.recv_datagram()).await {
            break datagram;
        }
    };

    assert_eq!(received, Bytes::from(payload));
}

#[tokio::test]
async fn ping_returns_pong_with_matching_timestamp() {
    let (url, hash) = spawn_server().await;
    let (client, _, _) = connect_and_init(&url, hash, "Alice", None).await;

    let ts = 987_654_321_u64;
    client
        .send_server(ClientToServerMessage::Ping { timestamp: ts })
        .await;

    let ServerToClientMessage::Pong { client_send_ts, .. } = client.recv_server().await else {
        panic!("Expected Pong");
    };
    assert_eq!(client_send_ts, ts);
}

#[tokio::test]
async fn kicked_client_session_is_closed() {
    let (url, hash, state) = spawn_server_with_state().await;
    let user_action_tx = setup_user_action_system(&state);

    let (alice, alices_room, alice_id) = connect_and_init(&url, hash.clone(), "Alice", None).await;
    let (bob, _, _) = connect_and_init(&url, hash, "Bob", Some(alices_room)).await;

    let mut user_action_tx = user_action_tx;
    user_action_tx
        .push(UserAction::Kick(alice_id.parse().unwrap()))
        .unwrap();

    // Bob is told Alice is gone
    let ServerToClientMessage::Kick { client_id } = bob.recv_server().await else {
        panic!("Expected Kick");
    };
    assert_eq!(client_id, alice_id);

    // And Alice's whole session is torn down, not just her streams
    timeout(RECV_TIMEOUT, alice.connection.closed())
        .await
        .expect("Kicked client's session was never closed");
}

#[tokio::test]
async fn client_disconnect_notifies_peers() {
    let (url, hash) = spawn_server().await;

    let (alice, alices_room, alice_id) = connect_and_init(&url, hash.clone(), "Alice", None).await;
    let (bob, _, _) = connect_and_init(&url, hash, "Bob", Some(alices_room)).await;

    alice.connection.close(VarInt::from_u32(0), b"Goodbye");

    let ServerToClientMessage::Kick { client_id } = bob.recv_server().await else {
        panic!("Expected Kick");
    };
    assert_eq!(client_id, alice_id);
}
