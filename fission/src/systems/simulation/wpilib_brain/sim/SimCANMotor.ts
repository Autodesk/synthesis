import type { SimSupplier } from "../SimDataFlow"
import { supplierTypeMap } from "../WPILibState"
import {
    CANMOTOR_BRAKE_MODE,
    CANMOTOR_BUS_VOLTAGE,
    CANMOTOR_MOTOR_CURRENT,
    CANMOTOR_NEUTRAL_DEADBAND,
    CANMOTOR_PERCENT_OUTPUT,
    CANMOTOR_SUPPLY_CURRENT,
    SimType,
} from "../WPILibTypes"
import SimDriverStation from "./SimDriverStation"
import SimGeneric from "./SimGeneric"

export default class SimCANMotor {
    private constructor() {}

    public static getPercentOutput(device: string): number | undefined {
        return SimDriverStation.isEnabled()
            ? SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_PERCENT_OUTPUT, 0.0)
            : 0.0
    }

    public static getBrakeMode(device: string): number | undefined {
        return SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_BRAKE_MODE, 0.0)
    }

    public static getNeutralDeadband(device: string): number | undefined {
        return SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_NEUTRAL_DEADBAND, 0.0)
    }

    public static setSupplyCurrent(device: string, current: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_SUPPLY_CURRENT, current)
    }

    public static setMotorCurrent(device: string, current: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_MOTOR_CURRENT, current)
    }

    public static setBusVoltage(device: string, voltage: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_BUS_VOLTAGE, voltage)
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => supplierTypeMap[SimType.CAN_MOTOR]!,
            getSupplierValue: () => SimCANMotor.getPercentOutput(device) ?? 0,
        }
    }
}
