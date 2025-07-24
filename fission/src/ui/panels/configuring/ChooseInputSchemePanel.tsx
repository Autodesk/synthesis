import Panel, { PanelPropsImpl } from "@/components/Panel"
import InputSchemeManager, { InputSchemeUseType } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { useEffect, useMemo } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "./assembly-config/ConfigurationType"
import { setSelectedScheme } from "./assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"

const ChooseInputSchemePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel, openPanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType == MiraType.ROBOT ? assembly : undefined
    }, [])

    useEffect(() => {
        closePanel("import-mirabuf")
        closePanel("configure")

        if (targetAssembly) {
            return
        }

        /** If the panel is closed before a scheme is selected, defaults to the top of the list */
        return () => {
            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex == undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            // Find first available scheme
            const scheme = InputSchemeManager.availableInputSchemesByBrain(brainIndex).find(
                scheme => scheme.status == InputSchemeUseType.AVAILABLE
            )?.scheme

            if (scheme) {
                InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
            }
            setSelectedConfigurationType(ConfigurationType.INPUTS)
            setSelectedScheme(scheme)
        }
    }, [closePanel, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

    return (
        <Panel
            name="Choose Input Scheme"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={false}
            icon={SynthesisIcons.GAMEPAD}
            cancelName="Close"
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                {brainIndex != undefined ? (
                    <InputSchemeSelection
                        brainIndex={brainIndex}
                        onSelect={() => closePanel(panelId)}
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

export default ChooseInputSchemePanel
