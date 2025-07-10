import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useMemo } from "react"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsSystem"
import World from "@/systems/World"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"

const TransformAssemblyPanel: React.FC = () => {
    const targetAssembly = useMemo(() => getSpotlightAssembly(), [])

    useEffect(() => {
        World.PhysicsSystem.HoldPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.PhysicsSystem.ReleasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    return (
        <Stack gap={2}>
            {targetAssembly && (
                <TransformGizmoControl
                    key="init-config-gizmo"
                    defaultMode="translate"
                    scaleDisabled={true}
                    size={3.0}
                    parent={targetAssembly}
                />
            )}
        </Stack>
    )
}

export default TransformAssemblyPanel
