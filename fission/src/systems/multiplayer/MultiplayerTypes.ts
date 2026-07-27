import type {Alliance, FieldPreferences, RobotPreferences, Station} from "@/systems/preferences/PreferenceTypes.ts"
import type { MessageType } from "@/systems/multiplayer/MultiplayerMessageTypes.ts"

export type MessageWithTimestamp = {
    [K in keyof MessageType]: {
        recipientId?: string
        client_id: string
        type: K
        data: MessageType[K]
        timestamp: number
    }
}[keyof MessageType]
export type Message = Omit<MessageWithTimestamp, "timestamp" | "client_id"> & Partial<MessageWithTimestamp>

export type EncodedAssembly = Uint8Array & { __: "encodedassembly" }
export type RemoteSceneObjectId = number & { __: "remotesceneobject" | "sceneobjectkey" }
export type LocalSceneObjectId = number & { __: "localsceneobject" | "sceneobjectkey" }

export type ClientInfo = {
    displayName: string
    clientId: string
    creationTime: number
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
