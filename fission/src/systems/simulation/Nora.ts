export type Supplier = {
    getSupplierValue(): unknown
}

export interface Receiver {
    setReceiverValue(val: unknown): void
}