import { Switch } from "@mui/base/Switch"
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
    tooltipText?: string
}

const StatefulCheckbox: React.FC<CheckboxProps> = ({ label, className, checked, hideLabel, onClick, tooltipText }) => {
    return (
        <Stack
            direction="row"
            justifyContent="space-between"
            alignItems="center"
            textAlign="center"
        >
            {hideLabel ? null : tooltipText ? (
                LabelWithTooltip(label, tooltipText)
            ) : (
                <Label size="sm" className={`mr-12 ${className} whitespace-nowrap`}>
                    {label}
                </Label>
            )}
            <Switch
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => onClick && onClick(e.target.checked)}
                {...SoundPlayer.checkboxSoundEffects()}
                slotProps={{
                    root: {
                        className: `group relative inline-block w-[24px] h-[24px] m-2.5 cursor-pointer transform transition-transform hover:scale-[1.03] active:scale-[1.06]`,
                    },
                    input: {
                        className: `cursor-inherit absolute w-full h-full top-0 left-0 opacity-0 z-10 border-none`,
                    },
                    track: ownerState => {
                        return {
                            className: `absolute block w-full h-full transition rounded-full border border-solid outline-none border-interactive-element-right dark:border-interactive-element-right group-[.base--focusVisible]:shadow-outline-switch ${ownerState.checked ? "bg-gradient-to-br from-interactive-element-left to-interactive-element-right" : "bg-background-secondary"} transform transition-transform group-hover:scale-[1.03] group-active:scale-[1.06]`,
                        }
                    },
                    thumb: {
                        className: `display-none`,
                    },
                }}
                checked={checked}
                role="checkbox"
            />
        </Stack>
    )
}

export default StatefulCheckbox
