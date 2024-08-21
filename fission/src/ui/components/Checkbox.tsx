import React, { useState } from "react"
import Label, { LabelSize } from "./Label"
import { Switch } from "@mui/base/Switch"
import { Box } from "@mui/material"
import { LabelWithTooltip } from "./StyledComponents"

type CheckboxProps = {
    label: string
    className?: string
    defaultState: boolean
    stateOverride?: boolean
    hideLabel?: boolean
    onClick?: (checked: boolean) => void
    tooltipText?: string
}

/**
 * A checkbox component with a label and optional tooltip.
 *
 * @param {CheckboxProps} props - The properties object.
 * @param {string} props.label - The label text that will be on the right of the checkbox.
 * @param {string} props.className - Custom styling options.
 * @param {boolean} props.defaultState - Should the box be checked when it's first created?.
 * @param {boolean} props.stateOverride - Controls the state of the checkbox, overriding user inputs.
 * @param {boolean} props.hideLabel - If true, the checkbox will not be labeled.
 * @param {function} props.onClick - Callback function to handle state changes of the checkbox.
 * @param {string} props.tooltipText - Text shows as a tooltip next to the label.
 *
 * @returns {JSX.Element} The rendered Dropdown component.
 */
const Checkbox: React.FC<CheckboxProps> = ({
    label,
    className,
    defaultState,
    stateOverride,
    hideLabel,
    onClick,
    tooltipText,
}) => {
    const [state] = useState(defaultState)
    return (
        <Box
            display="flex"
            flexDirection={"row"}
            justifyContent={"space-between"}
            alignItems={"center"}
            textAlign={"center"}
        >
            {hideLabel ? null : tooltipText ? (
                LabelWithTooltip(label, tooltipText)
            ) : (
                <Label size={LabelSize.Small} className={`mr-12 ${className} whitespace-nowrap`}>
                    {label}
                </Label>
            )}
            <Switch
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => onClick && onClick(e.target.checked)}
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
                defaultChecked={stateOverride != null ? undefined : state}
                id="checkbox-switch"
            />
        </Box>
    )
}

export default Checkbox
