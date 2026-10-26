import { areTypesCompatible } from "../Nora"
import { edgesOf, NodeKindId, type HandleIdAlias, type SimConfigData } from "./SimGraph"

export function validateConnection(config: SimConfigData, sourceId: HandleIdAlias, targetId: HandleIdAlias): boolean {
    const source = config.handles[sourceId]
    const target = config.handles[targetId]

    if (!source || !target) return false
    if (!target.many && edgesOf(config, targetId).length >= 1) return false // >= because 1 new connection would be invalid
    if (source.noraType === null || target.noraType === null) return true // null is wildcard, so any type works

    return areTypesCompatible(source.noraType, target.noraType)
}

export function recomputeJunctionTypes(config: SimConfigData) {
    const junctionHandles = Object.values(config.handles).filter(
        h => config.nodes[h.nodeId]?.kind === NodeKindId.JUNCTION
    )
    junctionHandles.forEach(h => (h.noraType = null))
    let changed = true
    while (changed) {
        changed = false

        Object.values(config.edges).forEach(({ sourceId, targetId }) => {
            const source = config.handles[sourceId]
            const target = config.handles[targetId]
            if (!source || !target) return

            // junction handle is guaranteed to be null, other one must not be, thus should always be the case that
            // exactly one noraType is null and one is not null
            const [typed, untyped] = source.noraType !== null ? [source, target] : [target, source]
            if (typed.noraType === null || untyped.noraType !== null) return
            if (config.nodes[untyped.nodeId]?.kind !== NodeKindId.JUNCTION) return

            junctionHandles.filter(x => x.nodeId === untyped.nodeId).forEach(x => (x.noraType = typed.noraType))
            changed = true
        })
    }
}
