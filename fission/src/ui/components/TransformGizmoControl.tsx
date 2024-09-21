import { useEffect, useState } from "react";
import TransformGizmoControlProps from "./TransformGizmoControlProps";
import GizmoSceneObject, { GizmoMode } from "@/systems/scene/GizmoSceneObject";
import { ToggleButton, ToggleButtonGroup } from "./ToggleButtonGroup";
import World from "@/systems/World";

/**
 * Creates GizmoSceneObject and gives you a toggle button group to control the modes of the gizmo.
 * The lifetime of the gizmo is entirely handles within this component and will be recreated depending
 * on the updates made to the parameters provided. You can setup initial properties of the gizmo with
 * the `postGizmoCreation` handle.
 * 
 * @param param0 Transform Gizmo Controls.
 * @returns TransformGizmoControl component.
 */
function TransformGizmoControl({
    defaultMesh,
    gizmoRef,
    size,
    parent,
    defaultMode,
    translateDisabled,
    rotateDisabled,
    scaleDisabled,
    sx,
    postGizmoCreation
}: TransformGizmoControlProps) {

    const [mode, setMode] = useState<GizmoMode>(defaultMode)

    useEffect(() => {
        console.debug('Gizmo Recreation')

        const gizmo = new GizmoSceneObject(
            "translate",
            size,
            defaultMesh,
            parent
        )

        parent?.PostGizmoCreation(gizmo)
        postGizmoCreation?.(gizmo)

        gizmoRef.current = gizmo

        return () => {
            World.SceneRenderer.RemoveSceneObject(gizmo.id)
        }
    }, [gizmoRef, defaultMesh, size, parent, postGizmoCreation])

    useEffect(() => {
        return () => {
            if (gizmoRef.current) {
                World.SceneRenderer.RemoveSceneObject(gizmoRef.current.id)
                gizmoRef.current = undefined
            }
        }
    }, [gizmoRef])

    const disableOptions = 2 >=
        ((translateDisabled ? 1 : 0)
        + (rotateDisabled ? 1 : 0)
        + (scaleDisabled ? 1 : 0))

    // If there are no modes enabled, consider the UI pointless.
    return disableOptions ? (<></>) : (
        <ToggleButtonGroup
            value={mode}
            exclusive
            onChange={(_, v) => {
                if (v == undefined) return

                setMode(v)
                gizmoRef.current?.SetMode(v)
            }}
            sx={{
                ...(sx ?? {}),
                alignSelf: "center",
            }}
        >
            { translateDisabled ? <></> : <ToggleButton value={"translate"}>Move</ToggleButton> }
            { scaleDisabled ? <></> : <ToggleButton value={"scale"}>Scale</ToggleButton> }
            { rotateDisabled ? <></> : <ToggleButton value={"rotate"}>Rotate</ToggleButton> }
        </ToggleButtonGroup>
    )
}

export default TransformGizmoControl;