import { Tooltip } from "@mui/material"
import type React from "react"
import { IconButton } from "@/ui/components/StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./TopBarConfig"

type TopBarButtonProps = {
    label: string
    icon: React.ReactNode
    disabledTooltip?: string
    onClick: () => void
}

/** component for all buttons located on topbar */
export const TopBarButton: React.FC<TopBarButtonProps> = ({ label, icon, disabledTooltip, onClick }) => {
    const disabled = disabledTooltip !== undefined

    return (
        <Tooltip title={disabledTooltip ?? label}>
            <span>
                <IconButton
                    size="medium"
                    disableRipple
                    disabled={disabled}
                    sx={{ ...TOP_BAR_ICON_BUTTON_SX, ...(disabled && { opacity: 0.4 }) }}
                    onClick={onClick}
                >
                    {icon}
                </IconButton>
            </span>
        </Tooltip>
    )
}
