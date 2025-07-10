import { Divider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useContext, useEffect, useMemo, useReducer } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { type InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { AddButtonInteractiveColor, DeleteButton, EditButton, SelectButton } from "@/ui/components/StyledComponents"
import { StateContext } from "@/ui/StateProvider"
import { CloseType, type Panel, UIContext } from "../../UIProvider"
import { ConfigurationType, setSelectedConfigurationType } from "./assembly-config/ConfigurationType"
import ConfigurePanel from "./assembly-config/ConfigurePanel"
import InputSchemeSelection from "./initial-config/InputSchemeSelection"

interface ChooseSchemePanelProps {
    panel?: Panel
}

const ChooseInputSchemePanel: React.FC<ChooseSchemePanelProps> = ({ panel }) => {
    const { openModal, openPanel, closePanel } = useContext(UIContext)
    const { setSelectedScheme } = useContext(StateContext)

    const targetAssembly = useMemo(() => {
        const assembly = getSpotlightAssembly()
        return assembly?.miraType === MiraType.ROBOT ? assembly : undefined
    }, [])

    // biome-ignore lint/correctness/useExhaustiveDependencies: <explanation>
    useEffect(() => {
        // TODO: figure out closing other panels (specifically import mirabuf and configure)

        if (targetAssembly) return

        return () => {
            const brainIndex = SynthesisBrain.GetBrainIndex(targetAssembly)

            if (brainIndex === undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]

            setSelectedConfigurationType(ConfigurationType.INPUTS)
            // TODO:
            setSelectedScheme(scheme)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.GetBrainIndex(targetAssembly)
    }, [targetAssembly])

    return (
        <Stack gap={2}>
            {brainIndex !== undefined && (
                <InputSchemeSelection
                    brainIndex={brainIndex}
                    onSelect={() => closePanel(panel!.id, CloseType.Accept)}
                    onEdit={() => openPanel(<ConfigurePanel />)}
                    // TODO:
                    // onCreateNew={() => openModal(<AssignNewSchemeModal />)}
                />
            )}
        </Stack>
    )
}

export default ChooseInputSchemePanel
