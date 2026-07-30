use std::panic;

use crossterm::{
    execute,
    terminal::{LeaveAlternateScreen, disable_raw_mode},
};

pub fn set_panic_hook_to_cleanup_terminal() {
    let original_hook = panic::take_hook();

    panic::set_hook(Box::new(move |panic_info| {
        let _ = disable_raw_mode();
        let _ = execute!(std::io::stdout(), LeaveAlternateScreen);

        original_hook(panic_info);
    }));
}
