use serde::{Deserialize, Serialize};

use crate::room::RoomId;
#[derive(Serialize, Deserialize)]
pub enum InitialMessage {
    Create,
    Join(RoomId),
}
