import type FTCBrain from "./FTCBrain"
import { type SimMap, worker } from "./FTCTypes"

export const simMaps = new Map<string, SimMap>()

let simBrain: FTCBrain | undefined
export function setSimBrain(brain: FTCBrain | undefined) {
    if (brain && !simMaps.has(brain.assemblyId)) {
        simMaps.set(brain.assemblyId, new Map())
    }
    if (simBrain) worker.getValue().postMessage({ command: "disable" })
    simBrain = brain
    if (simBrain) worker.getValue().postMessage({ command: "enable", reconnect: true })
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
