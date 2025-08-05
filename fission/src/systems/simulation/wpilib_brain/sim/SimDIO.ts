import type { NoraNumber } from "../../Nora"
import type { SimReceiver, SimSupplier } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { receiverTypeMap } from "../WPILibState"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimDIO {
    private constructor() {}

    public static setValue(device: string, value: boolean): boolean {
        return SimGeneric.set(SimType.DIO, device, "<>value", value)
    }

    public static getValue(device: string): boolean {
        return SimGeneric.get(SimType.DIO, device, "<>value", false)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.DIO]!,
            setReceiverValue: (a: NoraNumber) => {
                SimDIO.setValue(device, a > 0.5)
            },
        }
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => receiverTypeMap[SimType.DIO]!,
            getSupplierValue: () => (SimDIO.getValue(device) ? 1 : 0),
        }
    }
}

export class SimDigitalInput extends SimInput {
    private _valueSupplier: () => boolean

    /**
     * Creates a Simulation Digital Input object.
     *
     * @param device Device ID
     * @param valueSupplier Called each frame and returns what the value should be set to
     */
    constructor(device: string, valueSupplier: () => boolean) {
        super(device)
        this._valueSupplier = valueSupplier
    }

    private setValue(value: boolean) {
        SimDIO.setValue(this._device, value)
    }

    public getValue(): boolean {
        return SimDIO.getValue(this._device)
    }

    public update(_deltaT: number) {
        if (this._valueSupplier) this.setValue(this._valueSupplier())
    }
}
