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
}

export type ConfigureObjectBody = {
    sceneObjectId: SceneObjectId
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type NeedAssemblyBody = {
    sceneObjectId: SceneObjectId
    assemblyHash: string
}

export type UpdateBody = UpdateObjectData[]
export type CollisionBody = UpdateObjectData[]
export type UpdateObjectData = {
    sceneObjectKey: SceneObjectId
    gamePiecesControlled: RigidNodeId[] // rnIds within the field, since there's only one
    bodies: PhysicsBodyData[]
}

export type UpdatePhysicsBodyData = {
    sceneObjectId: SceneObjectId
} & PhysicsBodyData

export type PhysicsBodyData = {
    rigidNodeId: RigidNodeId // rnIds are relative to their scene object, so be sure to send the id for that too
    // {x, y, z, w?}
    linearVelocityStr: string
    angularVelocityStr: string
    positionStr: string
    rotationStr: string
}

export type LatencyInfoBody = { latencyMS: number }
