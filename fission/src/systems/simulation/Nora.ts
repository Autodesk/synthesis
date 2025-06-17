/**
 * To build input validation into the node editor, I had to
 * make this poor man's type system. Please make it better.
 *
 * We should be able to assign identifiers to the types and
 * probably have more in-tune mechanisms for handling the
 * junction situations. Right now its kinda patched together
 * with the averaging function setup I have below.
 */

export enum NoraTypes {
    Number = "num",
    Number2 = "(num,num)",
    Number3 = "(num,num,num)",
    Unknown = "unknown",
}

export type NoraNumber = number
export type NoraNumber2 = [NoraNumber, NoraNumber]
export type NoraNumber3 = [NoraNumber, NoraNumber, NoraNumber]
export type NoraUnknown = unknown

export type NoraType = NoraNumber | NoraNumber2 | NoraNumber3 | NoraUnknown

// Needed?
// export function constructNoraType(...types: NoraTypes[]): NoraTypes {
//     return `[${types.join(",")}]` as NoraTypes
// }

export function deconstructNoraType(type: NoraTypes): NoraTypes[] | undefined {
    if (type.charAt(0) != "(" || type.charAt(type.length - 1) != ")") return undefined
    return type.substring(1, type.length - 1).split(",") as NoraTypes[]
}

export function isNoraDeconstructable(type: NoraTypes): boolean {
    return type.charAt(0) == "(" && type.charAt(type.length - 1) == ")"
}

const averageFuncMap: { [k in NoraTypes]: ((...many: NoraType[]) => NoraType) | undefined } = {
    [NoraTypes.Number]: function (...many: NoraType[]): NoraType {
        return many.reduce<NoraNumber>((prev, next) => (prev += next as NoraNumber), 0)
    },
    [NoraTypes.Number2]: undefined,
    [NoraTypes.Number3]: undefined,
    [NoraTypes.Unknown]: undefined,
}

export function noraAverageFunc(type: NoraTypes): ((...many: NoraType[]) => NoraType) | undefined {
    return averageFuncMap[type]
}

export function hasNoraAverageFunc(type: NoraTypes): boolean {
    return averageFuncMap[type] != undefined
}
