use std::ops::Deref;

use serde::{Deserialize, Serialize};
use tokio_tungstenite::tungstenite::Message;
use uuid::Uuid;

use crate::model::MessagePrefix;

pub fn trim_uuid(uuid: &Uuid) -> String {
    uuid.to_string()[0..8].to_string()
}

/// Creates a new `Message::Binary` containing `bytes`,
/// prefixed with the byte value of `MessagePrefix`
pub fn prefix_message<M>(bytes: M, prefix: MessagePrefix) -> Message
where
    M: Deref<Target = [u8]>,
{
    let mut buf = vec![0u8; bytes.len() + 1];
    buf[1..].copy_from_slice(&bytes);
    buf[0] = prefix as u8;

    Message::Binary(buf.into())
}

pub fn serialize_messagepack<M>(message: M) -> Vec<u8>
where
    M: Serialize,
{
    rmp_serde::to_vec_named(&message).expect("Could not serialize message")
}

pub fn deserialize_messagepack<'de, S>(data: &'de [u8]) -> Result<S, rmp_serde::decode::Error>
where
    S: Deserialize<'de>,
{
    rmp_serde::from_slice(data)
}
