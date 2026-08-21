import { Box, Stack } from "@mui/material"
import type React from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigureSplitDropdown from "@/ui/components/topbar/ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX, TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"

type CodesimButton = {
    label: string
    mode: ConfigMode
    icon: React.ReactNode
    requiresCodesimBrain?: boolean
}

const CODESIM_BUTTONS: CodesimButton[] = [
    { label: "Brain", mode: ConfigMode.BRAIN, icon: <SynthesisIcons.BRAIN /> },
    { label: "Simulation", mode: ConfigMode.SIM, icon: <SynthesisIcons.MICROCHIP />, requiresCodesimBrain: true },
]

const CodesimControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { isField, isCodesimBrain, openConfig, disabledMessage } = useConfigureAssembly(selectedAssembly)

    // codesim is robot only
    const disabledTooltip = ({ requiresCodesimBrain }: CodesimButton) => {
        if (!selectedAssembly) return "Spawn an assembly first"
        if (isField) return "Select a robot to configure"
        if (requiresCodesimBrain && !isCodesimBrain) return "Set this robot's brain to WPILib or FTC first"
        return undefined
    }

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            {CODESIM_BUTTONS.map(button => (
                <TopBarButton
                    key={button.label}
                    label={button.label}
                    icon={<Box sx={TOP_BAR_GLYPH_SX}>{button.icon}</Box>}
                    disabledTooltip={disabledMessage ?? disabledTooltip(button)}
                    onClick={() => openConfig(button.mode)}
                />
            ))}

            <Box sx={TOP_BAR_DIVIDER_SX} />

            <ConfigureSplitDropdown disabledMessage={disabledMessage} selectedAssembly={selectedAssembly} />
        </Stack>
    )
}

export default CodesimControls
