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

export function deobf(s: string) {
    return decodeURIComponent(
        "%" +
            atob(s)
                .match(/.{1,2}/g)!
                .join("%")
    )
}

export function findListDifference<T>(previousList: T[], currentList: T[]): { added: T[]; removed: T[] } {
    const added = currentList.filter(item => !previousList.includes(item))
    const removed = previousList.filter(item => !currentList.includes(item))

    return { added, removed }
}

export async function hashBuffer(buffer: ArrayBuffer): Promise<string> {
    if (crypto?.subtle?.digest == null) {
        console.warn("Crypto not available, using timestamp as key")
        return Date.now().toString(16)
    }
    const hashBuffer = await crypto.subtle.digest("SHA-1", buffer)
    return Array.from(new Uint8Array(hashBuffer))
        .map(x => x.toString(16))
        .join("")
}
