//! A TUI dashboard for the multiplayer websocket server.
//!
//! The TUI runs on its own OS thread (spawned from `main` when `--tui` is passed) and
//! reads a cloned [`Snapshot`] of the shared [`State`] each frame, so it never
//! holds the state lock across a render.
//!
//! Layout: a tab bar paging through rooms two-at-a-time (horizontal split),
//! each room panel shows its user list above its log stream, the focused one is highlighted.
//!
//! The admin can select a user with the arrows and kick them with `k` (after a confirmation),
//! and lock or unlock the focused room with `l` to control whether new clients may join.
//!
//! DISCLAIMER: This system is sort of a mess
//! WARNING: Do not call any methods on state, you should pass messages down channels instead

use crate::kick::UserAction;
use crate::lock;
use crate::logging::{self, LogSnapshot, Logger, RoomLogs};
use crate::panic::set_panic_hook_to_cleanup_terminal;
use crate::state::{ClientId, RoomId, RoomSnapshot, Snapshot, State};
use crate::util::trim_uuid;

use std::collections::VecDeque;
use std::fmt::Write;
use std::sync::{Arc, Mutex};
use std::{io, time::Duration};
use std::{process, thread};

use crossterm::event::{self, Event, KeyCode, KeyEventKind, KeyModifiers};
use ratatui::layout::{Alignment, Constraint, Direction, Layout, Rect};
use ratatui::style::{Color, Modifier, Style};
use ratatui::widgets::{Block, BorderType, Clear, List, ListItem, ListState, Paragraph, Tabs};
use ratatui::{
    DefaultTerminal, Frame,
    text::{Line, Span},
};
use rtrb::Producer;
use uuid::Uuid;

/// How many room panels are shown side-by-side on a single tab.
const ROOMS_PER_TAB: usize = 2;

const COLOR_PALETTE: &[Color] = &[
    Color::Red,
    Color::Indexed(221),
    Color::Yellow,
    Color::Green,
    Color::Blue,
    Color::Magenta,
];
const COLOR_PALETTE_SIZE: usize = 6;

pub fn start_tui_thread(
    state: &Arc<State>,
    usr_msg_tx: Producer<UserAction>,
    logger: Arc<Mutex<Logger>>,
) {
    let tui_state_handle = state.clone();
    set_panic_hook_to_cleanup_terminal();

    // On an OS thread because crossterm (and thus ratatui) will block on user input
    // So it wouldn't play nice with tokio's runtime, which expects yielding
    thread::spawn(move || {
        if let Err(e) = run(tui_state_handle, usr_msg_tx, &logger) {
            eprintln!("TUI error: {e}");
        }

        // TODO Send message to main thread warning to panic
        process::exit(0);
    });
}

fn run(
    state: Arc<State>,
    usr_msg_tx: Producer<UserAction>,
    logger: &Arc<Mutex<Logger>>,
) -> io::Result<()> {
    let mut terminal = ratatui::init();
    let result = run_app(&mut terminal, state, usr_msg_tx, logger);
    ratatui::restore();
    result
}

fn run_app(
    terminal: &mut DefaultTerminal,
    state: Arc<State>,
    mut usr_msg_tx: Producer<UserAction>,
    logger: &Arc<Mutex<Logger>>,
) -> io::Result<()> {
    let mut app = App::new(state);

    loop {
        let logger_snapshot = lock!(logger).snapshot();
        let state_snapshot = app.state.snapshot();
        app.sync(&state_snapshot);

        terminal.draw(|frame| ui(frame, &app, &state_snapshot, &logger_snapshot))?;

        // Poll so the view refreshes with live activity even without input.
        if event::poll(Duration::from_millis(250))?
            && let Event::Key(key) = event::read()?
            && key.kind == KeyEventKind::Press
        {
            let room_logs_len = app
                .focused_room
                .clone()
                .map(|room_id| {
                    logger_snapshot
                        .1
                        .get::<str>(room_id.as_ref())
                        .map(|logs| logs.len())
                })
                .flatten()
                .unwrap_or(0);

            app.on_key(
                key.code,
                key.modifiers,
                &mut usr_msg_tx,
                room_logs_len,
                logger_snapshot.0.len(),
            );
        }

        if app.should_quit {
            return Ok(());
        }
    }
}

