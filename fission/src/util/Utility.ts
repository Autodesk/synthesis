import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import { convertJoltVec3ToThreeVector3 } from "./TypeConversions"
import World from "@/systems/World"
import JOLT from "./loading/JoltSyncLoader"

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

export function getOBBPoints(box: Jolt.OrientedBox): Jolt.Vec3[] {
    // get_mHalfExtents() and get_mOrientation() return REFERENCES to OBB-internal data,
    // not heap copies. Never call JOLT.destroy() on them.
    const halfExtents = box.get_mHalfExtents()
    const orientation = box.get_mOrientation()
    const corners = []

    for (let i = 0; i < 8; i++) {
        const x = i & 1 ? halfExtents.GetX() : -halfExtents.GetX()
        const y = i & 2 ? halfExtents.GetY() : -halfExtents.GetY()
        const z = i & 4 ? halfExtents.GetZ() : -halfExtents.GetZ()

        const localPoint = new JOLT.Vec3(x, y, z)
        corners.push(orientation.MulVec3(localPoint))
        JOLT.destroy(localPoint)
    }

    return corners
}

export function renderOBB(box: Jolt.OrientedBox): THREE.Points[] {
    const color = Math.floor(Math.random() * 0xffffff)
    const material = new THREE.PointsMaterial({ size: 0.1, color, sizeAttenuation: true })
    const points = getOBBPoints(box)
    const renderedDots: THREE.Points[] = []

    points.forEach(point => {
        const geometry = new THREE.BufferGeometry()
        const vertex = new Float32Array([point.GetX(), point.GetY(), point.GetZ()])
        const position = new THREE.BufferAttribute(vertex, 3)
        geometry.setAttribute("position", position)
        JOLT.destroy(point)

        const dot = new THREE.Points(geometry, material)
        World.sceneRenderer.addObject(dot)
        renderedDots.push(dot)
    })

    return renderedDots
}

export function renderAABox(box: Jolt.AABox): THREE.Line {
    const material = new THREE.LineBasicMaterial({ color: 0x00ff00 })
    const points = [convertJoltVec3ToThreeVector3(box.mMin, false), convertJoltVec3ToThreeVector3(box.mMax, false)]
    const geometry = new THREE.BufferGeometry().setFromPoints(points)

    const line = new THREE.Line(geometry, material)
    World.sceneRenderer.addObject(line)

    return line
}

export function renderThreeBox3(box: THREE.Box3) {
    const material = new THREE.LineBasicMaterial({ color: 0x00ff00 })
    const points = [box.min, box.max]
    const geometry = new THREE.BufferGeometry().setFromPoints(points)

    const line = new THREE.Line(geometry, material)
    World.sceneRenderer.addObject(line)
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

export function forPair<T, U>(listOne: T[], listTwo: U[], predicate: (one: T, two: U) => void): void {
    listOne.forEach(a => listTwo.forEach(b => predicate(a, b)))
}
