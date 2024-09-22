import Panel, { PanelPropsImpl } from "@/components/Panel"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import {
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { useEffect, useMemo } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "./assembly-config/ConfigurationType"
import { setSelectedScheme } from "./assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import World from "@/systems/World"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsSystem"

const InitialConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel, openPanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    const targetAssembly = useMemo(() => {
        return getSpotlightAssembly()
    }, [])

    useEffect(() => {
        World.PhysicsSystem.HoldPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.PhysicsSystem.ReleasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    useEffect(() => {
        closePanel("import-mirabuf")

        /** If the panel is closed before a scheme is selected, defaults to the top of the list */
        if (targetAssembly?.miraType == MiraType.ROBOT) {
            return () => {
                const brainIndex = SynthesisBrain.GetBrainIndex(targetAssembly)

                if (brainIndex == undefined) return
    
                if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return
    
                const scheme = InputSchemeManager.availableInputSchemes[0]
    
                InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
    
                setSelectedConfigurationType(ConfigurationType.INPUTS)
                setSelectedScheme(scheme)
            }
        } else {
            return () => { }
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.GetBrainIndex(targetAssembly)
    }, [targetAssembly])

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
                {targetAssembly ? (<TransformGizmoControl
                    key={"init-config-gizmo"}
                    defaultMode="translate"
                    scaleDisabled={true}
                    size={3.0}
                    parent={targetAssembly}
                />) : (<></>)}
                {brainIndex != undefined ? (<InputSchemeSelection
                    getBrainIndex={() => brainIndex}
                    onSelect={() => closePanel(panelId)}
                    onEdit={() => openPanel("configure")}
                    onCreateNew={() => openModal("assign-new-scheme")}
                />) : (<></>)}
            </div>
        </Panel>
    )
}

export default InitialConfigPanel
