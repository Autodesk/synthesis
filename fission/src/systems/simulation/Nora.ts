export enum NoraTypes {
    Number = "num",
    Number3 = "num3",
    Unknown = "unknown",
}

export type NoraNumber = number
export type NoraNumber3 = [ number, number, number ]
export type NoraUnknown = unknown

export type NoraType = NoraNumber | NoraNumber3 | NoraUnknown

export type Supplier = {
    getSupplierType(): NoraTypes
    getSupplierValue(): NoraType
}

export type Receiver = {
    getReceiverType(): NoraTypes
    setReceiverValue(val: NoraType): void
}

export function validate(s: Supplier, r: Receiver): boolean {
    return s.getSupplierType() === r.getReceiverType()
}