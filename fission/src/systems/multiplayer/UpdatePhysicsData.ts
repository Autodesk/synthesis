import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { PhysicsBodyData } from "./MultiplayerMessageTypes"
import type Jolt from "@synthesis.adsk/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import World from "../World"

/**
 * Updates the physics data for each given body on the given mirabuf scene object
 */
export function handleUpdateObjectPhysics(sceneObject: MirabufSceneObject, bodies: PhysicsBodyData[], peerId: string) {
    // Sets the physics data for each body in the assembly
    for (const { rigidNodeId, ...physicsData } of bodies) {
        const bodyId = sceneObject.mechanism.getBodyByNodeId(rigidNodeId)
        if (bodyId == null) {
            console.error(`BodyId: ${bodyId} sent by ${peerId} does not exist in bodyMap`)
            continue
        }

        applyPhysicsBodyData(bodyId, physicsData)
    }
}

/**
 * Updates the physics data for a specific body
 */
export function applyPhysicsBodyData(bodyId: Jolt.BodyID, data: Omit<PhysicsBodyData, "rigidNodeId">) {
    const clientBody = World.physicsSystem.getBody(bodyId)
    if (!clientBody) {
        console.error(`Body ${bodyId} not found`)
        return
    }

    const lin: { x: number; y: number; z: number } = JSON.parse(data.linearVelocityStr)
    const ang: { x: number; y: number; z: number } = JSON.parse(data.angularVelocityStr)
    const pos: { x: number; y: number; z: number } = JSON.parse(data.positionStr)
    const rot: { x: number; y: number; z: number; w: number } = JSON.parse(data.rotationStr)

    const linearVelocity = new JOLT.Vec3(lin.x, lin.y, lin.z)
    const angularVelocity = new JOLT.Vec3(ang.x, ang.y, ang.z)
    const position = new JOLT.RVec3(pos.x, pos.y, pos.z)
    const rotation = new JOLT.Quat(rot.x, rot.y, rot.z, rot.w)

    clientBody.SetLinearVelocity(linearVelocity)
    clientBody.SetAngularVelocity(angularVelocity)
    World.physicsSystem.setBodyPositionAndRotation(bodyId, position, rotation)

    JOLT.destroy(linearVelocity)
    JOLT.destroy(angularVelocity)
}
