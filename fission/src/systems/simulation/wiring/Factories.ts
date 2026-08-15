import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type Driver from "../driver/Driver"
import World from "@/systems/World"
import type Stimulus from "../stimulus/Stimulus"
import { SimType } from "../wpilib_brain/WPILibTypes"
import { getSimMap } from "../wpilib_brain/WPILibState"
import {
    addHandle,
    genNewId,
    type HandleInfo,
    handlesOf,
    type NodeIdAlias,
    NodeKindId,
    removeHandle,
    type SimConfigData,
    HandleIdAlias,
} from "./SimGraph"
import { NODE_ID_ROBOT_IO } from "./nodes/RobotIONode"
import { NODE_ID_SIM_IN } from "./nodes/SimInputNode"
import { NODE_ID_SIM_OUT } from "./nodes/SimOutputNode"
import type { NoraType } from "../Nora"
import { CAN_MOTOR_TYPE } from "../wpilib_brain/sim/SimCANMotor"
import { PWM_TYPE } from "../wpilib_brain/sim/SimPWM"
import { CAN_ENCODER_TYPE } from "../wpilib_brain/sim/SimCANEncoder"
import { ACCEL_TYPE } from "../stimulus/AccelStimulus"
import { GYRO_TYPE } from "../stimulus/GyroStimulus"
import { random } from "@/util/Random"
import { AggregateStrategy } from "../wpilib_brain/SimDataFlow"
import { XYPosition } from "@xyflow/react"

export function getDriverSignals(assembly: MirabufSceneObject): Driver[] {
    return World.simulationSystem.getSimulationLayer(assembly.mechanism)?.drivers ?? []
}

export function getStimulusSignals(assembly: MirabufSceneObject): Stimulus[] {
    return World.simulationSystem.getSimulationLayer(assembly.mechanism)?.stimuli ?? []
}

export function getSimDevices(
    type: SimType,
    requireInit: boolean = true
): [string, Map<string, number | boolean | string>][] {
    const devices = getSimMap()?.get(type) ?? new Map<string, Map<string, number>>()
    return [...devices.entries()].filter(([_, data]) => data.get("<init") || !requireInit).reverse()
}

export const displayNameCAN = (id: string) => {
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `CAN [${id.substring(a + 1, b)}]`
}

export const displayNamePWM = (id: string) => `PWM [${id}]`

export const displayNameSensor = (id: string, label: string) => {
    if (id.startsWith("BuiltIn")) return `${label} [Built In]`
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `${label} [${id.substring(0, a)} - ${id.substring(a + 1, b)}]`
}

export function defaultConfig(assembly: MirabufSceneObject): SimConfigData {
    const config: SimConfigData = { nodes: {}, handles: {}, edges: {} }

    config.nodes[NODE_ID_ROBOT_IO] = {
        id: NODE_ID_ROBOT_IO,
        kind: NodeKindId.ROBOT_IO,
        position: { x: 0, y: 0 },
        tooltip:
            "These handles represent the different devices we've discovered from your connected robot code. The left handles represent input devices such as sensors. The right handles represent output devices such as motor controllers.",
    }
    config.nodes[NODE_ID_SIM_IN] = {
        id: NODE_ID_SIM_IN,
        kind: NodeKindId.SIM_INPUT,
        position: { x: 800, y: 0 },
        tooltip:
            "These handles represent the input of the simulation. These are drivers for wheels, hinges, and sliders.",
    }
    config.nodes[NODE_ID_SIM_OUT] = {
        id: NODE_ID_SIM_OUT,
        kind: NodeKindId.SIM_OUTPUT,
        position: { x: -800, y: 0 },
        tooltip:
            "These handles represent the output of the simulation. These are stimuli for wheels, hinges, and sliders that represent encoder positions and speeds.",
    }

    syncRobotIOHandles(config)
    syncSimIOHandles(config, assembly)
    return config
}

function syncNodeHandles(config: SimConfigData, nodeId: NodeIdAlias, desired: HandleInfo[]) {
    if (!config.nodes[nodeId]) return

    const desiredIds = new Set(desired.map(x => x.id))
    handlesOf(config, nodeId)
        .filter(x => !desiredIds.has(x.id))
        .forEach(x => removeHandle(config, x.id))

    desired.forEach(info => {
        const existing = config.handles[info.id]
        if (existing) {
            existing.noraType = info.noraType
            existing.displayName = info.displayName
            existing.many = info.many
        } else {
            addHandle(config, info)
        }
    })
}

const ROBOT_IO_DEVICES: [SimType, NoraType, (id: string) => string, boolean, boolean][] = [
    [SimType.CAN_MOTOR, CAN_MOTOR_TYPE, displayNameCAN, true, true],
    [SimType.PWM, PWM_TYPE, displayNamePWM, true, true],
    [SimType.CAN_ENCODER, CAN_ENCODER_TYPE, displayNameCAN, false, false],
    [SimType.ACCELEROMETER, ACCEL_TYPE, id => displayNameSensor(id, "Accel"), false, false],
    [SimType.GYRO, GYRO_TYPE, id => displayNameSensor(id, "Gyro"), false, false],
]

