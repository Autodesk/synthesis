use std::{
    collections::HashMap,
    sync::{Arc, Mutex},
};
use tokio::net::TcpStream;

pub type RoomId = String;
pub type RoomMap = Arc<Mutex<HashMap<RoomId, Room>>>;

pub struct Room {
    members: Vec<TcpStream>,
    authority: TcpStream,
}
