import PhysicsSystem from "../physics/PhysicsSystem"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

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
    collision: CollisionData
    newObject: InitObjectData
    robotLeft: RobotLeftData
    ping: PingData
    pong: PingData
}

// biome-ignore lint: We're using this for type safety
export type EncodedAssembly = Uint8Array & { __: "" }
export type EncodedRootBody = string

export type Message = { [K in keyof MessageType]: { type: K; data: MessageType[K] } }[keyof MessageType]

export type ClientInfo = {
    displayName: string
    clientId: string
    isHost: boolean
    creationTime: number
}

export type InitObjectData = {
    sceneObjectKey: number
    assembly: EncodedAssembly
}

// TODO: Figure out if InitMultiplayerObjectData is still necessary
export type InitData = {
    physicsSystem: PhysicsSystem
    objects: EncodedAssembly[] // We need to send the entire scene object with rendering data and configuration (for fields and such)
}

export type UpdateObjectData = {
    sceneObjectKey: number
    // {x, y, z, w?}
    linearVelocityStr: string
    angularVelocityStr: string
    positionStr: string
    rotationStr: string
}

export type CollisionData = {
    physicsSystem: PhysicsSystem
    sceneObjects: Map<number, MirabufSceneObject>
}

export type RobotLeftData = {
    sceneObjectKey: number
}

export type PingData = { timestamp: number }
