import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { NoraTypes } from "../Nora"
import type WPILibBrain from "./WPILibBrain"
import { type SimMap, SimType, worker } from "./WPILibTypes"

export const simMaps = new Map<string, SimMap>()

let simBrain: WPILibBrain | undefined
export function setSimBrain(brain: WPILibBrain | undefined) {
    if (brain && !simMaps.has(brain.assemblyName)) {
        simMaps.set(brain.assemblyName, new Map())
    }
    if (simBrain) worker.getValue().postMessage({ command: "disable" })
    simBrain = brain
    if (simBrain)
        worker.getValue().postMessage({
            command: "enable",
            reconnect: PreferencesSystem.getGlobalPreference("SimAutoReconnect"),
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
    return simMaps.get(simBrain.assemblyName)
}

let isConnected: boolean = false

export function setConnected(connected: boolean) {
    isConnected = connected
}

export function getIsConnected() {
    return isConnected
}

export const supplierTypeMap: { [k in SimType]: NoraTypes | undefined } = {
    [SimType.PWM]: NoraTypes.NUMBER,
    [SimType.SIM_DEVICE]: undefined,
    [SimType.CAN_MOTOR]: NoraTypes.NUMBER,
    [SimType.SOLENOID]: NoraTypes.NUMBER,
    [SimType.CAN_ENCODER]: undefined,
    [SimType.GYRO]: undefined,
    [SimType.ACCELEROMETER]: undefined,
    [SimType.DIO]: NoraTypes.NUMBER, // ?
    [SimType.AI]: undefined,
    [SimType.AO]: NoraTypes.NUMBER,
    [SimType.DRIVERS_STATION]: undefined,
}

export const receiverTypeMap: { [k in SimType]: NoraTypes | undefined } = {
    [SimType.PWM]: undefined,
    [SimType.SIM_DEVICE]: undefined,
    [SimType.CAN_MOTOR]: undefined,
    [SimType.SOLENOID]: undefined,
    [SimType.CAN_ENCODER]: NoraTypes.NUMBER2,
    [SimType.GYRO]: NoraTypes.NUMBER3, // Wrong but its fine
    [SimType.ACCELEROMETER]: NoraTypes.NUMBER3,
    [SimType.DIO]: NoraTypes.NUMBER, // ?
    [SimType.AI]: NoraTypes.NUMBER,
    [SimType.AO]: undefined,
    [SimType.DRIVERS_STATION]: undefined,
}
