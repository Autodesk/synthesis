import type Jolt from "@synthesis.adsk/jolt-physics"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import type { LayerReserve } from "./PhysicsSystem"

export interface MechanismConstraint {
    parentBody: Jolt.BodyID
    childBody: Jolt.BodyID
    primaryConstraint: Jolt.Constraint
    maxVelocity: number
    info?: mirabuf.IInfo
    jointUserData?: Record<string, string>
    extraConstraints: Jolt.Constraint[]
    extraBodies: Jolt.BodyID[]
}

class Mechanism {
    public rootBody: string
    public nodeToBody: Map<RigidNodeId, Jolt.BodyID>
    public constraints: MechanismConstraint[] = []
    public stepListeners: Jolt.PhysicsStepListener[] = []
    public vehicleTesters: Jolt.VehicleCollisionTester[] = []
    public layerReserve?: LayerReserve
    public controllable: boolean
    public ghostBodies: Jolt.BodyID[] = []
    public touchedObjects: MirabufSceneObject[] = [] // [SceneObjectKey, rootBodyId]

    public urdfWheelForward?: { x: number; y: number; z: number }

    public constructor(
        rootBody: string,
        bodyMap: Map<string, Jolt.BodyID>,
        controllable: boolean,
        layerReserve?: LayerReserve
    ) {
        this.rootBody = rootBody
        this.nodeToBody = bodyMap
        this.controllable = controllable
        this.layerReserve = layerReserve
    }

    public addConstraint(mechConstraint: MechanismConstraint) {
        this.constraints.push(mechConstraint)
    }

    public addStepListener(listener: Jolt.PhysicsStepListener) {
        this.stepListeners.push(listener)
    }

    public addVehicleTester(tester: Jolt.VehicleCollisionTester) {
        this.vehicleTesters.push(tester)
    }

    public getBodyByNodeId(nodeId: string) {
        return this.nodeToBody.get(nodeId)
    }

    public disablePhysics() {}
}
export default Mechanism
