import type { NoraValue } from "../../Nora"
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
            getReceiverType: () => receiverTypeMap[SimType.ACCELEROMETER]!,
            setReceiverValue: ([x, y, z, vx, vy, vz]: NoraValue<6>) => {
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
