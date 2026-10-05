import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"
import type { SimReceiver } from "../SimDataFlow"
import { GYRO_TYPE } from "../../stimulus/GyroStimulus"
import type { BaseAxis, BaseType, BaseUnit, DerivativeOrder, NoraBaseValueOf, NoraValueOf } from "../../Nora"

export default class SimGyro {
    private constructor() {}

    public static setAngleX(
        device: string,
        angle: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ZERO
            axis?: BaseAxis.X
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_x", angle.value)
    }

    /// NOTE: z and y swapped since ThreeJS has y up but sensors have z up
    public static setAngleY(
        device: string,
        angle: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ZERO
            axis?: BaseAxis.Y
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_z", angle.value)
    }

    public static setAngleZ(
        device: string,
        angle: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ZERO
            axis?: BaseAxis.Z
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_y", angle.value)
    }

    public static setRateX(
        device: string,
        rate: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ONE
            axis?: BaseAxis.X
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_x", rate.value)
    }

    public static setRateY(
        device: string,
        rate: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ONE
            axis?: BaseAxis.Y
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_z", rate.value)
    }

    public static setRateZ(
        device: string,
        rate: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ONE
            axis?: BaseAxis.Z
        }>
    ): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_y", rate.value)
    }

    public static genReceiver(device: string): SimReceiver<typeof GYRO_TYPE> {
        return {
            receiverType: GYRO_TYPE,
            setReceiverValue: ([ax, ay, az, rx, ry, rz]: NoraValueOf<typeof GYRO_TYPE>) => {
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
