/// Type system for the code simulation wiring panel.
///
/// The purpose of the type system is to prevent the user from being able to misconfigure
/// their robot by connecting two nodes that aren't compatible. Each individual value is typed
/// based on what it can be used for. Each value's type can be represented as the combination of
/// the underlying primitive data type, a unit, and a derivative order.
/// E.g., an angular velocity would be a number that is the first derivative of an angle
///
/// A base unit of NONE is included for unitless values such as a motor's percent output.
///
/// See the `SerializedNoraBaseType` definition for information on the serialized representation
/// of the various types

// for node colors
import { generatePalette } from "@/util/Colors"

/// NOTE: the variant string values should match TypeScript types (i.e., possible as the result of `typeof`)
export enum BaseType {
    NUMBER = "number",
    BOOLEAN = "boolean",
}

export enum BaseUnit {
    NONE = "none",
    POSITION = "position",
    ANGLE = "angle",
}

export enum DerivativeOrder {
    ZERO = "0", // base
    ONE = "1", // velocity
    TWO = "2", // acceleration
}

export enum BaseAxis {
    X = "x",
    Y = "y",
    Z = "z",
}

/// A NoraBaseType represents a single unit of data within this type system. E.g., a single axis angle from a gyroscope
export type NoraBaseType = { displayName?: string } & (
    | { type: BaseType.NUMBER; unit: BaseUnit; order: DerivativeOrder; axis?: BaseAxis }
    | { type: BaseType.BOOLEAN }
)

export type NoraBaseValue = NoraBaseValueOf<NoraBaseType>
export type NoraBaseValueOf<T extends NoraBaseType> = T extends { type: BaseType.BOOLEAN }
    ? { value: boolean; baseType: T }
    : { value: number; baseType: T }

/// A NoraType represents the entire input/output of a driver or stimulus. E.g., all angles reported by a gyroscope
export type NoraType = readonly NoraBaseType[]

export type NoraValue = readonly NoraBaseValue[]
export type NoraValueOf<T extends NoraType> = { readonly [K in keyof T]: NoraBaseValueOf<T[K]> }

/// Numbers serialize to `number:<unit>:<order>[:<axis>]` while booleans serialize to just `boolean`
type SerializedNoraBaseType<T extends NoraBaseType> = T extends {
    type: BaseType.NUMBER
    unit: infer U extends BaseUnit
    order: infer O extends DerivativeOrder
    axis: infer A extends BaseAxis
}
    ? `${BaseType.NUMBER}:${U}:${O}:${A}`
    : T extends { type: BaseType.NUMBER; unit: infer U extends BaseUnit; order: infer O extends DerivativeOrder }
      ? `${BaseType.NUMBER}:${U}:${O}`
      : `${BaseType.BOOLEAN}`

/// Ends up expanding to a comma separated string containing all serialized Nora base types from the array provided
/// Resolves to `never` for empty or non-tuple types
type SerializedNoraType<T extends readonly NoraBaseType[]> = T extends readonly [infer H extends NoraBaseType]
    ? SerializedNoraBaseType<H>
    : T extends readonly [infer H extends NoraBaseType, ...infer R extends readonly NoraBaseType[]]
      ? `${SerializedNoraBaseType<H>},${SerializedNoraType<R>}`
      : never

/**
 * Enforces that T be const and satisfy `NoraType`, meaning we have better type safety when using `NoraTypeOf`
 */
export function noraType<const T extends NoraType>(t: T): T {
    return t
}

/**
 * Utility function for defining a number in a `NoraType` definition
 */
export function num<const U extends BaseUnit, const O extends DerivativeOrder>(
    unit: U,
    order: O,
    displayName?: string
): { type: BaseType.NUMBER; unit: U; order: O; displayName?: string } {
    return { type: BaseType.NUMBER, unit, order, displayName }
}

/**
 * Utility function for defining a number with an axis in a `NoraType` definition
 */
export function numAxis<const U extends BaseUnit, const O extends DerivativeOrder, const A extends BaseAxis>(
    unit: U,
    order: O,
    axis: A,
    displayName?: string
): { type: BaseType.NUMBER; unit: U; order: O; axis: A; displayName?: string } {
    return { type: BaseType.NUMBER, unit, order, axis, displayName }
}

/**
 * Utility function for defining a boolean in a `NoraType` definition
 */
export function bool(displayName?: string): { type: BaseType.BOOLEAN; displayName?: string } {
    return { type: BaseType.BOOLEAN, displayName }
}

