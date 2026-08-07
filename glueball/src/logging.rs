use std::collections::{HashMap, VecDeque};

use colored::{Color, ColoredString, Colorize};
use ratatui::{
    style::{Style, Stylize},
    text::Line,
};
use tokio::sync::mpsc::{Receiver, Sender};

use crate::room::RoomId;

pub const MAX_LOG_LINES: usize = 500;

pub type RoomLogs = HashMap<RoomId, VecDeque<Event>>;
pub type GlobalLog = VecDeque<Event>;
pub type LogSnapshot = (GlobalLog, RoomLogs);
pub type LogSender = Sender<(String, EventType, LogDestination)>;

#[macro_export]
macro_rules! info_global {
    ($log_tx:expr, $($arg:tt)*) => {
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Info, LogDestination::Global));
    }
}

#[macro_export]
macro_rules! warn_global {
    ($log_tx:expr, $($arg:tt)*) => {
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Warning, LogDestination::Global));
    }
}

#[macro_export]
macro_rules! error_global {
    ($log_tx:expr, $($arg:tt)*) => {
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Error, LogDestination::Global));
    }
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! info_room {
    ($log_tx:expr, $room_id:expr, $($arg:tt)*) => {{
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Info, LogDestination::Room($room_id.clone())));
    }};
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! warn_room {
    ($log_tx:expr, $room_id:expr, $($arg:tt)*) => {{
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Warning, LogDestination::Room($room_id.clone())));
    }};
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! error_room {
    ($log_tx:expr, $room_id:expr, $($arg:tt)*) => {{
        let _ = $log_tx.try_send((format!($($arg)*), EventType::Error, LogDestination::Room($room_id.clone())));
    }};
}

pub enum LogDestination {
    Global,
    Room(RoomId),
}

#[derive(Debug, Clone)]
pub enum EventType {
    Info,
    Warning,
    Error,
}

#[derive(Debug, Clone)]
pub struct Event {
    pub message: String,
    pub kind: EventType,
}

pub fn print_global(message: &str, kind: &EventType) {
    let color = colored::Color::from(kind);
    let colored_message = message.color(color);

    let global_str: ColoredString = "GLOBAL".color(Color::Magenta);
    println!("[{global_str}] {colored_message}");
}

pub fn print_room(message: &str, kind: &EventType, room_id: &RoomId) {
    let color = colored::Color::from(kind);
    let colored_message = message.color(color);

    let room_str: ColoredString = "ROOM".color(Color::Cyan);
    println!("[{room_str}: {room_id}] {colored_message}");
}

impl From<&EventType> for colored::Color {
    fn from(value: &EventType) -> Self {
        match value {
            EventType::Info => Self::Green,
            EventType::Warning => Self::Yellow,
            EventType::Error => Self::Red,
        }
    }
}

impl From<&EventType> for Style {
    fn from(value: &EventType) -> Self {
        match value {
            EventType::Info => Self::new().green(),
            EventType::Warning => Self::new().yellow(),
            EventType::Error => Self::new().red(),
        }
    }
}

impl From<&Event> for Line<'_> {
    fn from(value: &Event) -> Self {
        let color = Style::from(&value.kind);
        Line::from(value.message.clone()).style(color)
    }
}

pub struct Logger {
    global_log: GlobalLog,
    room_logs: RoomLogs,
    maximum_lines: usize,
}

impl Logger {
    pub fn new(maximum_lines: usize) -> Self {
        Self {
            global_log: VecDeque::new(),
            room_logs: HashMap::new(),
            maximum_lines,
        }
    }

    pub fn push_global(&mut self, message: String, kind: EventType) {
        if self.maximum_lines <= self.global_log.len() {
            self.global_log.pop_front();
        }

        let event = Event { message, kind };
        self.global_log.push_back(event);
    }

    pub fn push_room(&mut self, message: String, kind: EventType, room_id: RoomId) {
        let room_log = self.room_logs.entry(room_id).or_default();

        if self.maximum_lines <= room_log.len() {
            room_log.pop_front();
        }

        let event = Event { message, kind };
        room_log.push_back(event);
    }

    pub fn snapshot(&self) -> LogSnapshot {
        (self.global_log.clone(), self.room_logs.clone())
    }
}

pub fn spawn_log_receiver<F>(
    mut logging_rx: Receiver<(String, EventType, LogDestination)>,
    handle_log: F,
) where
    F: Fn(String, EventType, LogDestination) + Send + 'static,
{
    tokio::spawn(async move {
        loop {
            let Some((message, kind, log_destination)) = logging_rx.recv().await else {
                break;
            };

            handle_log(message, kind, log_destination);
        }
    });
}
