use crate::model::RoomInfo;

use anyhow::{Result, bail};
use dashmap::DashMap;
use dashmap::mapref::one::{Ref, RefMut};
use rand::RngExt;
use tokio::sync::mpsc::{self};
use tokio_tungstenite::tungstenite::Message;
use uuid::Uuid;

/// Maximum number of log lines retained in each log (both per-room and system logs)
/// Oldest lines are dropped once the buffer is full.
const VALID_ROOM_ID_CHARACTERS: [char; 36] = valid_room_id_characters();
const MAX_ROOM_COUNT: usize = 32;

pub struct State {
    pub users: ClientMap,
    pub rooms: RoomMap,
}

impl State {
    pub fn new() -> Self {
        Self {
            users: DashMap::new(),
            rooms: DashMap::new(),
        }
    }

    pub fn initialize_client_in_room(
        &self,
        tx: ClientSender,
        room_id: Option<RoomId>,
        name: &str,
    ) -> Option<(ClientId, RoomId)> {
        match room_id {
            None if self.room_count() == MAX_ROOM_COUNT => None,
            None => Some(self.add_room_and_host(name.to_string(), tx).ok()?),
            Some(room_id) => self
                .add_client_to_room(name, tx, &room_id)
                .map(|client_id| (client_id, room_id)),
        }
    }

    pub fn get_client_tx(&self, client_id: &ClientId) -> Option<ClientSender> {
        let room = self.get_room_of_client(client_id)?;
        room.get_sender(client_id)
    }

    pub fn add_room_and_host(
        &self,
        host_name: String,
        host_tx: ClientSender,
    ) -> Result<(ClientId, RoomId)> {
        let host_id = Uuid::new_v4();
        let room = Room {
            members: vec![Client::new(host_id, host_name.clone(), host_tx)],
            host: Some(host_id),
            locked: false,
            permanent: false,
        };

        let room_id = loop {
            let code = generate_6_digit_code();
            if !self.rooms.contains_key(&code) {
                break code;
            }
        };

        info_room!(room_id, "{host_name} [H] created room {room_id}",);

        self.users.insert(host_id, room_id.clone());
        self.rooms.insert(room_id.clone(), room);

        Ok((host_id, room_id))
    }

    /// # Safety
    /// Relinquish all locks on `self.room` before calling
    pub fn remove_client(&self, client_id: &ClientId) {
        let Some(room_id) = self.users.get(client_id).map(|a| a.value().clone()) else {
            return;
        };

        // Room lock held
        let Some(mut room) = self.rooms.get_mut(&room_id) else {
            return;
        };

        let room_closed = room.remove_client(client_id) == RoomStatus::Closed;
        drop(room);

        if room_closed {
            self.rooms.remove(&room_id);
            remove_room!(room_id);
        }
    }

    pub fn add_client_to_room(
        &self,
        client_name: &str,
        client_tx: ClientSender,
        room_id: &RoomId,
    ) -> Option<ClientId> {
        let client_id = Uuid::new_v4();
        let Some(mut room) = self.rooms.get_mut(room_id) else {
            warn_global!("Attempted to add {client_id} into non-existant room {room_id}");
            return None;
        };

        if room.locked {
            return None;
        }

        let client = Client::new(client_id, client_name.to_string(), client_tx);
        room.members.push(client);
        self.users.insert(client_id, room_id.clone());

        if room.host.is_none() {
            info_room!(room_id, "{client_name} became host of room",);
            room.host = Some(client_id);
        }

        info_room!(room_id, "{client_name} joined room",);

        Some(client_id)
    }

    pub fn new_permanent_room(&self, room_id: RoomId) {
        if !is_valid_room_id(&room_id) {
            error_global!(
                "Invalid permanent room id: {room_id}, must be 6 characters and each character must match `[0-9A-Z]`"
            );
            return;
        }

        let room = Room {
            members: Vec::new(),
            host: None,
            locked: false,
            permanent: true,
        };
        self.rooms.insert(room_id, room);
    }

