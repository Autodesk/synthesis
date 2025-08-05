import type Jolt from "@azaleacolburn/jolt-physics"
import type { JoltBodyIndexAndSequence } from "@/systems/physics/PhysicsTypes"

/**
 * An interface to create an association between a body and anything.
 */
export class BodyAssociate {
    readonly associatedBody: JoltBodyIndexAndSequence

    public constructor(bodyId: Jolt.BodyID) {
        this.associatedBody = bodyId.GetIndexAndSequenceNumber()
    }
}
