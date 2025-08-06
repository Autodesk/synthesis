import type { SxProps, Theme } from "@mui/material"
import type { MutableRefObject } from "react"
import type * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import type { GizmoMode } from "@/systems/scene/GizmoSceneObject"

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
