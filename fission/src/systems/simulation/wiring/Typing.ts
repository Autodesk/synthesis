import { areTypesCompatible } from "../Nora"
import { FuncType, type HandleIdAlias, type SimConfigData } from "./SimGraph"

export function validateConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    const sourceInfo = config.handles[sourceId]
    const targetInfo = config.handles[targetId]
    console.log(sourceInfo, targetInfo)
    if (sourceInfo === undefined || targetInfo === undefined) return false

    if (!targetInfo.many && Object.entries(config.adjacency[targetId])!.length >= 1) return false

    if (sourceInfo.noraType === null || targetInfo.noraType === null) return true

    return areTypesCompatible(sourceInfo.noraType, targetInfo.noraType)
}

export function recomputeJunctionTypes(config: SimConfigData) {
    const junctionHandles = Object.values(config.handles).filter(
        x => config.nodes[x.nodeId]?.funcType === FuncType.JUNCTION
    )
    junctionHandles.forEach(x => (x.noraType = null))

    let changed = true
    while (changed) {
        changed = false
        Object.values(config.edges).forEach(({ sourceId, targetId }) => {
            const source = config.handles[sourceId]
            const target = config.handles[targetId]
            if (!source || !target) return

            const [typed, untyped] = source.noraType !== null ? [source, target] : [target, source]
            if (typed.noraType === null || untyped.noraType !== null) return
            if (config.nodes[untyped.nodeId]?.funcType !== FuncType.JUNCTION) return

            junctionHandles.filter(x => x.nodeId === untyped.nodeId).forEach(x => (x.noraType = typed.noraType))
            changed = true
        })
    }
}
