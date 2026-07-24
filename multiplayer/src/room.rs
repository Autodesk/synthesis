use crate::logging::{Event, EventType};
use crate::messaging::{MessagePrefix, RoomInfo, ServerMessage, serialize_messagepack};
use crate::{info, prefix_message, warn};

use rand::RngExt;
use std::collections::{HashMap, VecDeque};
use tokio::sync::mpsc;
use tokio_tungstenite::tungstenite::Message;
use uuid::Uuid;

/// Maximum number of log lines retained in each log (both per-room and system logs)
/// Oldest lines are dropped once the buffer is full.
const MAX_LOG_LINES: usize = 500;

const VALID_ROOM_ID_CHARACTERS: [char; 36] = valid_room_id_characters();

pub struct State {
    users: ClientMap,
    rooms: RoomMap,
    /// Server-wide events not tied to a specific room
    /// (e.g. connections, handshakes, failed joins)
    system_log: VecDeque<Event>,
}

impl State {
    pub fn new() -> Self {
        Self {
            users: HashMap::new(),
            rooms: RoomMap::new(),
            system_log: VecDeque::new(),
        }
    }

    pub fn add_room_and_authority(
        &mut self,
        authority_name: String,
        authority_tx: ClientSender,
    ) -> (ClientId, RoomId) {
        let authority_id = Uuid::new_v4();
        let mut room = Room {
            members: vec![Client::new(authority_id, authority_name, authority_tx)],
            authority: Some(authority_id),
            locked: false,
            logs: VecDeque::new(),
        };

        let room_id = generate_6_digit_code();
        info!(room, "Authority {authority_id} created room {room_id}");

        self.users.insert(authority_id, room_id.clone());
        self.rooms.map.insert(room_id.clone(), room);

        (authority_id, room_id)
    }

    pub fn add_client_to_room(
        &mut self,
        client_name: String,
        client_tx: ClientSender,
        room_id: &RoomId,
    ) -> Option<ClientId> {
        let client_id = Uuid::new_v4();
        let Some(room) = self.rooms.map.get_mut(room_id) else {
            warn!(
                self,
                "Attempted to add {client_id} into non-existant room {room_id}"
            );
            return None;
        };

        if room.locked {
            return None;
        }

        let client = Client::new(client_id, client_name, client_tx);
        room.members.push(client);
        self.users.insert(client_id, room_id.clone());

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

    pub fn new_permanent_room(&mut self, room_id: RoomId) {
        let room = Room {
            members: Vec::new(),
            authority: None,
            locked: false,
            logs: VecDeque::new(),
        };
        self.rooms.map.insert(room_id, room);
    }

    fn get_room_of_client(&mut self, client_id: &ClientId) -> Option<(RoomId, &mut Room)> {
        let Some(room_id) = self.users.get(client_id) else {
            warn!(self, "Attempted to get client that does not exist");
            return None;
        };

        let room = self.rooms.map.get_mut(room_id)?;

        Some((room_id.clone(), room))
    }

    pub fn get_senders_from_user_room(&mut self, client_id: ClientId) -> Vec<ClientSender> {
        self.get_room_of_client(&client_id)
            .map(|a| a.1)
            .map_or_else(Vec::new, |room| room.get_senders(Some(&client_id)))
    }

    /// Flips whether new clients can join `room_id`.
    /// Returns the new locked state, or `None` if the room does not exist.
    pub fn toggle_room_lock(&mut self, room_id: &RoomId) -> Option<bool> {
        let room = self.rooms.map.get_mut(room_id)?;
        room.locked = !room.locked;
        let locked = room.locked;

        if locked {
            info!(room, "Room {room_id} locked");
        } else {
            info!(room, "Room {room_id} unlocked");
        }

        Some(locked)
    }

    pub fn kick(&mut self, client_id: ClientId) {
        let Some(room) = self.get_room_of_client(&client_id).map(|a| a.1) else {
            return;
        };

        let Some(tx) = room.get_sender(&client_id) else {
            return;
        };

        // The close message gets forwarded to the client getting kicked
        let _ = tx.blocking_send(Message::Close(None));

        // Send message toa ll other clients telling them `client_id` has been kicked
        let message = ServerMessage::Kick {
            client_id: client_id.to_string(),
        };

        let message_buffer_no_prefix = serialize_messagepack(message);
        let message = prefix_message(message_buffer_no_prefix, MessagePrefix::Server);

        for tx in room.get_senders(Some(&client_id)) {
            let _ = tx.blocking_send(message.clone());
        }

        info!(room, "Kicked {client_id}");
        self.remove_client(client_id);
    }

    pub fn list_rooms(&self) -> Vec<RoomInfo> {
        self.rooms
            .map
            .iter()
            .filter_map(|(id, room)| {
                let Some(authority) = room.authority else {
                    return None;
                };
                let Some(idx) = room
                    .members
                    .iter()
                    .position(|member| member.id == authority)
                else {
                    return None;
                };
                let authority = room.members[idx].name.clone();

                Some(RoomInfo {
                    id: id.to_string(),
                    authority,
                })
            })
            .collect()
    }

    /// Record a server-wide event
    pub fn log_generic(&mut self, msg: &str, kind: EventType) {
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
                id: id.clone(),
                authority: room.authority,
                locked: room.locked,
                members: room
                    .members
                    .iter()
                    .map(|client| (client.id, client.name.clone()))
                    .collect(),
                logs: room.logs.iter().cloned().collect(),
            })
            .collect();

