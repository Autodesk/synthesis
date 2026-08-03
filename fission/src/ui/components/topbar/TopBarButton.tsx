import { Tooltip } from "@mui/material"
import type React from "react"
import { IconButton } from "@/ui/components/StyledComponents"
import { TOP_BAR_ICON_BUTTON_ACTIVE_SX, TOP_BAR_ICON_BUTTON_SX } from "@/ui/components/topbar/TopBarConfig"

type TopBarButtonProps = {
    label: string
    icon: React.ReactNode
    disabledTooltip?: string
    active?: boolean
    onClick: () => void
}

/** component for all buttons located on topbar */
export const TopBarButton: React.FC<TopBarButtonProps> = ({ label, icon, disabledTooltip, active, onClick }) => {
    const disabled = disabledTooltip !== undefined

    return (
        <Tooltip title={disabledTooltip ?? label}>
            <span>
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
                    onClick={onClick}
                >
                    {icon}
                </IconButton>
            </span>
        </Tooltip>
    )
}
