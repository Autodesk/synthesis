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

    if (DEBUG_SNAP_TO_FACE) {
        const resultingMovingNormal = movingNormal.clone().normalize().applyQuaternion(rotation)
        console.debug("[MixAndMatch] mateFacesTransform", {
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
    }

    return transform
}

