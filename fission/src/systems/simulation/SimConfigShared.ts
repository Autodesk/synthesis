import type { XYPosition } from "@xyflow/react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type Driver from "@/systems/simulation/driver/Driver"
import type { DriverType } from "@/systems/simulation/driver/Driver"
import type { SimulationLayer } from "@/systems/simulation/SimulationSystem"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus"
import type { StimulusType } from "@/systems/simulation/stimulus/Stimulus"
import {
    AggregateStrategy,
    type SimFlow,
    type SimReceiver,
    type SimSupplier,
} from "@/systems/simulation/wpilib_brain/SimDataFlow"
import { getSimMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import World from "@/systems/World"
import WiringNode from "@/ui/panels/simulation/WiringNode"
import { random } from "@/util/Random"
import SimAccel from "./wpilib_brain/sim/SimAccel"
import SimCANEncoder, { CAN_ENCODER_TYPE } from "./wpilib_brain/sim/SimCANEncoder"
import SimCANMotor, { CAN_MOTOR_TYPE } from "./wpilib_brain/sim/SimCANMotor"
import SimPWM, { PWM_TYPE } from "./wpilib_brain/sim/SimPWM"
import { SimType } from "./wpilib_brain/WPILibTypes"
import SimGyro from "./wpilib_brain/sim/SimGyro"
import type { NoraType } from "./Nora"
import { ACCEL_TYPE } from "./stimulus/AccelStimulus"
import { GYRO_TYPE } from "./stimulus/GyroStimulus"

// #region ID handling
let id = 0
export function genRandomId(): string {
    return Math.floor(random() * Number.MAX_SAFE_INTEGER).toString()
}

const savedToGenMap = new Map<string, string>()
const genToSavedMap = new Map<string, string>()
function makeMapping(savedId: string): string {
    const genId = (++id).toString()
    savedToGenMap.set(savedId, genId)
    genToSavedMap.set(genId, savedId)
    return genId
}
export function savedIdToGenId(savedId: string): string {
    const genId = savedToGenMap.get(savedId)
    if (genId !== undefined) {
        return genId
    }
    return makeMapping(savedId)
}
export function genIdToSavedId(genId: string): string | undefined {
    return genToSavedMap.get(genId)
}

const genNewId = (existing: SimConfigData["handles"] | SimConfigData["edges"]) => {
    let handleId = ""
    do {
        handleId = genRandomId()
    } while (existing[handleId] !== undefined)

    return handleId
}

// #endregion

export const handleInfoDisplayCompare: (a: HandleInfo, b: HandleInfo) => number = (a, b) =>
    a.displayName.localeCompare(b.displayName)

export const NODE_ID_ROBOT_IO = "robot-io-node"
export const NODE_ID_SIM_OUT = "sim-output-node"
export const NODE_ID_SIM_IN = "sim-input-node"

export type ConfigState = "wiring" | "simIO" | "robotIO"
export type OriginType = SimType | StimulusType | DriverType
export enum FuncType {
    JUNCTION = "junct",
    CONSTRUCTOR = "construct",
    DECONSTRUCTOR = "deconstruct",
}

// NOTE: I decided to make noraType accept null specifically for Junction nodes:
// the input and output should be null to accept any type, and output should update based
// on the input type when (dis)connected
export type HandleInfo = {
    id: string
    nodeId: string
    noraType: NoraType | null
    originType?: OriginType
    originId: string

    displayName: string
    enabled: boolean

    many: boolean
    isSource: boolean
}

export type FlowControlsProps = {
    onCreateJunction?: () => void
}

// #region Utility functions
function getDriverSignals(assembly: MirabufSceneObject): Driver[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.drivers ?? []
}

function getStimulusSignals(assembly: MirabufSceneObject): Stimulus[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.stimuli ?? []
}

function getSimDevices(type: SimType, requireInit: boolean = true): [string, Map<string, number | boolean | string>][] {
    const devices = getSimMap()?.get(type) ?? new Map<string, Map<string, number>>()
    return [...devices.entries()].filter(([_, data]) => data.get("<init") || !requireInit).reverse()
}

function displayNameCAN(id: string) {
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `CAN [${id.substring(a + 1, b)}]`
}

function displayNamePWM(id: string) {
    return `PWM [${id}]`
}

function displayNameSensor(id: string, label: string) {
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

// #endregion

export type NodeInfo = {
    id: string
    type: string
    position: XYPosition
    tooltip?: string
    sources: HandleIdAlias[]
    targets: HandleIdAlias[]
} & (
        | {
            funcType: FuncType.JUNCTION
            aggregateStrategy: AggregateStrategy
        }
        | {
            funcType?: Exclude<FuncType, FuncType.JUNCTION>
        }
    )

type NodeIdAlias = string
type HandleIdAlias = string
type EdgeIdAlias = string
type EdgeAlias = { sourceId: HandleIdAlias; targetId: HandleIdAlias }

export type SimConfigData = {
    handles: { [k: HandleIdAlias]: HandleInfo }
    edges: { [k: EdgeIdAlias]: EdgeAlias }
    adjacency: { [k: HandleIdAlias]: { [j: EdgeIdAlias]: boolean } }
    nodes: { [k: NodeIdAlias]: NodeInfo }
}

export class SimConfig {
    private constructor() { }

    public static default(assembly: MirabufSceneObject) {
        const config: SimConfigData = {
            handles: {},
            edges: {},
            adjacency: {},
            nodes: {},
        }

        SimConfig.addRobotIONode(config)
        // #region Creating default handles for sim in/out
        const simInNode: NodeInfo = {
            id: NODE_ID_SIM_IN,
            type: WiringNode.name,
            position: { x: 800, y: 0 },
            tooltip:
                "These handles represent the input of the simulation. These are drivers for wheels, hinges, and sliders. Use the edit button to hide/reveal handles.",
            sources: [],
            targets: [],
        }
        const simOutNode: NodeInfo = {
            id: NODE_ID_SIM_OUT,
            type: WiringNode.name,
            position: { x: -800, y: 0 },
            tooltip:
                "These handles represent the output of the simulation. These are stimuli for wheels, hinges, and sliders that represent encoder positions and speeds. Use the edit button to hide/reveal handles.",
            sources: [],
            targets: [],
        }
        config.nodes[NODE_ID_SIM_IN] = simInNode
        config.nodes[NODE_ID_SIM_OUT] = simOutNode
        getDriverSignals(assembly).forEach(x => {
            if (x.info?.GUID) {
                const handle: HandleInfo = {
                    id: "",
                    nodeId: NODE_ID_SIM_IN,
                    noraType: x.receiverType,
                    originType: x.id.type,
                    originId: x.idStr,

                    displayName: x.displayName(),
                    enabled: true,

                    many: hasNoraAverageFunc(x.getReceiverType()),
                    isSource: false,
                }
                this.addHandle(config, handle)
                simInNode.targets.push(handle.id)
            }
        })
        getStimulusSignals(assembly).forEach(x => {
            if (x.info?.GUID) {
                const handle: HandleInfo = {
                    id: "",
                    nodeId: NODE_ID_SIM_OUT,
                    noraType: x.supplierType,
                    originType: x.id.type,
                    originId: x.idStr,

                    displayName: x.displayName(),
                    enabled: true,

                    many: hasNoraAverageFunc(x.getSupplierType()),
                    isSource: true,
                }
                this.addHandle(config, handle)
                simOutNode.sources.push(handle.id)
            } else {
                console.debug("Skipping stimulus", x)
            }
        })
        // #endregion

        return config
    }

    public static refreshRobotIO(config: SimConfigData) {
        SimConfig.addRobotIONode(config)
        // TODO: Try to restore connections that remain valid after refresh
    }

    private static addRobotIONode(config: SimConfigData) {
        if (config.nodes[NODE_ID_ROBOT_IO] !== undefined) {
            SimConfig.removeNode(config, NODE_ID_ROBOT_IO)
        }

        const robotIONode: NodeInfo = {
            id: NODE_ID_ROBOT_IO,
            type: WiringNode.name,
            position: { x: 0, y: 0 },
            tooltip:
                "These handles represent the different devices we've discovered from your connected robot code. The left handles represent input devices such as sensors. The right handles represent output devices such as motor controllers. Use the edit button to hide/reveal handles.",
            sources: [],
            targets: [],
        }
        config.nodes[NODE_ID_ROBOT_IO] = robotIONode
        // #region Adding all devices to Robot IO node
        getSimDevices(SimType.CAN_MOTOR).forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: CAN_MOTOR_TYPE,
                originType: SimType.CAN_MOTOR,
                originId: id,

                displayName: displayNameCAN(id),
                enabled: true,

                many: true,
                isSource: true,
            }
            this.addHandle(config, handle)
            robotIONode.sources.push(handle.id)
        })
        getSimDevices(SimType.CAN_ENCODER, false).forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: CAN_ENCODER_TYPE,
                originType: SimType.CAN_ENCODER,
                originId: id,

                displayName: displayNameCAN(id),
                enabled: true,

                many: hasNoraAverageFunc(receiverTypeMap[SimType.CAN_ENCODER]!),
                isSource: false,
            }
            this.addHandle(config, handle)
            robotIONode.targets.push(handle.id)
        })
        getSimDevices(SimType.PWM).forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: PWM_TYPE,
                originType: SimType.PWM,
                originId: id,

                displayName: displayNamePWM(id),
                enabled: true,

                many: true,
                isSource: true,
            }
            this.addHandle(config, handle)
            robotIONode.sources.push(handle.id)
        })
        getSimDevices(SimType.ACCELEROMETER, false).forEach(([id]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: ACCEL_TYPE,
                originType: SimType.ACCELEROMETER,
                originId: id,

                displayName: displayNameSensor(id, "Accel"),
                enabled: true,

                many: hasNoraAverageFunc(receiverTypeMap[SimType.ACCELEROMETER]!),
                isSource: false,
            }
            this.addHandle(config, handle)
            robotIONode.targets.push(handle.id)
        })
        getSimDevices(SimType.GYRO, false).forEach(([id]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: GYRO_TYPE,
                originType: SimType.GYRO,
                originId: id,

                displayName: displayNameSensor(id, "Gyro"),
                enabled: true,

                many: hasNoraAverageFunc(receiverTypeMap[SimType.GYRO]!),
                isSource: false,
            }
            this.addHandle(config, handle)
            robotIONode.targets.push(handle.id)
        })
        // TODO
        // getSimDevices(SimType.DIO, false).forEach(([id, data]) => {
        //     const handleIn: HandleInfo = {
        //         id: "",
        //         nodeId: NODE_ID_ROBOT_IO,
        //         noraType: receiverTypeMap[SimType.DIO]!,
        //         originType: SimType.DIO,
        //         originId: id,

        //         displayName: displayNameDI(id),
        //         enabled: data.get("<init") == true,

        //         many: true,
        //         isSource: false,
        //     }
        //     this.AddHandle(config, handleIn)
        //     robotIONode.targets.push(handleIn.id)
        //     const handleOut: HandleInfo = {
        //         id: "",
        //         nodeId: NODE_ID_ROBOT_IO,
        //         noraType: supplierTypeMap[SimType.DIO]!,
        //         originType: SimType.DIO,
        //         originId: id,

        //         displayName: displayNameDO(id),
        //         enabled: data.get("<init") == true,

        //         many: hasNoraAverageFunc(supplierTypeMap[SimType.DIO]!),
        //         isSource: true,
        //     }
        //     this.AddHandle(config, handleOut)
        //     robotIONode.targets.push(handleOut.id)
        // })
        // #endregion
    }

    private static addHandle(config: SimConfigData, info: HandleInfo) {
        let handleId = genNewId(config.handles)

        info.id = handleId
        config.handles[handleId] = info
        config.adjacency[handleId] = {}
    }

    /**
     * Deletes a handle and all connected edges
     */
    private static removeHandle(config: SimConfigData, id: HandleIdAlias): boolean {
        if (config.handles[id] === undefined) return false
        const edgeIds = config.adjacency[id]
        if (edgeIds === undefined) return false
        Object.keys(edgeIds).forEach(x => this.deleteEdge(config, x))
        delete config.adjacency[id]
        delete config.handles[id]
        return true
    }

    public static removeNode(config: SimConfigData, id: NodeIdAlias): boolean {
        if (config.nodes[id] === undefined) return false
        Object.values(config.handles)
            .filter(x => x.nodeId === id)
            .forEach(x => this.removeHandle(config, x.id))
        delete config.nodes[id]
        return true
    }

    public static addJunctionNode(config: SimConfigData): NodeIdAlias {
        let nodeId = genNewId(config.handles)
        const node: NodeInfo = {
            id: nodeId,
            type: WiringNode.name,
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
            noraType: NoraTypes.NUMBER,
            originType: SimType.SIM_DEVICE,
            originId: nodeId,

            displayName: "In",
            enabled: true,

            many: true,
            isSource: false,
        }
        SimConfig.addHandle(config, targetHandle)
        node.targets.push(targetHandle.id)

        const sourceHandle: HandleInfo = {
            id: "",
            nodeId: nodeId,
            noraType: NoraTypes.NUMBER,
            originType: SimType.SIM_DEVICE,
            originId: nodeId,

            displayName: "Out",
            enabled: true,

            many: true,
            isSource: true,
        }
        SimConfig.addHandle(config, sourceHandle)
        node.sources.push(sourceHandle.id)
        return nodeId
    }

    public static addDeconstructorNode(
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

            many: hasNoraAverageFunc(targetNoraType),
            isSource: false,
        }
        SimConfig.addHandle(config, targetHandle)
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
            SimConfig.addHandle(config, sourceHandle)
            node.sources.push(sourceHandle.id)
        })

        return targetHandle.id
    }

    public static addConstructorNode(
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
        SimConfig.addHandle(config, sourceHandle)
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

                many: hasNoraAverageFunc(targetType),
                isSource: false,
            }
            SimConfig.addHandle(config, targetHandle)
            node.targets.push(targetHandle.id)
        })

        return sourceHandle.id
    }

    public static getEdge(
        config: SimConfigData,
        sourceId: HandleIdAlias,
        targetId: HandleIdAlias
    ): EdgeIdAlias | undefined {
        const targetEdges = config.adjacency[targetId]!
        const sourceEdges = Object.keys(config.adjacency[sourceId]!)

        return sourceEdges.filter(edge => targetEdges[edge] !== undefined)[0] // should be guaranteed to be 0
    }

    public static validateConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
        const sourceInfo = config.handles[sourceId]
        const targetInfo = config.handles[targetId]
        if (sourceInfo === undefined || targetInfo === undefined) return false

        if (!targetInfo.many && Object.entries(config.adjacency[targetId])!.length >= 1) return false

        return sourceInfo.noraType === targetInfo.noraType
    }

    public static makeConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
        if (!SimConfig.validateConnection(config, sourceId, targetId)) {
            console.error("Failed to make edge")
            return false
        }

        if (SimConfig.getEdge(config, sourceId, targetId) !== undefined) {
            console.error("Connection already exists")
            return false
        }

        let edgeId = genNewId(config.edges)

        config.edges[edgeId] = { sourceId: sourceId, targetId: targetId }
        config.adjacency[sourceId][edgeId] = true
        config.adjacency[targetId][edgeId] = true
        return true
    }

    public static deleteConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
        const edgeId = SimConfig.getEdge(config, sourceId, targetId)
        return edgeId ? this.deleteEdge(config, edgeId) : false
    }

    public static deleteEdge(config: SimConfigData, edgeId: EdgeIdAlias): boolean {
        const edge = config.edges[edgeId]
        if (edge === undefined) return false
        delete config.adjacency[edge.sourceId][edgeId]
        delete config.adjacency[edge.targetId][edgeId]
        delete config.edges[edgeId]
        return true
    }

    public static compile(config: SimConfigData, assembly: MirabufSceneObject): SimFlow[] | undefined {
        const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
        if (!simLayer) {
            console.error("No sim layer found")
            return undefined
        }
        try {
            const flows: SimFlow[] = []
            Object.entries(config.handles).forEach(([id, info]) => {
                if (
                    !info.isSource &&
                    (info.nodeId === NODE_ID_ROBOT_IO || info.nodeId === NODE_ID_SIM_IN) &&
                    (Object.entries(config.adjacency[info.id])?.length ?? 0) > 0
                ) {
                    const flow = SimConfig.compileTargetHandle(config, simLayer, id, new Set<HandleIdAlias>())
                    if (flow) flows.push(flow)
                    else throw new Error("Failed to compile flows")
                }
            })
            return flows
        } catch (error) {
            console.error("Error thrown during compilation", error)
            return undefined
        }
    }

    public static compileTargetHandle(
        config: SimConfigData,
        simLayer: SimulationLayer,
        targetHandleId: HandleIdAlias,
        encountered: Set<HandleIdAlias>
    ): SimFlow | undefined {
        const edges = Object.keys(config.adjacency[targetHandleId])
        if (!edges || edges.length < 1) {
            console.warn("No edges found for target handle")
            return undefined
        }

        // Generate receiver
        const targetHandle = config.handles[targetHandleId]
        if (!targetHandle) return undefined

        const targetNoraType = targetHandle.noraType
        let receiver: SimReceiver | undefined = undefined
        if (targetHandle.nodeId === NODE_ID_ROBOT_IO) {
            switch (targetHandle.originType) {
                case SimType.CAN_ENCODER: {
                    receiver = SimCANEncoder.genReceiver(targetHandle.originId)
                    break
                }
                case SimType.ACCELEROMETER: {
                    receiver = SimAccel.genReceiver(targetHandle.originId)
                    break
                }
                case SimType.GYRO: {
                    receiver = SimGyro.genReceiver(targetHandle.originId)
                    break
                }
            }
        } else if (targetHandle.nodeId === NODE_ID_SIM_IN) {
            receiver = simLayer.getDriver(targetHandle.originId)
        } else {
            receiver = {
                receiverType: targetHandle.noraType,
                setReceiverValue: _ => {
                    console.debug("If you're seeing this, that means bad")
                },
            }
        }
        if (!receiver) return undefined

        if (!hasNoraAverageFunc(targetNoraType) && edges.length > 1) return

        const suppliers: SimSupplier[] = []
        edges.forEach(edgeId => {
            const edge = config.edges[edgeId]
            if (!edge) return
            const sourceHandle = config.handles[edge.sourceId]
            if (!sourceHandle || sourceHandle.noraType !== targetNoraType) return
            if (encountered.has(sourceHandle.id)) return
            encountered.add(sourceHandle.id)
            switch (sourceHandle.nodeId) {
                case NODE_ID_ROBOT_IO: {
                    // Get supplier from robot output
                    switch (sourceHandle.originType) {
                        case SimType.CAN_MOTOR: {
                            suppliers.push(SimCANMotor.genSupplier(sourceHandle.originId))
                            break
                        }
                        case SimType.PWM: {
                            suppliers.push(SimPWM.genSupplier(sourceHandle.originId))
                            break
                        }
                    }
                    break
                }
                case NODE_ID_SIM_OUT: {
                    // Get supplier from simulation output
                    const stim: SimSupplier | undefined = simLayer.getStimuli(sourceHandle.originId)
                    if (stim) suppliers.push(stim)
                    break
                }
                default: {
                    // Figure out function type
                    const node = config.nodes[sourceHandle.nodeId]
                    if (!node?.funcType) break
                    const index = node.sources.indexOf(sourceHandle.id)
                    if (index === -1) break
                    const funcSuppliers = SimConfig.compileFunctionNode(config, simLayer, node, encountered)
                    if (!funcSuppliers || funcSuppliers?.length !== node.sources.length) break
                    suppliers.push(funcSuppliers[index])
                }
            }
            encountered.delete(sourceHandle.id)
        })

        if (suppliers.length === 0) return undefined

        if (suppliers.length === 1) {
            return {
                supplier: suppliers[0],
                receiver: receiver,
            }
        }
        const func = noraAverageFunc(targetNoraType)
        if (!func) return undefined
        return {
            supplier: {
                supplierType: targetNoraType,
                getSupplierValue: func,
            },
            receiver: receiver,
        }
    }

    public static compileFunctionNode(
        config: SimConfigData,
        simLayer: SimulationLayer,
        node: NodeInfo,
        encountered: Set<HandleIdAlias>
    ): SimSupplier[] | undefined {
        switch (node.funcType) {
            case FuncType.CONSTRUCTOR: {
                if (node.sources.length !== 1 || node.targets.length < 1) {
                    return undefined
                }
                const outputType = config.handles[node.sources[0]].noraType
                const inputs = node.targets.map(x => {
                    const flow = SimConfig.compileTargetHandle(config, simLayer, x, encountered)
                    if (!flow) {
                        console.error(`Failed to compile flow. TargetHandleId: ${x}`)
                        throw new Error("Failed to compile SimConfig")
                    }
                    return flow.supplier
                })
                return [
                    {
                        supplierType: outputType,
                        getSupplierValue: () => inputs.flatMap(x => x.getSupplierValue()),
                    },
                ]
            }
            case FuncType.DECONSTRUCTOR: {
                if (node.sources.length < 1 || node.targets.length !== 1) {
                    return undefined
                }
                const inputType = config.handles[node.targets[0]].noraType
                const input = SimConfig.compileTargetHandle(config, simLayer, node.targets[0], encountered)
                if (!input) {
                    console.error(`Failed to compile flow. TargetHandleId: ${node.targets[0]}`)
                    throw new Error("Failed to compile SimConfig")
                }
                const suppliers: SimSupplier[] = []
                for (let i = 0; i < inputType.length; ++i) {
                    suppliers.push({
                        supplierType: [inputType[i]],
                        getSupplierValue: () => [input.supplier.getSupplierValue()[i]],
                    })
                }
                return suppliers
            }
            case FuncType.JUNCTION: {
                if (node.sources.length !== 1 || node.targets.length !== 1) {
                    return undefined
                }
                const input = SimConfig.compileTargetHandle(config, simLayer, node.targets[0], encountered)
                if (!input) {
                    console.error(`Failed to compile flow. TargetHandleId: ${node.targets[0]}`)
                    throw new Error("Failed to compile SimConfig")
                }
                return [input.supplier]
            }
        }
        return undefined
    }
}
