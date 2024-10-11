import Jolt from "@barclah/jolt-physics"
import Stimulus from "./Stimulus"
import World from "@/systems/World"
import { mirabuf } from "@/proto/mirabuf"

class ChassisStimulus extends Stimulus {
    private _body: Jolt.Body
    private _mass: number

    public get linearVelocity(): Jolt.Vec3 {
        return this._body.GetLinearVelocity()
    }
    public get angularVelocity(): Jolt.Vec3 {
        return this._body.GetAngularVelocity()
    }
    public get acceleration(): Jolt.Vec3 {
        return this._body.GetAccumulatedForce().Div(this._mass)
    }
    public get rotation(): Jolt.Vec3 {
        return this._body.GetRotation().GetEulerAngles()
    }

    public constructor(bodyId: Jolt.BodyID, info?: mirabuf.IInfo) {
        super(info)

        this._body = World.PhysicsSystem.GetBody(bodyId)
        this._mass = this._body.GetShape().GetMassProperties().mMass
    }

    public Update(_: number): void {}
}

export default ChassisStimulus
