import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useMemo } from "react"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsTypes"
import World from "@/systems/World"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { PanelImplProps } from "@/ui/components/Panel"

const TransformAssemblyPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const targetAssembly = useMemo(() => getSpotlightAssembly(), [])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "Assembly Setup", hideAccept: true, cancelText: "Close" }, {})
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
