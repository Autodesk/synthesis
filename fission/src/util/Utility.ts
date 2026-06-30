import Jolt from "@azaleacolburn/jolt-physics"
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

export function copyJoltRMat44(mat: Jolt.RMat44): Jolt.RMat44 {
    const [translation, rotation] = [mat.GetTranslation(), mat.GetQuaternion()]
    const newMat = new JOLT.RMat44().sRotationTranslation(rotation, translation)

    // JOLT.destroy(translation)
    // JOLT.destroy(rotation)

    return newMat
}

export function copyJoltMat44(mat: Jolt.Mat44): Jolt.Mat44 {
    const [translation, rotation] = [mat.GetTranslation(), mat.GetQuaternion()]
    const newMat = new JOLT.Mat44().sRotationTranslation(rotation, translation)

    // TODO
    // Tests fail when these are destroyed
    // JOLT.destroy(translation)
    // JOLT.destroy(rotation)

    return newMat
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
