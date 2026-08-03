import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"
import type { SimReceiver } from "../SimDataFlow"
import { receiverTypeMap } from "../WPILibState"
import type { NoraValue } from "../../Nora"

export default class SimGyro {
    private constructor() {}

    public static setAngleX(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_x", angle)
    }

    /// NOTE: z and y swapped since ThreeJS has y up but sensors have z up
    public static setAngleY(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_z", angle)
    }

    public static setAngleZ(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_y", angle)
    }

    public static setRateX(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_x", rate)
    }

    public static setRateY(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_z", rate)
    }

    public static setRateZ(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_y", rate)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.GYRO]!,
            setReceiverValue: ([ax, ay, az, rx, ry, rz]: NoraValue<6>) => {
                SimGyro.setAngleX(device, ax)
                SimGyro.setAngleY(device, ay)
                SimGyro.setAngleZ(device, az)
                SimGyro.setRateX(device, rx)
                SimGyro.setRateY(device, ry)
                SimGyro.setRateZ(device, rz)
            },
        }
    }
}
