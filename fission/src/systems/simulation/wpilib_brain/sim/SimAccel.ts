import type { NoraValue, NoraValueOf } from "../../Nora"
import { ACCEL_TYPE } from "../../stimulus/AccelStimulus"
import type { SimReceiver } from "../SimDataFlow"
import { receiverTypeMap } from "../WPILibState"
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
            getReceiverType: () => ACCEL_TYPE,
            setReceiverValue: ([x, y, z, vx, vy, vz]: NoraValueOf<typeof ACCEL_TYPE>) => {
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