const UNITS = new Set<string>(Object.values(BaseUnit))
const ORDERS = new Set<string>(Object.values(DerivativeOrder))
const AXES = new Set<string>(Object.values(BaseAxis))

/**
 * Serializes a Nora base type into its string representation
 *
 * @see {@link SerializedNoraBaseType} for the string representation
 */
export function serializeNoraBaseType<T extends NoraBaseType>(t: T): SerializedNoraBaseType<T> {
    switch (t.type) {
        case BaseType.NUMBER:
            return `${t.type}:${t.unit}:${t.order}${t.axis !== undefined ? `:${t.axis}` : ""}` as SerializedNoraBaseType<T>
        case BaseType.BOOLEAN:
            return `${t.type}` as SerializedNoraBaseType<T>
    }
}

/**
 * Tries to deserialize a string into its Nora base type
 *
 * @throws Throws an error if the provided string doesn't represent a serialized Nora base type
 */
export function deserializeNoraBaseType(serialized: string): NoraBaseType {
    if (serialized === BaseType.BOOLEAN) return { type: BaseType.BOOLEAN }

    const split = serialized.split(":")

    // 3 without axis, 4 with
    if (split.length < 3 || split.length > 4) throw new Error(`Invalid serialized type ${serialized}`)

    const [type, unit, order, axis] = split

    if (
        type !== BaseType.NUMBER ||
        !UNITS.has(unit as BaseUnit) ||
        !ORDERS.has(order as DerivativeOrder) ||
        (axis !== undefined && !AXES.has(axis as BaseAxis))
    )
        throw new Error(`Invalid serialized components in ${serialized}`)

    return { type, unit, order, axis } as NoraBaseType
}

/**
 * Tries to serialize a Nora type into its string representation
 * @see {@link SerializedNoraType} for the string representation
 *
 * @throws Throws an error if the provided Nora type is empty ([])
 */
export function serializeNoraType<const T extends readonly NoraBaseType[]>(t: T): SerializedNoraType<T> {
    if (t.length === 0) throw new Error("Tried to serialize a zero-length Nora type (illegal)")

    return t.map(serializeNoraBaseType).join(",") as SerializedNoraType<T>
}

/**
 * Tries to deserialize a string into its Nora type.
 *
 * @throws Throws an error if the provided string doesn't represent a serialized Nora type
 */
export function deserializeNoraType(serialized: string): NoraType {
    if (serialized === "") throw new Error("Tried to deserialize an empty Nora type (illegal)")

    return serialized.split(",").map(deserializeNoraBaseType)
}

/**
 * @returns {boolean} whether the two types are compatible
 */
export function areTypesCompatible(type1: NoraType | null, type2: NoraType | null): boolean {
    if (type1 === null && type2 === null) return true
    if (type1 !== type2 && (type1 === null || type2 === null)) return false

    return serializeNoraType(type1!) === serializeNoraType(type2!)
}

/**
 * @returns {boolean} whether the two base types are compatible
 */
export function areBaseTypesCompatible(type1: NoraBaseType, type2: NoraBaseType): boolean {
    return serializeNoraBaseType(type1) === serializeNoraBaseType(type2)
}

/**
 * @returns {boolean} whether the value adheres to the defined type
 */
export function valueMatchesType(value: NoraValue, type: NoraType): boolean {
    return (
        value.length === type.length &&
        areTypesCompatible(
            value.map(v => v.baseType),
            type
        )
    )
}

const NORA_TYPE_PALETTE = generatePalette({ size: 16, startHue: 160, saturation: 50, lightness: 45 })
const NORA_BASE_TYPE_PALETTE = generatePalette({ size: 16, startHue: 140, saturation: 100, lightness: 60 })

const getIndexFromHash = (str: string, max: number): number => {
    let hash = 0
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash)
    }
    return Math.abs(hash) % max
}

export const noraTypeToColorStr = (type: NoraType): string => {
    if (type.length === 1) return noraBaseTypeToColorStr(type[0])

    const serialized = type.map(t => serializeNoraBaseType(t)).join(",")
    const paletteIndex = getIndexFromHash(serialized, NORA_TYPE_PALETTE.length)

    return NORA_TYPE_PALETTE[paletteIndex]
}

export const noraBaseTypeToColorStr = (baseType: NoraBaseType): string => {
    const serialized = serializeNoraBaseType(baseType)
    const paletteIndex = getIndexFromHash(serialized, NORA_BASE_TYPE_PALETTE.length)

    return NORA_BASE_TYPE_PALETTE[paletteIndex]
}
