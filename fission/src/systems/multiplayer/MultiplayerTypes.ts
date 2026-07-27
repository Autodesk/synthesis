import type { Alliance, FieldPreferences, RobotPreferences, Station } from "@/systems/preferences/PreferenceTypes.ts"
import type { MessageType } from "@/systems/multiplayer/MultiplayerMessageTypes.ts"

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
export type RemoteSceneObjectId = number & { __: "remotesceneobject" | "sceneobjectkey" }
export type LocalSceneObjectId = number & { __: "localsceneobject" | "sceneobjectkey" }
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
