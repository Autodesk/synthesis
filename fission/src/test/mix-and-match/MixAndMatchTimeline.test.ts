import * as THREE from "three"
import { describe, expect, test } from "vitest"
import {
    isWeldedUnder,
    replayTimeline,
    subtreeOf,
    weldPairs,
    type TimelineState,
} from "@/mix-and-match/MixAndMatchTimeline"
import type { TimelineEntry } from "@/mix-and-match/MixAndMatchTypes"
import { convertArrayToThreeMatrix4, convertThreeMatrix4ToArray } from "@/util/TypeConversions"

function translation(x: number, y: number, z: number): number[] {
    return [...convertThreeMatrix4ToArray(new THREE.Matrix4().makeTranslation(x, y, z))]
}

function positionOf(state: TimelineState, id: string): THREE.Vector3 {
    return new THREE.Vector3().setFromMatrixPosition(convertArrayToThreeMatrix4(state.components.get(id)!.transform))
}

function spawn(id: string, x = 0, y = 0, z = 0): TimelineEntry {
    return { type: "spawn", componentId: id, libraryPartRef: `ref-${id}`, transform: translation(x, y, z) }
}

describe("Mix and Match Timeline Replay", () => {
    test("Spawns Components In Order", () => {
        const state = replayTimeline([spawn("c1"), spawn("c2", 1)])

        expect([...state.components.keys()]).toEqual(["c1", "c2"])
        expect(positionOf(state, "c2").x).toBeCloseTo(1)
    })

    test("Move Stores An Absolute Transform", () => {
        const state = replayTimeline([
            spawn("c1", 5),
            { type: "move", componentId: "c1", transform: translation(2, 0, 0) },
            { type: "move", componentId: "c1", transform: translation(3, 0, 0) },
        ])

        expect(positionOf(state, "c1").x).toBeCloseTo(3)
    })

    test("Moving A Parent Carries Its Welded Subtree", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2", 1),
            spawn("c3", 2),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(1, 0, 0) },
            { type: "weld", componentA: "c2", componentB: "c3", relativeOffset: translation(1, 0, 0) },
            { type: "move", componentId: "c1", transform: translation(0, 10, 0) },
        ])

        expect(positionOf(state, "c1").toArray()).toEqual([0, 10, 0])
        expect(positionOf(state, "c2").toArray()).toEqual([1, 10, 0])
        expect(positionOf(state, "c3").toArray()).toEqual([2, 10, 0])
    })

    test("Moving A Child Leaves Its Parent Alone", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2", 1),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(1, 0, 0) },
            { type: "move", componentId: "c2", transform: translation(1, 4, 0) },
        ])

        expect(positionOf(state, "c1").toArray()).toEqual([0, 0, 0])
        expect(positionOf(state, "c2").toArray()).toEqual([1, 4, 0])
    })

    test("A Component Has At Most One Weld Parent", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2"),
            spawn("c3"),
            { type: "weld", componentA: "c1", componentB: "c3", relativeOffset: translation(0, 0, 0) },
            { type: "weld", componentA: "c2", componentB: "c3", relativeOffset: translation(0, 0, 0) },
        ])

        expect(state.components.get("c3")!.weld!.parentId).toBe("c2")
        expect(weldPairs(state)).toEqual([{ parentId: "c2", childId: "c3" }])
    })

    test("Refuses Welds That Would Cycle", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2"),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(0, 0, 0) },
            { type: "weld", componentA: "c2", componentB: "c1", relativeOffset: translation(0, 0, 0) },
        ])

        expect(state.components.get("c1")!.weld).toBeUndefined()
        expect(state.components.get("c2")!.weld!.parentId).toBe("c1")
    })

    test("Delete Invalidates Welds Touching The Component", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2"),
            spawn("c3"),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(0, 0, 0) },
            { type: "weld", componentA: "c2", componentB: "c3", relativeOffset: translation(0, 0, 0) },
            { type: "delete", componentId: "c2" },
        ])

        expect(state.components.has("c2")).toBe(false)
        expect(state.components.get("c3")!.weld).toBeUndefined()
        expect(weldPairs(state)).toEqual([])
    })

    test("Resize Records The Selected Size", () => {
        const state = replayTimeline([spawn("c1"), { type: "resize", componentId: "c1", sizeOption: "28x28" }])

        expect(state.components.get("c1")!.sizeOption).toBe("28x28")
    })

    test("Scrubbing Back Reproduces The Earlier State", () => {
        const timeline: TimelineEntry[] = [
            spawn("c1"),
            spawn("c2", 1),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(1, 0, 0) },
            { type: "delete", componentId: "c2" },
        ]

        expect(replayTimeline(timeline, 0).components.size).toBe(0)
        expect(replayTimeline(timeline, 2).components.size).toBe(2)
        expect(replayTimeline(timeline, 3).components.get("c2")!.weld!.parentId).toBe("c1")
        expect(replayTimeline(timeline, 4).components.size).toBe(1)
    })

    test("Skips Entries For Components That No Longer Exist", () => {
        const state = replayTimeline([
            spawn("c1"),
            { type: "delete", componentId: "c1" },
            { type: "move", componentId: "c1", transform: translation(9, 9, 9) },
            { type: "resize", componentId: "c1", sizeOption: "big" },
        ])

        expect(state.components.size).toBe(0)
    })

    test("Subtree And Ancestry Helpers", () => {
        const state = replayTimeline([
            spawn("c1"),
            spawn("c2"),
            spawn("c3"),
            { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: translation(0, 0, 0) },
            { type: "weld", componentA: "c2", componentB: "c3", relativeOffset: translation(0, 0, 0) },
        ])

        expect(subtreeOf(state.components, "c1").sort()).toEqual(["c1", "c2", "c3"])
        expect(subtreeOf(state.components, "c3")).toEqual(["c3"])
        expect(isWeldedUnder(state.components, "c3", "c1")).toBe(true)
        expect(isWeldedUnder(state.components, "c1", "c3")).toBe(false)
    })
})
