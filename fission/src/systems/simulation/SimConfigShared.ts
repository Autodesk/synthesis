import type { XYPosition } from "@xyflow/react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type Driver from "@/systems/simulation/driver/Driver"
import type { DriverType } from "@/systems/simulation/driver/Driver"
import {
    deconstructNoraType,
    hasNoraAverageFunc,
    type NoraType,
    NoraTypes,
    noraAverageFunc,
} from "@/systems/simulation/Nora"
import type { SimulationLayer } from "@/systems/simulation/SimulationSystem"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus"
import type { StimulusType } from "@/systems/simulation/stimulus/Stimulus"
import type { SimFlow, SimReceiver, SimSupplier } from "@/systems/simulation/wpilib_brain/SimDataFlow"
import { getSimMap, receiverTypeMap, supplierTypeMap } from "@/systems/simulation/wpilib_brain/WPILibState"
import World from "@/systems/World"
import WiringNode from "@/ui/panels/simulation/WiringNode"
import { random } from "@/util/Random"
import SimAccel from "./wpilib_brain/sim/SimAccel"
import SimCANEncoder from "./wpilib_brain/sim/SimCANEncoder"
import SimCANMotor from "./wpilib_brain/sim/SimCANMotor"
import SimPWM from "./wpilib_brain/sim/SimPWM"
import { SimType } from "./wpilib_brain/WPILibTypes"

export const NORA_TYPES_COLORS: { [k in NoraTypes]: string } = {
    [NoraTypes.NUMBER]: "#5f60ff",
    [NoraTypes.NUMBER2]: "#2bc275",
    [NoraTypes.NUMBER3]: "#ffc21a",
    [NoraTypes.UNKNOWN]: "#bebebe",
}

let id = 0
export function genId(): number {
    return ++id
}

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

export const handleInfoDisplayCompare: (a: HandleInfo, b: HandleInfo) => number = (a, b) =>
    a.displayName.localeCompare(b.displayName)

export const NODE_ID_ROBOT_IO = "robot-io-node"
export const NODE_ID_SIM_OUT = "sim-output-node"
export const NODE_ID_SIM_IN = "sim-input-node"

export type ConfigState = "wiring" | "simIO" | "robotIO"
export type OriginType = SimType | StimulusType | DriverType
export enum FuncType {
    Junction = "junct",
    Constructor = "construct",
    Deconstructor = "deconstruct",
}

