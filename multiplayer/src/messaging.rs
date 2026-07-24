use std::ops::Deref;

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
#[serde(tag = "type", rename_all = "lowercase")]
#[ts(export)]
pub enum ServerMessage {
    Kick { client_id: String },
    RoomList(Vec<String>),
    SendInfo { room_id: RoomId, client_id: String },
}

/// The second least significant bit deserves love too
#[repr(u8)]
pub enum MessagePrefix {
    Client = 0b0000_0001,
    Server = 0b0000_0011,
}

pub fn serialize_messagepack<M>(message: M) -> Vec<u8>
where
    M: Serialize,
{
    rmp_serde::to_vec_named(&message).expect("Could not serialize message")
}

pub fn deserialize_messagepack<'de, S, B>(data: &'de B) -> Option<S>
where
    B: Deref<Target = [u8]> + 'de,
    S: Deserialize<'de>,
{
    rmp_serde::from_slice(data).ok()
}
