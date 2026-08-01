import * as THREE from "three"
import { describe, expect, test } from "vitest"
import { applyRelativeOffset, relativeOffsetBetween } from "@/mix-and-match/MixAndMatchPlacement"

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
