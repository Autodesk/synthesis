import type { XYPosition } from "@xyflow/react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { AggregateStrategy } from "@/systems/simulation/wpilib_brain/SimDataFlow"
import WiringNode from "@/ui/panels/simulation/WiringNode"
import { random } from "@/util/Random"
import type { NoraType } from "../Nora"
import { ACCEL_TYPE } from "../stimulus/AccelStimulus"
import { GYRO_TYPE } from "../stimulus/GyroStimulus"
import { CAN_ENCODER_TYPE } from "../wpilib_brain/sim/SimCANEncoder"
import { CAN_MOTOR_TYPE } from "../wpilib_brain/sim/SimCANMotor"
import { PWM_TYPE } from "../wpilib_brain/sim/SimPWM"
import { SimType } from "../wpilib_brain/WPILibTypes"
import { displayNameCAN, displayNamePWM, displayNameSensor, getDriverSignals, getSimDevices, getStimulusSignals } from "./Devices"
import { NODE_ID_ROBOT_IO } from "./nodes/RobotIONode"
import { NODE_ID_SIM_IN } from "./nodes/SimInputNode"
import { NODE_ID_SIM_OUT } from "./nodes/SimOutputNode"
import {
    addHandle,
    FuncType,
    genNewId,
    type HandleIdAlias,
    type HandleInfo,
    type NodeIdAlias,
    type NodeInfo,
    removeHandle,
    removeNode,
    type SimConfigData,
} from "./SimGraph"

export function defaultConfig(assembly: MirabufSceneObject): SimConfigData {
    const config: SimConfigData = {
        handles: {},
        edges: {},
        adjacency: {},
        nodes: {},
    }

    if (config.nodes[NODE_ID_ROBOT_IO] !== undefined) {
        removeNode(config, NODE_ID_ROBOT_IO)
    }

    const robotIONode: NodeInfo = {
        id: NODE_ID_ROBOT_IO,
        type: "robotIO",
        position: { x: 0, y: 0 },
        tooltip:
            "These handles represent the different devices we've discovered from your connected robot code. The left handles represent input devices such as sensors. The right handles represent output devices such as motor controllers. Use the edit button to hide/reveal handles.",
        targets: [],
        sources: [],
    }
    config.nodes[NODE_ID_ROBOT_IO] = robotIONode

    if (config.nodes[NODE_ID_SIM_IN] !== undefined) {
        removeNode(config, NODE_ID_SIM_IN)
    }

    const simInNode: NodeInfo = {
        id: NODE_ID_SIM_IN,
        type: "simInput",
        position: { x: 800, y: 0 },
        tooltip:
            "These handles represent the input of the simulation. These are drivers for wheels, hinges, and sliders. Use the edit button to hide/reveal handles.",
        targets: [],
        sources: [],
    }

    if (config.nodes[NODE_ID_SIM_OUT] !== undefined) {
        removeNode(config, NODE_ID_SIM_OUT)
    }

    const simOutNode: NodeInfo = {
        id: NODE_ID_SIM_OUT,
        type: "simOutput",
        position: { x: -800, y: 0 },
        tooltip:
            "These handles represent the output of the simulation. These are stimuli for wheels, hinges, and sliders that represent encoder positions and speeds. Use the edit button to hide/reveal handles.",
        targets: [],
        sources: [],
    }
    config.nodes[NODE_ID_SIM_IN] = simInNode
    config.nodes[NODE_ID_SIM_OUT] = simOutNode

    syncRobotIOHandles(config)
    syncSimIOHandles(config, assembly)

    return config
}

function syncNodeHandles(config: SimConfigData, nodeId: NodeIdAlias, desired: HandleInfo[]) {
    const node = config.nodes[nodeId]
    if (!node) return

    const desiredIds = new Set(desired.map(x => x.id))
    Object.values(config.handles)
        .filter(x => x.nodeId === nodeId && !desiredIds.has(x.id))
        .forEach(x => {
            removeHandle(config, x.id)
            node.sources = node.sources.filter(id => id !== x.id)
            node.targets = node.targets.filter(id => id !== x.id)
        })

    desired.forEach(info => {
        const existing = config.handles[info.id]
        if (existing) {
            existing.noraType = info.noraType
            existing.displayName = info.displayName
            existing.many = info.many
        } else {
            addHandle(config, info)
            ;(info.isSource ? node.sources : node.targets).push(info.id)
        }
    })
}

