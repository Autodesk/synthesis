#[macro_export]
macro_rules! log {
    ($room_or_state:ident, $($arg:tt)*) => {
        $room_or_state.log(format!($($arg)*))
    }
}
