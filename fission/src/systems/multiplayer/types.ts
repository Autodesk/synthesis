import MirabufInstance from "@/mirabuf/MirabufInstance"
import Mechanism from "../physics/Mechanism"
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
    info:ClientInfo
    init:InitData
    update:UpdateObjectData[]
    collision: CollisionData
    newObject: InitObjectData
    robotLeft:RobotLeftData
    ping: PingData
    pong:PingData
}

export type Message = {[K in keyof MessageType]: {type:K, data:MessageType[K]}}[keyof MessageType]


export type ClientInfo = {
    displayName: string
    clientId: string
    isHost: boolean
}

// TODO: Figure out if InitMultiplayerObjectData is still necessary
export type InitData = {
    physicsSystem: PhysicsSystem
    objects: InitObjectData[] // We need to send the entire scene object with rendering data and configuration (for fields and such)
}

export type UpdateObjectData = {
    sceneObjectKey: number
    mechanism: Mechanism
    instance: MirabufInstance
}

export type InitObjectData = {
    key: number // TODO Check if we actually have to sync up keys (i think it's best if we do)
    sceneObject: MirabufSceneObject
}

export type CollisionData = {
    physicsSystem: PhysicsSystem
    sceneObjects: Map<number, MirabufSceneObject>
}

export type RobotLeftData = {
    sceneObjectKey: number
}

export type PingData = { timestamp: number }
