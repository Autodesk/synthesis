import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type Driver from "@/systems/simulation/driver/Driver"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import World from "@/systems/World"
import type { SimType } from "../wpilib_brain/WPILibTypes"

export function getDriverSignals(assembly: MirabufSceneObject): Driver[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.drivers ?? []
}

export function getStimulusSignals(assembly: MirabufSceneObject): Stimulus[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.stimuli ?? []
}

export function getSimDevices(
    type: SimType,
    requireInit: boolean = true
): [string, Map<string, number | boolean | string>][] {
    const devices = getSimMap()?.get(type) ?? new Map<string, Map<string, number>>()
    return [...devices.entries()].filter(([_, data]) => data.get("<init") || !requireInit).reverse()
}

export function displayNameCAN(id: string) {
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `CAN [${id.substring(a + 1, b)}]`
}

export function displayNamePWM(id: string) {
    return `PWM [${id}]`
}

export function displayNameSensor(id: string, label: string) {
    if (id.startsWith("BuiltIn")) {
        return `${label} [Built In]`
    }
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `${label} [${id.substring(0, a)} - ${id.substring(a + 1, b)}]`
}

// TODO
// function displayNameDI(id: string) {
//     return `DI [${id}]`
// }

// function displayNameDO(id: string) {
//     return `DO [${id}]`
// }
