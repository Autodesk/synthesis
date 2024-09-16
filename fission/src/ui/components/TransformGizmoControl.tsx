import { useEffect, useState } from "react";
import TransformGizmoControlProps from "./TransformGizmoControlProps";
// import * as THREE from 'three';
import GizmoSceneObject, { GizmoMode } from "@/systems/scene/GizmoSceneObject";
import { ToggleButton, ToggleButtonGroup } from "./ToggleButtonGroup";
import World from "@/systems/World";

function TransformGizmoControl({
    defaultMesh,
    gizmoRef,
    size,
    parent,
    defaultMode,
    translateDisabled,
    rotateDisabled,
    scaleDisabled,
    sx
}: TransformGizmoControlProps) {

    const [mode, setMode] = useState<GizmoMode>(defaultMode)

    useEffect(() => {
        const gizmo = new GizmoSceneObject(
            defaultMesh,
            "translate",
            size,
            parent
        )

        gizmoRef.current = gizmo

        return () => {
            World.SceneRenderer.RemoveSceneObject(gizmo.id)
        }
    }, [gizmoRef, defaultMesh, size, parent])

    useEffect(() => {
        return () => {
            if (gizmoRef.current) {
                World.SceneRenderer.RemoveSceneObject(gizmoRef.current.id)
                gizmoRef.current = undefined
            }
        }
    }, [gizmoRef])

    // If there are no modes enabled, consider the UI pointless.
    return (translateDisabled && rotateDisabled && scaleDisabled) ? (<></>) : (
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