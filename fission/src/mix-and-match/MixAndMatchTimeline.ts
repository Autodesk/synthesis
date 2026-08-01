import { convertArrayToThreeMatrix4, convertThreeMatrix4ToArray } from "@/util/TypeConversions"
import type { ComponentId, LibraryPartRef, TimelineEntry, TransformArray } from "./MixAndMatchTypes"

/**
 * Pure replay of a session timeline into the state it describes. No scene, no physics, no Jolt, so
 * this stays cheap enough to re-run on every edit and testable on its own.
 */

export interface WeldLink {
    /** The component this one hangs off of. Welds form a tree: exactly one parent per component. */
    parentId: ComponentId
    /** Offset of this component's root relative to its parent's root, recorded at weld time. */
    relativeOffset: TransformArray
}

export interface ComponentState {
    id: ComponentId
    libraryPartRef: LibraryPartRef
    /** World transform of the component's root body. */
    transform: TransformArray
    sizeOption?: string
    weld?: WeldLink
}

export interface TimelineState {
    /** Live components, in spawn order. Deleted components are absent. */
    components: Map<ComponentId, ComponentState>
}

/** @returns The component and everything welded onto it, directly or transitively. */
export function subtreeOf(components: ReadonlyMap<ComponentId, ComponentState>, rootId: ComponentId): ComponentId[] {
    const collected: ComponentId[] = []
    const pending: ComponentId[] = [rootId]
    const seen = new Set<ComponentId>([rootId])

    while (pending.length > 0) {
        const current = pending.pop()!
        collected.push(current)

        components.forEach(component => {
            if (component.weld?.parentId !== current || seen.has(component.id)) return

            seen.add(component.id)
            pending.push(component.id)
        })
    }

    return collected
}

/** @returns Whether `ancestorId` is reachable by walking up `componentId`'s weld parents. */
export function isWeldedUnder(
    components: ReadonlyMap<ComponentId, ComponentState>,
    componentId: ComponentId,
    ancestorId: ComponentId
): boolean {
    const seen = new Set<ComponentId>()
    let current = components.get(componentId)?.weld?.parentId

    while (current != null && !seen.has(current)) {
        if (current === ancestorId) return true

        seen.add(current)
        current = components.get(current)?.weld?.parentId
    }

    return false
}

/** @returns Every active weld, child first, for baking into constraints. */
export function weldPairs(state: TimelineState): { parentId: ComponentId; childId: ComponentId }[] {
    const pairs: { parentId: ComponentId; childId: ComponentId }[] = []
    state.components.forEach(component => {
        if (component.weld) pairs.push({ parentId: component.weld.parentId, childId: component.id })
    })

    return pairs
}

function applyMove(state: TimelineState, componentId: ComponentId, transform: TransformArray) {
    const component = state.components.get(componentId)
    if (!component) return

    // A welded group is rigid, so dragging one part carries everything welded onto it by the same
    // world delta. Descendants don't get their own timeline entries; they fall out of this.
    const previous = convertArrayToThreeMatrix4(component.transform)
    const delta = convertArrayToThreeMatrix4(transform).multiply(previous.invert())

    component.transform = [...transform]

    subtreeOf(state.components, componentId).forEach(id => {
        if (id === componentId) return

        const descendant = state.components.get(id)!
        const moved = convertArrayToThreeMatrix4(descendant.transform).premultiply(delta)
        descendant.transform = [...convertThreeMatrix4ToArray(moved)]
    })
}

function applyWeld(state: TimelineState, parentId: ComponentId, childId: ComponentId, offset: TransformArray) {
    const child = state.components.get(childId)
    if (!child || parentId === childId || !state.components.has(parentId)) return

    // Welding a component onto its own descendant would close the tree into a cycle.
    if (isWeldedUnder(state.components, parentId, childId)) return

    child.weld = { parentId, relativeOffset: [...offset] }
}

function applyDelete(state: TimelineState, componentId: ComponentId) {
    if (!state.components.delete(componentId)) return

    // Welds pointing at a component that no longer exists are invalidated here rather than left for
    // bake to trip over.
    state.components.forEach(component => {
        if (component.weld?.parentId === componentId) component.weld = undefined
    })
}

/**
 * Replays a timeline into the state it describes.
 *
 * @param   entries Ordered timeline entries.
 * @param   count   How many entries to apply. Defaults to all of them; pass fewer to scrub back.
 * @returns The resulting state. Entries that no longer make sense (moving a deleted component,
 *          welding a component onto itself) are skipped rather than treated as errors.
 */
export function replayTimeline(entries: readonly TimelineEntry[], count?: number): TimelineState {
    const state: TimelineState = { components: new Map() }
    const limit = Math.max(0, Math.min(count ?? entries.length, entries.length))

    for (let i = 0; i < limit; i++) {
        const entry = entries[i]

        switch (entry.type) {
            case "spawn":
                state.components.set(entry.componentId, {
                    id: entry.componentId,
                    libraryPartRef: entry.libraryPartRef,
                    transform: [...entry.transform],
                })
                break
            case "move":
                applyMove(state, entry.componentId, entry.transform)
                break
            case "weld":
                applyWeld(state, entry.componentA, entry.componentB, entry.relativeOffset)
                break
            case "resize": {
                const component = state.components.get(entry.componentId)
                if (component) component.sizeOption = entry.sizeOption
                break
            }
            case "delete":
                applyDelete(state, entry.componentId)
                break
        }
    }

    return state
}
