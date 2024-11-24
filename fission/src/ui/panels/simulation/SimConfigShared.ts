import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Driver, { DriverType } from "@/systems/simulation/driver/Driver"
import { deconstructNoraType, hasNoraAverageFunc, NoraTypes } from "@/systems/simulation/Nora"
import Stimulus, { StimulusType } from "@/systems/simulation/stimulus/Stimulus"
import { receiverTypeMap, simMap, SimType, supplierTypeMap } from "@/systems/simulation/wpilib_brain/WPILibBrain"
import World from "@/systems/World"
import { Random } from "@/util/Random"
import { XYPosition } from "@xyflow/react"
import WiringNode from "./WiringNode"

let id = 0
export function genId(): number {
    return ++id
}

export function genRandomId(): number {
    return Math.floor(Random() * 1000000)
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
    if (genId != undefined) {
        return genId
    } else {
        return makeMapping(savedId)
    }
}
export function genIdToSavedId(genId: string): string | undefined {
    return genToSavedMap.get(genId)
}

export const configItemInfoCompare: (a: [string, ConfigItemInfo], b: [string, ConfigItemInfo]) => number
    = ([_aK, aV], [_bK, bV]) => aV.displayName.localeCompare(bV.displayName)

export const NODE_ID_ROBOT_IO = "robot-io-node"
export const NODE_ID_SIM_OUT = "sim-output-node"
export const NODE_ID_SIM_IN = "sim-input-node"

export type ConfigState = "wiring" | "simIO" | "robotIO"
export type HandleType = SimType | StimulusType | DriverType
export enum FuncType {
    Junction = "junct",
    Constructor = "construct",
    Deconstructor = "deconstruct"
}
export type ConfigItemInfo = {
    id: string,
    displayName: string,
    enabled: boolean,
    noraType: NoraTypes,
    handleType?: HandleType
    many: boolean
}

export type SourceHandle = {
    nodeId: string
    isSource: true
    noraType: NoraTypes
    handleType?: HandleType
    supplierId: string
}

