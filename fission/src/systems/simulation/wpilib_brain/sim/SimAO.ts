import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimAO {
    constructor() {}

    public static getVoltage(device: string): number {
        return SimGeneric.get(SimType.AI, device, ">voltage", 0.0)
    }
}
