use std::{env::home_dir, net::UdpSocket, ops::Deref, path::PathBuf};

use anyhow::{Result, bail};
use bytes::Bytes;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

use crate::model::{MessagePrefix, ServerToClientMessage};

#[macro_export]
macro_rules! lock {
    ($mutex:expr) => {
        $mutex
            .lock()
            .expect("Poisoned Mutex (panic on another thread). Aborting.")
    };
}

pub fn get_local_ip() -> Option<String> {
    let socket = UdpSocket::bind("0.0.0.0:0").ok()?;
    socket.connect("8.8.8.8:80").ok()?;

    let local_addr = socket.local_addr().ok()?;
    Some(local_addr.ip().to_string())
}

pub fn trim_uuid(uuid: &Uuid) -> String {
    let [a, b, c, d, ..] = uuid.as_bytes();
    format!("{a:02x}{b:02x}{c:02x}{d:02x}")
}

pub fn tilde_expansion(path: &mut PathBuf) -> Result<()> {
    let Ok(suffix) = path.strip_prefix("~/") else {
        return Ok(());
    };

    let Some(home_dir) = home_dir() else {
        bail!("Could not find your home dir");
    };

    *path = home_dir.join(suffix);

    Ok(())
}

pub fn server_sent_msg(message: ServerToClientMessage) -> Bytes {
    serialize_and_prefix(message, MessagePrefix::Server)
}

pub fn serialize_and_prefix<M>(message: M, prefix: MessagePrefix) -> Bytes
where
    M: Serialize,
{
    let bytes = serialize_messagepack(message);
    prefix_message(bytes, prefix)
}

/// Creates a new payload containing `bytes`,
/// prefixed with the byte value of `MessagePrefix`
fn prefix_message<M>(bytes: M, prefix: MessagePrefix) -> Bytes
where
    M: Deref<Target = [u8]>,
{
    let mut buf = vec![0u8; bytes.len() + 1];
    buf[1..].copy_from_slice(&bytes);
    buf[0] = prefix as u8;

    buf.into()
}

fn serialize_messagepack<M>(message: M) -> Vec<u8>
where
    M: Serialize,
{
    #[allow(clippy::expect_used)]
    rmp_serde::to_vec_named(&message)
        .expect("Serilization of message failed. This is a bug in Glueball.")
}

pub fn deserialize_messagepack<'de, S>(data: &'de [u8]) -> Result<S, rmp_serde::decode::Error>
where
    S: Deserialize<'de>,
{
    rmp_serde::from_slice(data)
}
