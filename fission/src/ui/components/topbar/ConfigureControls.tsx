import { Box } from "@mui/material"
import type React from "react"
import { useCallback, useMemo } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ConfigMode } from "@/panels/configuring/assembly-config/ConfigTypes"
import { CollapsibleGroup, type CollapsibleItem } from "@/ui/components/topbar/CollapsibleGroup"
import { ConfigureIcon } from "@/ui/components/topbar/ConfigureIcon"
import ConfigureSplitDropdown from "@/ui/components/topbar/ConfigureSplitDropdown"
import { TOP_BAR_DIVIDER_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { type ConfigureButton, useConfigureAssembly } from "@/ui/components/topbar/UseConfigureAssembly"
import { useTourAnchor } from "@/ui/tour/TourProviderHelpers"

type ConfigureButtonItemProps = {
    button: ConfigureButton
    disabledMessage?: string
    openConfig: (mode: ConfigMode) => void
    anchorRef?: React.Ref<HTMLSpanElement>
}

const ConfigureButtonItem: React.FC<ConfigureButtonItemProps> = ({
    button,
    disabledMessage,
    openConfig,
    anchorRef,
}) => {
    const onClick = useCallback(() => openConfig(button.mode), [button.mode, openConfig])

    return (
        <TopBarButton
            label={button.label}
            icon={<ConfigureIcon icon={button.icon} size={30} />}
            disabledTooltip={disabledMessage ? `${button.label}: unavailable - ${disabledMessage}` : undefined}
            onClick={onClick}
            anchorRef={anchorRef}
        />
    )
}

const ConfigureControls: React.FC<{ selectedAssembly?: MirabufSceneObject }> = ({ selectedAssembly }) => {
    const { configureButtons, openConfig, disabledMessage } = useConfigureAssembly(selectedAssembly)

    const intakeButtonRef = useTourAnchor("configure-intake-button")

    const items: CollapsibleItem[] = useMemo(
        () =>
            configureButtons.map(button => ({
                key: button.label,
                node: (
                    <ConfigureButtonItem
                        button={button}
                        disabledMessage={disabledMessage}
                        openConfig={openConfig}
                        anchorRef={button.mode === ConfigMode.INTAKE ? intakeButtonRef : undefined}
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