export function syncRobotIOHandles(config: SimConfigData) {
    const deviceTable: [SimType, NoraType, (id: string) => string, boolean, boolean][] = [
        [SimType.CAN_MOTOR, CAN_MOTOR_TYPE, displayNameCAN, true, true],
        [SimType.PWM, PWM_TYPE, displayNamePWM, true, true],
        [SimType.CAN_ENCODER, CAN_ENCODER_TYPE, displayNameCAN, false, false],
        [SimType.ACCELEROMETER, ACCEL_TYPE, id => displayNameSensor(id, "Accel"), false, false],
        [SimType.GYRO, GYRO_TYPE, id => displayNameSensor(id, "Gyro"), false, false],
    ]

    const desired = deviceTable.flatMap(([simType, noraType, displayName, isSource, requireInit]) =>
        getSimDevices(simType, requireInit).map(
            ([id, _]): HandleInfo => ({
                id: `${NODE_ID_ROBOT_IO}:${simType}:${id}`,
                nodeId: NODE_ID_ROBOT_IO,
                noraType: noraType,
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

                many: false,
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

export function refreshRobotIO(config: SimConfigData) {
    syncRobotIOHandles(config)
}

export function addJunctionNode(config: SimConfigData): NodeIdAlias {
    let nodeId = genNewId(config.handles)
    const node: NodeInfo = {
        id: nodeId,
        type: "junction",
        position: { x: 300 + random() * 100, y: -100 + random() * 50 },
        funcType: FuncType.JUNCTION,
        aggregateStrategy: AggregateStrategy.AVERAGE,
        targets: [],
        sources: [],
    }
    config.nodes[nodeId] = node

    const targetHandle: HandleInfo = {
        id: "",
        nodeId: nodeId,
        noraType: null,
        originType: SimType.SIM_DEVICE,
        originId: nodeId,

        displayName: "In",
        enabled: true,

        many: true,
        isSource: false,
    }
    addHandle(config, targetHandle)
    node.targets.push(targetHandle.id)

    const sourceHandle: HandleInfo = {
        id: "",
        nodeId: nodeId,
        noraType: null,
        originType: SimType.SIM_DEVICE,
        originId: nodeId,

        displayName: "Out",
        enabled: true,

        many: true,
        isSource: true,
    }
    addHandle(config, sourceHandle)
    node.sources.push(sourceHandle.id)
    return nodeId
}

export function addDeconstructorNode(
    config: SimConfigData,
    targetNoraType: NoraType,
    positionHint?: XYPosition
): HandleIdAlias | undefined {
    if (targetNoraType === undefined || targetNoraType.length === 0) return undefined

    let nodeId = genNewId(config.handles)

    const node: NodeInfo = {
        id: nodeId,
        type: WiringNode.name,
        position: positionHint ?? { x: 0, y: 0 },
        funcType: FuncType.DECONSTRUCTOR,
        targets: [],
        sources: [],
    }
    config.nodes[nodeId] = node

    const targetHandle: HandleInfo = {
        id: "",
        nodeId: nodeId,
        noraType: targetNoraType,
        originType: SimType.SIM_DEVICE,
        originId: `target_${nodeId}`,

        displayName: "In",
        enabled: true,

        many: false,
        isSource: false,
    }
    addHandle(config, targetHandle)
    node.targets.push(targetHandle.id)

    targetNoraType.forEach((sourceType, i) => {
        const sourceHandle: HandleInfo = {
            id: "",
            nodeId: nodeId,
            noraType: [sourceType],
            originType: SimType.SIM_DEVICE,
            originId: `source_${i}_${nodeId}`,

            displayName: `Out ${i + 1}`,
            enabled: true,

            many: true,
            isSource: true,
        }
        addHandle(config, sourceHandle)
        node.sources.push(sourceHandle.id)
    })

    return targetHandle.id
}

export function addConstructorNode(
    config: SimConfigData,
    sourceNoraType: NoraType,
    positionHint?: XYPosition
): HandleIdAlias | undefined {
    if (sourceNoraType === undefined || sourceNoraType.length === 0) return undefined

    let nodeId = genNewId(config.handles)

    const node: NodeInfo = {
        id: nodeId,
        type: WiringNode.name,
        position: positionHint ?? { x: 0, y: 0 },
        funcType: FuncType.CONSTRUCTOR,
        targets: [],
        sources: [],
    }
    config.nodes[nodeId] = node

    const sourceHandle: HandleInfo = {
        id: "",
        nodeId: nodeId,
        noraType: sourceNoraType,
        originType: SimType.SIM_DEVICE,
        originId: `source_${nodeId}`,

        displayName: "Out",
        enabled: true,

        many: true,
        isSource: true,
    }
    addHandle(config, sourceHandle)
    node.sources.push(sourceHandle.id)

    sourceNoraType.forEach((targetType, i) => {
        const targetHandle: HandleInfo = {
            id: "",
            nodeId: nodeId,
            noraType: [targetType],
            originType: SimType.SIM_DEVICE,
            originId: `target_${i}_${nodeId}`,

            displayName: `In ${i + 1}`,
            enabled: true,

            many: false,
            isSource: false,
        }
        addHandle(config, targetHandle)
        node.targets.push(targetHandle.id)
    })

    return sourceHandle.id
}
