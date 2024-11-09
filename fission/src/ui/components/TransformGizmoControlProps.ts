import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import GizmoSceneObject, { GizmoMode } from "@/systems/scene/GizmoSceneObject"
import { SxProps, Theme } from "@mui/material"
import { MutableRefObject } from "react"
import * as THREE from "three"

interface TransformGizmoControlProps {
    defaultMesh?: THREE.Mesh
    gizmoRef?: MutableRefObject<GizmoSceneObject | undefined>
    size: number
    parent?: MirabufSceneObject
    defaultMode: GizmoMode
    translateDisabled?: boolean
    rotateDisabled?: boolean
    scaleDisabled?: boolean
    sx?: SxProps<Theme>
    postGizmoCreation?: (gizmo: GizmoSceneObject) => void
    onAccept?: (gizmo: GizmoSceneObject | undefined) => void
    onCancel?: (gizmo: GizmoSceneObject | undefined) => void
}

export default TransformGizmoControlProps