export type TargetHandle = {
    nodeId: string
    isSource: false
    noraType: NoraTypes
    handleType?: HandleType
    receiverId: string
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

export type NodeInfo = {
    id: string
    type: string
    funcType?: FuncType
    position: XYPosition
}

export type SimConfigData = {
    sourceHandles: Map<string, ConfigItemInfo>
    targetHandles: Map<string, ConfigItemInfo>

    connections: Map<number, { sourceId: string, targetId: string }>
    adjacency: Map<string, Set<number>>

    nodes: Map<string, NodeInfo>
}

export class SimConfig {
    private constructor() { }

    public static Default(assembly: MirabufSceneObject) {
        const config: SimConfigData = {
            sourceHandles: new Map(),
            targetHandles: new Map(),

            connections: new Map(),
            adjacency: new Map(),

            nodes: new Map(),
        }
        config.nodes.set(NODE_ID_ROBOT_IO, { id: NODE_ID_ROBOT_IO, type: WiringNode.name, position: { x: 0, y: 0 } })
        config.nodes.set(NODE_ID_SIM_IN, { id: NODE_ID_SIM_IN, type: WiringNode.name, position: { x: 800, y: 0 } })
        config.nodes.set(NODE_ID_SIM_OUT, { id: NODE_ID_SIM_OUT, type: WiringNode.name, position: { x: -800, y: 0 } })
        getDriverSignals(assembly).forEach(x => {
            if (x.info?.GUID) {
                const handle: TargetHandle = {
                    nodeId: NODE_ID_SIM_IN,
                    isSource: false,
                    handleType: x.id.type,
                    noraType: x.getReceiverType(),
                    receiverId: x.idStr
                }
                const id = JSON.stringify(handle)
                this.AddTargetHandle(config, id, { id: id, displayName: x.DisplayName(), enabled: true, noraType: handle.noraType, handleType: x.id.type, many: hasNoraAverageFunc(handle.noraType) })
            }
        })
        getStimulusSignals(assembly).forEach(x => {
            if (x.info?.GUID) {
                const handle: SourceHandle = {
                    nodeId: NODE_ID_SIM_OUT,
                    isSource: true,
                    handleType: x.id.type,
                    noraType: x.getSupplierType(),
                    supplierId: x.idStr
                }
                const id = JSON.stringify(handle)
                this.AddSourceHandle(config, id, { id: id, displayName: x.DisplayName(), enabled: true, noraType: handle.noraType, handleType: x.id.type, many: true })
            }
        })
        getCANMotors().forEach(([id, _]) => {
            const handle: SourceHandle = {
                nodeId: NODE_ID_ROBOT_IO,
                isSource: true,
                handleType: SimType.CANMotor,
                noraType: supplierTypeMap[SimType.CANMotor]!,
                supplierId: id
            }
            const handleId = JSON.stringify(handle)
            this.AddSourceHandle(config, handleId, { id: handleId, displayName: displayNameCAN(id), enabled: true, noraType: handle.noraType, handleType: SimType.CANMotor, many: true })
        })
        getCANEncoder().forEach(([id, _]) => {
            const handle: TargetHandle = {
                nodeId: NODE_ID_ROBOT_IO,
                isSource: false,
                handleType: SimType.CANEncoder,
                noraType: receiverTypeMap[SimType.CANEncoder]!,
                receiverId: id
            }
            const handleId = JSON.stringify(handle)
            this.AddTargetHandle(config, handleId, { id: handleId, displayName: displayNameCAN(id), enabled: true, noraType: handle.noraType, handleType: SimType.CANEncoder, many: hasNoraAverageFunc(handle.noraType) })
        })
        getPWMDevices().forEach(([id, _]) => {
            const handle: SourceHandle = {
                nodeId: NODE_ID_ROBOT_IO,
                isSource: true,
                handleType: SimType.PWM,
                noraType: supplierTypeMap[SimType.PWM]!,
                supplierId: id
            }
            const handleId = JSON.stringify(handle)
            this.AddSourceHandle(config, handleId, { id: handleId, displayName: displayNamePWM(id), enabled: true, noraType: handle.noraType, handleType: SimType.PWM, many: true })
        })
        getAccelDevices().forEach(([id, data]) => {
            const handle: TargetHandle = {
                nodeId: NODE_ID_ROBOT_IO,
                isSource: false,
                handleType: SimType.Accel,
                noraType: receiverTypeMap[SimType.Accel]!,
                receiverId: id
            }
            const handleId = JSON.stringify(handle)
            this.AddTargetHandle(config, handleId, { id: id, displayName: displayNameAccel(id), enabled: data.get("<init") == true, noraType: handle.noraType, handleType: SimType.Accel, many: hasNoraAverageFunc(handle.noraType) })
        })

        return config
    }

    private static getNodeId(config: SimConfigData): string {
        const id = (Random() * 1000000).toFixed()
        return config.nodes.has(id) ? this.getNodeId(config) : id
    }

    public static AddTargetHandle(config: SimConfigData, id: string, info: ConfigItemInfo): boolean {
        if (config.targetHandles.has(id))
            return false
        config.targetHandles.set(id, info)
        config.adjacency.set(id, new Set())
        return true
    }

    public static AddSourceHandle(config: SimConfigData, id: string, info: ConfigItemInfo): boolean {
        if (config.sourceHandles.has(id))
            return false
        config.sourceHandles.set(id, info)
        config.adjacency.set(id, new Set())
        return true
    }

    public static RemoveTargetHandle(config: SimConfigData, id: string): boolean {
        if (!config.targetHandles.has(id))
            return false;
        const edgeIds = config.adjacency.get(id)
        if (edgeIds == undefined)
            return false
        edgeIds.forEach(x => this.DeleteEdge(config, x))
        config.adjacency.delete(id)
        config.targetHandles.delete(id)
        return true
    }

    public static RemoveSourceHandle(config: SimConfigData, id: string): boolean {
        if (!config.sourceHandles.has(id))
            return false;
        const edgeIds = config.adjacency.get(id)
        if (edgeIds == undefined)
            return false
        edgeIds.forEach(x => this.DeleteEdge(config, x))
        config.adjacency.delete(id)
        config.sourceHandles.delete(id)
        return true
    }

    // private static AddNode(config: SimConfigData, info: NodeInfo, handleCreator: HandleCreator): boolean {
    //     if (config.nodes.has(info.id))
    //         return false
    //     config.nodes.set(info.id, info)
    //     handleCreator.createNodeHandles(config)
    //     return true
    // }

    public static DeleteNode(config: SimConfigData, id: string): boolean {
        if (!config.nodes.has(id))
            return false
        const targetHandles: string[] = []
        const sourceHandles: string[] = []
        config.targetHandles.forEach((_, k) => {
            const handle = JSON.parse(k) as TargetHandle
            if (handle.nodeId == id) {
                targetHandles.push(k)
            }
        })
        config.sourceHandles.forEach((_, k) => {
            const handle = JSON.parse(k) as SourceHandle
            if (handle.nodeId == id)
                sourceHandles.push(k)
        })
        targetHandles.forEach(x => this.RemoveTargetHandle(config, x))
        sourceHandles.forEach(x => this.RemoveSourceHandle(config, x))
        return config.nodes.delete(id)
    }

    public static AddJunctionNode(config: SimConfigData): string {
        const id = this.getNodeId(config)
        config.nodes.set(id, {
            id: id,
            type: WiringNode.name,
            position: { x: 0, y: 0 },
            funcType: FuncType.Junction
        })
        const targetHandle: TargetHandle = {
            nodeId: id,
            handleType: SimType.SimDevice,
            isSource: false,
            noraType: NoraTypes.Number,
            receiverId: `target_${id}`
        }
        const targetId = JSON.stringify(targetHandle)
        const sourceHandle: SourceHandle = {
            nodeId: id,
            handleType: SimType.SimDevice,
            isSource: true,
            noraType: NoraTypes.Number,
            supplierId: `source_${id}`
        }
        const sourceId = JSON.stringify(sourceHandle)
        this.AddTargetHandle(config, targetId, { displayName: "In", enabled: true, id: targetId, noraType: NoraTypes.Number, many: hasNoraAverageFunc(targetHandle.noraType) })
        this.AddSourceHandle(config, sourceId, { displayName: "Out", enabled: true, id: sourceId, noraType: NoraTypes.Number, many: true })
        return id
    }

    public static AddDeconstructorNode(config: SimConfigData, targetNoraType: NoraTypes, positionHint?: XYPosition): string | undefined {
        const types = deconstructNoraType(targetNoraType)
        if (types == undefined || types.length == 0)
            return undefined

        const id = this.getNodeId(config)
        config.nodes.set(id, {
            id: id,
            type: WiringNode.name,
            position: positionHint ?? { x: 0, y: 0 },
            funcType: FuncType.Deconstructor
        })
        const targetHandle: TargetHandle = {
            nodeId: id,
            handleType: SimType.SimDevice,
            isSource: false,
            noraType: targetNoraType,
            receiverId: `target_${id}`
        }
        const targetId = JSON.stringify(targetHandle)
        this.AddTargetHandle(config, targetId, { displayName: "In", enabled: true, id: targetId, noraType: targetHandle.noraType, many: hasNoraAverageFunc(targetHandle.noraType) })

        types.forEach((sourceType, i) => {
            const sourceHandle: SourceHandle = {
                nodeId: id,
                handleType: SimType.SimDevice,
                isSource: true,
                noraType: sourceType,
                supplierId: `source_${i}_${id}`
            }
            const sourceId = JSON.stringify(sourceHandle)
            this.AddSourceHandle(config, sourceId, { displayName: `Out ${i + 1}`, enabled: true, id: sourceId, noraType: sourceHandle.noraType, many: true })
        })

        return targetId
    }

    public static AddConstructorNode(config: SimConfigData, sourceNoraType: NoraTypes, positionHint?: XYPosition): string | undefined {
        const types = deconstructNoraType(sourceNoraType)
        if (types == undefined || types.length == 0)
            return undefined

        const id = this.getNodeId(config)
        config.nodes.set(id, {
            id: id,
            type: WiringNode.name,
            position: positionHint ?? { x: 0, y: 0 },
            funcType: FuncType.Constructor
        })
        const sourceHandle: SourceHandle = {
            nodeId: id,
            handleType: SimType.SimDevice,
            isSource: true,
            noraType: sourceNoraType,
            supplierId: `target_${id}`
        }
        const sourceId = JSON.stringify(sourceHandle)
        this.AddSourceHandle(config, sourceId, { displayName: "In", enabled: true, id: sourceId, noraType: sourceHandle.noraType, many: true })

        types.forEach((targetType, i) => {
            const targetHandle: TargetHandle = {
                nodeId: id,
                handleType: SimType.SimDevice,
                isSource: false,
                noraType: targetType,
                receiverId: `source_${i}_${id}`
            }
            const targetId = JSON.stringify(targetHandle)
            this.AddTargetHandle(config, targetId, { displayName: `In ${i + 1}`, enabled: true, id: targetId, noraType: targetHandle.noraType, many: hasNoraAverageFunc(targetType) })
        })

        return sourceId
    }

    // private static determineHandleType(config: SimConfigData, handle: DataHandle | JunctionHandle): HandleType | undefined {
    //     if (!handle.isJunction)
    //         return handle.type;

    //     const junct = config.junctions.get(handle.id)!
    //     return handle.dir == "source" ? junct?.sourceType : junct?.targetType
    // }

    // private static validateGraph(config: SimConfigData, source: string, target: string): boolean {
    //     const validatedSources = new Set<string>()
    //     const checkList: string[] = [...config.simOut.keys(), ...config.robotOut.keys()]
    //     while (checkList.length > 0) {
    //         const sourceId = checkList.pop()
    //     }
    // }

    public static GetEdge(config: SimConfigData, sourceId: string, targetId: string): number | undefined {
        const targetEdges = config.adjacency.get(targetId)!
        return [...config.adjacency.get(sourceId)!.keys()].filter(x => targetEdges.has(x))[0]
    }

    public static ValidateConnection(config: SimConfigData, sourceId: string, targetId: string): boolean {
        const sourceInfo = config.sourceHandles.get(sourceId)
        const targetInfo = config.targetHandles.get(targetId)
        if (sourceInfo == undefined || targetInfo == undefined)
            return false

        if (!targetInfo.many && config.adjacency.get(targetId)!.size >= 1)
            return false

        return sourceInfo.noraType == targetInfo.noraType
    }

    public static MakeConnection(config: SimConfigData, sourceId: string, targetId: string): boolean {
        if (!this.ValidateConnection(config, sourceId, targetId)) {
            console.debug("Failed to make edge")
            return false
        }

        if (this.GetEdge(config, sourceId, targetId) != undefined) {
            console.debug("Connection already exists")
            return false
        }
        
        let edgeId = genRandomId()
        while (config.connections.has(edgeId)) {
            edgeId = genRandomId()
        }
        config.connections.set(edgeId, { sourceId: sourceId, targetId: targetId })
        config.adjacency.get(sourceId)?.add(edgeId)
        config.adjacency.get(targetId)?.add(edgeId)
        return true
    }

    public static DeleteConnection(config: SimConfigData, sourceId: string, targetId: string): boolean {
        const edgeId = this.GetEdge(config, sourceId, targetId)
        if (edgeId == undefined)
            return false
        config.adjacency.get(sourceId)?.delete(edgeId)
        config.adjacency.get(targetId)?.delete(edgeId)
        config.connections.delete(edgeId)
        return true
    }

    public static DeleteEdge(config: SimConfigData, edgeId: number): boolean {
        const edge = config.connections.get(edgeId)
        if (edge == undefined)
            return false
        config.adjacency.get(edge.sourceId)!.delete(edgeId)
        config.adjacency.get(edge.targetId)!.delete(edgeId)
        config.connections.delete(edgeId)
        return true
    }
}
