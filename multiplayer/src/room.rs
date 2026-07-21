use crate::logging::{Event, EventType};
use crate::{info, warn};

use std::collections::{HashMap, VecDeque};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;
use uuid::Uuid;

/// Maximum number of log lines retained in each log (both per-room and system logs)
/// Oldest lines are dropped once the buffer is full.
const MAX_LOG_LINES: usize = 500;

pub struct State {
    users: ClientMap,
    rooms: RoomMap,
    /// Server-wide events not tied to a specific room
    /// (e.g. connections, handshakes, failed joins)
    system_log: VecDeque<Event>,
}

impl State {
    pub fn new() -> Self {
        State {
            users: HashMap::new(),
            rooms: RoomMap::new(),
            system_log: VecDeque::new(),
        }
    }

    pub fn add_room_and_host(&mut self, host_tx: ClientSender) -> (ClientId, RoomId) {
        let host_id = Uuid::new_v4();
        let mut room = Room {
            members: vec![(host_id, host_tx)],
            authority: host_id,
            host: host_id,
            logs: VecDeque::new(),
        };

        let room_id = self.rooms.idx;
        info!(room, "Host {host_id} created room {room_id}");

        self.users.insert(host_id, room_id);
        self.rooms.map.insert(room_id, room);
        self.rooms.idx += 1;

        (host_id, room_id)
    }

    pub fn add_client_to_room(
        &mut self,
        client_tx: ClientSender,
        room_id: RoomId,
    ) -> Option<ClientId> {
        let client_id = Uuid::new_v4();
        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            warn!(
                self,
                "Attempted to add {client_id} into non-existant room {room_id}"
            );
            return None;
        };

        room.members.push((client_id, client_tx));
        self.users.insert(client_id, room_id);

        info!(room, "{client_id} joined room {room_id}");

        Some(client_id)
    }

    pub fn remove_client(&mut self, client_id: ClientId) {
        let Some((room_id, room)) = self.get_room_of_client(&client_id) else {
            warn!(
                self,
                "Attempted to remove {client_id} from room that does not exist"
            );
            return;
        };

        info!(room, "{client_id} left");
        if room.remove_client(&client_id) == RoomStatus::Closed {
            self.rooms
                .map
                .remove(&room_id)
                .expect("Failed to remove room");
        }

        self.users.remove(&client_id);
    }

    fn get_room_of_client(&mut self, client_id: &ClientId) -> Option<(RoomId, &mut Room)> {
        let Some(room_id) = self.users.get(&client_id) else {
            warn!(self, "Attempted to get client that does not exist");
            return None;
        };

        let Some(room) = self.rooms.map.get_mut(&room_id) else {
            return None;
        };

        Some((*room_id, room))
    }

    pub fn get_senders_from_user_room(&mut self, client_id: ClientId) -> Vec<ClientSender> {
        match self.get_room_of_client(&client_id).map(|a| a.1) {
            Some(room) => room.get_senders(Some(client_id)),
            None => Vec::new(),
        }
    }

    pub fn kick(&mut self, client_id: ClientId) {
        let Some(room) = self.get_room_of_client(&client_id).map(|a| a.1) else {
            return;
        };

        let Some(tx) = room.get_sender(&client_id) else {
            return;
        };

        // The close message gets forwarded to the client
        let _ = tx.try_send(Message::Close(None));

        info!(room, "Kicked {client_id}");
        self.remove_client(client_id);
    }

    /// Record a server-wide event
    pub fn log_generic(&mut self, msg: String, kind: EventType) {
        let event = Event {
            kind,
            message: format!("{}  {msg}", timestamp()),
        };

        push_capped(&mut self.system_log, event);
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

pub type ClientId = Uuid;
pub type ClientMap = HashMap<ClientId, RoomId>;

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
    members: Vec<(ClientId, ClientSender)>,
    /// The physics system authority of the room
    authority: ClientId,
    /// The admin of the room (capable of kicking members and ending the room)
    host: ClientId,
    /// Recent activity for this room, newest last. Capped at [`MAX_LOG_LINES`].
    logs: VecDeque<Event>,
}

impl Room {
    pub fn get_senders(&self, exclude: Option<ClientId>) -> Vec<ClientSender> {
        self.members
            .iter()
            .filter(|(id, _)| exclude != Some(*id))
            .map(|(_, tx)| tx.clone())
            .collect()
    }

    fn get_sender(&self, id: &ClientId) -> Option<ClientSender> {
        self.members
            .iter()
            .find(|(uid, _)| uid == id)
            .map(|(_, tx)| tx.clone())
    }

    fn log_generic(&mut self, msg: String, kind: EventType) {
        let event = Event {
            kind,
            message: format!("{}  {msg}", timestamp()),
        };
        push_capped(&mut self.logs, event);
    }

    pub fn remove_client(&mut self, client_id: &ClientId) -> RoomStatus {
        let Some(idx) = self
            .members
            .iter()
            .map(|client| client.0)
            .position(|id| id == *client_id)
        else {
            warn!(self, "Attempted to remove client from room they are not in");
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
    pub system_log: Box<[Event]>,
}

pub struct RoomSnapshot {
    pub id: RoomId,
    pub host: ClientId,
    pub authority: ClientId,
    pub members: Vec<ClientId>,
    pub logs: Vec<Event>,
}

fn push_capped<T>(buf: &mut VecDeque<T>, line: T) {
    buf.push_back(line);
    while buf.len() > MAX_LOG_LINES {
        buf.pop_front();
    }
}

fn timestamp() -> String {
    chrono::Local::now().format("%H:%M:%S").to_string()
}
