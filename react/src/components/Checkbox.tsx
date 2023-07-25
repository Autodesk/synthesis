import React, { useState } from "react"
import Stack, { StackDirection } from "./Stack"
import Label, { LabelSize } from "./Label"

type CheckboxProps = {
    label: string
    defaultState: boolean
    onClick?: () => void
}

const Checkbox: React.FC<CheckboxProps> = ({
    label,
    defaultState,
    onClick,
}) => {
    const [state, setState] = useState(defaultState)
    return (
        <Stack direction={StackDirection.Horizontal}>
            <Label size={LabelSize.Medium}>{label}</Label>
            <input
                type="checkbox"
                checked={state}
                onClick={e => {
                    setState((e.target as HTMLInputElement).checked)
                    if (onClick) onClick()
                }}
                className="bg-gray-500 duration-200 cursor-pointer appearance-none w-5 h-5 rounded-full checked:bg-gradient-to-br checked:from-red-700 checked:to-orange-400"
            />
        </Stack>
    )
}

export default Checkbox
