import * as THREE from "three"
import { describe, expect, test } from "vitest"
import { snapToFaceOffset } from "@/mix-and-match/MixAndMatchPlacement"

function box(center: [number, number, number], size: [number, number, number]): THREE.Box3 {
    return new THREE.Box3().setFromCenterAndSize(new THREE.Vector3(...center), new THREE.Vector3(...size))
}

const UNIT = [1, 1, 1] as [number, number, number]

describe("Snap To Face", () => {
    test("Closes A Gap Along The Separated Axis", () => {
        const offset = snapToFaceOffset(box([0, 0, 0], UNIT), box([3, 0, 0], UNIT))

        expect(offset.toArray()).toEqual([2, 0, 0])
    })

    test("Works In The Negative Direction", () => {
        const offset = snapToFaceOffset(box([0, 0, 0], UNIT), box([-3, 0, 0], UNIT))

        expect(offset.toArray()).toEqual([-2, 0, 0])
    })

    test("Only Moves Along One Axis", () => {
        const offset = snapToFaceOffset(box([0, 0, 0], UNIT), box([4, 0.5, 0.25], UNIT))

        expect(offset.y).toBe(0)
        expect(offset.z).toBe(0)
        expect(offset.x).toBeCloseTo(3)
    })

    test("Picks The Axis The Parts Are Furthest Apart On Relative To Their Size", () => {
        // Separated by 3 in x and 2 in y, but the parts are 10 long in x, so the y gap is the visible one.
        const offset = snapToFaceOffset(box([0, 0, 0], [10, 1, 1]), box([3, 2, 0], [10, 1, 1]))

        expect(offset.x).toBe(0)
        expect(offset.y).toBeCloseTo(1)
    })

    test("Pulls An Overlapping Part Back Out To Flush", () => {
        const offset = snapToFaceOffset(box([0, 0, 0], UNIT), box([0.25, 0, 0], UNIT))

        expect(offset.x).toBeCloseTo(-0.75)
    })

    test("Does Nothing For An Empty Box", () => {
        expect(snapToFaceOffset(new THREE.Box3(), box([3, 0, 0], UNIT)).toArray()).toEqual([0, 0, 0])
        expect(snapToFaceOffset(box([0, 0, 0], UNIT), new THREE.Box3()).toArray()).toEqual([0, 0, 0])
    })
})
