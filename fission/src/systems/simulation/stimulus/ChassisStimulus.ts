import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import { type NoraNumber3, NoraTypes } from "../Nora"
import Stimulus, { type StimulusID } from "./Stimulus"

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

    public constructor(id: StimulusID, bodyId: Jolt.BodyID, info?: mirabuf.IInfo) {
        super(id, info)

        this._body = World.physicsSystem.getBody(bodyId)
        this._mass = this._body.GetShape().GetMassProperties().mMass
    }

    public update(_: number): void {}

    public getSupplierType(): NoraTypes {
        return NoraTypes.NUMBER3
    }
    public getSupplierValue(): NoraNumber3 {
        throw new Error("Method not implemented.")
    }
    public displayName(): string {
        return "Chassis [Accel|Gyro]"
    }
}

export default ChassisStimulus
