import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type WPILibBrain from "./WPILibBrain"
import { FTC_WS_URL, type SimMap, WPILIB_WS_URL, worker } from "./WPILibTypes"

export const simMaps = new Map<string, SimMap>()

let simBrain: WPILibBrain | undefined
export function setSimBrain(brain: WPILibBrain | undefined) {
    if (brain && !simMaps.has(brain.assemblyId)) {
        simMaps.set(brain.assemblyId, new Map())
    }
    if (simBrain) worker.getValue().postMessage({ command: "disable" })
    simBrain = brain
    if (simBrain)
        worker.getValue().postMessage({
            command: "enable",
            url: simBrain.brainType === "ftc" ? FTC_WS_URL : WPILIB_WS_URL,
            reconnect: PreferencesSystem.getUserPreference("SimAutoReconnect"),
        })
}

export function getSimBrain() {
    return simBrain
}

export function hasSimBrain() {
    return simBrain != undefined
}

export function getSimMap(): SimMap | undefined {
    if (!simBrain) return undefined
    return simMaps.get(simBrain.assemblyId)
}

let isConnected: boolean = false

export function setConnected(connected: boolean) {
    isConnected = connected
}

export function getIsConnected() {
    return isConnected
}