struct App {
    state: Arc<State>,
    /// Index of current tab
    tab: usize,
    /// Index of focused panel within tab
    focused_panel: usize,
    /// Index of selected user within panel
    selected_user: usize,
    /// Whether there is currently a pending action to kick a user
    pending_kick: Option<ClientId>,
    should_quit: bool,

    /// Cursor distance from the bottom in the focused room log (0 = latest entry).
    room_log_cursor: usize,
    /// Cursor distance from the bottom in the system log (0 = latest entry).
    system_log_cursor: usize,

    /// Informatoin cached from the last `sync` so key handling can act without a snapshot.
    tab_count: usize,
    panels_on_tab: usize,
    focused_members: Vec<(ClientId, String)>,
    /// Id of the currently focused room, if any, so the lock toggle can act without a snapshot.
    focused_room: Option<RoomId>,
}

impl App {
    const fn new(state: Arc<State>) -> Self {
        Self {
            state,
            tab: 0,
            focused_panel: 0,
            selected_user: 0,
            pending_kick: None,
            should_quit: false,

            room_log_cursor: 0,
            system_log_cursor: 0,

            tab_count: 1,
            panels_on_tab: 0,
            focused_members: Vec::new(),
            focused_room: None,
        }
    }

    /// Reconcile cursor/tab/panel selection against the latest snapshot, so the
    /// selection stays valid as rooms and members come and go.
    fn sync(&mut self, snapshot: &Snapshot) {
        self.tab_count = snapshot.rooms.len().div_ceil(ROOMS_PER_TAB).max(1);
        if self.tab >= self.tab_count {
            self.tab = self.tab_count - 1;
        }

        let base = self.tab * ROOMS_PER_TAB;
        self.panels_on_tab = snapshot.rooms.len().saturating_sub(base).min(ROOMS_PER_TAB);

        if self.panels_on_tab == 0 {
            self.focused_panel = 0;
            self.selected_user = 0;
            self.focused_members.clear();
            self.focused_room = None;
            return;
        }

        if self.focused_panel >= self.panels_on_tab {
            self.focused_panel = self.panels_on_tab - 1;
        }

        let focused = &snapshot.rooms[base + self.focused_panel];
        let new_focused_room = Some(focused.id.clone());
        if self.focused_room != new_focused_room {
            self.room_log_cursor = 0;
        }
        self.focused_room = new_focused_room;
        self.focused_members.clone_from(&focused.members);

        if self.focused_members.is_empty() {
            self.selected_user = 0;
        } else if self.selected_user >= self.focused_members.len() {
            self.selected_user = self.focused_members.len() - 1;
        }
    }

