import { type NoraNumber, type NoraType, NoraTypes } from "../Nora"

export type SimSupplier = {
    getSupplierType(): NoraTypes
    getSupplierValue(): NoraType
}

export type SimReceiver = {
    getReceiverType(): NoraTypes
    setReceiverValue(val: NoraType): void
}

export type SimFlow = {
    supplier: SimSupplier
    receiver: SimReceiver
}

export function validate(s: SimSupplier, r: SimReceiver): boolean {
    return s.getSupplierType() === r.getReceiverType()
}

export class SimSupplierAverage implements SimSupplier {
    private _suppliers: SimSupplier[]

    public constructor(suppliers?: SimSupplier[]) {
        if (!suppliers || suppliers.some(x => x.getSupplierType() != NoraTypes.NUMBER)) {
            this._suppliers = []
        } else {
            this._suppliers = suppliers
        }
    }

    public addSupplier(supplier: SimSupplier) {
        if (supplier.getSupplierType() == NoraTypes.NUMBER) {
            this._suppliers.push(supplier)
        }
    }

    getSupplierType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    getSupplierValue(): NoraNumber {
        return this._suppliers.reduce((prev, next) => prev + (next.getSupplierValue() as NoraNumber), 0)
    }
}

export class SimReceiverDistribution implements SimReceiver {
    private _receivers: SimReceiver[]

    public constructor(receivers?: SimReceiver[]) {
        if (!receivers || receivers.some(x => x.getReceiverType() != NoraTypes.NUMBER)) {
            this._receivers = []
        } else {
            this._receivers = receivers
        }
    }

    public addReceiver(receiver: SimReceiver) {
        if (receiver.getReceiverType() == NoraTypes.NUMBER) {
            this._receivers.push(receiver)
        }
    }

    getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    setReceiverValue(value: NoraNumber) {
        this._receivers.forEach(x => x.setReceiverValue(value))
    }
}
