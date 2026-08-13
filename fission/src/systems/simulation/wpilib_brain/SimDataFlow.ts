import { areTypesCompatible, BaseType, valueMatchesType, type NoraType, type NoraValueOf } from "../Nora"

export type SimSupplier<T extends NoraType> = {
    get supplierType(): T
    getSupplierValue(): NoraValueOf<T>
}

export type SimReceiver<T extends NoraType> = {
    get receiverType(): T
    setReceiverValue(val: NoraValueOf<T>): void
}

export type SimFlow<T extends NoraType = NoraType> = {
    supplier: SimSupplier<T>
    receiver: SimReceiver<T>
}

export function validate<T extends NoraType, U extends NoraType>(s: SimSupplier<T>, r: SimReceiver<U>): boolean {
    return areTypesCompatible(s.supplierType, r.receiverType)
}

export enum AggregateStrategy {
    AVERAGE,
}

type AggregateValuesFunc = (type: NoraType, vals: NoraValueOf<NoraType>[]) => NoraValueOf<NoraType>

const aggregateAverage: AggregateValuesFunc = <T extends NoraType>(type: T, vals: NoraValueOf<T>[]): NoraValueOf<T> => {
    if (vals.length === 0) throw new Error("Tried to aggregate empty NoraValue array")

    if (vals.some(v => v.length !== type.length)) throw new Error("Tried to aggregate array of differing NoraValues")

    if (vals.some(v => !valueMatchesType(v, type)))
        throw new Error("Tried to aggregate NoraValues with mismatching types")

    let ret = []

    // TODO: test this
    for (let i = 0; i < type.length; i++) {
        const t = type[i]
        const avg = vals.map(v => Number(v[i].value)).reduce((acc, v) => acc + v, 0) / vals.length
        if (t.type === BaseType.BOOLEAN) {
            // majority vote
            ret.push(avg > 0.5)
        } else if (t.type === BaseType.NUMBER) {
            ret.push(avg)
        }
    }

    return ret as NoraValueOf<T>
}

const AGGREGATE_FUNCTIONS: { [key in AggregateStrategy]: AggregateValuesFunc } = {
    [AggregateStrategy.AVERAGE]: aggregateAverage,
}

export function aggregateValues<T extends NoraType>(strategy: AggregateStrategy, type: T, vals: NoraValueOf<T>[]) {
    return AGGREGATE_FUNCTIONS[strategy](type, vals)
}
