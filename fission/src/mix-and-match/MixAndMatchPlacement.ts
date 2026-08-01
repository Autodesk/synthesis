import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import {
    convertJoltMat44ToThreeMatrix4,
    convertThreeQuaternionToJoltQuat,
    convertThreeVector3ToJoltRVec3,
} from "@/util/TypeConversions"

/** Below this, a re-place would be a no-op and writing to the bodies just adds jitter. */
const TRANSFORM_EPSILON = 1e-6

/**
 * A component's world transform is the world transform of its root body — the part its own mira
 * declares as `"grounded"`, which every internal joint hangs off of.
 */
export function componentWorldTransform(component: MirabufSceneObject): THREE.Matrix4 {
    const rootBodyId = component.getRootNodeId()
    const body = rootBodyId ? World.physicsSystem.getBody(rootBodyId) : undefined

    return body ? convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()) : new THREE.Matrix4()
}

/** Moves every body of a component rigidly, preserving whatever pose its internal joints are in. */
export function moveComponentBy(component: MirabufSceneObject, delta: THREE.Matrix4) {
    component.mirabufInstance.parser.rigidNodes.forEach(rn => {
        const bodyId = component.mechanism.getBodyByNodeId(rn.id)
        const body = bodyId ? World.physicsSystem.getBody(bodyId) : undefined
        if (!bodyId || !body) return

        const next = convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()).premultiply(delta)
        const position = new THREE.Vector3()
        const rotation = new THREE.Quaternion()
        next.decompose(position, rotation, new THREE.Vector3())

        World.physicsSystem.setBodyPositionAndRotation(
            bodyId,
            convertThreeVector3ToJoltRVec3(position),
            convertThreeQuaternionToJoltQuat(rotation)
        )
    })

    component.updateMeshTransforms()
}

function isCloseTo(a: THREE.Matrix4, b: THREE.Matrix4): boolean {
    return a.elements.every((value, i) => Math.abs(value - b.elements[i]) < TRANSFORM_EPSILON)
}

export function setComponentWorldTransform(component: MirabufSceneObject, target: THREE.Matrix4) {
    const current = componentWorldTransform(component)
    if (isCloseTo(current, target)) return

    moveComponentBy(component, target.clone().multiply(current.invert()))
}

/** World-space bounds of everything a component renders. */
export function componentWorldBounds(component: MirabufSceneObject): THREE.Box3 {
    const bounds = new THREE.Box3()
    component.mirabufInstance.batches.forEach(batch => {
        batch.computeBoundingBox()
        if (batch.boundingBox) bounds.union(batch.boundingBox)
    })

    return bounds
}

/**
 * Translation that brings `moving` flush against the nearest face of `target`.
 *
 * This is a one-shot positioning aid, nothing more: it picks the axis the two are most separated
 * along and slides `moving` until their bounds touch on that axis. It creates no relationship
 * between the parts, and welding remains a separate explicit action.
 *
 * @returns The offset to translate `moving` by, or a zero vector if either box is empty.
 */
export function snapToFaceOffset(moving: THREE.Box3, target: THREE.Box3): THREE.Vector3 {
    if (moving.isEmpty() || target.isEmpty()) return new THREE.Vector3()

    const movingCenter = moving.getCenter(new THREE.Vector3())
    const targetCenter = target.getCenter(new THREE.Vector3())
    const movingHalf = moving.getSize(new THREE.Vector3()).multiplyScalar(0.5)
    const targetHalf = target.getSize(new THREE.Vector3()).multiplyScalar(0.5)

    const separation = targetCenter.clone().sub(movingCenter)
    const axes = ["x", "y", "z"] as const

    // The axis where the two are furthest apart relative to how big they are on that axis is the one
    // the user is visually reading as "the gap".
    const axis = axes.reduce((best, current) => {
        const reach = movingHalf[current] + targetHalf[current]
        const bestReach = movingHalf[best] + targetHalf[best]
        return Math.abs(separation[current]) / (reach || 1) > Math.abs(separation[best]) / (bestReach || 1)
            ? current
            : best
    })

    const direction = separation[axis] < 0 ? -1 : 1
    const flushCenter = targetCenter[axis] - direction * (targetHalf[axis] + movingHalf[axis])

    const offset = new THREE.Vector3()
    offset[axis] = flushCenter - movingCenter[axis]

    return offset
}
