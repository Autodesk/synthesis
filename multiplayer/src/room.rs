use log::warn;
use std::{collections::HashMap, net::SocketAddr};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;

pub struct State {
    users: UserMap,
    rooms: RoomMap,
}

impl State {
    pub fn new() -> Self {
        State {
            users: HashMap::new(),
            rooms: RoomMap::new(),
        }
    }

    pub fn add_room_and_host(&mut self, host_id: UserId, host_tx: ClientSender) {
        let room = Room {
            members: vec![(host_id, host_tx)],
            authority: host_id,
            host: host_id,
        };

        self.users.insert(host_id, self.rooms.idx);
        self.rooms.map.insert(self.rooms.idx, room);
    }

    pub fn add_client_to_room(
        &mut self,
        client_id: UserId,
        client_tx: ClientSender,
        room_id: RoomId,
    ) {
        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            warn!("Attempted to insert user into room {room_id} which does not exist");
            return;
        };

        room.members.push((client_id, client_tx));
        self.users.insert(client_id, room_id);
    }

    pub fn remove_client(&mut self, client_id: UserId) {
        let Some(room_id) = self.users.get(&client_id) else {
            warn!("Attempted to remove client that does not exist");
            return;
        };

        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            warn!("Attempted to remove client from room that does not exist");
            return;
        };

        if room.remove_client(&client_id) == RoomStatus::Closed {
            self.rooms
                .map
                .remove(&room_id)
                .expect("Failed to remove room");
        }
        self.users.remove(&client_id);
    }

    pub fn get_senders_from_user_room(&mut self, client_id: UserId) -> Vec<ClientSender> {
        let Some(room_id) = self.users.get(&client_id) else {
            warn!("Attempted to broadcast as user that does not exist");
            return Vec::new();
        };

        let Some(room) = self.rooms.map.get(&room_id) else {
            warn!("Attempted to broadcast to room that does not exist {room_id}");
            return Vec::new();
        };

        room.get_senders(Some(client_id))
    }
}

pub type UserId = SocketAddr;
pub type UserMap = HashMap<UserId, RoomId>;

pub type ClientSender = mpsc::Sender<Message>;

pub type RoomId = u32;
pub struct RoomMap {
    map: HashMap<RoomId, Room>,
    // Index of next room created
    // Increment when creating new rooms
    idx: RoomId,
}

impl RoomMap {
    pub fn new() -> Self {
        Self {
            map: HashMap::new(),
            idx: 0,
        }
    }
}

#[derive(PartialEq)]
pub enum RoomStatus {
    Closed,
    Open,
}

pub struct Room {
    /// A list of each connected client and their write channel
    members: Vec<(UserId, ClientSender)>,
    /// The physics system authority of the room
    authority: UserId,
    /// The admin of the room (capable of kicking members and ending the room)
    host: UserId,
}

impl Room {
    pub fn get_senders(&self, exclude: Option<UserId>) -> Vec<ClientSender> {
        self.members
            .iter()
            .filter_map(|(id, tx)| {
                if exclude == Some(*id) {
                    Some(tx.clone())
                } else {
                    None
                }
            })
            .collect()
    }

    pub fn remove_client(&mut self, client_id: &UserId) -> RoomStatus {
        let Some(idx) = self
            .members
            .iter()
            .map(|client| client.0)
            .position(|id| id == *client_id)
        else {
            warn!("Attempted to remove client from room they are not in");
            return RoomStatus::Open;
        };

        self.members.remove(idx);

        if *client_id == self.authority {
            match self.members.first() {
                Some(next) => self.authority = next.0,
                None => return RoomStatus::Closed,
            }
        }

        RoomStatus::Open
    }
}