    pub fn get_room_of_client_mut(&self, client_id: &ClientId) -> Option<RefMut<'_, RoomId, Room>> {
        let Some(room_id) = self.users.get(client_id) else {
            warn_global!("Attempted to get client that does not exist");
            return None;
        };

        self.rooms.get_mut(&room_id.value().clone())
    }

    pub fn get_room_of_client(&self, client_id: &ClientId) -> Option<Ref<'_, RoomId, Room>> {
        let room_id = self.users.get(client_id)?.value().clone();

        self.rooms.get(&room_id)
    }

    pub fn get_senders_from_user_room(&self, client_id: ClientId) -> Vec<ClientSender> {
        let Some(room) = self.get_room_of_client(&client_id) else {
            return Vec::new();
        };

        room.get_peer_senders(&client_id)
    }

    /// Flips whether new clients can join `room_id`.
    /// Returns the new locked state, or `None` if the room does not exist.
    pub fn toggle_room_lock(&self, room_id: &RoomId) -> Option<bool> {
        let mut room = self.rooms.get_mut(room_id)?;
        room.locked = !room.locked;
        let locked = room.locked;

        if locked {
            info_room!(room_id, "Room Locked");
        } else {
            info_room!(room_id, "Room Unlocked");
        }

        Some(locked)
    }

    pub fn list_rooms(&self) -> Vec<RoomInfo> {
        self.rooms
            .iter()
            .map(|room| {
                let host = room
                    .members
                    .iter()
                    .position(|member| Some(member.id) == room.host)
                    .map(|idx| room.members[idx].name.clone());

                RoomInfo {
                    id: room.key().clone(),
                    host,
                    locked: room.locked,
                }
            })
            .collect()
    }

    pub fn room_count(&self) -> usize {
        self.rooms.len()
    }

    /// Takes a snapshot of the application state so the TUI
    /// doesn't hold a lock on it
    pub fn snapshot(&self) -> Snapshot {
        let mut rooms: Vec<RoomSnapshot> = self
            .rooms
            .iter()
            .map(|room| RoomSnapshot {
                id: room.key().clone(),
                host: room.host,
                locked: room.locked,
                members: room
                    .members
                    .iter()
                    .map(|client| (client.id, client.name.clone()))
                    .collect(),
            })
            .collect();

        rooms.sort_by(|a, b| a.id.cmp(&b.id));

        Snapshot { rooms }
    }
}

