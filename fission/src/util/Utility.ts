import type Jolt from "@synthesis.adsk/jolt-physics"
import Pako from "pako"
import JOLT from "./loading/JoltSyncLoader"
import { globalAddToast } from "@/components/GlobalUIControls.ts"

export function ternaryOnce<A, B>(obj: A | undefined, ifTrue: (x: A) => B, ifFalse: () => B): B {
    return obj ? ifTrue(obj) : ifFalse()
}

export function getFontSize(element: Element): number {
    const str = window.getComputedStyle(element).fontSize
    return Number(str.substring(0, str.length - 2))
}

export function capitalize(word: string): string {
    return word[0].toUpperCase() + word.slice(1)
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

export async function hashBuffer(buffer: ArrayBuffer, fallbackHash?: string): Promise<string> {
    if (crypto?.subtle?.digest == null) {
        console.warn("Crypto not available, using fallback hash or timestamp as key")
        return fallbackHash ?? Date.now().toString(16)
    }
    const hashBuffer = await crypto.subtle.digest("SHA-1", buffer)
    return Array.from(new Uint8Array(hashBuffer))
        .map(x => x.toString(16).padStart(2, "0"))
        .join("")
}

export function forPair<T, U>(listOne: T[], listTwo: U[], predicate: (one: T, two: U) => void): void {
    listOne.forEach(a => listTwo.forEach(b => predicate(a, b)))
}

export function unzipMira(buff: Uint8Array): Uint8Array {
    // Check if file is gzipped via magic gzip numbers 31 139
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff)
    } else {
        return buff
    }
}

export function hexStringToUint8Array(hexString: string) {
    const arrayBuffer = new Uint8Array(hexString.length / 2)
    for (let i = 0; i < hexString.length; i += 2) {
        arrayBuffer[i / 2] = parseInt(hexString.substring(i, i + 2), 16)
    }
    return arrayBuffer
}

// biome-ignore lint/suspicious/noExplicitAny: JSON.parse returns `any`
export function tryParse(data: string): any {
    try {
        return JSON.parse(data)
    } catch (error) {
        console.error("Could not parse JSON", error)
        return null
    }
}

export function downloadBlob(filename: string, data: BlobPart): void {
    const blob = new Blob([data], {
        type: "application/octet-stream",
    })
    const url = URL.createObjectURL(blob)

    const a = document.createElement("a")
    a.href = url
    a.download = filename
    document.body.appendChild(a)
    a.click()
    setTimeout(() => {
        document.body.removeChild(a)
        URL.revokeObjectURL(url)
    }, 0)
}

export function copyVec3(vec: Jolt.Vec3): Jolt.Vec3 {
    return new JOLT.Vec3(vec.GetX(), vec.GetY(), vec.GetZ())
}

/**
 * Returns a promise that will resolve in the next event loop iteration.
 * Useful in long, blocking functions to allow the UI to update
 */
export const yieldToMain = () => new Promise<void>(resolve => setTimeout(resolve, 0))

export async function waitUntil(condition: () => boolean, interval: number = 1000, timeout?: number) {
    let handle: NodeJS.Timeout | string | number | undefined
    try {
        return await new Promise<boolean>(resolve => {
            if (timeout != null) {
                setTimeout(() => resolve(false), timeout)
            }

            handle = setInterval(() => {
                if (condition()) {
                    resolve(true)
                }
            }, interval)
        })
    } finally {
        clearInterval(handle)
    }
}

export async function withTimeout(promise: Promise<boolean>, timeoutMessage: string, duration: number = 5000) {
    let timeout: NodeJS.Timeout
    return await Promise.race([
        promise,
        new Promise<boolean>(res => {
            timeout = setTimeout(() => {
                globalAddToast("warning", timeoutMessage)
                res(false)
            }, duration)
        }),
    ]).then(v => {
        clearTimeout(timeout)
        return v
    })
}

export function isDefined<T>(item: T | undefined): item is T {
    return item !== undefined
}

export type RecursivePartial<T> = {
    [P in keyof T]?: T[P] extends (infer U)[]
        ? RecursivePartial<U>[]
        : T[P] extends object | undefined
          ? RecursivePartial<T[P]>
          : T[P]
}
