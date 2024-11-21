export enum NoraTypes {
    Number = "num",
    Number2 = "[num,num]",
    Number3 = "[num,num,num]",
    Unknown = "unknown",
}

export type NoraNumber = number
export type NoraNumber2 = [ NoraNumber, NoraNumber ]
export type NoraNumber3 = [ NoraNumber, NoraNumber, NoraNumber ]
export type NoraUnknown = unknown

export type NoraType = NoraNumber | NoraNumber2 | NoraNumber3 | NoraUnknown

// Needed?
// export function constructNoraType(...types: NoraTypes[]): NoraTypes {
//     return `[${types.join(",")}]` as NoraTypes
// }

export function deconstructNoraType(type: NoraTypes): NoraTypes[] | undefined {
    if (type.charAt(0) != "[" || type.charAt(type.length - 1) != "]")
        return undefined
    return type.substring(1, type.length - 1).split(",") as NoraTypes[]
}

export function isNoraDeconstructable(type: NoraTypes): boolean {
    return type.charAt(0) == "[" && type.charAt(type.length - 1) == "]"
}