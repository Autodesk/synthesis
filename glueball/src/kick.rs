use crate::{
    lock,
    model::ServerToClientMessage,
    room::{ClientId, State},
    util::server_sent_msg,
};
use rtrb::{Consumer, Producer, RingBuffer};
use std::sync::{Arc, Mutex};
use tokio_tungstenite::tungstenite::Message;

const MAX_PENDING_KICK_MESSAGES: usize = 12;

pub fn setup_kick_system(state: &Arc<Mutex<State>>) -> Producer<ClientId> {
    let (kick_tx, kick_rx) = RingBuffer::new(MAX_PENDING_KICK_MESSAGES);
    spawn_kick_receiver(kick_rx, state.clone());

    kick_tx
}

fn spawn_kick_receiver(mut kick_rx: Consumer<ClientId>, state: Arc<Mutex<State>>) {
    tokio::spawn(async move {
        loop {
            if let Ok(client_id) = kick_rx.pop() {
                kick(state.clone(), client_id).await;
            }
        }
    });
}

async fn kick(state: Arc<Mutex<State>>, client_id: ClientId) {
    let Some(client_tx) = lock!(state).get_client_tx(&client_id) else {
        return;
    };

    // The close message gets forwarded to the client getting kicked
    let _ = client_tx.send(Message::Close(None)).await;

    tell_room_client_was_kicked(state.clone(), &client_id).await;

    // Lock must come later to avoid removing client before
    // `tell_room_client_left` can get everyone else in the room
    lock!(state).remove_client(client_id);
}

// This function take a lock itself to avoid holding the lock while sending kick messages
async fn tell_room_client_was_kicked(state: Arc<Mutex<State>>, client_id: &ClientId) {
    // Send message to all other clients telling them `client_id` has been kicked
    let message = server_sent_msg(ServerToClientMessage::Kick {
        client_id: client_id.to_string(),
    });

    let senders = lock!(state).get_senders_from_user_room(*client_id);
    let outgoing = senders.iter().map(|tx| tx.send(message.clone()));
    futures_util::future::join_all(outgoing).await;
}
