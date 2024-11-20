import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver, { DriverType } from "@/systems/simulation/driver/Driver"
import { NoraType, NoraTypes } from "@/systems/simulation/Nora"
import Stimulus, { StimulusType } from "@/systems/simulation/stimulus/Stimulus"
import { simMap, SimType } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import { Random } from "@/util/Random"
import { XYPosition } from "@xyflow/react"

export const NODE_ID_ROBOT_IO = "robot-io-node"
export const NODE_ID_SIM_OUT = "sim-output-node"
export const NODE_ID_SIM_IN = "sim-input-node"

export const ROBOT_IN_PREFIX = "robotIn"
export const ROBOT_OUT_PREFIX = "robotOut"
export const SIM_IN_PREFIX = "simIn"
export const SIM_OUT_PREFIX = "simOut"
export const JUNCTION_IN_PREFIX = "junctIn"
export const JUNCTION_OUT_PREFIX = "junctOut"

export type ConfigState = "wiring" | "simIO" | "robotIO"

type HandleType = SimType | StimulusType | DriverType

export type ConfigItemInfo = {
    id: string,
    displayName: string,
    enabled: boolean,
    type: HandleType
}

// Maps source types to target types.
const TYPE_COMPATABILITIES: { [source in HandleType]: HandleType[] } = {
    PWM: [
        DriverType.Driv_Hinge,
        DriverType.Driv_Slider,
        DriverType.Driv_Wheel,
    ],
    SimDevice: [],
    CANMotor: [
        DriverType.Driv_Hinge,
        DriverType.Driv_Slider,
        DriverType.Driv_Wheel,
    ],
    Solenoid: [
        DriverType.Driv_Slider,
    ],
    CANEncoder: [],
    Gyro: [],
    Accel: [],
    DIO: [],
    AI: [],
    AO: [
        DriverType.Driv_Hinge,
        DriverType.Driv_Slider,
        DriverType.Driv_Wheel,
    ],
    Stim_ChassisAccel: [
        SimType.Accel,
    ],
    Stim_Encoder: [
        SimType.CANEncoder,
        DriverType.Driv_Hinge,
        DriverType.Driv_Slider,
        DriverType.Driv_Wheel,
    ],
    Stim_Unknown: [],
    Driv_Hinge: [],
    Driv_Wheel: [],
    Driv_Slider: [],
    Driv_Unknown: [],
}

const TYPE_DIRECTIONS: { [type in HandleType]: "source" | "target" } = {
    PWM: "source",
    SimDevice: "source",
    CANMotor: "source",
    Solenoid: "source",
    CANEncoder: "target",
    Gyro: "target",
    Accel: "target",
    DIO: "target", // Uhhhhhh
    AI: "target",
    AO: "source",
    Stim_ChassisAccel: "source",
    Stim_Encoder: "source",
    Stim_Unknown: "source",
    Driv_Hinge: "target",
    Driv_Wheel: "target",
    Driv_Slider: "target",
    Driv_Unknown: "target"
}

export const configItemInfoCompare: (a: [string, ConfigItemInfo], b: [string, ConfigItemInfo]) => number
    = ([_aK, aV], [_bK, bV]) => aV.displayName.localeCompare(bV.displayName)

type SourceHandle = {
    nodeId: string
    isSource: true
    noraType: NoraTypes
    handleType: HandleType
    supplierId: string
}

type TargetHandle = {
    nodeId: string
    isSource: false
    noraType: NoraTypes
    handleType: HandleType
}

export type JunctionInfo = {
    inHandle: string,
    outHandle: string,
    position: XYPosition,
    sourceType?: HandleType,
    targetType?: HandleType
}

export type FlowControlsProps = {
    onCreateJunction?: () => void,
}

export function getDriverSignals(assembly: MirabufSceneObject): Driver[] {
    const simLayer = World.SimulationSystem.GetSimulationLayer(assembly.mechanism)
    return simLayer?.drivers ?? []
}

export function getStimulusSignals(assembly: MirabufSceneObject): Stimulus[] {
    const simLayer = World.SimulationSystem.GetSimulationLayer(assembly.mechanism)
    return simLayer?.stimuli ?? []
}

export function getCANMotors(): [string, Map<string, number | boolean | string>][] {
    const cans = simMap.get(SimType.CANMotor) ?? new Map<string, Map<string, number>>()
    return [...cans.entries()].filter(([_, data]) => data.get("<init")).reverse()
}

export function getCANEncoder(): [string, Map<string, string | boolean | number>][] {
    return [...(simMap.get(SimType.CANEncoder)?.entries() ?? [])]
}

export function getPWMDevices(): [string, Map<string, string | boolean | number>][] {
    const pwms = simMap.get(SimType.PWM)
    if (pwms) {
        return [...pwms.entries()].filter(([_, data]) => data.get("<init"))
    }
    return []
}

export function getAccelDevices(): [string, Map<string, string | boolean | number>][] {
    return [...(simMap.get(SimType.Accel)?.entries() ?? [])]
}

function displayNameCAN(id: string) {
    const a = id.indexOf('[')
    const b = id.indexOf(']')
    if (a === -1 || b === -1 || b - a < 2)
        return id
    return `CAN [${id.substring(a + 1, b)}]`
}

function displayNamePWM(id: string) {
    return `PWM [${id}]`
}

function displayNameAccel(id: string) {
    if (id.startsWith("BuiltIn")) {
        return "Accel [Built In]"
    } else {
        const a = id.indexOf('[')
        const b = id.indexOf(']')
        if (a === -1 || b === -1 || b - a < 2)
            return id
        return `Accel [${id.substring(0, a)} - ${id.substring(a + 1, b)}]`
    }
}

