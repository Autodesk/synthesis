import type { MiraType } from "@/mirabuf/MirabufLoader.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { MatchModeConfig } from "@/panels/configuring/MatchModeConfigPanel.tsx"
import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes.ts"
import type PhysicsSystem from "../physics/PhysicsSystem"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

export interface MessageType {
    info: ClientInfo
    update: UpdateObjectData[]
    collision: UpdateObjectData[] // just a comprehensive list instead
    newObject: InitObjectData
    needAssembly: AssemblyRequestData
    deleteObject: SceneObjectId // sceneObjectKey
    configureObject: ObjectPreferences // sceneObjectKey
    disableObjectPhysics: SceneObjectId // sceneObjectKey
    enableObjectPhysics: SceneObjectId // sceneObjectKey
    ping: PingData
    pong: PingData
    matchModeState: MatchModeStateData
    matchModePenalty: MatchModePenalty
}

export interface MatchModePenalty {
    objectId: SceneObjectId
    points: number
    description: string
}
export type MatchModeStateData =
    | {
          event: "start"
          config: MatchModeConfig
          moveRobots: boolean
      }
    | { event: "cancel" }

export type MessageWithTimestamp = {
    [K in keyof MessageType]: { type: K; data: MessageType[K]; timestamp: number }
}[keyof MessageType]
export type Message = Omit<MessageWithTimestamp, "timestamp"> & Partial<Pick<MessageWithTimestamp, "timestamp">>

// biome-ignore-start lint/style/useNamingConvention: used for type safety
export type EncodedAssembly = Uint8Array & { __: "encodedassembly" }
// biome-ignore-end lint/style/useNamingConvention: used for type safety

export type ClientInfo = {
    displayName: string
    clientId: string
    isHost: boolean
    creationTime: number
}

export type InitObjectData = {
    sceneObjectKey: SceneObjectId
    assembly?: EncodedAssembly
    assemblyHash: string
    miraType: MiraType
    initialPreferences: RobotConfiguration | FieldConfiguration
    bodyIds: number[] // Jolt.BodyID.GetSequenceAndIndexNumber() (used for creating the bodyMap)
}

export type RobotConfiguration = {
    intakePreferences: string // IntakePreferences
    ejectorPreferences: string // EjectorPreferences
    alliance?: Alliance
    station?: Station
}
export type FieldConfiguration = {
    fieldPreferences: string // FieldPreferences
}
export type ObjectPreferences = {
    sceneObjectKey: SceneObjectId
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type AssemblyRequestData = {
    sceneObjectKey: SceneObjectId
    assemblyHash: string
}

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

export type CollisionData = {
    physicsSystem: PhysicsSystem
    sceneObjects: Map<number, MirabufSceneObject>
}

export type PingData = { timestamp: number }
