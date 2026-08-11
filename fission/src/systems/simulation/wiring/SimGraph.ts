// NOTE: I decided to make noraType accept null specifically for Junction nodes:
// the input and output should be null to accept any type, and output should update based

import type { XYPosition } from "@xyflow/react"
import type { DriverType } from "@/systems/simulation/driver/Driver"
import type { StimulusType } from "@/systems/simulation/stimulus/Stimulus"
import { random } from "@/util/Random"
import type { AggregateStrategy } from "../wpilib_brain/SimDataFlow"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import type { NoraType } from "../Nora"
import { recomputeJunctionTypes, validateConnection } from "./Typing"

export type OriginType = SimType | StimulusType | DriverType
export enum FuncType {
    JUNCTION = "junct",
    CONSTRUCTOR = "construct",
    DECONSTRUCTOR = "deconstruct",
}

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

export type NodeIdAlias = string
export type HandleIdAlias = string
export type EdgeIdAlias = string
export type EdgeAlias = { sourceId: HandleIdAlias; targetId: HandleIdAlias }

export type SimConfigData = {
    handles: { [k: HandleIdAlias]: HandleInfo }
    edges: { [k: EdgeIdAlias]: EdgeAlias }
    adjacency: { [k: HandleIdAlias]: { [j: EdgeIdAlias]: boolean } }
    nodes: { [k: NodeIdAlias]: NodeInfo }
}

// #region ID handling
function genRandomId(): string {
    return Math.floor(random() * Number.MAX_SAFE_INTEGER).toString()
}

export const genNewId = (existing: SimConfigData["handles"] | SimConfigData["edges"]) => {
    let handleId = ""
    do {
        handleId = genRandomId()
    } while (existing[handleId] !== undefined)

    return handleId
}

// #endregion

export const handleInfoDisplayCompare: (a: HandleInfo, b: HandleInfo) => number = (a, b) =>
    a.displayName.localeCompare(b.displayName)

export function addHandle(config: SimConfigData, info: HandleInfo) {
    const handleId = info.id !== "" ? info.id : genNewId(config.handles)

    info.id = handleId
    config.handles[handleId] = info
    config.adjacency[handleId] = {}
}

/**
 * Deletes a handle and all connected edges
 */
export function removeHandle(config: SimConfigData, id: HandleIdAlias): boolean {
    if (config.handles[id] === undefined) return false
    const edgeIds = config.adjacency[id]
    if (edgeIds === undefined) return false
    Object.keys(edgeIds).forEach(x => deleteEdge(config, x))
    delete config.adjacency[id]
    delete config.handles[id]
    return true
}

export function removeNode(config: SimConfigData, id: NodeIdAlias): boolean {
    if (config.nodes[id] === undefined) return false
    Object.values(config.handles)
        .filter(x => x.nodeId === id)
        .forEach(x => removeHandle(config, x.id))
    delete config.nodes[id]
    return true
}

export function getEdge(
    config: SimConfigData,
    sourceId: HandleIdAlias,
    targetId: HandleIdAlias
): EdgeIdAlias | undefined {
    const targetEdges = config.adjacency[targetId]!
    const sourceEdges = Object.keys(config.adjacency[sourceId]!)

    return sourceEdges.filter(edge => targetEdges[edge] !== undefined)[0] // should be guaranteed to be 0
}

export function makeConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    if (!validateConnection(config, sourceId, targetId)) {
        console.error("Failed to make edge")
        return false
    }

    if (getEdge(config, sourceId, targetId) !== undefined) {
        console.error("Connection already exists")
        return false
    }

    let edgeId = genNewId(config.edges)

    config.edges[edgeId] = { sourceId: sourceId, targetId: targetId }
    config.adjacency[sourceId][edgeId] = true
    config.adjacency[targetId][edgeId] = true
    recomputeJunctionTypes(config)
    return true
}

export function deleteConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    const edgeId = getEdge(config, sourceId, targetId)
    return edgeId ? deleteEdge(config, edgeId) : false
}

export function deleteEdge(config: SimConfigData, edgeId: EdgeIdAlias): boolean {
    const edge = config.edges[edgeId]
    if (edge === undefined) return false
    delete config.adjacency[edge.sourceId][edgeId]
    delete config.adjacency[edge.targetId][edgeId]
    delete config.edges[edgeId]
    recomputeJunctionTypes(config)
    return true
}
