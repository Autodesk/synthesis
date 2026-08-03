/**
 * To build input validation into the node editor, I had to
 * make this poor man's type system. Please make it better.
 *
 * We should be able to assign identifiers to the types and
 * probably have more in-tune mechanisms for handling the
 * junction situations. Right now its kinda patched together
 * with the averaging function setup I have below.
 */

// type is "<unit>:<shape>", but absent unit means only shape determines connection viability
// (e.g., NUMBER3 can be connected to any 3-arity type)
export enum NoraTypes {
    NUMBER = "num",
    NUMBER2 = "(num,num)",
    NUMBER3 = "(num,num,num)",
    NUMBER6 = "(num,num,num,num,num,num)",
    GYRO = "gyro:(num,num,num,num,num,num)",
    ACCEL = "accel:(num,num,num,num,num,num)",
    UNKNOWN = "unknown",
}

function shapeOf(type: NoraTypes): string {
    const i = type.indexOf(":")
    return i === -1 ? type : type.substring(i + 1)
}

type Tuple<N extends number, T, R extends T[] = []> = R["length"] extends N ? R : Tuple<N, T, [...R, T]>

export type NoraNumber = number
export type NoraValue<N extends number, T = number> = N extends 1 ? T : Tuple<N, T>
export type NoraUnknown = unknown

export type NoraType = NoraValue<1> | NoraValue<2> | NoraValue<3> | NoraValue<6> | NoraUnknown

export function deconstructNoraType(type: NoraTypes): NoraTypes[] | undefined {
    const shape = shapeOf(type)
    if (shape.charAt(0) != "(" || shape.charAt(shape.length - 1) != ")") return undefined
    return shape.substring(1, shape.length - 1).split(",") as NoraTypes[]
}

export function isNoraDeconstructable(type: NoraTypes): boolean {
    const shape = shapeOf(type)
    return shape.charAt(0) == "(" && shape.charAt(shape.length - 1) == ")"
}

const averageFuncMap: { [shape: string]: ((...many: NoraType[]) => NoraType) | undefined } = {
    [NoraTypes.NUMBER]: function (...many: NoraType[]): NoraType {
        return many.reduce<NoraNumber>((prev, next) => prev + (next as NoraNumber), 0)
    },
}

export function noraAverageFunc(type: NoraTypes): ((...many: NoraType[]) => NoraType) | undefined {
    return averageFuncMap[shapeOf(type)]
}

export function hasNoraAverageFunc(type: NoraTypes): boolean {
    return averageFuncMap[shapeOf(type)] != undefined
}
