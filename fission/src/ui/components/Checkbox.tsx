import { Stack, Switch } from "@mui/material"
import type React from "react"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import Label from "./Label"
import { LabelWithTooltip } from "./StyledComponents"

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
    /**
     * Whether to disable the checkbox
     */
    disabled?: boolean
}

const Checkbox: React.FC<CheckboxProps> = ({ label, className, checked, hideLabel, onClick, tooltip, disabled }) => {
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
                {...SoundPlayer.getInstance().checkboxSoundEffects()}
                checked={checked}
                disabled={disabled}
                role="checkbox"
            />
        </Stack>
    )
}

export default Checkbox
