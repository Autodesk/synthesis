use crate::room::RoomId;
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
    Kick {
        client_id: String,
    },
    RoomList {
        rooms: Vec<RoomInfo>,
    },
    SendInfo {
        room_id: RoomId,
        client_id: String,
    },
    Pong {
        last_ping_timestamp: u64,
        current_server_timestamp: u64,
    },
}

/// The second least significant bit deserves love too
#[repr(u8)]
pub enum MessagePrefix {
    // Indicates client-client communication
    Client = 0b0000_0001,
    // Indicates client-server communication
    Server = 0b0000_0011,
}
