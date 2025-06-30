export function ternaryOnce<A, B>(obj: A | undefined, ifTrue: (x: A) => B, ifFalse: () => B): B {
    return obj ? ifTrue(obj) : ifFalse()
}

export function getFontSize(element: Element): number {
    const str = window.getComputedStyle(element).fontSize
    return Number(str.substring(0, str.length - 2))
}

export function clamp(num: number, min: number, max: number): number {
    return Math.min(Math.max(num, min), max)
}

export function* inspect<T>(iterable: Iterable<T>, fn: (item: T) => void): IterableIterator<T> {
    for (const item of iterable) {
        fn(item) // side effect
        yield item // pass the item along unchanged
    }
}
