import Jolt from "@barclah/jolt-physics"
import { LayerReserve } from "./PhysicsSystem"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { mirabuf } from "@/proto/mirabuf"

export interface MechanismConstraint {
    parentBody: Jolt.BodyID
    childBody: Jolt.BodyID
    primaryConstraint: Jolt.Constraint
    maxVelocity: number
    info?: mirabuf.IInfo
    extraConstraints: Jolt.Constraint[]
    extraBodies: Jolt.BodyID[]
}

class Mechanism {
    public rootBody: string
    public nodeToBody: Map<RigidNodeId, Jolt.BodyID>
    public constraints: Array<MechanismConstraint>
    public stepListeners: Array<Jolt.PhysicsStepListener>
    public layerReserve: LayerReserve | undefined
    public controllable: boolean
    public ghostBodies: Array<Jolt.BodyID>

    public constructor(
        rootBody: string,
        bodyMap: Map<string, Jolt.BodyID>,
        controllable: boolean,
        layerReserve?: LayerReserve
    ) {
        this.rootBody = rootBody
        this.nodeToBody = bodyMap
        this.constraints = []
        this.stepListeners = []
        this.controllable = controllable
        this.ghostBodies = []
        this.layerReserve = layerReserve
    }

    public AddConstraint(mechConstraint: MechanismConstraint) {
        this.constraints.push(mechConstraint)
    }

    public AddStepListener(listener: Jolt.PhysicsStepListener) {
        this.stepListeners.push(listener)
    }

    public GetBodyByNodeId(nodeId: string) {
        return this.nodeToBody.get(nodeId)
    }

    public DisablePhysics() {}
}

export default Mechanism