    fn on_key(
        &mut self,
        code: KeyCode,
        mods: KeyModifiers,
        user_msg_tx: &mut Producer<UserAction>,
        room_log_len: usize,
        sys_log_len: usize,
    ) {
        // While a kick is pending, only y/n/esc are meaningful.
        if self.pending_kick.is_some() {
            match code {
                KeyCode::Char('y' | 'Y') => {
                    if let Some(user_id) = self.pending_kick.take() {
                        let _ = user_msg_tx.push(UserAction::Kick(user_id));

                        self.selected_user = self.selected_user.saturating_sub(1);
                    }
                }
                KeyCode::Char('n' | 'N') | KeyCode::Esc => {
                    self.pending_kick = None;
                }
                _ => {}
            }
            return;
        }

        match code {
            KeyCode::Char('c') if mods.contains(KeyModifiers::CONTROL) => self.should_quit = true,
            KeyCode::Char('q') | KeyCode::Esc => self.should_quit = true,

            KeyCode::Tab => self.tab = (self.tab + 1) % self.tab_count,
            KeyCode::BackTab => self.tab = (self.tab + self.tab_count - 1) % self.tab_count,

            KeyCode::Left => self.focused_panel = self.focused_panel.saturating_sub(1),
            KeyCode::Right => self.focused_panel += 1, // clamped in `sync`

            KeyCode::Up => self.selected_user = self.selected_user.saturating_sub(1),
            KeyCode::Down => self.selected_user += 1, // clamped in `sync`

            KeyCode::Char('k') => {
                if let Some((uid, _)) = self.focused_members.get(self.selected_user) {
                    self.pending_kick = Some(*uid);
                }
            }
            KeyCode::Char('l') => {
                if let Some(room_id) = &self.focused_room {
                    let _ = user_msg_tx.push(UserAction::Lock(room_id.clone()));
                }
            }

            // Room log cursor: [ moves up (older), ] moves down (newer).
            // Clamping to valid range is handled in render_logs.
            KeyCode::Char('[') => {
                self.room_log_cursor =
                    usize::min(self.room_log_cursor + 1, room_log_len.saturating_sub(1))
            }
            KeyCode::Char(']') => self.room_log_cursor = self.room_log_cursor.saturating_sub(1),

            // System log cursor: { moves up (older), } moves down (newer).
            // Clamping to valid range is handled in render_logs.
            KeyCode::Char('{') => {
                self.system_log_cursor =
                    usize::min(self.system_log_cursor + 1, sys_log_len.saturating_sub(1))
            }
            KeyCode::Char('}') => self.system_log_cursor = self.system_log_cursor.saturating_sub(1),

            _ => {}
        }
    }
}

fn ui(frame: &mut Frame, app: &App, snapshot: &Snapshot, log_snapshot: &LogSnapshot) {
    let chunks = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Length(3),
            Constraint::Min(0),
            Constraint::Length(1),
        ])
        .split(frame.area());

    // Split the middle region into the room panels and a system-log strip.
    let middle = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Min(0), Constraint::Length(6)])
        .split(chunks[1]);

    render_tabs(frame, chunks[0], app, snapshot);
    render_body(frame, middle[0], app, snapshot, &log_snapshot.1);
    render_system_log(frame, middle[1], &log_snapshot.0, app.system_log_cursor);
    render_status(frame, chunks[2]);

    if let Some(uid) = app.pending_kick {
        render_kick_popup(frame, uid);
    }
}

fn render_tabs(frame: &mut Frame, area: Rect, app: &App, snapshot: &Snapshot) {
    let block = Block::bordered().title(format!(" Rooms ({} active) ", snapshot.rooms.len()));

    if snapshot.rooms.is_empty() {
        frame.render_widget(block, area);
        return;
    }

    let titles: Vec<Line> = (0..app.tab_count)
        .map(|t| {
            let base = t * ROOMS_PER_TAB;
            let mut label = format!("Room {}", snapshot.rooms[base].id);
            if let Some(r) = snapshot.rooms.get(base + 1) {
                let _ = write!(&mut label, " / {}", r.id);
            }
            Line::from(label)
        })
        .collect();

    let tabs = Tabs::new(titles)
        .block(block)
        .select(app.tab)
        .highlight_style(
            Style::default()
                .fg(Color::Yellow)
                .add_modifier(Modifier::BOLD),
        );

    frame.render_widget(tabs, area);
}

