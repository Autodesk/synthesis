use anyhow::{Result, bail};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;
use uuid::Uuid;

pub type ClientId = Uuid;
pub type ClientSender = mpsc::Sender<Message>;
pub type RoomId = String;

#[derive(PartialEq, Eq)]
pub enum RoomStatus {
    Closed,
    Open,
}

pub struct Room {
    /// A list of each connected client and their write channel
    pub members: Vec<Client>,
    /// Host (initially the creator) of the room
    pub host: Option<ClientId>,
    /// Whether new players can enter a room
    pub locked: bool,
    /// Whether the room closes when it has no players
    pub permanent: bool,
}

impl Room {
    pub fn new_with_host(host_id: &ClientId, host_name: &str, host_tx: ClientSender) -> Self {
        Self {
            members: vec![Client::new(*host_id, host_name.to_string(), host_tx)],
            host: Some(*host_id),
            locked: false,
            permanent: false,
        }
    }

    pub fn get_client_name(&self, client_id: &ClientId) -> Result<String> {
        let Some(client) = self.members.iter().find(|user| user.id == *client_id) else {
            bail!("Client not in room");
        };

        Ok(client.name.clone())
    }

    pub fn get_peer_senders(&self, exclude: &ClientId) -> Vec<ClientSender> {
        self.members
            .iter()
            .filter(|client| *exclude != client.id)
            .map(|client| client.tx.clone())
            .collect()
    }

    pub fn get_sender(&self, id: &ClientId) -> Option<ClientSender> {
        self.members
            .iter()
            .find(|client| client.id == *id)
            .map(|client| client.tx.clone())
    }

    pub fn remove_client(&mut self, client_id: &ClientId) -> RoomStatus {
        let Some(idx) = self
            .members
            .iter()
            .map(|client| client.id)
            .position(|id| id == *client_id)
        else {
            warn_global!("Attempted to remove client from room they are not in");
            return RoomStatus::Open;
        };

        self.members.remove(idx);

        if Some(*client_id) == self.host {
            match self.members.first() {
                Some(next) => self.host = Some(next.id),
                None if self.permanent => self.host = None,
                None => return RoomStatus::Closed,
            }
        }

        RoomStatus::Open
    }
}

pub struct Client {
    pub id: ClientId,
    pub name: String,
    pub tx: ClientSender,
}

impl Client {
    pub const fn new(id: ClientId, name: String, tx: ClientSender) -> Self {
        Self { id, name, tx }
    }
}
