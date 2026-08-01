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

/** How close two unit vectors have to be to opposed before the rotation between them is ambiguous. */
const ANTIPARALLEL_EPSILON = 1e-6

const WORLD_UP = new THREE.Vector3(0, 1, 0)

/**
 * Axis to flip `normal` 180° about, chosen so the part keeps its up direction where it can.
 *
 * Any axis perpendicular to `normal` reverses it, and they differ only by how much they roll everything
 * else. Taking the one closest to world up leaves the part's up untouched, since it becomes the axis of
 * rotation itself.
 */
function flipAxisFor(normal: THREE.Vector3): THREE.Vector3 {
    const axis = WORLD_UP.clone().sub(normal.clone().multiplyScalar(WORLD_UP.dot(normal)))

    // The face points straight up or straight down, e.g. mating a lid onto a floor, so the part has to
    // turn over no matter which axis is used. Any perpendicular will do.
    if (axis.lengthSq() < ANTIPARALLEL_EPSILON) return new THREE.Vector3(0, 0, 1).cross(normal).normalize()

    return axis.normalize()
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

/**
 * Offset of a child's root relative to its parent's root, which is what a weld records.
 *
 * Let W be the child's world transform, R the parent's, and L the child expressed in the parent's
 * frame: W = R L, so L = R^-1 W. Same premultiply/invert pattern as
 * `ConfigureGamepiecePickupInterface.save()` uses to pin a configured point to a body.
 */
export function relativeOffsetBetween(parentWorld: THREE.Matrix4, childWorld: THREE.Matrix4): THREE.Matrix4 {
    return childWorld.clone().premultiply(parentWorld.clone().invert())
}

/** Inverse of {@link relativeOffsetBetween}: W = R L. */
export function applyRelativeOffset(parentWorld: THREE.Matrix4, relativeOffset: THREE.Matrix4): THREE.Matrix4 {
    return relativeOffset.clone().premultiply(parentWorld)
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
    const from = movingNormal.clone().normalize()
    const to = targetNormal.clone().normalize().negate()

    // A straight 180° turn has no single answer, and `setFromUnitVectors` resolves it by grabbing whichever
    // perpendicular falls out of the component ordering — so picking two faces that already point the same
    // way rolls the part by an amount nobody asked for, and which way it rolls depends on the axis the
    // faces happen to lie along.
    const isFlip = from.dot(to) < -1 + ANTIPARALLEL_EPSILON
    const rotation = isFlip
        ? new THREE.Quaternion().setFromAxisAngle(flipAxisFor(from), Math.PI)
        : new THREE.Quaternion().setFromUnitVectors(from, to)

    // Rotate about movingPoint (so it doesn't wander off during the rotation), then slide it onto targetPoint.
    const transform = new THREE.Matrix4()
        .makeTranslation(targetPoint.x, targetPoint.y, targetPoint.z)
        .multiply(new THREE.Matrix4().makeRotationFromQuaternion(rotation))
        .multiply(new THREE.Matrix4().makeTranslation(-movingPoint.x, -movingPoint.y, -movingPoint.z))

    return transform
}
