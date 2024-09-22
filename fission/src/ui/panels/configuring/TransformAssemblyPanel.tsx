import Panel, { PanelPropsImpl } from "@/components/Panel"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useEffect, useMemo } from "react"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import World from "@/systems/World"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsSystem"

const TransformAssemblyPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const targetAssembly = useMemo(() => {
        return getSpotlightAssembly()
    }, [])

    useEffect(() => {
        World.PhysicsSystem.HoldPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.PhysicsSystem.ReleasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    return (
        <Panel
            name="Assembly Setup"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={false}
            icon={SynthesisIcons.Gamepad}
            cancelEnabled={true}
            cancelName="Close"
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                {targetAssembly ? (
                    <TransformGizmoControl
                        key={"init-config-gizmo"}
                        defaultMode="translate"
                        scaleDisabled={true}
                        size={3.0}
                        parent={targetAssembly}
                    />
                ) : (
                    <></>
                )}
            </div>
        </Panel>
    )
}

export default TransformAssemblyPanel
