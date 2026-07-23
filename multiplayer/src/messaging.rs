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

#[derive(Serialize, Deserialize, Debug, PartialEq, ts_rs::TS)]
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
    Client = 0b00000001,
    Server = 0b00000011,
}

pub fn serialize_messagepack<'a, M>(message: M) -> Vec<u8>
where
    M: Serialize,
{
    let mut message_buffer_no_prefix = Vec::new();
    let mut serializer = rmp_serde::Serializer::new(&mut message_buffer_no_prefix);
    message
        .serialize(&mut serializer)
        .expect("Cound not serialize kick message");

    message_buffer_no_prefix
}

pub fn deserialize_messagepack<'de, B, S>(data: &'de B) -> Option<S>
where
    B: Deref<Target = [u8]> + 'de,
    S: Deserialize<'de>,
{
    rmp_serde::from_slice(&data).ok()
}
