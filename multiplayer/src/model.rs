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
}

#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[ts(export)]
pub struct RoomInfo {
    pub id: RoomId,
    pub locked: bool,
    pub authority: Option<String>,
}

#[derive(Serialize, Deserialize, Debug, PartialEq, Eq, ts_rs::TS)]
#[serde(tag = "type", rename_all = "lowercase")]
#[ts(export)]
pub enum ServerMessage {
    Kick { client_id: String },
    RoomList { rooms: Vec<RoomInfo> },
    SendInfo { room_id: RoomId, client_id: String },
}

/// The second least significant bit deserves love too
#[repr(u8)]
pub enum MessagePrefix {
    Client = 0b0000_0001,
    Server = 0b0000_0011,
}
