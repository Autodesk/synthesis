import { NoraTypes, NoraType, NoraNumber } from "../Nora"

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
        if (!suppliers || suppliers.some(x => x.getSupplierType() != NoraTypes.Number)) {
            this._suppliers = []
        } else {
            this._suppliers = suppliers
        }
    }

    public AddSupplier(supplier: SimSupplier) {
        if (supplier.getSupplierType() == NoraTypes.Number) {
            this._suppliers.push(supplier)
        }
    }

    getSupplierType(): NoraTypes {
        return NoraTypes.Number
    }
    getSupplierValue(): NoraNumber {
        return this._suppliers.reduce((prev, next) => (prev += next.getSupplierValue() as NoraNumber), 0)
    }
}

export class SimReceiverDistribution implements SimReceiver {
    private _receivers: SimReceiver[]

    public constructor(receivers?: SimReceiver[]) {
        if (!receivers || receivers.some(x => x.getReceiverType() != NoraTypes.Number)) {
            this._receivers = []
        } else {
            this._receivers = receivers
        }
    }

    public AddReceiver(receiver: SimReceiver) {
        if (receiver.getReceiverType() == NoraTypes.Number) {
            this._receivers.push(receiver)
        }
    }

    getReceiverType(): NoraTypes {
        return NoraTypes.Number
    }
    setReceiverValue(value: NoraNumber) {
        this._receivers.forEach(x => x.setReceiverValue(value))
    }
}
