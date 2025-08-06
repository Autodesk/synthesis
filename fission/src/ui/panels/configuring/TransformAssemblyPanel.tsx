import { Stack } from "@mui/material"
import type React from "react"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { useEffect, useMemo } from "react"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsTypes"
import type { PanelImplProps } from "@/ui/components/Panel"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const TransformAssemblyPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const targetAssembly = useMemo(() => getSpotlightAssembly(), [])

    useEffect(() => {
        PhysicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            PhysicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
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
