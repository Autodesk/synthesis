import React, { useState } from "react"
import Stack, { StackDirection } from "./Stack"
import Label, { LabelSize } from "./Label"
import { Switch } from '@mui/base/Switch'

type CheckboxProps = {
    label: string
    className?: string
    defaultState: boolean
    stateOverride?: boolean
    onClick?: (checked: boolean) => void
}

const Checkbox: React.FC<CheckboxProps> = ({
    label,
    className,
    defaultState,
    stateOverride,
    onClick,
}) => {
    return (
        <Stack direction={StackDirection.Horizontal}>
            <Label
                size={LabelSize.Medium}
                className={`mr-8 ${className} whitespace-nowrap`}
            >
                {label}
            </Label>
            <Switch
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => onClick && onClick(e.target.checked)}
                slotProps={{
                    root: {
                        className: `group relative inline-block w-[38px] h-[24px] m-2.5 cursor-pointer`
                    },
                    input: {
                        className: `cursor-inherit absolute w-full h-full top-0 left-0 opacity-0 z-10 border-none`
                    },
                    track: (ownerState) => {
                        return {
                            className: `absolute block w-full h-full transition rounded-full border border-solid outline-none border-slate-300 dark:border-gray-700 group-[.base--focusVisible]:shadow-outline-switch ${ownerState.checked ? 'bg-purple-500' : 'bg-slate-100 dark:bg-slate-900 hover:bg-slate-200 dark:hover:bg-slate-800'}`
                        }
                    },
                    thumb: (ownerState) => {
                        return {
                            className: `block w-4 h-4 top-1 rounded-2xl border border-solid outline-none border-slate-300 dark:border-gray-700 transition shadow-[0_1px_2px_rgb(0_0_0_/_0.1)] dark:shadow-[0_1px_2px_rgb(0_0_0_/_0.25)] ${ownerState.checked ? 'left-[18px] bg-white shadow-[0_0_0_rgb(0_0_0_/_0.3)]' : 'left-[4px] bg-white'} relative transition-all`
                        }
                    }
                }}
            />
        </Stack>
    )
}
// <input
//     type="checkbox"
//     defaultChecked={stateOverride != null ? undefined : state}
//     onChange={e => {
//         const checked = (e.target as HTMLInputElement).checked
//         setState(checked)
//         if (onClick) onClick(checked)
//     }}
//     className="bg-interactive-background translate-y-1/4 duration-200 cursor-pointer appearance-none w-5 h-5 rounded-full checked:bg-gradient-to-br checked:from-interactive-element-left checked:to-interactive-element-right"
//     checked={stateOverride != null ? stateOverride : undefined}
// />

export default Checkbox
