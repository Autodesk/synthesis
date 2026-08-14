import { Tooltip } from "@mui/material"
import type React from "react"
import { useCallback } from "react"
import { reportUIInteraction } from "@/systems/analytics/AnalyticsSystem"
import { IconButton } from "@/ui/components/StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"

type TopBarButtonProps = {
    label: string
    icon: React.ReactNode
    disabledTooltip?: string
    onClick: () => void
}

/** component for all buttons located on topbar */
export const TopBarButton: React.FC<TopBarButtonProps> = ({ label, icon, disabledTooltip, onClick }) => {
    const { blockState } = useUIContext()
    const disabled = disabledTooltip !== undefined || blockState.blocked

    const onButtonClicked = useCallback(() => {
        reportUIInteraction("Top Bar Button", label)
        onClick()
    }, [label, onClick])

    return (
        <Tooltip title={disabledTooltip ?? label}>
            <span>
                <IconButton
                    size="medium"
                    disableRipple
                    disabled={disabled}
                    sx={{ ...TOP_BAR_ICON_BUTTON_SX, ...(disabled && { opacity: 0.4 }) }}
                    onClick={onButtonClicked}
                >
                    {icon}
                </IconButton>
            </span>
        </Tooltip>
    )
}
