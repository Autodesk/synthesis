import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type Mechanism from "@/systems/physics/Mechanism"
import World from "@/systems/World"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions"
import type { NoraNumber6 } from "../../Nora"
import type { SimReceiver } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { RECEIVER_TYPE_MAP } from "../WPILibState"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimAccel {
    private constructor() {}

    public static setX(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">x", accel)
    }

    /// NOTE: z and y swapped since ThreeJS has y up but sensors have z up
    public static setY(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">z", accel)
    }

    public static setZ(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">y", accel)
    }

    public static setVelX(device: string, vel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vx", vel)
    }

    public static setVelY(device: string, vel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vz", vel)
    }

    public static setVelZ(device: string, vel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vy", vel)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => RECEIVER_TYPE_MAP[SimType.ACCELEROMETER]!,
            setReceiverValue: ([x, y, z, vx, vy, vz]: NoraNumber6) => {
                SimAccel.setX(device, x)
                SimAccel.setY(device, y)
                SimAccel.setZ(device, z)
                SimAccel.setVelX(device, vx)
                SimAccel.setVelY(device, vy)
                SimAccel.setVelZ(device, vz)
            },
        }
    }
}

export class SimAccelInput extends SimInput {
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _joltBody?: Jolt.Body
    private _prevVel: THREE.Vector3

    private static readonly GRAVITY = new THREE.Vector3(0, -9.8, 0)
    private static readonly GRAVITY_MAGNITUDE = SimAccelInput.GRAVITY.length()

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)
        this._prevVel = new THREE.Vector3(0, 0, 0)

        if (this._joltID) this._joltBody = World.physicsSystem.getBody(this._joltID)!
    }

    public update(deltaT: number) {
        if (!this._joltBody) return
        const vel = this._joltBody.GetLinearVelocity()

        SimAccel.setVelX(this._device, vel.GetX())
        SimAccel.setVelY(this._device, vel.GetY())
        SimAccel.setVelZ(this._device, vel.GetZ())

        const worldVel = convertJoltVec3ToThreeVector3(vel, false)

        if (deltaT > 0) {
            const worldAccel = worldVel.clone().sub(this._prevVel).divideScalar(deltaT)

            const specificForce = worldAccel.sub(SimAccelInput.GRAVITY).divideScalar(SimAccelInput.GRAVITY_MAGNITUDE)

            const rot = convertJoltQuatToThreeQuaternion(this._joltBody.GetRotation(), false)
            const localAccel = specificForce.applyQuaternion(rot.invert())

            SimAccel.setX(this._device, localAccel.x)
            SimAccel.setY(this._device, localAccel.y)
            SimAccel.setZ(this._device, localAccel.z)
        }

        this._prevVel = worldVel
    }
}
