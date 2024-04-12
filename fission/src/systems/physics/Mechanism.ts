import Jolt from "@barclah/jolt-physics";
import { LayerReserve } from "./PhysicsSystem";

export interface MechanismConstraint {
    parentBody: Jolt.BodyID,
    childBody: Jolt.BodyID,
    constraint: Jolt.Constraint
}

class Mechanism {
    public rootBody: string;
    public nodeToBody: Map<string, Jolt.BodyID>;
    public constraints: Array<MechanismConstraint>;
    public layerReserve: LayerReserve | undefined;

    public constructor(rootBody: string, bodyMap: Map<string, Jolt.BodyID>, layerReserve?: LayerReserve) {
        this.rootBody = rootBody;
        this.nodeToBody = bodyMap;
        this.constraints = [];
        this.layerReserve = layerReserve;
    }

    public AddConstraint(mechConstraint: MechanismConstraint) {
        this.constraints.push(mechConstraint);
    }

    public GetBodyByNodeId(nodeId: string) {
        return this.nodeToBody.get(nodeId);
    }
}

export default Mechanism;