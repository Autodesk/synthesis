import { useCallback, useEffect, useMemo, useRef } from "react"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"

const DIRECTION_INDICATOR_COLOR = 0xffcc33
const INDICATOR_RENDER_ORDER = 1000

function createIndicatorMaterial(): THREE.MeshToonMaterial {
    const material = World.sceneRenderer.createToonMaterial(DIRECTION_INDICATOR_COLOR)
    material.depthTest = false
    material.depthWrite = false
    material.transparent = true
    return material
}

function createDirectionConeGeometry(facing: "+z" | "-z" = "+z"): THREE.ConeGeometry {
    const sign = facing === "+z" ? 1 : -1
    return new THREE.ConeGeometry(0.12, 0.5, 8).rotateX(sign * (Math.PI / 2)).translate(0, 0, sign * 0.35)
}

/**
 * Wires a {@link GizmoSceneObject} to a field-relative position at `pos` (relative to the field's
 * position transform) on creation.
 */
export function useFieldRelativeGizmoPosition(
    selectedField: MirabufSceneObject,
    pos: Readonly<[number, number, number]>
) {
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const postGizmoCreation = useCallback(
        (gizmo: GizmoSceneObject) => {
            const fieldRef = selectedField.getXZPositionTransform(new THREE.Vector3())
            gizmo.obj.position.set(fieldRef.x + pos[0], fieldRef.y + pos[1], fieldRef.z + pos[2])
        },
        [selectedField, pos]
    )

    const readFieldRelativePosition = useCallback((): [number, number, number] => {
        if (!gizmoRef.current) return [pos[0], pos[1], pos[2]]
        gizmoRef.current.obj.updateWorldMatrix(true, false)
        const worldPos = gizmoRef.current.obj.getWorldPosition(new THREE.Vector3())
        const fieldRef = selectedField.getXZPositionTransform(new THREE.Vector3())
        return [worldPos.x - fieldRef.x, worldPos.y - fieldRef.y, worldPos.z - fieldRef.z]
    }, [selectedField, pos])

    return { gizmoRef, postGizmoCreation, readFieldRelativePosition }
}

/**
 * Creates a cone-shaped mesh used to visualize which way a field-relative point faces,
 * Keep its rotation in sync with {@link useSyncIndicatorRotation}.
 */
export function useDirectionIndicatorMesh(facing: "+z" | "-z" = "+z") {
    const mesh = useMemo(() => {
        const m = new THREE.Mesh(createDirectionConeGeometry(facing), createIndicatorMaterial())
        m.renderOrder = 1000
        return m
    }, [facing])

    useEffect(() => {
        return () => {
            mesh.geometry.dispose()
            const material = mesh.material as THREE.MeshToonMaterial
            material.gradientMap?.dispose()
            material.dispose()
        }
    }, [mesh])

    return mesh
}

/**
 * Keeps a direction-indicator mesh's own rotation in sync with the given yaw/pitch (radians)
 * whenever they change.
 */
export function useSyncIndicatorRotation(indicatorMesh: THREE.Object3D, yaw: number, pitch: number = 0) {
    useEffect(() => {
        indicatorMesh.rotation.set(pitch, yaw, 0, "YXZ")
    }, [indicatorMesh, yaw, pitch])
}

export interface FieldPointMarker {
    pos: Readonly<[number, number, number]>
    yaw?: number
    pitch?: number
}

/**
 * Small dot + cone markers for a list of field-relative points, to visualize them all at once in the scene
 */
export function useFieldPointMarkers(
    selectedField: MirabufSceneObject,
    points: readonly FieldPointMarker[],
    facing: "+z" | "-z" = "+z"
) {
    useEffect(() => {
        const fieldRef = selectedField.getXZPositionTransform(new THREE.Vector3())

        const markers = points.map(({ pos, yaw, pitch }) => {
            const material = createIndicatorMaterial()
            const geometries: THREE.BufferGeometry[] = [new THREE.SphereGeometry(0.1, 12, 12)]
            const dot = new THREE.Mesh(geometries[0], material)
            dot.renderOrder = INDICATOR_RENDER_ORDER

            const group = new THREE.Group()
            group.position.set(fieldRef.x + pos[0], fieldRef.y + pos[1], fieldRef.z + pos[2])
            group.add(dot)

            if (yaw !== undefined) {
                const coneGeometry = createDirectionConeGeometry(facing)
                geometries.push(coneGeometry)
                const cone = new THREE.Mesh(coneGeometry, material)
                cone.renderOrder = INDICATOR_RENDER_ORDER
                group.rotation.set(pitch ?? 0, yaw, 0, "YXZ")
                group.add(cone)
            }

            World.sceneRenderer.scene.add(group)

            return { group, material, geometries }
        })

        return () => {
            markers.forEach(({ group, material, geometries }) => {
                World.sceneRenderer.scene.remove(group)
                geometries.forEach(geometry => geometry.dispose())
                material.gradientMap?.dispose()
                material.dispose()
            })
        }
    }, [selectedField, points, facing])
}
