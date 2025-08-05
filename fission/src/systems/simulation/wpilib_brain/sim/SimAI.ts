import { SimInput } from "../SimInput"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimAI {
    constructor() {}

    public static setValue(device: string, value: number): boolean {
        return SimGeneric.set(SimType.AI, device, ">voltage", value)
    }

    /**
     * The number of averaging bits
     */
    public static getAvgBits(device: string) {
        return SimGeneric.get(SimType.AI, device, "<avg_bits")
    }
    /**
     * The number of oversampling bits
     */
    public static getOversampleBits(device: string) {
        return SimGeneric.get(SimType.AI, device, "<oversample_bits")
    }
    /**
     * Input voltage, in volts
     */
    public static setVoltage(device: string, voltage: number) {
        return SimGeneric.set(SimType.AI, device, ">voltage", voltage)
    }
    /**
     * If the accumulator is initialized in the robot program
     */
    public static getAccumInit(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_init")
    }
    /**
     * The accumulated value
     */
    public static setAccumValue(device: string, accumValue: number) {
        return SimGeneric.set(SimType.AI, device, ">accum_value", accumValue)
    }
    /**
     * The number of accumulated values
     */
    public static setAccumCount(device: string, accumCount: number) {
        return SimGeneric.set(SimType.AI, device, ">accum_count", accumCount)
    }
    /**
     * The center value of the accumulator
     */
    public static getAccumCenter(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_center")
    }
    /**
     * The accumulator's deadband
     */
    public static getAccumDeadband(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_deadband")
    }
}

export class SimAnalogInput extends SimInput {
    private _valueSupplier: () => number

    constructor(device: string, valueSupplier: () => number) {
        super(device)
        this._valueSupplier = valueSupplier
    }

    public update(_deltaT: number) {
        SimAI.setValue(this._device, this._valueSupplier())
    }
}
