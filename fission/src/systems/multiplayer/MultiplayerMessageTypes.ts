import type { MatchModeConfig } from "@/panels/configuring/MatchModeConfigPanel.tsx"

import type { MiraType } from "@/mirabuf/MirabufLoader.ts"
import type {
    ClientInfo,
    EncodedAssembly,
    FieldConfiguration,
    RobotConfiguration,
} from "@/systems/multiplayer/MultiplayerTypes.ts"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

export interface MessageType {
    info: InfoBody
    update: UpdateBody
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
export type MatchModeStateBody =
    | {
          event: "start"
          config: MatchModeConfig
          moveRobots: boolean
      }
    | { event: "cancel" }

export type NewObjectBody = {
    sceneObjectKey: SceneObjectId
    assembly?: EncodedAssembly
    assemblyHash: string
    miraType: MiraType
    initialPreferences: RobotConfiguration | FieldConfiguration
    bodyIds: number[] // Jolt.BodyID.GetSequenceAndIndexNumber() (used for creating the bodyMap)
}

export type ConfigureObjectBody = {
    sceneObjectKey: SceneObjectId
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type NeedAssemblyBody = {
    sceneObjectKey: SceneObjectId
    assemblyHash: string
}

export type UpdateBody = UpdateObjectData[]
export type CollisionBody = UpdateObjectData[]
export type UpdateObjectData = {
    sceneObjectKey: SceneObjectId
    gamePiecesControlled: number[] // BodyID
    // {x, y, z, w?}
    bodies: {
        bodyId: number // BodyID
        linearVelocityStr: string
        angularVelocityStr: string
        positionStr: string
        rotationStr: string
    }[]
}

export type LatencyInfoBody = { latencyMS: number }
