import * as THREE from "three"
import { describe, expect, test } from "vitest"
import { applyRelativeOffset, relativeOffsetBetween, snapToFaceOffset } from "@/mix-and-match/MixAndMatchPlacement"

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

describe("Weld Offset", () => {
    const parentWorld = new THREE.Matrix4()
        .makeRotationY(Math.PI / 3)
        .premultiply(new THREE.Matrix4().makeTranslation(1, 2, 3))
    const childWorld = new THREE.Matrix4()
        .makeRotationX(Math.PI / 5)
        .premultiply(new THREE.Matrix4().makeTranslation(-2, 0.5, 4))

    test("Rebuilds The Child World Transform From The Parent", () => {
        const offset = relativeOffsetBetween(parentWorld, childWorld)
        const rebuilt = applyRelativeOffset(parentWorld, offset)

        rebuilt.elements.forEach((value, i) => expect(value).toBeCloseTo(childWorld.elements[i]))
    })

    test("Follows The Parent When It Moves", () => {
        const offset = relativeOffsetBetween(parentWorld, childWorld)
        const movedParent = parentWorld.clone().premultiply(new THREE.Matrix4().makeTranslation(0, 10, 0))
        const movedChild = applyRelativeOffset(movedParent, offset)

        const childPosition = new THREE.Vector3().setFromMatrixPosition(childWorld)
        const movedPosition = new THREE.Vector3().setFromMatrixPosition(movedChild)

        expect(movedPosition.x).toBeCloseTo(childPosition.x)
        expect(movedPosition.y).toBeCloseTo(childPosition.y + 10)
        expect(movedPosition.z).toBeCloseTo(childPosition.z)
    })

    test("Leaves Identity Alone", () => {
        const offset = relativeOffsetBetween(new THREE.Matrix4(), childWorld)

        offset.elements.forEach((value, i) => expect(value).toBeCloseTo(childWorld.elements[i]))
    })
})