export type HandleInfo = {
    id: string
    nodeId: string
    noraType: NoraTypes
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

export function getDriverSignals(assembly: MirabufSceneObject): Driver[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.drivers ?? []
}

export function getStimulusSignals(assembly: MirabufSceneObject): Stimulus[] {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    return simLayer?.stimuli ?? []
}

export function getCANMotors(): [string, Map<string, number | boolean | string>][] {
    const cans = getSimMap()?.get(SimType.CAN_MOTOR) ?? new Map<string, Map<string, number>>()
    return [...cans.entries()].filter(([_, data]) => data.get("<init")).reverse()
}

export function getCANEncoder(): [string, Map<string, string | boolean | number>][] {
    return [...(getSimMap()?.get(SimType.CAN_ENCODER)?.entries() ?? [])]
}

export function getPWMDevices(): [string, Map<string, string | boolean | number>][] {
    const pwms = getSimMap()?.get(SimType.PWM)
    if (pwms) {
        return [...pwms.entries()].filter(([_, data]) => data.get("<init"))
    }
    return []
}

export function getAccelDevices(): [string, Map<string, string | boolean | number>][] {
    return [...(getSimMap()?.get(SimType.ACCELEROMETER)?.entries() ?? [])]
}

export function getDIODevices(): [string, Map<string, string | boolean | number>][] {
    return [...(getSimMap()?.get(SimType.DIO)?.entries() ?? [])]
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

function displayNameAccel(id: string) {
    if (id.startsWith("BuiltIn")) {
        return "Accel [Built In]"
    }
    const a = id.indexOf("[")
    const b = id.indexOf("]")
    if (a === -1 || b === -1 || b - a < 2) return id
    return `Accel [${id.substring(0, a)} - ${id.substring(a + 1, b)}]`
}

// TODO
// function displayNameDI(id: string) {
//     return `DI [${id}]`
// }

// function displayNameDO(id: string) {
//     return `DO [${id}]`
// }

export type NodeInfo = {
    id: string
    type: string
    funcType?: FuncType
    position: XYPosition
    tooltip?: string
    sources: HandleId_Alias[]
    targets: HandleId_Alias[]
}

type NodeId_Alias = string
type HandleId_Alias = string
type EdgeId_Alias = string
type Edge_Alias = { sourceId: HandleId_Alias; targetId: HandleId_Alias }

export type SimConfigData = {
    handles: { [k: HandleId_Alias]: HandleInfo }
    edges: { [k: EdgeId_Alias]: Edge_Alias }
    adjacency: { [k: HandleId_Alias]: { [j: EdgeId_Alias]: boolean } }
    nodes: { [k: NodeId_Alias]: NodeInfo }
}

export class SimConfig {
    private constructor() {}

    public static Default(assembly: MirabufSceneObject) {
        const config: SimConfigData = {
            handles: {},
            edges: {},
            adjacency: {},
            nodes: {},
        }

        SimConfig.AddRobotIONode(config)
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
                    noraType: x.getReceiverType(),
                    originType: x.id.type,
                    originId: x.idStr,

                    displayName: x.displayName(),
                    enabled: true,

                    many: hasNoraAverageFunc(x.getReceiverType()),
                    isSource: false,
                }
                this.AddHandle(config, handle)
                simInNode.targets.push(handle.id)
            }
        })
        getStimulusSignals(assembly).forEach(x => {
            if (x.info?.GUID) {
                const handle: HandleInfo = {
                    id: "",
                    nodeId: NODE_ID_SIM_OUT,
                    noraType: x.getSupplierType(),
                    originType: x.id.type,
                    originId: x.idStr,

                    displayName: x.displayName(),
                    enabled: true,

                    many: hasNoraAverageFunc(x.getSupplierType()),
                    isSource: true,
                }
                this.AddHandle(config, handle)
                simOutNode.sources.push(handle.id)
            } else {
                console.debug("Skipping stimulus", x)
            }
        })

        return config
    }

    public static refreshRobotIO(config: SimConfigData) {
        SimConfig.AddRobotIONode(config)
        // TODO: Try to restore connections that remain valid after refresh
    }

    private static AddRobotIONode(config: SimConfigData) {
        if (config.nodes[NODE_ID_ROBOT_IO] !== undefined) {
            SimConfig.RemoveNode(config, NODE_ID_ROBOT_IO)
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
        getCANMotors().forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: supplierTypeMap[SimType.CAN_MOTOR]!,
                originType: SimType.CAN_MOTOR,
                originId: id,

                displayName: displayNameCAN(id),
                enabled: true,

                many: true,
                isSource: true,
            }
            this.AddHandle(config, handle)
            robotIONode.sources.push(handle.id)
        })
        getCANEncoder().forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: receiverTypeMap[SimType.CAN_ENCODER]!,
                originType: SimType.CAN_ENCODER,
                originId: id,

                displayName: displayNameCAN(id),
                enabled: true,

                many: hasNoraAverageFunc(receiverTypeMap[SimType.CAN_ENCODER]!),
                isSource: false,
            }
            this.AddHandle(config, handle)
            robotIONode.targets.push(handle.id)
        })
        getPWMDevices().forEach(([id, _]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: supplierTypeMap[SimType.PWM]!,
                originType: SimType.PWM,
                originId: id,

                displayName: displayNamePWM(id),
                enabled: true,

                many: true,
                isSource: true,
            }
            this.AddHandle(config, handle)
            robotIONode.sources.push(handle.id)
        })
        getAccelDevices().forEach(([id, data]) => {
            const handle: HandleInfo = {
                id: "",
                nodeId: NODE_ID_ROBOT_IO,
                noraType: receiverTypeMap[SimType.ACCELEROMETER]!,
                originType: SimType.ACCELEROMETER,
                originId: id,

                displayName: displayNameAccel(id),
                enabled: data.get("<init") === true,

                many: hasNoraAverageFunc(receiverTypeMap[SimType.ACCELEROMETER]!),
                isSource: false,
            }
            this.AddHandle(config, handle)
            robotIONode.targets.push(handle.id)
        })
        // TODO
        // getDIODevices().forEach(([id, data]) => {
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
    }

    private static AddHandle(config: SimConfigData, info: HandleInfo) {
        let handleId = ""
        do {
            handleId = genRandomId()
        } while (config.handles[handleId] !== undefined)
        info.id = handleId
        config.handles[handleId] = info
        config.adjacency[handleId] = {}
    }

    private static RemoveHandle(config: SimConfigData, id: HandleId_Alias): boolean {
        if (config.handles[id] === undefined) return false
        const edgeIds = config.adjacency[id]
        if (edgeIds === undefined) return false
        ;[...Object.keys(edgeIds)].forEach(x => this.DeleteEdge(config, x))
        delete config.adjacency[id]
        delete config.handles[id]
        return true
    }

    public static RemoveNode(config: SimConfigData, id: NodeId_Alias): boolean {
        if (config.nodes[id] === undefined) return false
        ;[...Object.values(config.handles)].filter(x => x.nodeId === id).forEach(x => this.RemoveHandle(config, x.id))
        delete config.nodes[id]
        return true
    }

    public static AddJunctionNode(config: SimConfigData): NodeId_Alias {
        let nodeId = ""
        do {
            nodeId = genRandomId()
        } while (config.handles[nodeId] !== undefined)
        const node: NodeInfo = {
            id: nodeId,
            type: WiringNode.name,
            position: { x: 300 + random() * 100, y: -100 + random() * 50 },
            funcType: FuncType.Junction,
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
        SimConfig.AddHandle(config, targetHandle)
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
        SimConfig.AddHandle(config, sourceHandle)
        node.sources.push(sourceHandle.id)
        return nodeId
    }

    public static AddDeconstructorNode(
        config: SimConfigData,
        targetNoraType: NoraTypes,
        positionHint?: XYPosition
    ): HandleId_Alias | undefined {
        const types = deconstructNoraType(targetNoraType)
        if (types === undefined || types.length === 0) return undefined

        let nodeId = ""
        do {
            nodeId = genRandomId()
        } while (config.handles[nodeId] !== undefined)
        const node: NodeInfo = {
            id: nodeId,
            type: WiringNode.name,
            position: positionHint ?? { x: 0, y: 0 },
            funcType: FuncType.Deconstructor,
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
        SimConfig.AddHandle(config, targetHandle)
        node.targets.push(targetHandle.id)

        types.forEach((sourceType, i) => {
            const sourceHandle: HandleInfo = {
                id: "",
                nodeId: nodeId,
                noraType: sourceType,
                originType: SimType.SIM_DEVICE,
                originId: `source_${i}_${nodeId}`,

                displayName: `Out ${i + 1}`,
                enabled: true,

                many: true,
                isSource: true,
            }
            SimConfig.AddHandle(config, sourceHandle)
            node.sources.push(sourceHandle.id)
        })

        return targetHandle.id
    }

    public static AddConstructorNode(
        config: SimConfigData,
        sourceNoraType: NoraTypes,
        positionHint?: XYPosition
    ): HandleId_Alias | undefined {
        const types = deconstructNoraType(sourceNoraType)
        if (types === undefined || types.length === 0) return undefined

        let nodeId = ""
        do {
            nodeId = genRandomId()
        } while (config.handles[nodeId] !== undefined)
        const node: NodeInfo = {
            id: nodeId,
            type: WiringNode.name,
            position: positionHint ?? { x: 0, y: 0 },
            funcType: FuncType.Constructor,
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
        SimConfig.AddHandle(config, sourceHandle)
        node.sources.push(sourceHandle.id)

        types.forEach((targetType, i) => {
            const targetHandle: HandleInfo = {
                id: "",
                nodeId: nodeId,
                noraType: targetType,
                originType: SimType.SIM_DEVICE,
                originId: `target_${i}_${nodeId}`,

                displayName: `In ${i + 1}`,
                enabled: true,

                many: hasNoraAverageFunc(targetType),
                isSource: false,
            }
            SimConfig.AddHandle(config, targetHandle)
            node.targets.push(targetHandle.id)
        })

        return sourceHandle.id
    }

    public static GetEdge(
        config: SimConfigData,
        sourceId: HandleId_Alias,
        targetId: HandleId_Alias
    ): EdgeId_Alias | undefined {
        const targetEdges = config.adjacency[targetId]!
        return [...Object.keys(config.adjacency[sourceId]!)].filter(x => targetEdges[x] !== undefined)[0]
    }

    public static ValidateConnection(
        config: SimConfigData,
        sourceId: HandleId_Alias,
        targetId: HandleId_Alias
    ): boolean {
        const sourceInfo = config.handles[sourceId]
        const targetInfo = config.handles[targetId]
        if (sourceInfo === undefined || targetInfo === undefined) return false

        if (!targetInfo.many && Object.entries(config.adjacency[targetId])!.length >= 1) return false

        return sourceInfo.noraType === targetInfo.noraType
    }

    public static MakeConnection(config: SimConfigData, sourceId: HandleId_Alias, targetId: HandleId_Alias): boolean {
        if (!SimConfig.ValidateConnection(config, sourceId, targetId)) {
            console.error("Failed to make edge")
            return false
        }

        if (SimConfig.GetEdge(config, sourceId, targetId) !== undefined) {
            console.error("Connection already exists")
            return false
        }

        let edgeId = genRandomId()
        while (config.edges[edgeId] !== undefined) {
            edgeId = genRandomId()
        }
        config.edges[edgeId] = { sourceId: sourceId, targetId: targetId }
        config.adjacency[sourceId][edgeId] = true
        config.adjacency[targetId][edgeId] = true
        return true
    }

    public static DeleteConnection(config: SimConfigData, sourceId: HandleId_Alias, targetId: HandleId_Alias): boolean {
        const edgeId = SimConfig.GetEdge(config, sourceId, targetId)
        if (edgeId === undefined) return false
        delete config.adjacency[sourceId][edgeId]
        delete config.adjacency[targetId][edgeId]
        delete config.edges[edgeId]
        return true
    }

    public static DeleteEdge(config: SimConfigData, edgeId: EdgeId_Alias): boolean {
        const edge = config.edges[edgeId]
        if (edge === undefined) return false
        delete config.adjacency[edge.sourceId][edgeId]
        delete config.adjacency[edge.targetId][edgeId]
        delete config.edges[edgeId]
        return true
    }

    public static Compile(config: SimConfigData, assembly: MirabufSceneObject): SimFlow[] | undefined {
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
                    const flow = SimConfig.CompileTargetHandle(config, simLayer, id, new Set<HandleId_Alias>())
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

    public static CompileTargetHandle(
        config: SimConfigData,
        simLayer: SimulationLayer,
        targetHandleId: HandleId_Alias,
        encountered: Set<HandleId_Alias>
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
            }
        } else if (targetHandle.nodeId === NODE_ID_SIM_IN) {
            receiver = simLayer.getDriver(targetHandle.originId)
        } else {
            receiver = {
                getReceiverType: () => targetHandle.noraType,
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
                    const funcSuppliers = SimConfig.CompileFunctionNode(config, simLayer, node, encountered)
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
                getSupplierType: () => targetNoraType,
                getSupplierValue: func,
            },
            receiver: receiver,
        }
    }

    public static CompileFunctionNode(
        config: SimConfigData,
        simLayer: SimulationLayer,
        node: NodeInfo,
        encountered: Set<HandleId_Alias>
    ): SimSupplier[] | undefined {
        switch (node.funcType) {
            case FuncType.Constructor: {
                if (node.sources.length !== 1 || node.targets.length < 1) {
                    return undefined
                }
                const outputType = config.handles[node.sources[0]].noraType
                const inputs = node.targets.map(x => {
                    const flow = SimConfig.CompileTargetHandle(config, simLayer, x, encountered)
                    if (!flow) {
                        console.error(`Failed to compile flow. TargetHandleId: ${x}`)
                        throw new Error("Failed to compile SimConfig")
                    }
                    return flow.supplier
                })
                return [
                    {
                        getSupplierType: () => outputType,
                        getSupplierValue: () => inputs.map(x => x.getSupplierValue()),
                    },
                ]
            }
            case FuncType.Deconstructor: {
                if (node.sources.length < 1 || node.targets.length !== 1) {
                    return undefined
                }
                const inputType = config.handles[node.targets[0]].noraType
                const input = SimConfig.CompileTargetHandle(config, simLayer, node.targets[0], encountered)
                if (!input) {
                    console.error(`Failed to compile flow. TargetHandleId: ${node.targets[0]}`)
                    throw new Error("Failed to compile SimConfig")
                }
                const deconstructedType = deconstructNoraType(inputType)
                if (!deconstructedType) return undefined
                const suppliers: SimSupplier[] = []
                for (let i = 0; i < deconstructedType.length; ++i) {
                    suppliers.push({
                        getSupplierType: () => deconstructedType[i],
                        getSupplierValue: () => (input.supplier.getSupplierValue() as NoraType[])[i],
                    })
                }
                return suppliers
            }
            case FuncType.Junction: {
                if (node.sources.length !== 1 || node.targets.length !== 1) {
                    return undefined
                }
                const input = SimConfig.CompileTargetHandle(config, simLayer, node.targets[0], encountered)
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
