import type Jolt from "@azaleacolburn/jolt-physics"
import type * as THREE from "three"
import { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { convertJoltVec3ToThreeVector3, convertThreeVector3ToJoltVec3 } from "./TypeConversions"

export function rayCastForRigidBody(
    mousePos: [number, number]
): { bodyId: Jolt.BodyID; hitPoint: THREE.Vector3; association: RigidNodeAssociate } | undefined {
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

    return { bodyId: hit.data.mBodyID, hitPoint: convertJoltVec3ToThreeVector3(hit.point, false), association }
}
