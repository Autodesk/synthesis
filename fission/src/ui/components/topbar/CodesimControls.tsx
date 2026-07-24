import { Box, Stack } from "@mui/material"
import type React from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ConfigMode } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import CodeConnectionIndicator from "./CodeConnectionIndicator"
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

const CodesimControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { isField, isWpilibBrain, openConfig } = useConfigureAssembly(selectedAssembly)

    // codesim is robot only
    const disabledTooltip = ({ requiresWpilibBrain }: CodesimButton) => {
        if (!selectedAssembly) return "Spawn an assembly first"
        if (isField) return "Select a robot to configure"
        if (requiresWpilibBrain && !isWpilibBrain) return "Set this robot's brain to WPILib first"
        return undefined
    }

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            {CODESIM_BUTTONS.map(button => (
                <TopBarButton
                    key={button.label}
                    label={button.label}
                    icon={<Box sx={TOP_BAR_GLYPH_SX}>{button.icon}</Box>}
                    disabledTooltip={disabledTooltip(button)}
                    onClick={() => openConfig(button.mode)}
                />
            ))}

            <CodeConnectionIndicator />

            <Box sx={TOP_BAR_DIVIDER_SX} />

            <ConfigureSplitDropdown selectedAssembly={selectedAssembly} />
        </Stack>
    )
}

export default CodesimControls
