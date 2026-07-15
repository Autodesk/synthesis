import { useCallback, useEffect, useRef } from "react"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"

/** Holds a physics pause for as long as the calling component is mounted (e.g. while editing a field-relative point). */
export function useHoldPhysicsPauseWhileMounted() {
    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])
}

/** Runs `callback` whenever the assembly config panel is saved (e.g. to persist in-progress edits). */
export function useConfigurationSavedListener(callback: () => void) {
    useEffect(() => EventSystem.listen("ConfigurationSavedEvent", callback), [callback])
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
            const fieldRef = selectedField.getPositionTransform(new THREE.Vector3())
            gizmo.obj.position.set(fieldRef.x + pos[0], fieldRef.y + pos[1], fieldRef.z + pos[2])
        },
        [selectedField, pos]
    )

    const readFieldRelativePosition = useCallback((): [number, number, number] => {
        if (!gizmoRef.current) return [pos[0], pos[1], pos[2]]
        gizmoRef.current.obj.updateWorldMatrix(true, false)
        const worldPos = gizmoRef.current.obj.getWorldPosition(new THREE.Vector3())
        const fieldRef = selectedField.getPositionTransform(new THREE.Vector3())
        return [worldPos.x - fieldRef.x, worldPos.y - fieldRef.y, worldPos.z - fieldRef.z]
    }, [selectedField, pos])

    return { gizmoRef, postGizmoCreation, readFieldRelativePosition }
}
