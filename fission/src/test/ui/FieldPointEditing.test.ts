import { renderHook } from "@testing-library/react"
import * as THREE from "three"
import { beforeEach, describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import {
    type FieldPointMarker,
    useFieldPointMarkers,
    useFieldRelativeGizmoPosition,
} from "@/ui/panels/configuring/assembly-config/interfaces/FieldPointEditing"

let scene: THREE.Scene

vi.mock("@/systems/World", () => ({
    default: {
        get sceneRenderer() {
            return { scene }
        },
    },
}))

function fieldAt(x: number, y: number, z: number): MirabufSceneObject {
    return {
        getXZPositionTransform: (vec: THREE.Vector3) => vec.set(x, y, z),
    } as unknown as MirabufSceneObject
}

function emptyGizmo(): GizmoSceneObject {
    return { obj: new THREE.Object3D() } as unknown as GizmoSceneObject
}

function tracksDisposal(target: THREE.Material | THREE.BufferGeometry): () => boolean {
    let disposed = false
    target.addEventListener("dispose", () => {
        disposed = true
    })
    return () => disposed
}

beforeEach(() => {
    scene = new THREE.Scene()
})

describe("useFieldRelativeGizmoPosition", () => {
    test("places a new gizmo at the field-relative offset", () => {
        const { result } = renderHook(() => useFieldRelativeGizmoPosition(fieldAt(10, 0, -5), [1, 2, 3]))
        const gizmo = emptyGizmo()

        result.current.postGizmoCreation(gizmo)

        expect(gizmo.obj.position.toArray()).toEqual([11, 2, -2])
    })

    test("reads a dragged gizmo back as field-relative coordinates", () => {
        const { result } = renderHook(() => useFieldRelativeGizmoPosition(fieldAt(10, 0, -5), [1, 2, 3]))
        const gizmo = emptyGizmo()
        result.current.postGizmoCreation(gizmo)
        result.current.gizmoRef.current = gizmo

        expect(result.current.readFieldRelativePosition()).toEqual([1, 2, 3])

        gizmo.obj.position.x += 4

        expect(result.current.readFieldRelativePosition()).toEqual([5, 2, 3])
    })
})

describe("useFieldPointMarkers", () => {
    test("adds a marker per point and removes them on unmount", () => {
        const points: FieldPointMarker[] = [{ pos: [1, 0, 2], yaw: Math.PI / 2 }, { pos: [-1, 0, 0] }]

        const { unmount } = renderHook(() => useFieldPointMarkers(fieldAt(10, 0, -5), points))

        expect(scene.children).toHaveLength(2)

        const [oriented, plain] = scene.children
        expect(oriented.position.toArray()).toEqual([11, 0, -3])
        expect(oriented.rotation.y).toBeCloseTo(Math.PI / 2)
        expect(oriented.children).toHaveLength(2) // dot + direction cone
        expect(plain.children).toHaveLength(1) // dot only, no yaw to indicate

        const geometryDisposed = tracksDisposal((oriented.children[0] as THREE.Mesh).geometry)

        unmount()

        expect(scene.children).toHaveLength(0)
        expect(geometryDisposed()).toBe(true)
    })
})
