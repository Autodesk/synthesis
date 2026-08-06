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

/** Toggle to trace snap-to-face/mate-faces geometry (selected faces, bounds, computed offsets) while diagnosing placement issues. */
export const DEBUG_SNAP_TO_FACE = true

/**
 * Logs pre-serialized to a single JSON string.
 *
 * Plain `console.debug(label, obj)` renders nested arrays/objects as collapsed, interactive
 * `Array(3)`/`Object` placeholders in devtools — copying or auto-saving the console (rather than
 * manually expanding every entry first) loses the actual numbers. Stringifying up front means the
 * real values are in the text no matter how the log is captured.
 */
export function debugLog(label: string, data: Record<string, unknown>) {
    if (!DEBUG_SNAP_TO_FACE) return
    console.debug(label, JSON.stringify(data))
}

/**
 * A component's world transform is the world transform of its root body — the part its own mira
 * declares as `"grounded"`, which every internal joint hangs off of.
 */
export function componentWorldTransform(component: MirabufSceneObject): THREE.Matrix4 {
    const rootBodyId = component.getRootNodeId()
    const body = rootBodyId ? World.physicsSystem.getBody(rootBodyId) : undefined

    return body ? convertJoltMat44ToThreeMatrix4(body.GetWorldTransform()) : new THREE.Matrix4()
}

/**
 * The transform a gizmo attached to `component` sits at, which is *not* `componentWorldTransform`.
 *
 * `MirabufSceneObject.postGizmoCreation` seats a new gizmo at the root body's center-of-mass
 * transform, and `GizmoSceneObject` bakes every body's offset relative to that. Since
 * `GizmoSceneObject.setTransform` also force-updates — it re-drives every body from those baked
 * offsets on the next frame — handing it a root *world* transform silently translates the whole
 * component by the root shape's COM offset. For a mira root body (a compound of every part in the
 * `grounded` node) that offset is meters, not rounding error.
 */
export function componentGizmoTransform(component: MirabufSceneObject): THREE.Matrix4 {
    const rootBodyId = component.getRootNodeId()
    const body = rootBodyId ? World.physicsSystem.getBody(rootBodyId) : undefined

    // NOTE Jolt getter, returns a scratch reference. Do not destroy.
    return body ? convertJoltMat44ToThreeMatrix4(body.GetCenterOfMassTransform()) : new THREE.Matrix4()
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

    debugLog("[MixAndMatch] snapToFaceOffset", {
        movingBounds: { min: moving.min.toArray(), max: moving.max.toArray() },
        targetBounds: { min: target.min.toArray(), max: target.max.toArray() },
        movingCenter: movingCenter.toArray(),
        targetCenter: targetCenter.toArray(),
        axis,
        direction,
        offset: offset.toArray(),
    })

    return offset
}

/**
 * Rigid transform that rotates and slides `moving` so a picked point on it lands on a picked point of
 * `target`, with its surface normal there facing the opposite of `target`'s normal — i.e. the two
 * picked faces end up flush and facing each other, not just the two bounding boxes.
 *
 * @param movingPoint  World-space point clicked on `moving`.
 * @param movingNormal World-space surface normal at `movingPoint`.
 * @param targetPoint  World-space point clicked on `target`.
 * @param targetNormal World-space surface normal at `targetPoint`.
 */
export function mateFacesTransform(
    movingPoint: THREE.Vector3,
    movingNormal: THREE.Vector3,
    targetPoint: THREE.Vector3,
    targetNormal: THREE.Vector3
): THREE.Matrix4 {
    const rotation = new THREE.Quaternion().setFromUnitVectors(
        movingNormal.clone().normalize(),
        targetNormal.clone().normalize().negate()
    )

    // Rotate about movingPoint (so it doesn't wander off during the rotation), then slide it onto targetPoint.
    const transform = new THREE.Matrix4()
        .makeTranslation(targetPoint.x, targetPoint.y, targetPoint.z)
        .multiply(new THREE.Matrix4().makeRotationFromQuaternion(rotation))
        .multiply(new THREE.Matrix4().makeTranslation(-movingPoint.x, -movingPoint.y, -movingPoint.z))

    const resultingMovingNormal = movingNormal.clone().normalize().applyQuaternion(rotation)
    debugLog("[MixAndMatch] mateFacesTransform", {
        movingPoint: movingPoint.toArray(),
        movingNormal: movingNormal.toArray(),
        targetPoint: targetPoint.toArray(),
        targetNormal: targetNormal.toArray(),
        rotation: rotation.toArray(),
        // Should end up ~antiparallel to targetNormal (dot ~ -1) once the faces are mated.
        resultingMovingNormal: resultingMovingNormal.toArray(),
        alignmentDot: resultingMovingNormal.dot(targetNormal.clone().normalize()),
        transform: transform.toArray(),
    })

    return transform
}
