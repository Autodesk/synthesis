import { Tooltip } from "@mui/material"
import type React from "react"
import { useCallback } from "react"
import { reportUIInteraction } from "@/systems/analytics/AnalyticsSystem"
import { IconButton } from "@/ui/components/StyledComponents"
import { TOP_BAR_ICON_BUTTON_ACTIVE_SX, TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"

type TopBarButtonProps = {
    label: string
    icon: React.ReactNode
    disabledTooltip?: string
    active?: boolean
    onClick: () => void
    /** Attaches the button to a guided-tour anchor so a tour card can point at it. */
    anchorRef?: React.Ref<HTMLSpanElement>
}

/** component for all buttons located on topbar */
export const TopBarButton: React.FC<TopBarButtonProps> = ({
    label,
    icon,
    disabledTooltip,
    active,
    onClick,
    anchorRef,
}) => {
    const { blockState } = useUIContext()
    const disabled = disabledTooltip !== undefined || blockState.blocked

    const onButtonClicked = useCallback(() => {
        reportUIInteraction("Top Bar Button", label)
        onClick()
    }, [label, onClick])

    return (
        <Tooltip title={disabledTooltip ?? label}>
            <span ref={anchorRef}>
                <IconButton
                    size="medium"
                    disableRipple
                    disabled={disabled}
                    aria-pressed={active}
                    sx={{
                        ...TOP_BAR_ICON_BUTTON_SX,
                        ...(disabled && { opacity: 0.4 }),
                        ...(active && TOP_BAR_ICON_BUTTON_ACTIVE_SX),
                    }}
                    onClick={onButtonClicked}
                >
                    {icon}
                </IconButton>
            </span>
        </Tooltip>
    )
}
