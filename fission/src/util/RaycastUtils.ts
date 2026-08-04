import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { convertJoltVec3ToThreeVector3, convertThreeVector3ToJoltVec3 } from "./TypeConversions"

export function rayCastForRigidBody(
    mousePos: [number, number]
): { bodyId: Jolt.BodyID; hitPoint: THREE.Vector3; hitNormal: THREE.Vector3 | undefined; association: RigidNodeAssociate } | undefined {
    const origin = World.sceneRenderer.mainCamera.position
    const ignoredBodies: Jolt.BodyID[] = []

    function performRayCast() {
        const worldSpace = World.sceneRenderer.pixelToWorldSpace(mousePos[0], mousePos[1])
        const direction = worldSpace.sub(origin).normalize().multiplyScalar(40.0)

        return World.physicsSystem.rayCast(
            convertThreeVector3ToJoltVec3(origin),
            convertThreeVector3ToJoltVec3(direction),
            true,
            ...ignoredBodies
        )
    }

    let hit = performRayCast()
    /** Transparent objects such as scoring zones should be ignored by `raycasting` [SYNTH-106] */
    while (hit && !(World.physicsSystem.getBodyAssociation(hit.data.mBodyID) instanceof RigidNodeAssociate)) {
        ignoredBodies.push(hit.data.mBodyID)
        hit = performRayCast()
    }

    if (!hit) return undefined

    const association = World.physicsSystem.getBodyAssociation(hit.data.mBodyID) as RigidNodeAssociate

    return {
        bodyId: hit.data.mBodyID,
        hitPoint: convertJoltVec3ToThreeVector3(hit.point, false),
        hitNormal: hit.normal ? convertJoltVec3ToThreeVector3(hit.normal, false) : undefined,
        association,
    }
}

/**
 * Raycasts against actual render geometry rather than physics colliders.
 *
 * Physics bodies are often convex hulls that approximate a part's real shape, so a picked point and
 * normal read off a body can face a different way than the visible surface under the cursor. This is
 * for callers that need the true mesh surface, e.g. face-picking for mix-and-match part placement.
 */
export function rayCastMesh(
    mousePos: [number, number],
    objects: THREE.Object3D[]
): { object: THREE.Object3D; point: THREE.Vector3; normal: THREE.Vector3 } | undefined {
    const ndc = new THREE.Vector2(
        (mousePos[0] / window.innerWidth) * 2 - 1,
        -((mousePos[1] / window.innerHeight) * 2 - 1)
    )

    const raycaster = new THREE.Raycaster()
    raycaster.setFromCamera(ndc, World.sceneRenderer.mainCamera)

    const hit = raycaster.intersectObjects(objects, false)[0]
    if (!hit?.face) return undefined

    // BatchedMesh instances are positioned per-instance rather than via the object's own transform, so
    // the face normal has to go through that instance's matrix, not just the batch's matrixWorld.
    const worldMatrix = hit.object.matrixWorld.clone()
    if ((hit.object as THREE.BatchedMesh).isBatchedMesh && hit.batchId != undefined) {
        const instanceMatrix = new THREE.Matrix4()
        ;(hit.object as THREE.BatchedMesh).getMatrixAt(hit.batchId, instanceMatrix)
        worldMatrix.multiply(instanceMatrix)
    }

    const normal = hit.face.normal.clone().applyNormalMatrix(new THREE.Matrix3().getNormalMatrix(worldMatrix))

    return { object: hit.object, point: hit.point.clone(), normal: normal.normalize() }
}
