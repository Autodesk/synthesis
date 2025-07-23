import React, { useState } from "react"
import StatefulCheckbox from "@/components/StatefulCheckbox.tsx"

type CheckboxProps = {
    label: string
    className?: string
    defaultState: boolean
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
 * @returns {React.ReactElement} The rendered Dropdown component.
 */
const Checkbox: React.FC<CheckboxProps> = ({
    label,
    className,
    defaultState,
    hideLabel,
    onClick,
    tooltipText,
}: CheckboxProps): React.ReactElement => {
    const [state, setState] = useState(defaultState)
    return (
        <StatefulCheckbox
            label={label}
            checked={state}
            className={className}
            hideLabel={hideLabel}
            tooltipText={tooltipText}
            onClick={v => {
                setState(v)
                onClick?.(v)
            }}
        />
    )
}

export default Checkbox
