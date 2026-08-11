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
    for (const body of bodies) {
        const bodyId = sceneObject.mechanism.getBodyByNodeId(body[0])
        if (bodyId == null) {
            console.error(`BodyId: ${bodyId} sent by ${peerId} does not exist in bodyMap`)
            continue
        }

        applyPhysicsBodyData(bodyId, body)
    }
}

/**
 * Updates the physics data for a specific body
 */
export function applyPhysicsBodyData(bodyId: Jolt.BodyID, data: PhysicsBodyData) {
    const clientBody = World.physicsSystem.getBody(bodyId)
    if (!clientBody) {
        console.error(`Body ${bodyId} not found`)
        return
    }

    // The caller has already resolved `rnId` into `bodyId`, so skip past it
    const [, [linX, linY, linZ], [angX, angY, angZ], [posX, posY, posZ], [rotX, rotY, rotZ, rotW]] = data

    const linearVelocity = new JOLT.Vec3(linX, linY, linZ)
    const angularVelocity = new JOLT.Vec3(angX, angY, angZ)
    const position = new JOLT.RVec3(posX, posY, posZ)
    const rotation = new JOLT.Quat(rotX, rotY, rotZ, rotW)

    clientBody.SetLinearVelocity(linearVelocity)
    clientBody.SetAngularVelocity(angularVelocity)
    World.physicsSystem.setBodyPositionAndRotation(bodyId, position, rotation)

    JOLT.destroy(linearVelocity)
    JOLT.destroy(angularVelocity)
}
