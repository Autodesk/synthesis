import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useMemo } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import type { PanelImplProps } from "@/ui/components/Panel"
import { useStateContext } from "@/ui/StateProvider"
import { CloseType, useUIContext } from "../../UIProvider"
import ConfigurePanel from "./assembly-config/ConfigurePanel"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"
import AssignNewSchemeModal from "@/ui/modals/configuring/inputs/AssignNewSchemeModal"

const ChooseInputSchemePanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { openModal, openPanel, closePanel } = useUIContext()
    const { setSelectedScheme, setConfigurationType } = useStateContext()

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType === MiraType.ROBOT ? assembly : undefined
    }, [])

    useEffect(() => {
        if (targetAssembly) return

        return () => {
            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex === undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]

            setConfigurationType("INPUTS")
            if (scheme) setSelectedScheme(scheme)
        }
    }, [setSelectedScheme, setConfigurationType, targetAssembly])

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
                        openPanel(<ConfigurePanel />)
                        closePanel(panel!.id, CloseType.Overwrite)
                    }}
                    onCreateNew={() => {
                        openModal(<AssignNewSchemeModal />)
                        closePanel(panel!.id, CloseType.Overwrite)
                    }}
                />
            )}
        </Stack>
    )
}

export default ChooseInputSchemePanel
