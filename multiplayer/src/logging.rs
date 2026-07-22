use ratatui::{
    style::{Style, Stylize},
    text::Line,
};

#[macro_export]
macro_rules! info {
    ($room_or_state:ident, $($arg:tt)*) => {
        $room_or_state.log_generic(format!($($arg)*), EventType::Info)
    }
}

#[macro_export]
macro_rules! warn {
    ($room_or_state:ident, $($arg:tt)*) => {
        $room_or_state.log_generic(format!($($arg)*), EventType::Warning)
    }
}

#[macro_export]
macro_rules! error {
    ($room_or_state:ident, $($arg:tt)*) => {
        $room_or_state.log_generic(format!($($arg)*), EventType::Error)
    }
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! info_lock {
    ($state:ident, $($arg:tt)*) => {{
        let mut guard = $state.lock().unwrap();
        info!(guard, $($arg)*);
    }};
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! warn_lock {
    ($state:ident, $($arg:tt)*) => {{
        let mut guard = $state.lock().unwrap();
        warn!(guard, $($arg)*);
    }};
}

/// Takes a lock on `state`
#[macro_export]
macro_rules! error_lock {
    ($state:ident, $($arg:tt)*) => {{
        let mut guard = $state.lock().unwrap();
        error!(guard, $($arg)*);
    }};
}

#[derive(Debug, Clone)]
pub enum EventType {
    Info,
    Warning,
    Error,
}

#[derive(Debug, Clone)]
pub struct Event {
    pub kind: EventType,
    pub message: String,
}

impl From<EventType> for Style {
    fn from(value: EventType) -> Self {
        match value {
            EventType::Info => Style::new().white(),
            EventType::Warning => Style::new().yellow(),
            EventType::Error => Style::new().red(),
        }
    }
}

impl From<Event> for Line<'_> {
    fn from(value: Event) -> Self {
        let color = Style::from(value.kind);
        Line::from(value.message).style(color)
    }
}
