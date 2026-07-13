import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import Pako from "pako"
import World from "@/systems/World"

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

export function renderAxisAlignedBox(
    box: Jolt.AABox,
    color: THREE.ColorRepresentation = 0x00aaff,
    opacity: number = 0.3
): THREE.Mesh {
    const size = box.GetSize()
    const geo = new THREE.BoxGeometry(size.GetX(), size.GetY(), size.GetZ())
    const material = new THREE.MeshPhongMaterial({
        color,
        opacity,
        transparent: true,
        depthWrite: false,
        side: THREE.DoubleSide,
    })
    const mesh = new THREE.Mesh(geo, material)

    const t = box.GetCenter()
    mesh.position.set(t.GetX(), t.GetY(), t.GetZ())

    World.sceneRenderer.addObject(mesh)
    return mesh
}

export function renderOrientedBox(
    obb: Jolt.OrientedBox,
    color: THREE.ColorRepresentation = 0x00aaff,
    opacity: number = 0.3
): THREE.Mesh {
    const halfExtents = obb.mHalfExtents
    const geo = new THREE.BoxGeometry(halfExtents.GetX() * 2, halfExtents.GetY() * 2, halfExtents.GetZ() * 2)
    const material = new THREE.MeshPhongMaterial({
        color,
        opacity,
        transparent: true,
        depthWrite: false,
        side: THREE.DoubleSide,
    })
    const mesh = new THREE.Mesh(geo, material)

    const orientation = obb.mOrientation
    const t = orientation.GetTranslation()
    const q = orientation.GetQuaternion()
    mesh.position.set(t.GetX(), t.GetY(), t.GetZ())
    mesh.quaternion.set(q.GetX(), q.GetY(), q.GetZ(), q.GetW())

    World.sceneRenderer.addObject(mesh)
    return mesh
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
