import World from "@/systems/World"
import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"

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
