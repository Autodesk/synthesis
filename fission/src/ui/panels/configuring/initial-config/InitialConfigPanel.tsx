import Panel, { PanelPropsImpl } from "@/components/Panel"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { useCallback, useEffect, useMemo } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "../assembly-config/ConfigurationType"
import { setSelectedScheme } from "../assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelection from "./InputSchemeSelection"
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    const closeFinish = useCallback(() => {
        if (targetAssembly?.miraType == MiraType.ROBOT) {
            const brainIndex = SynthesisBrain.GetBrainIndex(targetAssembly)

            if (brainIndex == undefined) return

            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]

            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)

            setSelectedConfigurationType(ConfigurationType.INPUTS)
            setSelectedScheme(scheme)
        }

        closePanel(panelId)
    }, [closePanel, panelId, targetAssembly])

    const closeDelete = useCallback(() => {
        if (targetAssembly) {
            World.SceneRenderer.RemoveSceneObject(targetAssembly.id)
        }

        closePanel(panelId)
    }, [closePanel, panelId, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.GetBrainIndex(targetAssembly)
    }, [targetAssembly])

    return (
        <Panel
            name="Assembly Setup"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={true}
            acceptName="Finish"
            onAccept={() => closeFinish()}
            icon={SynthesisIcons.Gamepad}
            cancelEnabled={true}
            cancelName="Delete"
            onCancel={() => closeDelete()}
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
                        onAccept={closeFinish}
                        onCancel={closeDelete}
                    />
                ) : (
                    <></>
                )}
                {brainIndex != undefined ? (
                    <InputSchemeSelection
                        brainIndex={brainIndex}
                        onSelect={() => {}}
                        onEdit={() => openPanel("configure")}
                        onCreateNew={() => openModal("assign-new-scheme")}
                    />
                ) : (
                    <></>
                )}
            </div>
        </Panel>
    )
}

export default InitialConfigPanel
