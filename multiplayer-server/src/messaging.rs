use joltc_sys::{JPC_BodyID, JPC_Quat, JPC_RVec3, JPC_Vec3};
use std::collections::HashMap;

pub type BodyId = JPC_BodyID;
pub type Position = JPC_RVec3;
pub type Velocity = JPC_Vec3;
pub type Rotation = JPC_Quat;

pub enum Alliance {
    Red,
    Blue,
}

pub enum Station {
    One,
    Two,
    Three,
}

pub enum MiraType {
    Robot,
    Field,
}

// TODO
// Figure out what to do about Zach's weird tagging
pub struct EncodedAssembly {}

pub enum SceneObjectId {
    Remote(u32),
    Local(u32),
}
pub struct Body {
    body_id: BodyId,
    linear_velocity: Velocity,
    angular_velocity: Velocity,
    position: Position,
    rotation: Rotation,
}

pub struct MessageWithTimestamp {
    data: Message,
    timestamp: u64,
}

pub enum Message {
    Info(ClientInfo),
    Update(Vec<UpdateObjectData>),
}

pub struct ClientInfo {
    display_name: String,
    client_id: String,
    is_host: bool,
    creation_time: u64,
}

pub struct InitObjectData {
    scene_object_key: RemoteSceneObjectId,
    assembly: Option<EncodedAssembly>,
    assembly_hash: String,
    mira_type: MiraType,
    initial_preferences: MiraConfiguration,
    body_ids: Vec<BodyId>,
}

pub enum MiraConfiguration {
    Robot {
        intake_preferences: String,
        ejector_preferences: String,
        allance: Option<Alliance>,
        station: Option<Station>,
    },
    Field {
        preferences: String,
    },
}

pub struct ObjectPreferences {
    scene_object_key: RemoteSceneObjectId,
    object_configuration_data: MiraConfiguration,
}

pub struct AssemblyRequestData {
    scene_object_key: RemoteSceneObjectId,
    assembly_hash: String,
}

pub struct UpdateObjectData {
    scene_object_key: RemoteSceneObjectId,
    game_pieces_controlled: Vec<BodyId>,
    bodies: Vec<Body>,
}

pub struct CollisionData<T, B> {
    physics_system: T,
    scene_objects: HashMap<u32, B>,
}
