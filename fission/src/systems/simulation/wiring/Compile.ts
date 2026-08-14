import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { areTypesCompatible, type NoraType } from "../Nora"
import { AggregateStrategy, aggregateValues, type SimFlow, type SimSupplier } from "../wpilib_brain/SimDataFlow"
import { type CompileCtx, NODE_KINDS } from "./NodeKinds"
import { edgesOf, type HandleIdAlias, type SimConfigData } from "./SimGraph"
import World from "@/systems/World"

export function compileSuppliersFor(ctx: CompileCtx, targetHandleId: HandleIdAlias): SimSupplier<NoraType> | undefined {
    const target = ctx.config.handles[targetHandleId]
    if (!target) return undefined

    const suppliers = edgesOf(ctx.config, targetHandleId)
        .map(([_, e]) => ctx.config.handles[e.sourceId])
        .filter(source => source && !ctx.encountered.has(source.id))
        .filter(source => areTypesCompatible(source!.noraType!, target.noraType!))
        .flatMap(source => {
            ctx.encountered.add(source!.id)
            const supplier = NODE_KINDS[ctx.config.nodes[source!.nodeId]!.kind].makeSupplier?.(source!, ctx)
            ctx.encountered.delete(source!.id)
            return supplier ? [supplier] : []
        })

    if (suppliers.length === 0) return undefined
    if (suppliers.length === 1) return suppliers[0]

    const strategy = ctx.config.nodes[target.nodeId]?.aggregateStrategy ?? AggregateStrategy.AVERAGE

    return {
        supplierType: target.noraType!,
        getSupplierValue: () =>
            aggregateValues(
                strategy,
                target.noraType!,
                suppliers.map(s => s.getSupplierValue())
            ),
    }
}

export function compile(config: SimConfigData, assembly: MirabufSceneObject): SimFlow[] | undefined {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    if (!simLayer) return undefined

    const ctx: CompileCtx = { config, simLayer, encountered: new Set() }
    try {
        return Object.values(config.handles)
            .filter(h => !h.isSource && h.enabled && edgesOf(config, h.id).length > 0)
            .filter(h => NODE_KINDS[config.nodes[h.nodeId]!.kind].makeReceiver !== undefined)
            .map(h => {
                const receiver = NODE_KINDS[config.nodes[h.nodeId]!.kind].makeReceiver!(h, ctx)
                const supplier = compileSuppliersFor(ctx, h.id)
                if (!receiver || !supplier) throw new Error(`Failed to compile flow for handle ${h.id}`)
                return { supplier, receiver }
            })
    } catch (error) {
        console.error("Compilation failed", error)
        return undefined
    }
}
