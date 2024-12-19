export function ternaryOnce<A, B>(obj: A | undefined, ifTrue: (x: A) => B, ifFalse: () => B): B {
    return obj ? ifTrue(obj) : ifFalse()
}

export function getFontSize(element: Element): number {
    const str = window.getComputedStyle(element).fontSize
    return Number(str.substring(0, str.length - 2))
}