        rooms.sort_by_key(|r| r.id.clone());

        Snapshot {
            rooms,
            system_log: self.system_log.iter().cloned().collect(),
        }
    }
}

const fn valid_room_id_characters() -> [char; 36] {
    let mut chars = ['\0'; 36];
    let mut ch: u8 = 48;
    let mut idx = 0;

    while ch <= 57 {
        chars[idx] = ch as char;
        ch += 1;
        idx += 1;
    }

    ch = 65;
    while ch <= 90 {
        chars[idx] = ch as char;
        ch += 1;
        idx += 1;
    }

    chars
}

fn generate_6_digit_code() -> String {
    let mut rng = rand::rng();
    let mut code = String::with_capacity(6);

    for _ in 0..6 {
        code.push(VALID_ROOM_ID_CHARACTERS[rng.random_range(0..VALID_ROOM_ID_CHARACTERS.len())]);
    }

    code
}

pub type ClientId = Uuid;
pub type ClientMap = HashMap<ClientId, RoomId>;

pub type ClientSender = mpsc::Sender<Message>;

pub type RoomId = String;
pub struct RoomMap {
    map: HashMap<RoomId, Room>,
}

impl RoomMap {
    pub fn new() -> Self {
        Self {
            map: HashMap::new(),
        }
    }
}

#[derive(PartialEq, Eq)]
pub enum RoomStatus {
    Closed,
    Open,
}

pub struct Room {
    /// A list of each connected client and their write channel
    members: Vec<Client>,
    /// The physics system authority of the room
    authority: Option<ClientId>,
    /// Whether new players can enter a room
    locked: bool,
    /// Recent activity for this room, newest last. Capped at [`MAX_LOG_LINES`].
    logs: VecDeque<Event>,
}

impl Room {
    pub fn get_senders(&self, exclude: Option<&ClientId>) -> Vec<ClientSender> {
        self.members
            .iter()
            .filter(|client| exclude != Some(&client.id))
            .map(|client| client.tx.clone())
            .collect()
    }

    fn get_sender(&self, id: &ClientId) -> Option<ClientSender> {
        self.members
            .iter()
            .find(|client| client.id == *id)
            .map(|client| client.tx.clone())
    }

    fn log_generic(&mut self, msg: &str, kind: EventType) {
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
            .map(|client| client.id)
            .position(|id| id == *client_id)
        else {
            warn!(self, "Attempted to remove client from room they are not in");
            return RoomStatus::Open;
        };

        self.members.remove(idx);

        if Some(*client_id) == self.authority {
            match self.members.first() {
                Some(next) => self.authority = Some(next.id),
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
    const fn new(id: ClientId, name: String, tx: ClientSender) -> Self {
        Self { id, name, tx }
    }
}

/// An immutable, cloned view of server state for rendering.
pub struct Snapshot {
    pub rooms: Vec<RoomSnapshot>,
    pub system_log: Box<[Event]>,
}

pub struct RoomSnapshot {
    pub id: RoomId,
    pub authority: Option<ClientId>,
    pub locked: bool,
    pub members: Vec<(ClientId, String)>,
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