fn render_body(
    frame: &mut Frame,
    area: Rect,
    app: &App,
    snapshot: &Snapshot,
    room_logs: &RoomLogs,
) {
    if snapshot.rooms.is_empty() {
        let placeholder = Paragraph::new("No active rooms.\nWaiting for a client to create one…")
            .alignment(Alignment::Center)
            .block(Block::bordered());
        frame.render_widget(placeholder, area);
        return;
    }

    let columns = Layout::default()
        .direction(Direction::Horizontal)
        .constraints([Constraint::Percentage(50), Constraint::Percentage(50)])
        .split(area);

    let base = app.tab * ROOMS_PER_TAB;
    for slot in 0..ROOMS_PER_TAB {
        let col = columns[slot];
        match snapshot.rooms.get(base + slot) {
            Some(room) => {
                let focused = slot == app.focused_panel;

                let fallback = VecDeque::new();
                let room_log = room_logs.get(&room.id).unwrap_or(&fallback);

                let log_cursor = if focused { app.room_log_cursor } else { 0 };
                render_room_panel(
                    frame,
                    col,
                    room,
                    room_log,
                    focused,
                    app.selected_user,
                    log_cursor,
                );
            }
            None => {
                frame.render_widget(Block::bordered().title(" (empty) "), col);
            }
        }
    }
}

fn render_room_panel(
    frame: &mut Frame,
    area: Rect,
    room: &RoomSnapshot,
    logs: &VecDeque<logging::Event>,
    focused: bool,
    cursor: usize,
    log_cursor: usize,
) {
    let border_style = if focused {
        Style::default().fg(Color::Yellow)
    } else {
        Style::default().fg(Color::DarkGray)
    };

    let mut title = vec![Span::raw(format!(
        " Room {}  ·  {} user(s) ",
        room.id,
        room.members.len()
    ))];
    if room.locked {
        title.push(Span::styled(
            " 🔒 LOCKED ",
            Style::default().fg(Color::Red).add_modifier(Modifier::BOLD),
        ));
    }

    let outer = Block::bordered()
        .border_type(if focused {
            BorderType::Thick
        } else {
            BorderType::Plain
        })
        .border_style(border_style)
        .title(Line::from(title));
    let inner = outer.inner(area);
    frame.render_widget(outer, area);

    let rows = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Percentage(45), Constraint::Percentage(55)])
        .split(inner);

    render_users(frame, rows[0], room, focused, cursor);
    render_logs(frame, rows[1], logs, log_cursor);
}

fn render_users(frame: &mut Frame, area: Rect, room: &RoomSnapshot, focused: bool, cursor: usize) {
    let member_to_item = |member_and_idx: (usize, &(Uuid, String))| -> ListItem<'_> {
        let (i, (uid, name)) = member_and_idx;
        let color = COLOR_PALETTE[i % COLOR_PALETTE_SIZE];
        let host_marker = if Some(*uid) == room.host { "  [H]" } else { "" };
        let uid = trim_uuid(uid);

        let label = format!("{uid} ({name}){host_marker}");
        ListItem::new(label).style(Style::new().fg(color))
    };

    let items: Vec<ListItem> = room
        .members
        .iter()
        .enumerate()
        .map(member_to_item)
        .collect();

    let list = List::new(items)
        .block(Block::bordered().title(" Users "))
        .highlight_style(
            Style::default()
                .bg(Color::Blue)
                .fg(Color::White)
                .add_modifier(Modifier::BOLD),
        )
        .highlight_symbol("▶ ");

    if focused && !room.members.is_empty() {
        let mut list_state = ListState::default();
        list_state.select(Some(cursor.min(room.members.len() - 1)));
        frame.render_stateful_widget(list, area, &mut list_state);
    } else {
        frame.render_widget(list, area);
    }
}

