import * as THREE from "three"
import { describe, expect, test } from "vitest"
import MixAndMatchBuild from "@/mix-and-match/MixAndMatchBuild"
import { convertThreeMatrix4ToArray } from "@/util/TypeConversions"

const ORIGIN = [...convertThreeMatrix4ToArray(new THREE.Matrix4())]

/**
 * Scrub-vs-truncate: scrubbing is a preview and never edits the timeline. Rolling back and resuming
 * is the only thing that discards steps, and it is always explicit.
 */
describe("Mix and Match Scrub", () => {
    function build() {
        const created = new MixAndMatchBuild()
        const frame = created.spawn("frame", ORIGIN)
        const pod = created.spawn("pod", ORIGIN)
        created.weld(frame, pod, ORIGIN)

        return created
    }

    test("Scrubbing Keeps The Whole Timeline", () => {
        const created = build()
        created.scrubTo(1)

        expect(created.timeline).toHaveLength(3)
        expect(created.state.components.size).toBe(1)
    })

    test("Scrubbing Is Reversible", () => {
        const created = build()
        const before = created.state.components.get("c2")!.weld!.parentId

        created.scrubTo(0)
        created.scrubTo(created.timeline.length)

        expect(created.state.components.get("c2")!.weld!.parentId).toBe(before)
    })

    test("Clamps Out Of Range Markers", () => {
        const created = build()

        created.scrubTo(-5)
        expect(created.marker).toBe(0)

        created.scrubTo(500)
        expect(created.marker).toBe(3)
        expect(created.isScrubbed).toBe(false)
    })

    test("Resuming Here Discards Only What Follows The Playhead", () => {
        const created = build()
        created.scrubTo(2)

        expect(created.discardedByNextEdit).toBe(1)

        created.truncateToMarker()

        expect(created.timeline).toHaveLength(2)
        expect(created.isScrubbed).toBe(false)
        expect(created.state.components.get("c2")!.weld).toBeUndefined()
    })

    test("Resuming While Caught Up Does Nothing", () => {
        const created = build()
        created.truncateToMarker()

        expect(created.timeline).toHaveLength(3)
    })
})
