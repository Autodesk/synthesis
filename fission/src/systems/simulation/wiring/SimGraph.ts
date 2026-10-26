import type { XYPosition } from "@xyflow/react"
import type { NoraType } from "../Nora"
import type { AggregateStrategy } from "../wpilib_brain/SimDataFlow"
import { random } from "@/util/Random"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import type { StimulusType } from "../stimulus/Stimulus"
import type { DriverType } from "../driver/Driver"
import { recomputeJunctionTypes, validateConnection } from "./Typing"

export enum NodeKindId {
    ROBOT_IO = "robotIO",
    SIM_INPUT = "simInput",
    SIM_OUTPUT = "simOutput",
    JUNCTION = "junction",
    CONSTRUCTOR = "constructor",
    DECONSTRUCTOR = "deconstructor",
}

export type OriginType = SimType | StimulusType | DriverType

export type HandleIdAlias = string
export type NodeIdAlias = string
export type EdgeIdAlias = string

export type HandleInfo = {
    id: HandleIdAlias
    nodeId: NodeIdAlias
    noraType: NoraType | null
    originType?: OriginType
    originId: string
    index?: number
    displayName: string
    enabled: boolean
    many: boolean
    isSource: boolean
}

export type NodeInfo = {
    id: NodeIdAlias
    kind: NodeKindId
    position: XYPosition
    tooltip?: string
    aggregateStrategy?: AggregateStrategy
}

export type EdgeAlias = { sourceId: HandleIdAlias; targetId: HandleIdAlias }

export type SimConfigData = {
    nodes: { [k: NodeIdAlias]: NodeInfo }
    handles: { [k: HandleIdAlias]: HandleInfo }
    edges: { [k: EdgeIdAlias]: EdgeAlias }
}

const genRandomId = () => Math.floor(random() * Number.MAX_SAFE_INTEGER).toString()

export const genNewId = (existing: SimConfigData[keyof SimConfigData]) => {
    let handleId = ""
    do {
        handleId = genRandomId()
    } while (existing[handleId] !== undefined)
    return handleId
}

export const handleInfoDisplayCompare: (a: HandleInfo, b: HandleInfo) => number = (a, b) =>
    a.displayName.localeCompare(b.displayName)

export const edgesOf = (config: SimConfigData, handleId: HandleIdAlias) =>
    Object.entries(config.edges).filter(([_, e]) => e.sourceId === handleId || e.targetId === handleId)

export const handlesOf = (config: SimConfigData, nodeId: NodeIdAlias) =>
    Object.values(config.handles).filter(h => h.nodeId === nodeId)

export const nodeSources = (config: SimConfigData, nodeId: NodeIdAlias) =>
    handlesOf(config, nodeId)
        .filter(h => h.isSource)
        .sort((a, b) => (a.index ?? 0) - (b.index ?? 0))

export const nodeTargets = (config: SimConfigData, nodeId: NodeIdAlias) =>
    handlesOf(config, nodeId)
        .filter(h => !h.isSource)
        .sort((a, b) => (a.index ?? 0) - (b.index ?? 0))

/* ------------------------------------------------------------------------------------------------------------ */

export function addHandle(config: SimConfigData, info: HandleInfo) {
    info.id = info.id !== "" ? info.id : genNewId(config.handles)
    config.handles[info.id] = info
}

export function removeHandle(config: SimConfigData, id: HandleIdAlias): boolean {
    if (config.handles[id] === undefined) return false

    edgesOf(config, id).forEach(([edge, _]) => removeEdge(config, edge))
    delete config.handles[id]
    return true
}

export function removeNode(config: SimConfigData, id: NodeIdAlias): boolean {
    if (config.nodes[id] === undefined) return false

    handlesOf(config, id).forEach(handle => removeHandle(config, handle.id))
    delete config.nodes[id]
    return true
}

export function removeEdge(config: SimConfigData, id: EdgeIdAlias): boolean {
    if (config.edges[id] === undefined) return false

    delete config.edges[id]
    recomputeJunctionTypes(config)
    return true
}

export const getEdge = (config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias) =>
    Object.entries(config.edges).find(([_, e]) => e.sourceId === sourceId && e.targetId === targetId)?.[0]

export function makeConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    if (!validateConnection(config, sourceId, targetId)) return false
    if (getEdge(config, sourceId, targetId) !== undefined) return false

    config.edges[genNewId(config.edges)] = { sourceId, targetId }
    recomputeJunctionTypes(config)

    return true
}

export function removeConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    const edgeId = getEdge(config, sourceId, targetId)
    return edgeId ? removeEdge(config, edgeId) : false
}
