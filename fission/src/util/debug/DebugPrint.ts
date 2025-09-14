import type Jolt from "@azaleacolburn/jolt-physics"
import type * as THREE from "three"
import type { RigidNodeReadOnly } from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"

export function printRigidNodeParts(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly) {
    nodes.forEach(x => {
        console.log(`[ ${x.id} ]:`)
        x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`))
        console.log("")
    })
}

export function threeMatrix4ToString(mat: THREE.Matrix4) {
    const arr = mat.toArray()
    return (
        `[\n${arr[0].toFixed(4)}, ${arr[4].toFixed(4)}, ${arr[8].toFixed(4)}, ${arr[12].toFixed(4)},\n` +
        `${arr[1].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[13].toFixed(4)},\n` +
        `${arr[2].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[14].toFixed(4)},\n` +
        `${arr[3].toFixed(4)}, ${arr[7].toFixed(4)}, ${arr[11].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
    )
}

export function mirabufTransformToString(mat: mirabuf.ITransform) {
    const arr = mat.spatialMatrix!
    return (
        `[\n${arr[0].toFixed(4)}, ${arr[1].toFixed(4)}, ${arr[2].toFixed(4)}, ${arr[3].toFixed(4)},\n` +
        `${arr[4].toFixed(4)}, ${arr[5].toFixed(4)}, ${arr[6].toFixed(4)}, ${arr[7].toFixed(4)},\n` +
        `${arr[8].toFixed(4)}, ${arr[9].toFixed(4)}, ${arr[10].toFixed(4)}, ${arr[11].toFixed(4)},\n` +
        `${arr[12].toFixed(4)}, ${arr[13].toFixed(4)}, ${arr[14].toFixed(4)}, ${arr[15].toFixed(4)},\n]`
    )
}

export function mirabufVector3ToString(v: mirabuf.Vector3, units: number = 3) {
    return `(${v.x.toFixed(units)}, ${v.y.toFixed(units)}, ${v.z.toFixed(units)})`
}

export function threeVector3ToString(v: THREE.Vector3, units: number = 3) {
    return `(${v.x.toFixed(units)}, ${v.y.toFixed(units)}, ${v.z.toFixed(units)})`
}

export function threeQuaternionToString(v: THREE.Quaternion, units: number = 3) {
    return `(${v.x.toFixed(units)}, ${v.y.toFixed(units)}, ${v.z.toFixed(units)}, ${v.w.toFixed(units)})`
}

export function joltVec3ToString(v: Jolt.Vec3 | Jolt.RVec3, units: number = 3) {
    return `(${v.GetX().toFixed(units)}, ${v.GetY().toFixed(units)}, ${v.GetZ().toFixed(units)})`
}