function decodeHandleId(id: string): DataHandle | JunctionHandle | undefined {
    const parts = id.split(":")
    if (parts.length < 2) {
        return undefined
    }
    
    switch (parts[0]) {
        case SIM_IN_PREFIX:
        case SIM_OUT_PREFIX:
        case ROBOT_IN_PREFIX:
        case ROBOT_OUT_PREFIX:
            return { id: parts[2], type: parts[1] as HandleType, origin: parts[0], isJunction: false, dir: TYPE_DIRECTIONS[parts[1] as HandleType] }
        case JUNCTION_IN_PREFIX:
        case JUNCTION_OUT_PREFIX:
            return { id: parts[1], dir: parts[0] == JUNCTION_IN_PREFIX ? "target" : "source", isJunction: true }
    }
    return undefined
}

export type SimConfigData = {
    simOut: Map<string, ConfigItemInfo>
    simIn: Map<string, ConfigItemInfo>
    robotIn: Map<string, ConfigItemInfo>
    robotOut: Map<string, ConfigItemInfo>

    junctions: Map<string, JunctionInfo>

    connections: Map<string, string>

    simOutPosition: XYPosition
    simInPosition: XYPosition
    robotIOPosition: XYPosition
}

function genJunctionId(config: SimConfigData): string {
    const id = (Random() * 10000).toFixed()
    return config.junctions.has(id) ? genJunctionId(config) : id
}

export class SimConfig {
    private constructor() { }

    public static Default(assembly: MirabufSceneObject) {
        const config: SimConfigData = {
            simOut: new Map(),
            simIn: new Map(),
            robotIn: new Map(),
            robotOut: new Map(),

            junctions: new Map(),

            connections: new Map(),

            simInPosition: { x: 800, y: 0 },
            simOutPosition: { x: -800, y: 0 },
            robotIOPosition: { x: 0, y: 0 },
        }
        getDriverSignals(assembly).forEach(x => {
            if (x.info?.GUID)
                config.simIn.set(`${SIM_IN_PREFIX}:${x.id.type}:${x.info.GUID}`, { id: x.info.GUID, displayName: x.DisplayName(), enabled: true, type: x.id.type })
        })
        getStimulusSignals(assembly).forEach(x => {
            if (x.info?.GUID)
                config.simOut.set(`${SIM_OUT_PREFIX}:${x.id.type}:${x.info.GUID}`, { id: x.info.GUID, displayName: x.DisplayName(), enabled: true, type: x.id.type })
        })
        getCANMotors().forEach(([id, _]) => {
            config.robotOut.set(`${ROBOT_OUT_PREFIX}:${SimType.CANMotor}:${id}`, { id: id, displayName: displayNameCAN(id), enabled: true, type: SimType.CANMotor })
        })
        getCANEncoder().forEach(([id, _]) => {
            config.robotIn.set(`${ROBOT_IN_PREFIX}:${SimType.CANEncoder}:${id}`, { id: id, displayName: displayNameCAN(id), enabled: true, type: SimType.CANEncoder })
        })
        getPWMDevices().forEach(([id, _]) => {
            config.robotOut.set(`${ROBOT_OUT_PREFIX}:${SimType.PWM}:${id}`, { id: id, displayName: displayNamePWM(id), enabled: true, type: SimType.PWM })
        })
        getAccelDevices().forEach(([id, data]) => {
            config.robotIn.set(`${ROBOT_IN_PREFIX}:${SimType.Accel}:${id}`, { id: id, displayName: displayNameAccel(id), enabled: data.get("<init") == true, type: SimType.Accel })
        })

        return config
    }

    public static MakeJunction(config: SimConfigData): string {
        const id = genJunctionId(config)
        config.junctions.set(id, {
            outHandle: `${JUNCTION_OUT_PREFIX}:${id}`,
            inHandle: `${JUNCTION_IN_PREFIX}:${id}`,
            position: { x: 0, y: 0 },
        })
        return id
    }

    public static DeleteJunction(config: SimConfigData, id: string): boolean {
        // TODO: Handle connections to junction.
        return config.junctions.delete(id)
    }

    private static determineHandleType(config: SimConfigData, handle: DataHandle | JunctionHandle): HandleType | undefined {
        if (!handle.isJunction)
            return handle.type;

        const junct = config.junctions.get(handle.id)!
        return handle.dir == "source" ? junct?.sourceType : junct?.targetType
    }

    private static validateGraph(config: SimConfigData, source: string, target: string): boolean {
        const validatedSources = new Set<string>()
        const checkList: string[] = [...config.simOut.keys(), ...config.robotOut.keys()]
        while (checkList.length > 0) {
            const sourceId = checkList.pop()
        }
    }

    private static validateEdge(config: SimConfigData, source: string, target: string): boolean {
        const sourceHandle = decodeHandleId(source)
        const targetHandle = decodeHandleId(target)
        if (sourceHandle == undefined || targetHandle == undefined)
            return false

        if (sourceHandle.dir == targetHandle.dir)
            return false

        const sourceHandleType = this.determineHandleType(config, sourceHandle)
        const targetHandleType = this.determineHandleType(config, targetHandle)
        if (sourceHandleType == undefined || targetHandleType == undefined) {
            console.debug("Two empty junctions connected to eachother,")
        }
    }

    public static MakeEdge(config: SimConfigData, source: string, target: string): boolean {
        if (!this.validateEdge(config, source, target))
            return false

        config.connections.push({ source: source, target: target })
        return true
    }
}
