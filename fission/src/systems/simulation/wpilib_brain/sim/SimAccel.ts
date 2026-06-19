import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import type Mechanism from "@/systems/physics/Mechanism"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
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

    private readonly _scratchQuat = new THREE.Quaternion()
    private readonly _scratchMat = new THREE.Matrix4()
    private readonly _scratchVel = new THREE.Vector3()

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)
        this._prevVel = new THREE.Vector3(0, 0, 0)
    }

    public update(deltaT: number) {
        if (!this._joltID) return
        const body = World.physicsSystem.getBody(this._joltID)

        const jRot = body.GetRotation()
        this._scratchQuat.set(jRot.GetX(), jRot.GetY(), jRot.GetZ(), jRot.GetW())
        JOLT.destroy(jRot)

        this._scratchMat.makeRotationFromQuaternion(this._scratchQuat).transpose()

        const jVel = body.GetLinearVelocity()
        this._scratchVel.set(jVel.GetX(), jVel.GetY(), jVel.GetZ())
        JOLT.destroy(jVel)
        this._scratchVel.applyMatrix4(this._scratchMat)

        const ax = (this._scratchVel.x - this._prevVel.x) / deltaT
        const ay = (this._scratchVel.y - this._prevVel.y) / deltaT
        const az = (this._scratchVel.z - this._prevVel.z) / deltaT

        SimGeneric.setMany(SimType.ACCELEROMETER, this._device, {
            ">x": ax,
            ">y": ay,
            ">z": az,
        })

        this._prevVel.copy(this._scratchVel)
    }
}
