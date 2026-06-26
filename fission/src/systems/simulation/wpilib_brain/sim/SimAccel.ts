import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type Mechanism from "@/systems/physics/Mechanism"
import World from "@/systems/World"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions"
import type { NoraNumber3 } from "../../Nora"
import type { SimReceiver } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { receiverTypeMap } from "../WPILibState"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimAccel {
    private constructor() {}

    public static setX(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">x", accel)
    }

    public static setY(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">y", accel)
    }

    public static setZ(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">z", accel)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.ACCELEROMETER]!,
            setReceiverValue: ([x, y, z]: NoraNumber3) => {
                SimAccel.setX(device, x)
                SimAccel.setY(device, y)
                SimAccel.setZ(device, z)
            },
        }
    }
}

export class SimAccelInput extends SimInput {
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _prevVel: THREE.Vector3

    private static readonly GRAVITY = new THREE.Vector3(0, -9.8, 0)
    private static readonly GRAVITY_MAGNITUDE = SimAccelInput.GRAVITY.length()

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)
        this._prevVel = new THREE.Vector3(0, 0, 0)
    }

    public update(deltaT: number) {
        if (!this._joltID) return
        const body = World.physicsSystem.getBody(this._joltID)!

        const worldVel = convertJoltVec3ToThreeVector3(body.GetLinearVelocity())
        const worldAccel = worldVel.clone().sub(this._prevVel).divideScalar(deltaT)

        const specificForce = worldAccel.sub(SimAccelInput.GRAVITY).divideScalar(SimAccelInput.GRAVITY_MAGNITUDE)

        const rot = convertJoltQuatToThreeQuaternion(body.GetRotation(), true)
        const localAccel = specificForce.applyQuaternion(rot.invert())

        SimAccel.setX(this._device, localAccel.x)
        SimAccel.setY(this._device, localAccel.y)
        SimAccel.setZ(this._device, localAccel.z)

        this._prevVel = worldVel
    }
}
