import * as THREE from "three"
import { describe, expect, test } from "vitest"
import MixAndMatchBuild from "@/mix-and-match/MixAndMatchBuild"
import { parseSession, serializeSession } from "@/mix-and-match/MixAndMatchDocument"
import { convertThreeMatrix4ToArray } from "@/util/TypeConversions"

function translation(x: number, y: number, z: number): number[] {
    return [...convertThreeMatrix4ToArray(new THREE.Matrix4().makeTranslation(x, y, z))]
}

const ORIGIN = translation(0, 0, 0)

describe("Mix and Match Build", () => {
    test("Assigns Fresh Component Ids", () => {
        const build = new MixAndMatchBuild()

        expect(build.spawn("ref", ORIGIN)).toBe("c1")
        expect(build.spawn("ref", ORIGIN)).toBe("c2")
        build.delete("c1")
        expect(build.spawn("ref", ORIGIN)).toBe("c3")
    })

    test("Every Action Lands On The Timeline", () => {
        const build = new MixAndMatchBuild()
        const frame = build.spawn("frame", ORIGIN)
        const pod = build.spawn("pod", translation(1, 0, 0))
        build.move(pod, translation(1, 0, 1))
        build.weld(frame, pod, translation(1, 0, 1))
        build.resize(frame, "28x28")
        build.delete(pod)

        expect(build.timeline.map(x => x.type)).toEqual(["spawn", "spawn", "move", "weld", "resize", "delete"])
        expect(build.marker).toBe(6)
        expect(build.isScrubbed).toBe(false)
    })

    test("Deleting A Component Cascades To Everything Welded Onto It", () => {
        const build = new MixAndMatchBuild()
        const frame = build.spawn("frame", ORIGIN)
        const pod = build.spawn("pod", translation(1, 0, 0))
        const antenna = build.spawn("antenna", translation(2, 0, 0))
        build.weld(frame, pod, translation(1, 0, 0))
        build.weld(pod, antenna, translation(1, 0, 0))

        build.delete(frame)

        expect(build.state.components.size).toBe(0)
    })

    test("Rejects Self Welds And Cycles Without Recording Them", () => {
        const build = new MixAndMatchBuild()
        const a = build.spawn("a", ORIGIN)
        const b = build.spawn("b", ORIGIN)

        expect(build.weld(a, a, ORIGIN)).toBe(false)
        expect(build.weld(a, b, ORIGIN)).toBe(true)
        expect(build.weld(b, a, ORIGIN)).toBe(false)
        expect(build.timeline.filter(x => x.type === "weld")).toHaveLength(1)
    })

    test("Scrubbing Previews Without Editing The Timeline", () => {
        const build = new MixAndMatchBuild()
        build.spawn("a", ORIGIN)
        build.spawn("b", ORIGIN)
        build.scrubTo(1)

        expect(build.state.components.size).toBe(1)
        expect(build.timeline).toHaveLength(2)
        expect(build.isScrubbed).toBe(true)
        expect(build.discardedByNextEdit).toBe(1)

        build.scrubTo(2)
        expect(build.state.components.size).toBe(2)
    })

    test("Editing While Scrubbed Back Truncates The Future", () => {
        const build = new MixAndMatchBuild()
        build.spawn("a", ORIGIN)
        build.spawn("b", ORIGIN)
        build.scrubTo(1)
        build.spawn("c", ORIGIN)

        expect(build.timeline.map(x => (x.type === "spawn" ? x.libraryPartRef : x.type))).toEqual(["a", "c"])
        expect(build.marker).toBe(2)
    })

    test("Resumes From A Serialized Session", () => {
        const build = new MixAndMatchBuild()
        const frame = build.spawn("frame", ORIGIN)
        const pod = build.spawn("pod", translation(1, 0, 0))
        build.weld(frame, pod, translation(1, 0, 0))

        const resumed = new MixAndMatchBuild(parseSession(serializeSession(build.session))!)

        expect(resumed.timeline).toEqual(build.timeline)
        expect(resumed.marker).toBe(build.marker)
        expect(resumed.state.components.get(pod)!.weld!.parentId).toBe(frame)
        // Ids keep counting from the resumed roster rather than restarting and colliding.
        expect(resumed.spawn("extra", ORIGIN)).toBe("c3")
    })
})
