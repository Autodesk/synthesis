import type React from "react"
import { useEffect, useState } from "react"
import InputSystem from "@/systems/input/InputSystem"
import GizmoSceneObject, { type GizmoMode } from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import { ToggleButton, ToggleButtonGroup } from "./StyledComponents"
import type TransformGizmoControlProps from "./TransformGizmoControlProps"

/**
 * Creates GizmoSceneObject and gives you a toggle button group to control the modes of the gizmo.
 * The lifetime of the gizmo is entirely handles within this component and will be recreated depending
 * on the updates made to the parameters provided. You can setup initial properties of the gizmo with
 * the `postGizmoCreation` handle.
 *
 * @param param0 Transform Gizmo Controls.
 * @returns TransformGizmoControl component.
 */
const TransformGizmoControl: React.FC<TransformGizmoControlProps> = ({
    defaultMesh,
    gizmoRef,
    size,
    parent,
    defaultMode,
    translateDisabled,
    rotateDisabled,
    scaleDisabled,
    sx,
    postGizmoCreation,
    onAccept,
    onCancel,
}: TransformGizmoControlProps) => {
    const [mode, setMode] = useState<GizmoMode>(defaultMode)
    const [gizmo, setGizmo] = useState<GizmoSceneObject | undefined>(undefined)

    useEffect(() => {
        const gizmo = new GizmoSceneObject("translate", size, defaultMesh, parent, (gizmo: GizmoSceneObject) => {
            parent?.postGizmoCreation(gizmo)
            postGizmoCreation?.(gizmo)
        })

        if (gizmoRef) gizmoRef.current = gizmo

        setGizmo(gizmo)

        return () => {
            World.sceneRenderer.removeSceneObject(gizmo.id)
        }
    }, [gizmoRef, defaultMesh, size, parent, postGizmoCreation])

    useEffect(() => {
        return () => {
            if (gizmoRef) gizmoRef.current = undefined
        }
    }, [gizmoRef])

    const disableOptions = 2 <= (translateDisabled ? 1 : 0) + (rotateDisabled ? 1 : 0) + (scaleDisabled ? 1 : 0)

    const buttons = []
    if (!translateDisabled)
        buttons.push(
            <ToggleButton key="translate-button" value={"translate"}>
                Move
            </ToggleButton>
        )
    if (!rotateDisabled)
        buttons.push(
            <ToggleButton key="rotate-button" value={"rotate"}>
                Rotate
            </ToggleButton>
        )
    if (!scaleDisabled)
        buttons.push(
            <ToggleButton key="scale-button" value={"scale"}>
                Scale
            </ToggleButton>
        )

    useEffect(() => {
        const func = () => {
            // creating enter key and escape key event listeners
            if (InputSystem.isKeyPressed("Enter")) {
                onAccept?.(gizmo)
            } else if (InputSystem.isKeyPressed("Escape")) {
                onCancel?.(gizmo)
            }

            cancelAnimationFrame(animHandle)
            animHandle = requestAnimationFrame(func)
        }
        let animHandle = requestAnimationFrame(func)

        return () => {
            cancelAnimationFrame(animHandle)
        }
    }, [gizmo, onAccept, onCancel])

    // If there are no modes enabled, consider the UI pointless.
    return disableOptions ? undefined : (
        <>
            <ToggleButtonGroup
                value={mode}
                exclusive
                onChange={(_, v) => {
                    if (v === undefined) return

                    setMode(v)
                    gizmo?.setMode(v)
                }}
                sx={{
                    ...(sx ?? {}),
                    alignSelf: "center",
                    display: "flex",
                    justifyContent: "center",
                }}
            >
                {buttons}
            </ToggleButtonGroup>
        </>
    )
}

export default TransformGizmoControl
