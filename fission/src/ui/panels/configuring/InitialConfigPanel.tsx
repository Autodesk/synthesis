import Panel, { PanelPropsImpl } from "@/components/Panel"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import {
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { useEffect } from "react"
import { ConfigurationType, setSelectedConfigurationType } from "./assembly-config/ConfigurePanel"
import { setSelectedScheme } from "./assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"

/** We store the selected brain index globally to specify which robot the input scheme should be bound to. */
let selectedBrainIndexGlobal: number | undefined = undefined

// eslint-disable-next-line react-refresh/only-export-components
export function setSelectedBrainIndexGlobal(index: number | undefined) {
    selectedBrainIndexGlobal = index
}

function getBrainIndex() {
    return selectedBrainIndexGlobal != undefined ? selectedBrainIndexGlobal : SynthesisBrain.brainIndexMap.size - 1
}

const InitialConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel, openPanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    useEffect(() => {
        closePanel("import-mirabuf")

        /** If the panel is closed before a scheme is selected, defaults to the top of the list */
        return () => {
            const brainIndex = getBrainIndex()

            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]

            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)

            setSelectedConfigurationType(ConfigurationType.INPUTS)
            setSelectedScheme(scheme)
            openPanel("configure")
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    useEffect(() => {
        return () => {
            selectedBrainIndexGlobal = undefined
        }
    }, [])

    return (
        <Panel
            name="Choose Input Scheme"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={false}
            icon={SynthesisIcons.Gamepad}
            cancelEnabled={selectedBrainIndexGlobal != undefined}
            cancelName="Close"
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                <InputSchemeSelection
                    getBrainIndex={getBrainIndex}
                    onSelect={() => closePanel(panelId)}
                    onEdit={() => openPanel("configure")}
                    onCreateNew={() => openModal("assign-new-scheme")}
                />
            </div>
        </Panel>
    )
}

export default InitialConfigPanel
