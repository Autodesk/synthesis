import type { MatchModeConfig } from "@/panels/configuring/MatchModeConfigPanel.tsx"
import type { MiraType } from "@/mirabuf/MirabufLoader.ts"
import type {
    ClientInfo,
    EncodedAssembly,
    FieldConfiguration,
    RobotConfiguration,
} from "@/systems/multiplayer/MultiplayerTypes.ts"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"

export interface MessageType {
    info: InfoBody
    update: UpdateBody
    /**
     * Used for sending updates of specific physics bodies
     * Important for dragging pieces because no robots touch them and no game piece asset support
     *
     * WARNING:
     * When game piece asset support is merged, this should be reverted to use `UpdateObjectData`
     */
    updatePhysicsBody: UpdatePhysicsBodyData
    collision: CollisionBody // just a comprehensive list instead
    newObject: NewObjectBody
    needAssembly: NeedAssemblyBody
    deleteObject: SceneObjectId
    configureObject: ConfigureObjectBody
    disableObjectPhysics: SceneObjectId
    enableObjectPhysics: SceneObjectId
    latencyInfo: LatencyInfoBody
    matchModeState: MatchModeStateBody
    matchModePenalty: MatchModePenaltyBody
}

export interface InfoBody {
    info: ClientInfo
    introduceSelf: boolean
}

export interface MatchModePenaltyBody {
    objectId: SceneObjectId
    points: number
    description: string
}

// Maybe ongoing and start should just be the same state?
export type MatchModeStateBody =
    | {
          event: "start"
          config: MatchModeConfig
          moveRobots: boolean
          startTime: number
      }
    | {
          event: "ongoing"
          config: MatchModeConfig
          startTime: number
      }
    | { event: "cancel" }

export type NewObjectBody = {
    sceneObjectId: SceneObjectId
    assembly?: EncodedAssembly
    assemblyHash: string
    miraType: MiraType
    initialPreferences: RobotConfiguration | FieldConfiguration
    // It's only important that you set this property if you're spawning a field.
    // This is because all robot physics updates are sent every time the physics system updates
    // While updates to game piece positions (when moved via drag mode) are sent out only when they're moved
    initialPhysicsData?: PhysicsBodyData[]
}

export type ConfigureObjectBody = {
    sceneObjectId: SceneObjectId
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type NeedAssemblyBody = {
    sceneObjectId: SceneObjectId
    assemblyHash: string
}

/**
 * The part of a robot's state that isn't attached to any one body. Its bodies
 * travel separately, as one `updatePhysicsBody` per body.
 */
export type UpdateBody = {
    objId: SceneObjectId
    gamePiecesControlled: RigidNodeId[] // rnIds within the field, since there's only one
}

export type CollisionBody = UpdateObjectData[]
export type UpdateObjectData = {
    sceneObjectKey: SceneObjectId
    gamePiecesControlled: RigidNodeId[] // rnIds within the field, since there's only one
    bodies: PhysicsBodyData[]
}

/**
 * One body's state, addressed by the scene object owning it.
 *
 * Each of these is sent as its own message so it fits in a datagram, and so that
 * losing one costs a single body for a single tick.
 */
export type UpdatePhysicsBodyData = [objId: SceneObjectId, body: PhysicsBodyData]

/**
 * One body's physics state.
 *
 * Packed as a tuple rather than an object: these go out per physics tick, and the
 * field names cost more on the wire than the numbers they label. The order below
 * *is* the wire schema, so a change here has to land on both the producer
 * (`PhysicsSystem`) and the consumer (`UpdatePhysicsData`) at once.
 */
export type PhysicsBodyData = [
    // rnIds are relative to their scene object, so be sure to send the id for that too
    rnId: RigidNodeId,
    linVel: [x: number, y: number, z: number],
    rotVel: [x: number, y: number, z: number],
    pos: [x: number, y: number, z: number],
    rot: [x: number, y: number, z: number, w: number],
]

export type LatencyInfoBody = { latencyMS: number }
