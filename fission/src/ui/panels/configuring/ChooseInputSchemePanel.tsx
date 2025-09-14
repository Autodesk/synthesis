import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useMemo } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { InputSchemeUseType } from "@/systems/input/InputTypes"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { PanelImplProps } from "@/ui/components/Panel"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import NewInputSchemeModal from "@/ui/modals/configuring/inputs/NewInputSchemeModal"
import { CloseType, useUIContext } from "../../helpers/UIProviderHelpers"
import ConfigurePanel from "./assembly-config/ConfigurePanel"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"

const ChooseInputSchemePanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { openModal, openPanel, closePanel, configureScreen } = useUIContext()
    const { setSelectedScheme } = useStateContext()

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType === MiraType.ROBOT ? assembly : undefined
    }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "Choose Input Scheme", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    useEffect(() => {
        if (targetAssembly) return

        return () => {
            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex === undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            // Find first available scheme
            const scheme = InputSchemeManager.availableInputSchemesByBrain(brainIndex).find(
                scheme => scheme.status == InputSchemeUseType.AVAILABLE
            )?.scheme

            if (scheme) {
                InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
            }
            if (scheme) setSelectedScheme(scheme)
        }
    }, [closePanel, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

    return (
        <Stack gap={2}>
            {brainIndex !== undefined && (
                <InputSchemeSelection
                    brainIndex={brainIndex}
                    onSelect={() => closePanel(panel!.id, CloseType.Accept)}
                    onEdit={() => {
                        openPanel(ConfigurePanel, { configurationType: "INPUTS" })
                        closePanel(panel!.id, CloseType.Overwrite)
                    }}
                    onCreateNew={() => {
                        openModal(NewInputSchemeModal, undefined)
                        closePanel(panel!.id, CloseType.Overwrite)
                    }}
                />
            )}
        </Stack>
    )
}

export default ChooseInputSchemePanel
