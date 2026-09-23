import { BaseUnit, DerivativeOrder, noraType, num, type BaseType, type NoraBaseValueOf } from "../../Nora"
import type { SimSupplier } from "../SimDataFlow"
import { PWM_POSITION, PWM_SPEED, SimType } from "../WPILibTypes"
import SimDriverStation from "./SimDriverStation"
import SimGeneric from "./SimGeneric"

export const PWM_TYPE = noraType([num(BaseUnit.NONE, DerivativeOrder.ZERO), num(BaseUnit.NONE, DerivativeOrder.ONE)])

export default class SimPWM {
    private constructor() {}

    public static getSpeed(
        device: string
    ): NoraBaseValueOf<{ type: BaseType.NUMBER; unit: BaseUnit.NONE; order: DerivativeOrder.ONE }> {
        return SimDriverStation.isEnabled()
            ? { value: SimGeneric.get(SimType.PWM, device, PWM_SPEED, 0.0), baseType: PWM_TYPE[1] }
            : { value: 0.0, baseType: PWM_TYPE[1] }
    }

    public static getPosition(
        device: string
    ): NoraBaseValueOf<{ type: BaseType.NUMBER; unit: BaseUnit.NONE; order: DerivativeOrder.ZERO }> {
        return { value: SimGeneric.get(SimType.PWM, device, PWM_POSITION, 0.0), baseType: PWM_TYPE[0] }
    }

    public static genSupplier(device: string): SimSupplier<typeof PWM_TYPE> {
        return {
            supplierType: PWM_TYPE,
            getSupplierValue: () => [SimPWM.getPosition(device), SimPWM.getSpeed(device)],
        }
    }
}
