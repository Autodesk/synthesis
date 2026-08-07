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

/** World-space axis-aligned bounds of every render batch belonging to `component`. */
export function componentWorldBounds(component: MirabufSceneObject): THREE.Box3 {
    const bounds = new THREE.Box3()
    component.mirabufInstance.batches.forEach(batch => {
        batch.computeBoundingBox()
        if (batch.boundingBox) bounds.union(batch.boundingBox)
    })

    return bounds
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

