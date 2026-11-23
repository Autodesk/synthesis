//! The purpose of this module is to provide resources for ensuring that
//! the terminal is properly cleaned up whenever the process exists.
//!
//! It provides two mechanisms for doing so:
//! - The [`Cleanup`] struct for when a function returns
//! - The [`set_panic_hook_to_cleanup_terminal`] function when the program panics

use std::panic;

use crossterm::{
    execute,
    terminal::{LeaveAlternateScreen, disable_raw_mode},
};

/// Initialize an instances of this struct at the top of your main function
/// Its `Drop` implementation will ensure that the terminal is cleaned up before the process exits.
pub struct Cleanup;

impl Drop for Cleanup {
    fn drop(&mut self) {
        cleanup_terminal();
    }
}

/// Sets the process to cleanup before panicking
pub fn set_panic_hook_to_cleanup_terminal() {
    let original_hook = panic::take_hook();

    panic::set_hook(Box::new(move |panic_info| {
        cleanup_terminal();

        original_hook(panic_info);
    }));
}

pub fn cleanup_terminal() {
    let _ = disable_raw_mode();
    let _ = execute!(std::io::stdout(), LeaveAlternateScreen);
}
