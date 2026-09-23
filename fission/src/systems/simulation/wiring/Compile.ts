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

export type CompileResult = { flows: SimFlow[]; error?: undefined } | { flows?: undefined; error: string }

export function compile(config: SimConfigData, assembly: MirabufSceneObject): CompileResult {
    const simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)
    if (!simLayer) return { error: "No simulation layer found for the selected robot" }

    const ctx: CompileCtx = { config, simLayer, encountered: new Set() }
    try {
        const flows = Object.values(config.handles)
            .filter(h => !h.isSource && h.enabled && edgesOf(config, h.id).length > 0)
            .filter(h => NODE_KINDS[config.nodes[h.nodeId]!.kind].makeReceiver !== undefined)
            .map(h => {
                const receiver = NODE_KINDS[config.nodes[h.nodeId]!.kind].makeReceiver!(h, ctx)
                const supplier = compileSuppliersFor(ctx, h.id)
                if (!receiver) throw new Error(`Could not create a receiver for '${h.displayName}'`)
                if (!supplier) throw new Error(`Could not create a supplier for '${h.displayName}'`)
                return { supplier, receiver }
            })
        return { flows }
    } catch (error) {
        console.error("Compilation failed", error)
        return { error: error instanceof Error ? error.message : String(error) }
    }
}
