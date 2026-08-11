//! Message delivery for the `WebTransport` wire protocol.
//!
//! A message on the wire is unchanged from the `WebSocket` protocol: a single
//! [`MessagePrefix`](crate::model::MessagePrefix) byte followed by a
//! `MessagePack` body. What changes is how one message is delimited from the
//! next.
//!
//! * Every message travelling over a stream gets its own unidirectional stream.
//!   The sender writes the payload and finishes the stream; that finish *is* the
//!   message boundary, so no length prefix is needed. Replies go out as their own
//!   stream rather than back down the one that carried the request.
//! * Datagrams already arrive as whole messages, so they are sent as-is.
//!
//! Streams are independent, so ordering is only guaranteed within a message,
//! never between two of them.

use anyhow::{Result, bail};
use bytes::Bytes;
use wtransport::{Connection, RecvStream};

/// Size of the buffer a stream is drained into.
const READ_CHUNK_SIZE: usize = 8 * 1024;

/// Largest message we are willing to read off a stream. A peer that writes more
/// than this without finishing is either broken or hostile, so we give up rather
/// than buffer for it.
const MAX_MESSAGE_SIZE: usize = 4 * 1024 * 1024;

/// How a message travelled, and therefore how anything derived from it should
/// travel back out.
#[derive(Copy, Clone, PartialEq, Eq)]
pub enum Delivery {
    /// On a stream of its own: ordered and guaranteed.
    Stream,
    /// As a datagram: unordered, and dropped rather than retransmitted.
    Datagram,
}

impl Delivery {
    /// Queues `payload` for delivery over this same kind of channel.
    pub const fn queue(self, payload: Bytes) -> Outbound {
        match self {
            Self::Stream => Outbound::Stream(payload),
            Self::Datagram => Outbound::Datagram(payload),
        }
    }
}

/// A payload queued for delivery to a single client.
#[derive(Clone)]
pub enum Outbound {
    /// Send to the client on a stream of its own.
    Stream(Bytes),
    /// Send to the client as a datagram.
    Datagram(Bytes),
    /// Terminate the client's session.
    Close,
}

/// Reads a whole message: everything the peer wrote to `read` before finishing
/// the stream.
pub async fn read_message(mut read: RecvStream) -> Result<Bytes> {
    let mut payload = Vec::new();
    let mut chunk = [0u8; READ_CHUNK_SIZE];

    // A `None` read is the peer finishing the stream, which ends the message
    while let Some(count) = read.read(&mut chunk).await? {
        if payload.len() + count > MAX_MESSAGE_SIZE {
            bail!("message exceeds the {MAX_MESSAGE_SIZE} byte limit");
        }

        payload.extend_from_slice(&chunk[..count]);
    }

    Ok(payload.into())
}

/// Sends `payload` to the client as a message of its own.
///
/// The stream is finished before returning, because an unfinished stream leaves
/// the client waiting for a boundary that never arrives.
pub async fn write_message(connection: &Connection, payload: &[u8]) -> Result<()> {
    let mut write = connection.open_uni().await?.await?;

    write.write_all(payload).await?;
    write.finish().await?;

    Ok(())
}
