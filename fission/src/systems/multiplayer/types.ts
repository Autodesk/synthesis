import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes.ts"
import type PhysicsSystem from "../physics/PhysicsSystem"

export type Metrics = {
    startTime: number
    totalFrames: number
    frameTimes: number[]
    inputsSent: number
    messagesReceived: number
    bytesReceived: number
    bytesSent: number
    connectionTime: number
    averageFPS: number
    networkStats: {
        packetsLost: number
        roundTripTimes: number[]
        jitter: number
    }
}

interface MessageType {
    info: ClientInfo
    init: InitData
    update: UpdateObjectData[]
    metadataUpdate: MetadataUpdateData
    collision: UpdateObjectData[] // just a comprehensive list instead
    newObject: InitObjectData
    needAssembly: AssemblyRequestData
    deleteObject: number // sceneObjectKey
    configureObject: ObjectPreferences // sceneObjectKey
    robotLeft: RobotLeftData
    ping: PingData
    pong: PingData
}

export type Message = { [K in keyof MessageType]: { type: K; data: MessageType[K] } }[keyof MessageType]

// biome-ignore lint: We're using this for type safety
export type EncodedAssembly = Uint8Array & { __: "encodedassembly" }
export type EncodedRootBody = string

export type ClientInfo = {
    displayName: string
    clientId: string
    isHost: boolean
    creationTime: number
}

export type InitObjectData = {
    sceneObjectKey: number
    assembly?: EncodedAssembly
    assemblyHash: string
    initialPreferences: RobotConfiguration | FieldConfiguration
}

export type RobotConfiguration = {
    intakePreferences: string // IntakePreferences
    ejectorPreferences: string // EjectorPreferences
}
export type FieldConfiguration = {
    fieldPreferences: string // FieldPreferences
}
export type ObjectPreferences = {
    sceneObjectKey: number
    objectConfigurationData: RobotConfiguration | FieldConfiguration
}

export type AssemblyRequestData = {
    sceneObjectKey: number
    assemblyHash: string
}

export type MetadataUpdateData = {
    sceneObjectKey: number
    alliance?: Alliance
    station?: Station
}

// TODO: Figure out if InitMultiplayerObjectData is still necessary
export type InitData = {
    physicsSystem: PhysicsSystem
    objects: EncodedAssembly[] // We need to send the entire scene object with rendering data and configuration (for fields and such)
}

export type UpdateObjectData = {
    sceneObjectKey: number
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

export type RobotLeftData = {
    sceneObjectKey: number
}

export type PingData = { timestamp: number }