export function syncRobotIOHandles(config: SimConfigData) {
    const desired = ROBOT_IO_DEVICES.flatMap(([simType, noraType, displayName, isSource, requireInit]) =>
        getSimDevices(simType, requireInit).map(
            ([id, _]): HandleInfo => ({
                id: `${NODE_ID_ROBOT_IO}:${simType}:${id}`,
                nodeId: NODE_ID_ROBOT_IO,
                noraType,
                originType: simType,
                originId: id,
                displayName: displayName(id),
                enabled: true,
                many: true,
                isSource,
            })
        )
    )
    syncNodeHandles(config, NODE_ID_ROBOT_IO, desired)
}

export function syncSimIOHandles(config: SimConfigData, assembly: MirabufSceneObject) {
    const driverHandles = getDriverSignals(assembly)
        .filter(x => x.info?.GUID)
        .map(
            (x): HandleInfo => ({
                id: `${NODE_ID_SIM_IN}:${x.idStr}`,
                nodeId: NODE_ID_SIM_IN,
                noraType: x.receiverType,
                originType: x.id.type,
                originId: x.idStr,
                displayName: x.displayName(),
                enabled: true,
                many: true,
                isSource: false,
            })
        )
    syncNodeHandles(config, NODE_ID_SIM_IN, driverHandles)

    const stimulusHandles = getStimulusSignals(assembly)
        .filter(x => x.info?.GUID)
        .map(
            (x): HandleInfo => ({
                id: `${NODE_ID_SIM_OUT}:${x.idStr}`,
                nodeId: NODE_ID_SIM_OUT,
                noraType: x.supplierType,
                originType: x.id.type,
                originId: x.idStr,
                displayName: x.displayName(),
                enabled: true,
                many: true,
                isSource: true,
            })
        )
    syncNodeHandles(config, NODE_ID_SIM_OUT, stimulusHandles)
}

export function addJunctionNode(config: SimConfigData): NodeIdAlias {
    const nodeId = genNewId(config.nodes)
    config.nodes[nodeId] = {
        id: nodeId,
        kind: NodeKindId.JUNCTION,
        position: { x: 300 + random() * 10, y: -100 + random() * 50 },
        aggregateStrategy: AggregateStrategy.AVERAGE,
    }

    addHandle(config, {
        id: "",
        nodeId,
        noraType: null,
        originId: nodeId,
        displayName: "In",
        enabled: true,
        many: true,
        isSource: false,
    })

    addHandle(config, {
        id: "",
        nodeId,
        noraType: null,
        originId: nodeId,
        displayName: "Out",
        enabled: true,
        many: true,
        isSource: true,
    })

    return nodeId
}

export function addConstructorNode(
    config: SimConfigData,
    sourceNoraType: NoraType,
    positionHint?: XYPosition
): HandleIdAlias | undefined {
    if (sourceNoraType.length === 0) return undefined

    const nodeId = genNewId(config.nodes)
    config.nodes[nodeId] = {
        id: nodeId,
        kind: NodeKindId.CONSTRUCTOR,
        position: positionHint ?? { x: 0, y: 0 },
    }

    const sourceHandle: HandleInfo = {
        id: "",
        nodeId,
        noraType: sourceNoraType,
        originId: nodeId,
        displayName: "Out",
        enabled: true,
        many: true,
        isSource: true,
    }
    addHandle(config, sourceHandle)

    sourceNoraType.forEach((componentType, i) => {
        addHandle(config, {
            id: "",
            nodeId,
            noraType: [componentType],
            originId: nodeId,
            index: i,
            displayName: componentType.displayName ?? `In ${i + 1}`,
            enabled: true,
            many: false, // TODO: is this right?
            isSource: false,
        })
    })

    return sourceHandle.id
}

export function addDeconstructorNode(
    config: SimConfigData,
    targetNoraType: NoraType,
    positionHint?: XYPosition
): HandleIdAlias | undefined {
    if (targetNoraType.length === 0) return undefined

    const nodeId = genNewId(config.nodes)
    config.nodes[nodeId] = {
        id: nodeId,
        kind: NodeKindId.DECONSTRUCTOR,
        position: positionHint ?? { x: 0, y: 0 },
    }

    const targetHandle: HandleInfo = {
        id: "",
        nodeId,
        noraType: targetNoraType,
        originId: nodeId,
        displayName: "In",
        enabled: true,
        many: false, // TODO: is this right?
        isSource: false,
    }
    addHandle(config, targetHandle)

    targetNoraType.forEach((componentType, i) => {
        console.log(JSON.stringify(componentType))
        addHandle(config, {
            id: "",
            nodeId,
            noraType: [componentType],
            originId: nodeId,
            index: i,
            displayName: componentType.displayName ?? `Out ${i + 1}`,
            enabled: true,
            many: true,
            isSource: true,
        })
    })

    return targetHandle.id
}