pub fn is_valid_room_id(s: &str) -> bool {
    s.trim().len() == 6
        && s.chars()
            .all(|c| c.is_ascii_digit() || c.is_ascii_uppercase())
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
pub type ClientMap = DashMap<ClientId, RoomId>;

pub type ClientSender = mpsc::Sender<Message>;

pub type RoomId = String;
pub type RoomMap = DashMap<RoomId, Room>;

#[derive(PartialEq, Eq)]
pub enum RoomStatus {
    Closed,
    Open,
}

pub struct Room {
    /// A list of each connected client and their write channel
    members: Vec<Client>,
    /// Host (initially the creator) of the room
    host: Option<ClientId>,
    /// Whether new players can enter a room
    locked: bool,
    /// Whether the room closes when it has no players
    permanent: bool,
}

impl Room {
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
    const fn new(id: ClientId, name: String, tx: ClientSender) -> Self {
        Self { id, name, tx }
    }
}

/// An immutable, cloned view of server state for rendering.
pub struct Snapshot {
    pub rooms: Vec<RoomSnapshot>,
}

pub struct RoomSnapshot {
    pub id: RoomId,
    pub host: Option<ClientId>,
    pub locked: bool,
    pub members: Vec<(ClientId, String)>,
}

#[cfg(test)]
mod tests {
    use super::{ClientSender, MAX_ROOM_COUNT, State, is_valid_room_id};
    use crate::LOG_TX;
    use tokio::sync::mpsc;

    fn log_tx() {
        let (tx, _rx) = mpsc::channel(64);
        let _ = LOG_TX.set(tx);
    }

    fn client_tx() -> ClientSender {
        let (tx, _rx) = mpsc::channel(64);
        tx
    }

    #[test]
    fn create_room_adds_host() {
        log_tx();
        let state = State::new();
        state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();
        assert_eq!(state.room_count(), 1);

        let rooms = state.list_rooms();
        assert_eq!(rooms[0].host.as_deref(), Some("Alice"));
        assert!(!rooms[0].locked);
    }

    #[test]
    fn join_existing_room() {
        log_tx();
        let state = State::new();
        let (_, room_id) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();

        assert!(
            state
                .add_client_to_room("Bob", client_tx(), &room_id)
                .is_some()
        );
        assert_eq!(state.room_count(), 1);
    }

    #[test]
    fn join_nonexistent_room_returns_none() {
        log_tx();
        let state = State::new();
        assert!(
            state
                .add_client_to_room("Bob", client_tx(), &"ZZZZZZ".to_string())
                .is_none()
        );
    }

    #[test]
    fn join_locked_room_returns_none() {
        log_tx();
        let state = State::new();
        let (_, room_id) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();
        state.toggle_room_lock(&room_id);

        assert!(
            state
                .add_client_to_room("Bob", client_tx(), &room_id)
                .is_none()
        );
    }

    #[test]
    fn last_client_leaving_closes_room() {
        log_tx();
        let state = State::new();
        let (client_id, _) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();
        state.remove_client(&client_id);

        assert_eq!(state.room_count(), 0);
    }

    #[test]
    fn host_leaving_transfers_to_next_member() {
        log_tx();
        let state = State::new();
        let (host_id, room_id) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();

        state
            .add_client_to_room("Bob", client_tx(), &room_id)
            .unwrap();
        state.remove_client(&host_id);

        assert_eq!(state.room_count(), 1);
    }

    #[test]
    fn permanent_room_stays_open_when_empty() {
        log_tx();
        let state = State::new();

        let room_id = "PERM01".to_string();
        state.new_permanent_room(room_id.clone());

        let client_id = state
            .add_client_to_room("Alice", client_tx(), &room_id)
            .unwrap();
        state.remove_client(&client_id);

        assert_eq!(state.room_count(), 1);
    }

    #[test]
    fn list_rooms_shows_host_and_lock_status() {
        log_tx();
        let state = State::new();
        assert!(state.list_rooms().is_empty());

        let (_, room_id) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();
        state.toggle_room_lock(&room_id);
        let rooms = state.list_rooms();

        assert_eq!(rooms.len(), 1);
        assert_eq!(rooms[0].host.as_deref(), Some("Alice"));
        assert!(rooms[0].locked);
    }

    #[test]
    fn toggle_lock_flips_state() {
        log_tx();
        let state = State::new();
        let (_, room_id) = state
            .add_room_and_host("Alice".to_string(), client_tx())
            .unwrap();

        assert_eq!(state.toggle_room_lock(&room_id), Some(true));
        assert_eq!(state.toggle_room_lock(&room_id), Some(false));
    }

    #[test]
    fn max_rooms_prevents_new_room() {
        log_tx();
        let state = State::new();
        for i in 0..MAX_ROOM_COUNT {
            state
                .add_room_and_host(format!("Client{i}"), client_tx())
                .unwrap();
        }

        assert!(
            state
                .initialize_client_in_room(client_tx(), None, "overflow")
                .is_none()
        );
    }

    #[test]
    fn valid_room_id_accepts_alphanumeric_uppercase() {
        assert!(is_valid_room_id("AZA1EA"));
        assert!(is_valid_room_id("000000"));
        assert!(is_valid_room_id("ZZZZZZ"));
    }

    #[test]
    fn valid_room_id_rejects_wrong_length_or_lowercase() {
        assert!(!is_valid_room_id("ERIC"));
        assert!(!is_valid_room_id("ALINA"));
        assert!(!is_valid_room_id("abc123"));
        assert!(!is_valid_room_id(""));
    }
}
