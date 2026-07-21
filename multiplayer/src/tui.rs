//! A terminal dashboard for the websocket server.
//!
//! Runs on its own OS thread (spawned from `main` when `--tui` is passed) and
//! reads a cloned [`Snapshot`] of the shared [`State`] each frame, so it never
//! holds the state lock across a render. Layout: a tab bar paging through rooms
//! two-at-a-time, each room panel showing its user list above its log stream,
//! and a status bar of key hints. The focused room panel is highlighted; the
//! admin selects a user with the arrows and kicks with `k` (confirmed y/n).

use std::io;
use std::sync::{Arc, Mutex};
use std::time::Duration;

use crossterm::event::{self, Event, KeyCode, KeyEventKind, KeyModifiers};
use ratatui::layout::{Alignment, Constraint, Direction, Layout, Rect};
use ratatui::style::{Color, Modifier, Style};
use ratatui::text::Line;
use ratatui::widgets::{
    Block, BorderType, Clear, List, ListItem, ListState, Paragraph, Tabs, Wrap,
};
use ratatui::{DefaultTerminal, Frame};

use crate::room::{ClientId, RoomSnapshot, Snapshot, State};

/// How many room panels are shown side-by-side on a single tab.
const ROOMS_PER_TAB: usize = 2;

pub fn run(state: Arc<Mutex<State>>) -> io::Result<()> {
    let mut terminal = ratatui::init();
    let result = run_app(&mut terminal, state);
    ratatui::restore();
    result
}

fn run_app(terminal: &mut DefaultTerminal, state: Arc<Mutex<State>>) -> io::Result<()> {
    let mut app = App::new(state);

    loop {
        let snapshot = app.state.lock().unwrap().snapshot();
        app.sync(&snapshot);

        terminal.draw(|frame| ui(frame, &app, &snapshot))?;

        // Poll so the view refreshes with live activity even without input.
        if event::poll(Duration::from_millis(250))? {
            if let Event::Key(key) = event::read()? {
                if key.kind == KeyEventKind::Press {
                    app.on_key(key.code, key.modifiers);
                }
            }
        }

        if app.should_quit {
            return Ok(());
        }
    }
}

struct App {
    state: Arc<Mutex<State>>,
    /// Index of current tab
    tab: usize,
    /// Index of focused panel within tab
    focused_panel: usize,
    /// Index of selected user within panel
    selected_user: usize,
    /// Whether there is currently a pending action to kick a user
    pending_kick: Option<ClientId>,
    should_quit: bool,

    /// Informatoin cached from the last `sync` so key handling can act without a snapshot.
    tab_count: usize,
    panels_on_tab: usize,
    focused_members: Vec<ClientId>,
}

impl App {
    fn new(state: Arc<Mutex<State>>) -> Self {
        Self {
            state,
            tab: 0,
            focused_panel: 0,
            selected_user: 0,
            pending_kick: None,
            should_quit: false,
            tab_count: 1,
            panels_on_tab: 0,
            focused_members: Vec::new(),
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
            return;
        }

        if self.focused_panel >= self.panels_on_tab {
            self.focused_panel = self.panels_on_tab - 1;
        }

        self.focused_members = snapshot.rooms[base + self.focused_panel].members.clone();

        if self.focused_members.is_empty() {
            self.selected_user = 0;
        } else if self.selected_user >= self.focused_members.len() {
            self.selected_user = self.focused_members.len() - 1;
        }
    }

    fn on_key(&mut self, code: KeyCode, mods: KeyModifiers) {
        // While a kick is pending, only y/n/esc are meaningful.
        if self.pending_kick.is_some() {
            match code {
                KeyCode::Char('y') | KeyCode::Char('Y') => {
                    if let Some(user_id) = self.pending_kick.take() {
                        self.state.lock().unwrap().kick(user_id);
                        self.selected_user = self.selected_user.saturating_sub(1);
                    }
                }
                KeyCode::Char('n') | KeyCode::Char('N') | KeyCode::Esc => {
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
            //
            KeyCode::Up => self.selected_user = self.selected_user.saturating_sub(1),
            KeyCode::Down => self.selected_user += 1, // clamped in `sync`
            //
            KeyCode::Char('k') => {
                if let Some(uid) = self.focused_members.get(self.selected_user) {
                    self.pending_kick = Some(*uid);
                }
            }
            _ => {}
        }
    }
}

fn ui(frame: &mut Frame, app: &App, snapshot: &Snapshot) {
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
    render_body(frame, middle[0], app, snapshot);
    render_system_log(frame, middle[1], snapshot);
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
                label.push_str(&format!(" / {}", r.id));
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

fn render_body(frame: &mut Frame, area: Rect, app: &App, snapshot: &Snapshot) {
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
                render_room_panel(frame, col, room, focused, app.selected_user);
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
    focused: bool,
    cursor: usize,
) {
    let border_style = if focused {
        Style::default().fg(Color::Yellow)
    } else {
        Style::default().fg(Color::DarkGray)
    };

    let outer = Block::bordered()
        .border_type(if focused {
            BorderType::Thick
        } else {
            BorderType::Plain
        })
        .border_style(border_style)
        .title(format!(
            " Room {}  ·  {} user(s) ",
            room.id,
            room.members.len()
        ));
    let inner = outer.inner(area);
    frame.render_widget(outer, area);

    let rows = Layout::default()
        .direction(Direction::Vertical)
        .constraints([Constraint::Percentage(45), Constraint::Percentage(55)])
        .split(inner);

    render_users(frame, rows[0], room, focused, cursor);
    render_logs(frame, rows[1], room);
}

fn render_users(frame: &mut Frame, area: Rect, room: &RoomSnapshot, focused: bool, cursor: usize) {
    let items: Vec<ListItem> = room
        .members
        .iter()
        .map(|uid| {
            let mut label = uid.to_string();
            if *uid == room.host {
                label.push_str("  [host]");
            }
            if *uid == room.authority {
                label.push_str("  [auth]");
            }
            ListItem::new(label)
        })
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

fn render_logs(frame: &mut Frame, area: Rect, room: &RoomSnapshot) {
    // Show the newest lines that fit (area height minus the two border rows).
    let visible = area.height.saturating_sub(2) as usize;
    let start = room.logs.len().saturating_sub(visible);
    let text: Vec<Line> = room.logs[start..]
        .iter()
        .map(|l| Line::from(l.as_str()))
        .collect();

    let logs = Paragraph::new(text)
        .block(Block::bordered().title(" Logs "))
        .wrap(Wrap { trim: false });

    frame.render_widget(logs, area);
}

fn render_system_log(frame: &mut Frame, area: Rect, snapshot: &Snapshot) {
    let visible = area.height.saturating_sub(2) as usize;
    let start = snapshot.system_log.len().saturating_sub(visible);
    let text: Vec<Line> = snapshot.system_log[start..]
        .iter()
        .map(|l| Line::from(l.as_str()))
        .collect();

    let panel = Paragraph::new(text)
        .block(Block::bordered().title(" System "))
        .style(Style::default().fg(Color::DarkGray))
        .wrap(Wrap { trim: false });
    frame.render_widget(panel, area);
}

fn render_status(frame: &mut Frame, area: Rect) {
    let hints =
        " q quit  │  Tab/⇧Tab page rooms  │  ←/→ focus panel  │  ↑/↓ select user  │  k kick ";
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
