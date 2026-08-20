import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertJoltVec3ToThreeVector3,
} from "@/util/TypeConversions"
import Stimulus, { type StimulusID } from "./Stimulus"
import { BaseUnit, DerivativeOrder, noraType, type NoraValueOf, BaseAxis, numAxis } from "../Nora"

export const ACCEL_TYPE = noraType([
    numAxis(BaseUnit.POSITION, DerivativeOrder.TWO, BaseAxis.X, "Accel X"),
    numAxis(BaseUnit.POSITION, DerivativeOrder.TWO, BaseAxis.Y, "Accel Y"),
    numAxis(BaseUnit.POSITION, DerivativeOrder.TWO, BaseAxis.Z, "Accel Z"),
    numAxis(BaseUnit.POSITION, DerivativeOrder.ONE, BaseAxis.X, "Vel X"),
    numAxis(BaseUnit.POSITION, DerivativeOrder.ONE, BaseAxis.Y, "Vel Y"),
    numAxis(BaseUnit.POSITION, DerivativeOrder.ONE, BaseAxis.Z, "Vel Z"),
])

class AccelStimulus extends Stimulus<typeof ACCEL_TYPE> {
    private _body: Jolt.Body
    private _delta: THREE.Matrix4

    private static readonly GRAVITY = new THREE.Vector3(0, -9.8, 0)
    private static readonly GRAVITY_MAGNITUDE = AccelStimulus.GRAVITY.length()

    private _prevVel = new THREE.Vector3()
    private _prevOmega = new THREE.Vector3()

    private _accel = new THREE.Vector3()
    private _vel = new THREE.Vector3()

    public constructor(id: StimulusID, bodyId: Jolt.BodyID, deltaTransformation: number[], info?: mirabuf.IInfo) {
        super(id, info)

        this._body = World.physicsSystem.getBody(bodyId)!
        this._delta = convertArrayToThreeMatrix4(deltaTransformation)
    }

    /**
     * World-space mount position and rotation.
     */
    private mountFrame(): { position: THREE.Vector3; rotation: THREE.Quaternion } {
        const world = this._delta.clone().premultiply(convertJoltMat44ToThreeMatrix4(this._body.GetWorldTransform()))
        const position = new THREE.Vector3()
        const rotation = new THREE.Quaternion()
        world.decompose(position, rotation, new THREE.Vector3())
        return { position, rotation }
    }

    public update(deltaT: number): void {
        const { position: mountPos, rotation: mountRot } = this.mountFrame()

        const velCom = convertJoltVec3ToThreeVector3(this._body.GetLinearVelocity(), false)
        const omega = convertJoltVec3ToThreeVector3(this._body.GetAngularVelocity(), false)
        const com = convertJoltVec3ToThreeVector3(this._body.GetCenterOfMassPosition(), false)
        const r = mountPos.clone().sub(com)

        this._vel = velCom.clone().add(omega.clone().cross(r))

        if (deltaT > 0) {
            const accelCom = velCom.clone().sub(this._prevVel).divideScalar(deltaT)
            const alpha = omega.clone().sub(this._prevOmega).divideScalar(deltaT)

            const accelPoint = accelCom.add(alpha.cross(r)).add(omega.clone().cross(omega.clone().cross(r)))

            const specificForce = accelPoint.sub(AccelStimulus.GRAVITY).divideScalar(AccelStimulus.GRAVITY_MAGNITUDE)
            this._accel = specificForce.applyQuaternion(mountRot.invert())
        }

        this._prevVel = velCom
        this._prevOmega = omega
    }

    public get supplierType() {
        return ACCEL_TYPE
    }

    protected supplyValue(): NoraValueOf<typeof ACCEL_TYPE> {
        return [
            { value: this._accel.x, baseType: ACCEL_TYPE[0] },
            { value: this._accel.y, baseType: ACCEL_TYPE[1] },
            { value: this._accel.z, baseType: ACCEL_TYPE[2] },
            { value: this._vel.x, baseType: ACCEL_TYPE[3] },
            { value: this._vel.y, baseType: ACCEL_TYPE[4] },
            { value: this._vel.z, baseType: ACCEL_TYPE[5] },
        ]
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Accel]`
    }
}

export default AccelStimulus
