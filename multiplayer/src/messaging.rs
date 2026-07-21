use crate::room::RoomId;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, ts_rs::TS)]
#[ts(export)]
pub struct InitialMessage {
    pub room_id: Option<RoomId>,
    pub name: String
}

#[derive(Serialize, Deserialize, ts_rs::TS)]
#[ts(export)]
pub struct InitialResponse {
    pub room_id: RoomId,
    pub client_id: String,
}
