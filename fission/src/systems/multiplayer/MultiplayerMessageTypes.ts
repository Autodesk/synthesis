import type { MatchModeConfig } from "@/panels/configuring/MatchModeConfigPanel.tsx"

import type { MiraType } from "@/mirabuf/MirabufLoader.ts"
import type {
    ClientInfo,
    EncodedAssembly,
    FieldConfiguration,
    RemoteSceneObjectId,
    RobotConfiguration,
} from "@/systems/multiplayer/MultiplayerTypes.ts"

export interface MessageType {
    info: InfoBody
    update: UpdateBody
    collision: CollisionBody // just a comprehensive list instead
    newObject: NewObjectBody
    needAssembly: NeedAssemblyBody
    deleteObject: RemoteSceneObjectId // sceneObjectKey
    configureObject: ConfigureObjectBody // sceneObjectKey
    disableObjectPhysics: RemoteSceneObjectId // sceneObjectKey
    enableObjectPhysics: RemoteSceneObjectId // sceneObjectKey
    ping: PingData
    pong: PingData
    matchModeState: MatchModeStateBody
    matchModePenalty: MatchModePenaltyBody
}

export interface InfoBody {
    info: ClientInfo
    introduceSelf: boolean
}

export interface MatchModePenaltyBody {
    objectId: RemoteSceneObjectId
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
    sceneObjectKey: RemoteSceneObjectId
    assembly?: EncodedAssembly
    assemblyHash: string
    miraType: MiraType
    initialPreferences: RobotConfiguration | FieldConfiguration
    bodyIds: number[] // Jolt.BodyID.GetSequenceAndIndexNumber() (used for creating the bodyMap)
}

export type ConfigureObjectBody = {
    sceneObjectKey: RemoteSceneObjectId
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type NeedAssemblyBody = {
    sceneObjectKey: RemoteSceneObjectId
    assemblyHash: string
}

export type UpdateBody = UpdateObjectData[]
export type CollisionBody = UpdateObjectData[]
export type UpdateObjectData = {
    sceneObjectKey: RemoteSceneObjectId
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

export type PingData = { timestamp: number }
