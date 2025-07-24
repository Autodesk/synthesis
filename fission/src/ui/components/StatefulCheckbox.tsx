import { Switch, Tooltip } from "@mui/material"
import { Stack, Typography } from "@mui/material"
import React from "react"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { LabelWithTooltip } from "./StyledComponents"
import Label from "./Label"

type CheckboxProps = {
    /**
     * The label text that will be on the right of the checkbox.
     */
    label: string
    /**
     * Custom styling options.
     */
    className?: string
    /**
     * A variable that controls the current state of the checkbox
     */
    checked: boolean

    /**
     * If true, the checkbox will not be labeled.
     */
    hideLabel?: boolean
    /**
     * Callback function to handle state changes of the checkbox.
     * @param checked new state of the checkbox
     */
    onClick?: (checked: boolean) => void
    /**
     * Text to show as a tooltip next to the label.
     */
    tooltip?: string
}

const StatefulCheckbox: React.FC<CheckboxProps> = ({ label, className, checked, hideLabel, onClick, tooltip }) => {
    return (
        <Stack direction="row" justifyContent="space-between" alignItems="center" textAlign="center">
            {hideLabel ? null : tooltip ? (
                LabelWithTooltip(label, tooltip)
            ) : (
                <Label size="sm" className={`mr-12 ${className} whitespace-nowrap`}>
                    {label}
                </Label>
            )}
            <Switch
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => onClick && onClick(e.target.checked)}
                {...SoundPlayer.checkboxSoundEffects()}
                checked={checked}
                role="checkbox"
            />
        </Stack>
    )
}

export default StatefulCheckbox
