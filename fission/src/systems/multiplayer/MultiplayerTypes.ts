import type { Alliance, FieldPreferences, RobotPreferences, Station } from "@/systems/preferences/PreferenceTypes.ts"
import type { MessageType } from "@/systems/multiplayer/MultiplayerMessageTypes.ts"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"

export type MessageWithTimestamp = {
    [K in keyof MessageType]: {
        recipientId?: string
        clientId: string
        type: K
        data: MessageType[K]
        timestamp: number
    }
}[keyof MessageType]
export type Message = Omit<MessageWithTimestamp, "timestamp" | "clientId"> & Partial<MessageWithTimestamp>

// biome-ignore-start lint/style/useNamingConvention: Otherwise types are same
export type EncodedAssembly = Uint8Array & { __: "encodedassembly" }
// biome-ignore-end lint/style/useNamingConvention: Otherwise types are same

export type ClientInfo = {
    displayName: string
    clientId: string
    creationTime: number
}

export type ClientAndLatencyInfo = ClientInfo & {
    latency?: number
    lastUpdateTime?: number
}

export function shortClientId(info: ClientInfo) {
    return info.clientId.slice(0, 8)
}

export type RobotConfiguration = {
    robotPreferences: RobotPreferences // EjectorPreferences
    alliance?: Alliance
    station?: Station
}
export type FieldConfiguration = {
    fieldPreferences: FieldPreferences // FieldPreferences
}

type MappedMessageType<T> = {
    [K in keyof T]: T[K] extends never ? { event: K; data?: undefined } : { event: K; data: T[K] }
}[keyof T]

export type FromWorkerMessage = MappedMessageType<FromWorkerMessages>
export type ToWorkerMessage = MappedMessageType<ToWorkerMessages>

interface FromWorkerMessages {
    peerMessage: MessageWithTimestamp
    serverMessage: ServerToClientMessage
    open: never
    close: never
    error: string
}

export interface ToWorkerMessages {
    connect: { url: string }
    disconnect: never
    peerMessage: MessageWithTimestamp
    serverMessage: ClientToServerMessage
    initialize: { roomId: string | null; displayName: string }
}
