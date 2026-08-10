import { bool, noraType, type NoraValueOf, type NoraBaseValueOf, type BaseType } from "../../Nora"
import type { SimReceiver, SimSupplier } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export const DIO_TYPE = noraType([bool()])

export default class SimDIO {
    private constructor() { }

    public static setValue(
        device: string,
        value: NoraBaseValueOf<{
            type: BaseType.BOOLEAN
        }>
    ): boolean {
        return SimGeneric.set(SimType.DIO, device, "<>value", value.value)
    }

    public static getValue(device: string): NoraBaseValueOf<{ type: BaseType.BOOLEAN }> {
        return { value: SimGeneric.get(SimType.DIO, device, "<>value", false), baseType: DIO_TYPE[0] }
    }

    public static genReceiver(device: string): SimReceiver<typeof DIO_TYPE> {
        return {
            receiverType: DIO_TYPE,
            setReceiverValue: ([a]: NoraValueOf<typeof DIO_TYPE>) => {
                SimDIO.setValue(device, a)
            },
        }
    }

    public static genSupplier(device: string): SimSupplier<typeof DIO_TYPE> {
        return {
            supplierType: DIO_TYPE,
            getSupplierValue: () => [SimDIO.getValue(device)],
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
        SimDIO.setValue(this._device, { value, baseType: DIO_TYPE[0] })
    }

    public getValue(): boolean {
        return SimDIO.getValue(this._device).value
    }

    public update(_deltaT: number) {
        if (this._valueSupplier) this.setValue(this._valueSupplier())
    }
}
