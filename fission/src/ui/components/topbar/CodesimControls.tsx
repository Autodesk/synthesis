import { Box, Stack } from "@mui/material"
import type React from "react"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import { AssemblySelect } from "./AssemblySelect"
import ConfigureSplitDropdown from "./ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX, TOP_BAR_GLYPH_SX } from "./TopBarConfig"
import { TopBarButton } from "./TopBarButton"
import { useConfigureAssembly } from "./UseConfigureAssembly"

type CodesimButton = {
    label: string
    mode: ConfigMode
    icon: React.ReactNode
    requiresWpilibBrain?: boolean
}

const CODESIM_BUTTONS: CodesimButton[] = [
    { label: "Brain", mode: ConfigMode.BRAIN, icon: <SynthesisIcons.BRAIN /> },
    { label: "Simulation", mode: ConfigMode.SIM, icon: <SynthesisIcons.MICROCHIP />, requiresWpilibBrain: true },
]

const CodesimControls: React.FC = () => {
    const { assemblies, selectedConfigAssembly, isField, isWpilibBrain, openConfig, selectAssemblyById } =
        useConfigureAssembly()

    // codesim is robot only
    const disabledTooltip = ({ requiresWpilibBrain }: CodesimButton) => {
        if (!selectedConfigAssembly) return "Spawn an assembly first"
        if (isField) return "Select a robot to configure"
        if (requiresWpilibBrain && !isWpilibBrain) return "Set this robot's brain to WPILib first"
        return undefined
    }

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <AssemblySelect
                assemblies={assemblies}
                selectedConfigAssembly={selectedConfigAssembly}
                onSelect={selectAssemblyById}
                sx={{ borderRadius: 1, height: 34, minWidth: 195, fontSize: 12 }}
            />

            {CODESIM_BUTTONS.map(button => (
                <TopBarButton
                    key={button.label}
                    label={button.label}
                    icon={<Box sx={TOP_BAR_GLYPH_SX}>{button.icon}</Box>}
                    disabledTooltip={disabledTooltip(button)}
                    onClick={() => openConfig(button.mode)}
                />
            ))}

            {/* Divider line */}
            <Box sx={TOP_BAR_DIVIDER_SX} />

            <ConfigureSplitDropdown />
        </Stack>
    )
}

export default CodesimControls
