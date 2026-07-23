use crate::room::RoomId;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, ts_rs::TS)]
#[ts(export)]
pub struct InitialMessage {
    pub room_id: Option<RoomId>,
    pub name: String,
}

#[derive(Serialize, Deserialize, ts_rs::TS)]
#[ts(export)]
pub struct InitialResponse {
    pub room_id: RoomId,
    pub client_id: String,
}

#[derive(Serialize, Deserialize, Debug, PartialEq, ts_rs::TS)]
#[serde(tag = "type", rename_all = "lowercase")]
#[ts(export)]
pub enum ServerMessageData {
    Kick { client_id: String },
}

/// The second least significant bit deserves love too
#[repr(u8)]
pub enum MessagePrefix {
    Client = 0b00000001,
    Server = 0b00000011,
}
