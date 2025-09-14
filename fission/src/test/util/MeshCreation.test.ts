import * as THREE from "three"
import { Vector3 } from "three"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { createMeshForShape, deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"

describe("Mesh Creation Tests", () => {
    test("Sphere Mesh Creation", () => {
        const sphereShape = new JOLT.SphereShape(4.0)
        const shapeResult = createMeshForShape(sphereShape)

        expect(shapeResult).toBeDefined()
        shapeResult.computeBoundingSphere()
        expect(shapeResult.boundingSphere?.radius).toBeCloseTo(4.0, 2)
    })

    test("Box Mesh Creation", () => {
        const boxShape = new JOLT.BoxShape(new JOLT.Vec3(0.5, 2, 4.5))
        const shapeResult = createMeshForShape(boxShape)

        expect(shapeResult).toBeDefined()
        shapeResult.computeBoundingBox()
        const boxSize = new Vector3()
        shapeResult.boundingBox?.getSize(boxSize)
        expect(boxSize.x).toBeCloseTo(1.0, 2)
        expect(boxSize.y).toBeCloseTo(4.0, 2)
        expect(boxSize.z).toBeCloseTo(9.0, 2)
    })
})

describe("Delta Field Transform Physical Properties Tests", () => {
    test("Returns Identity Transform When Both Matrices Are Identity", () => {
        const identity = new THREE.Matrix4()
        const result = deltaFieldTransformsPhysicalProp(identity, identity)

        expect(result.translation).toEqual(new THREE.Vector3(0, 0, 0))
        expect(result.rotation).toEqual(new THREE.Quaternion(0, 0, 0, 1))
        expect(result.scale).toEqual(new THREE.Vector3(1, 1, 1))
    })

    test("Applies Translation From Delta Only", () => {
        const delta = new THREE.Matrix4().makeTranslation(5, 0, 0)
        const field = new THREE.Matrix4()
        const result = deltaFieldTransformsPhysicalProp(delta, field)

        expect(result.translation).toEqual(new THREE.Vector3(5, 0, 0))
        expect(result.rotation.equals(new THREE.Quaternion(0, 0, 0, 1))).toBe(true)
        expect(result.scale.equals(new THREE.Vector3(1, 1, 1))).toBe(true)
    })

    test("Composes Rotation And Scale With Premultiply", () => {
        const delta = new THREE.Matrix4().makeRotationZ(Math.PI / 2)
        const field = new THREE.Matrix4().makeScale(2, 2, 2)
        const result = deltaFieldTransformsPhysicalProp(delta, field)

        expect(result.scale.x).toBeCloseTo(2)
        expect(result.scale.y).toBeCloseTo(2)
        expect(result.scale.z).toBeCloseTo(2)

        const expectedQuat = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 0, 1), Math.PI / 2)
        expect(result.rotation.angleTo(expectedQuat)).toBeLessThan(1e-6)
    })

    test("Handles Scaled Translation", () => {
        const delta = new THREE.Matrix4().makeTranslation(1, 2, 3)
        const field = new THREE.Matrix4().makeScale(2, 2, 2)

        const result = deltaFieldTransformsPhysicalProp(delta, field)

        expect(result.translation).toEqual(new THREE.Vector3(2, 4, 6))
        expect(result.scale).toEqual(new THREE.Vector3(2, 2, 2))
    })
})
