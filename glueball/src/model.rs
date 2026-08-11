use crate::state::RoomId;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, ts_rs::TS)]
#[serde(tag = "type", rename_all = "lowercase")]
#[ts(export)]
pub enum ClientToServerMessage {
    RequestRooms,
    InitializeConnection {
        room_id: Option<RoomId>,
        name: String,
    },
    // Since this is sent while the simulation is running, the sent buffer must be prefixed with
    // `MessagePrefix::Server`
    Ping {
        timestamp: u64,
    },
}

/// Answer to `GET /cert`, carrying what a client needs to pin this server's
/// certificate with `serverCertificateHashes` when connecting.
///
/// A browser will not offer to trust a self-signed certificate for
/// `WebTransport` the way it does for HTTPS, so pinning the digest is the only
/// way a self-signed server is reachable at all.
#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[ts(export)]
pub struct CertificateHashes {
    pub hashes: Vec<CertificateHash>,
}

/// One digest, shaped like the `WebTransportHash` dictionary a browser expects.
#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[ts(export)]
pub struct CertificateHash {
    pub algorithm: String,
    /// The raw digest bytes, ready to be handed to `new Uint8Array(value)`
    pub value: Vec<u8>,
}

impl CertificateHash {
    pub fn sha256(digest: &[u8; 32]) -> Self {
        Self {
            algorithm: "sha-256".to_string(),
            value: digest.to_vec(),
        }
    }
}

#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[ts(export)]
pub struct RoomInfo {
    pub id: RoomId,
    pub locked: bool,
    pub host: Option<String>,
}

#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[serde(tag = "type", rename_all = "lowercase")]
#[ts(export)]
pub enum ServerToClientMessage {
    Kick { client_id: String },
    RoomList { rooms: Vec<RoomInfo> },
    SendInfo { room_id: RoomId, client_id: String },
    Pong { client_send_ts: u64, server_ts: u64 },
}

/// The second least significant bit deserves love too
#[repr(u8)]
pub enum MessagePrefix {
    // Indicates client-client communication
    #[allow(dead_code)]
    Client = 0b0000_0001,
    // Indicates client-server communication
    Server = 0b0000_0011,
}