fn render_logs(
    frame: &mut Frame,
    area: Rect,
    logs: &VecDeque<logging::Event>,
    cursor_from_end: usize,
) {
    let visible = area.height.saturating_sub(2) as usize;
    let len = logs.len();

    if len == 0 {
        frame.render_widget(Block::bordered().title(" Logs "), area);
        return;
    }

    // Cursor as absolute index: 0 = oldest, len-1 = newest.
    let cursor_abs = (len - 1).saturating_sub(cursor_from_end);

    // Centre the cursor in the view window, then clamp so we don't overrun.
    let view_start = cursor_abs.saturating_sub(visible / 2);
    let view_start = if view_start + visible > len {
        len.saturating_sub(visible)
    } else {
        view_start
    };
    let view_end = (view_start + visible).min(len);

    let text: Vec<Line> = logs
        .iter()
        .enumerate()
        .skip(view_start)
        .take(view_end - view_start)
        .map(|(i, event)| {
            let style = Style::from(&event.kind);
            let line = Line::from(event.message.clone());
            if i == cursor_abs {
                line.style(style.bg(Color::Indexed(245)))
            } else {
                line.style(style)
            }
        })
        .collect();

    let title = if cursor_from_end > 0 {
        format!(" Logs [{}/{}] ", cursor_abs + 1, len)
    } else {
        " Logs ".to_string()
    };

    frame.render_widget(
        Paragraph::new(text).block(Block::bordered().title(title)),
        area,
    );
}

fn render_system_log(
    frame: &mut Frame,
    area: Rect,
    global_log: &VecDeque<logging::Event>,
    cursor_from_end: usize,
) {
    let visible = area.height.saturating_sub(2) as usize;
    let len = global_log.len();

    if len == 0 {
        frame.render_widget(
            Block::bordered()
                .title(" System ")
                .style(Style::default().fg(Color::DarkGray)),
            area,
        );
        return;
    }

    let cursor_abs = (len - 1).saturating_sub(cursor_from_end);

    let view_start = cursor_abs.saturating_sub(visible / 2);
    let view_start = if view_start + visible > len {
        len.saturating_sub(visible)
    } else {
        view_start
    };
    let view_end = (view_start + visible).min(len);

    let text: Vec<Line> = global_log
        .iter()
        .enumerate()
        .skip(view_start)
        .take(view_end - view_start)
        .map(|(i, event)| {
            let line = Line::from(event.message.clone());
            if i == cursor_abs {
                line.style(Style::from(&event.kind).bg(Color::Indexed(245)))
            } else {
                line.style(Style::from(&event.kind))
            }
        })
        .collect();

    let title = if cursor_from_end > 0 {
        format!(" System [{}/{}] ", cursor_abs + 1, len)
    } else {
        " System ".to_string()
    };

    frame.render_widget(
        Paragraph::new(text).block(Block::bordered().title(title)),
        area,
    );
}

fn render_status(frame: &mut Frame, area: Rect) {
    let hints = " q quit  │  Tab/⇧Tab page rooms  │  ←/→ focus panel  │  ↑/↓ select user  │  k kick  │  l lock/unlock  │  [/] scroll room log  │  {/} scroll system log ";
    let status = Paragraph::new(hints).style(Style::default().fg(Color::Black).bg(Color::Gray));
    frame.render_widget(status, area);
}

fn render_kick_popup(frame: &mut Frame, uid: ClientId) {
    let area = centered_rect(50, 20, frame.area());
    frame.render_widget(Clear, area);

    let block = Block::bordered()
        .border_type(BorderType::Double)
        .border_style(Style::default().fg(Color::Red))
        .title(" Confirm kick ");

    let text = vec![
        Line::from(""),
        Line::from(format!("Kick {uid}?")),
        Line::from(""),
        Line::from("[y] yes    [n] no"),
    ];

    let popup = Paragraph::new(text)
        .alignment(Alignment::Center)
        .block(block);
    frame.render_widget(popup, area);
}

/// A rect centered within `area`, sized as a percentage of it.
fn centered_rect(percent_x: u16, percent_y: u16, area: Rect) -> Rect {
    let vertical = Layout::default()
        .direction(Direction::Vertical)
        .constraints([
            Constraint::Percentage((100 - percent_y) / 2),
            Constraint::Percentage(percent_y),
            Constraint::Percentage((100 - percent_y) / 2),
        ])
        .split(area);

    Layout::default()
        .direction(Direction::Horizontal)
        .constraints([
            Constraint::Percentage((100 - percent_x) / 2),
            Constraint::Percentage(percent_x),
            Constraint::Percentage((100 - percent_x) / 2),
        ])
        .split(vertical[1])[1]
}
