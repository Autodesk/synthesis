import type { SimSupplier } from "../SimDataFlow"
import { SUPPLIER_TYPE_MAP } from "../WPILibState"
import { PWM_POSITION, PWM_SPEED, SimType } from "../WPILibTypes"
import SimDriverStation from "./SimDriverStation"
import SimGeneric from "./SimGeneric"

export default class SimPWM {
    private constructor() {}

    public static getSpeed(device: string): number | undefined {
        return SimDriverStation.isEnabled() ? SimGeneric.get(SimType.PWM, device, PWM_SPEED, 0.0) : 0.0
    }

    public static getPosition(device: string): number | undefined {
        return SimGeneric.get(SimType.PWM, device, PWM_POSITION, 0.0)
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => SUPPLIER_TYPE_MAP[SimType.PWM]!,
            getSupplierValue: () => SimPWM.getSpeed(device) ?? 0,
        }
    }
}
