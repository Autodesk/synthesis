use log::warn;
use std::collections::VecDeque;
use std::{collections::HashMap, net::SocketAddr};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;

/// Maximum number of log lines retained in each log (both per-room and system logs)
/// Oldest lines are dropped once the buffer is full.
const MAX_LOG_LINES: usize = 500;

pub struct State {
    users: UserMap,
    rooms: RoomMap,
    /// Server-wide events not tied to a specific room
    /// (e.g. connections, handshakes, failed joins)
    system_log: VecDeque<String>,
}

impl State {
    pub fn new() -> Self {
        State {
            users: HashMap::new(),
            rooms: RoomMap::new(),
            system_log: VecDeque::new(),
        }
    }

    pub fn add_room_and_host(&mut self, host_id: UserId, host_tx: ClientSender) {
        let mut room = Room {
            members: vec![(host_id, host_tx)],
            authority: host_id,
            host: host_id,
            logs: VecDeque::new(),
        };

        let room_id = self.rooms.idx;
        room.log(format!("Host {host_id} created room {room_id}"));

        self.users.insert(host_id, room_id);
        self.rooms.map.insert(room_id, room);
        self.rooms.idx += 1;
    }

    pub fn add_client_to_room(
        &mut self,
        client_id: UserId,
        client_tx: ClientSender,
        room_id: RoomId,
    ) {
        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            self.system_log(format!(
                "Attempted to insert {client_id} into non-existant room {room_id}"
            ));
            return;
        };

        room.members.push((client_id, client_tx));
        self.users.insert(client_id, room_id);

        room.log(format!("Client {client_id} joined room {room_id}"));
    }

    pub fn remove_client(&mut self, client_id: UserId) {
        let Some(room_id) = self.users.get(&client_id).copied() else {
            warn!("Attempted to remove client that does not exist");
            return;
        };

        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            warn!("Attempted to remove client from room that does not exist");
            return;
        };

        room.log(format!("Client {client_id} left room {room_id}"));
        if room.remove_client(&client_id) == RoomStatus::Closed {
            self.rooms
                .map
                .remove(&room_id)
                .expect("Failed to remove room");
        }

        self.users.remove(&client_id);
    }

    pub fn get_senders_from_user_room(&self, client_id: UserId) -> Vec<ClientSender> {
        let Some(room_id) = self.users.get(&client_id) else {
            warn!("Attempted to broadcast as user that does not exist");
            return Vec::new();
        };

        let Some(room) = self.rooms.map.get(room_id) else {
            warn!("Attempted to broadcast to room that does not exist {room_id}");
            return Vec::new();
        };

        room.get_senders(Some(client_id))
    }

    pub fn kick(&mut self, client_id: UserId) {
        let Some(room_id) = self.users.get(&client_id) else {
            return;
        };

        let Some(room) = self.rooms.map.get_mut(room_id) else {
            return;
        };

        let Some(tx) = room.get_sender(&client_id) else {
            return;
        };

        // The close message gets forwarded to the client
        let _ = tx.try_send(Message::Close(None));

        room.log(format!("Kicked {client_id} from room {room_id}"));
        self.remove_client(client_id);
    }

    /// Record a server-wide event
    pub fn system_log(&mut self, msg: String) {
        push_capped(&mut self.system_log, format!("{}  {msg}", timestamp()));
    }

    /// Record an event in a user's room
    pub fn log_user_room(&mut self, client_id: UserId, msg: String) {
        let Some(room_id) = self.users.get(&client_id) else {
            return;
        };

        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            return;
        };

        room.log(msg);
    }

    /// Takes a snapshot of the application state so the TUI
    /// doesn't hold a lock on it
    pub fn snapshot(&self) -> Snapshot {
        let mut rooms: Vec<RoomSnapshot> = self
            .rooms
            .map
            .iter()
            .map(|(id, room)| RoomSnapshot {
                id: *id,
                host: room.host,
                authority: room.authority,
                members: room.members.iter().map(|(uid, _)| *uid).collect(),
                logs: room.logs.iter().cloned().collect(),
            })
            .collect();

        rooms.sort_by_key(|r| r.id);

        Snapshot {
            rooms,
            system_log: self.system_log.iter().cloned().collect(),
        }
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
    /// Recent activity for this room, newest last. Capped at [`MAX_LOG_LINES`].
    logs: VecDeque<String>,
}

impl Room {
    pub fn get_senders(&self, exclude: Option<UserId>) -> Vec<ClientSender> {
        self.members
            .iter()
            .filter(|(id, _)| exclude != Some(*id))
            .map(|(_, tx)| tx.clone())
            .collect()
    }

    fn get_sender(&self, id: &UserId) -> Option<ClientSender> {
        self.members
            .iter()
            .find(|(uid, _)| uid == id)
            .map(|(_, tx)| tx.clone())
    }

    fn log(&mut self, msg: String) {
        push_capped(&mut self.logs, format!("{}  {msg}", timestamp()));
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

/// An immutable, cloned view of server state for rendering.
pub struct Snapshot {
    pub rooms: Vec<RoomSnapshot>,
    pub system_log: Vec<String>,
}

pub struct RoomSnapshot {
    pub id: RoomId,
    pub host: UserId,
    pub authority: UserId,
    pub members: Vec<UserId>,
    pub logs: Vec<String>,
}

fn push_capped(buf: &mut VecDeque<String>, line: String) {
    buf.push_back(line);
    while buf.len() > MAX_LOG_LINES {
        buf.pop_front();
    }
}

fn timestamp() -> String {
    chrono::Local::now().format("%H:%M:%S").to_string()
}
