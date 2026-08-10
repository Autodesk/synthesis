import type { BaseAxis, BaseType, BaseUnit, DerivativeOrder, NoraBaseValueOf, NoraValueOf } from "../../Nora"
import { ACCEL_TYPE } from "../../stimulus/AccelStimulus"
import type { SimReceiver } from "../SimDataFlow"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimAccel {
    private constructor() { }

    public static setX(
        device: string,
        accel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.TWO
            axis?: BaseAxis.X
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">x", accel.value)
    }

    /// NOTE: z and y swapped since ThreeJS has y up but sensors have z up
    public static setY(
        device: string,
        accel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.TWO
            axis?: BaseAxis.Y
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">z", accel.value)
    }

    public static setZ(
        device: string,
        accel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.TWO
            axis?: BaseAxis.Z
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">y", accel.value)
    }

    public static setVelX(
        device: string,
        vel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.ONE
            axis?: BaseAxis.X
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vx", vel.value)
    }

    public static setVelY(
        device: string,
        vel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.ONE
            axis?: BaseAxis.Y
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vz", vel.value)
    }

    public static setVelZ(
        device: string,
        vel: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.POSITION
            order: DerivativeOrder.ONE
            axis?: BaseAxis.Z
        }>
    ): boolean {
        return SimGeneric.set(SimType.ACCELEROMETER, device, ">vy", vel.value)
    }

    public static genReceiver(device: string): SimReceiver<typeof ACCEL_TYPE> {
        return {
            receiverType: ACCEL_TYPE,
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
