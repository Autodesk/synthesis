use crate::{
    model::ServerToClientMessage,
    room::{ClientId, ClientSender, RoomId, RoomStatus},
    state::State,
    util::server_sent_msg,
};
use rtrb::{Consumer, Producer, RingBuffer};
use std::sync::Arc;
use tokio_tungstenite::tungstenite::Message;

const MAX_PENDING_KICK_MESSAGES: usize = 12;

pub enum UserAction {
    Lock(RoomId),
    Kick(ClientId),
}

pub fn setup_user_action_system(state: &Arc<State>) -> Producer<UserAction> {
    let (tx, rx) = RingBuffer::new(MAX_PENDING_KICK_MESSAGES);
    spawn_user_action_receiver(rx, state.clone());

    tx
}

fn spawn_user_action_receiver(mut rx: Consumer<UserAction>, state: Arc<State>) {
    tokio::spawn(async move {
        loop {
            let Ok(user_message) = rx.pop() else {
                tokio::task::yield_now().await;
                continue;
            };

            match user_message {
                UserAction::Lock(room_id) => {
                    let _ = state.toggle_room_lock(&room_id);
                }
                UserAction::Kick(client_id) => kick(state.clone(), client_id).await,
            }
        }
    });
}

async fn kick(state: Arc<State>, client_id: ClientId) {
    let Some(KickOutcome {
        client_name,
        client_tx,
        peer_senders,
    }) = state.kick_client(&client_id)
    else {
        return;
    };

    info_global!("Kicked {}", client_name);

    // The close message gets forwarded to the client getting kicked
    // TODO Don't have send a close back / deal with double removal
    let _ = client_tx.send(Message::Close(None)).await;

    // Send message to all other clients telling them `client_id` has been kicked
    let message = server_sent_msg(ServerToClientMessage::Kick {
        client_id: client_id.to_string(),
    });

    let outgoing = peer_senders.iter().map(|tx| tx.send(message.clone()));
    futures_util::future::join_all(outgoing).await;
}

struct KickOutcome {
    pub client_name: String,
    pub client_tx: ClientSender,
    pub peer_senders: Vec<ClientSender>,
}

impl State {
    fn kick_client(&self, client_id: &ClientId) -> Option<KickOutcome> {
        let room_id = self.users.get(client_id)?.value().clone();
        let mut room = self.rooms.get_mut(&room_id)?; // Take room lock

        let client_name = room.get_client_name(client_id).ok()?;
        let client_tx = room.get_sender(client_id)?;
        let peer_senders = room.get_peer_senders(client_id);

        let room_closed = room.remove_client(client_id) == RoomStatus::Closed;
        drop(room); // Relinquish room lock

        if room_closed {
            self.rooms.remove(&room_id);
            remove_room!(room_id);
        }

        self.users.remove(client_id);

        Some(KickOutcome {
            client_name,
            client_tx,
            peer_senders,
        })
    }
}
