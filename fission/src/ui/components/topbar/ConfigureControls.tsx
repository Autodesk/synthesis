import { Box } from "@mui/material"
import type React from "react"
import { useMemo } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/panels/configuring/assembly-config/ConfigTypes"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { ConfigureIcon } from "@/ui/components/topbar/ConfigureIcon"
import ConfigureSplitDropdown from "@/ui/components/topbar/ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"
import { useTourAnchor } from "@/ui/tour/TourProviderHelpers"

const ConfigureControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { configureButtons, openConfig, disabledMessage } = useConfigureAssembly(selectedAssembly)

    const intakeButtonRef = useTourAnchor("configure-intake-button")

    const items: CollapsibleItem[] = useMemo(
        () =>
            configureButtons.map(({ icon, label, mode }) => ({
                key: label,
                node: (
                    <TopBarButton
                        label={label}
                        icon={<ConfigureIcon icon={icon} size={30} />}
                        disabledTooltip={disabledMessage}
                        onClick={() => openConfig(mode)}
                        anchorRef={mode === ConfigMode.INTAKE ? intakeButtonRef : undefined}
                    />
                ),
            })),
        [configureButtons, disabledMessage, openConfig, intakeButtonRef]
    )

    // TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow
    return (
        <CollapsibleGroup
            items={items}
            always={
                <>
                    <Box sx={TOP_BAR_DIVIDER_SX} />
                    <ConfigureSplitDropdown disabledMessage={disabledMessage} selectedAssembly={selectedAssembly} />
                </>
            }
        />
    )
}

export default ConfigureControls
