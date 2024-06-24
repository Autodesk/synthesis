import React, { useState } from "react"
import Stack, { StackDirection } from "./Stack"
import Label, { LabelSize } from "./Label"

type RadioProps = {
    label: string
    className?: string
    defaultState: boolean
    onClick?: () => void
}

const Radio: React.FC<RadioProps> = ({ label, className, defaultState, onClick }) => {
    const [, setState] = useState(defaultState)
    return (
        <Stack direction={StackDirection.Horizontal}>
            <Label size={LabelSize.Medium} className={`mr-8 ${className} whitespace-nowrap`}>
                {label}
            </Label>
            <input
                type="radio"
                onClick={e => {
                    setState((e.target as HTMLInputElement).checked)
                    if (onClick) onClick()
                }}
                className="bg-interactive-background translate-y-1/4 duration-200 cursor-pointer appearance-none w-5 h-5 rounded-full checked:bg-gradient-to-br checked:from-interactive-element-left checked:to-interactive-element-right"
            />
        </Stack>
    )
}

export default Radio
