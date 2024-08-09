import React, { useState } from "react"
import Stack, { StackDirection } from "./Stack"
import Label, { LabelSize } from "./Label"
import { Switch } from "@mui/base/Switch"

type CheckboxProps = {
    label: string
    className?: string
    defaultState: boolean
    stateOverride?: boolean
    onClick?: (checked: boolean) => void
}

const Checkbox: React.FC<CheckboxProps> = ({ label, className, defaultState, stateOverride, onClick }) => {
    const [state] = useState(defaultState)
    return (
        <Stack direction={StackDirection.Horizontal} className="items-center">
            <Label size={LabelSize.Small} className={`mr-12 ${className} whitespace-nowrap`}>
                {label}
            </Label>
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
            />
        </Stack>
    )
}

export default Checkbox
