/* eslint-disable @typescript-eslint/no-explicit-any */
import React from "react"
import Label, { LabelSize } from "./Label"
import {
    Unstable_NumberInput as BaseNumberInput,
    NumberInputOwnerState,
    NumberInputProps,
} from "@mui/base/Unstable_NumberInput"

const resolveSlotProps = (fn: any, args: any) => (typeof fn === "function" ? fn(args) : fn)

type InputProps = {
    placeholder: string
    defaultValue?: number
    label?: string
    onInput?: (value: number | null) => void
    className?: string
}

const NumberInput: React.FC<InputProps> = ({ placeholder, defaultValue, label, onInput, className }) => {
    return (
        <>
            {label && <Label size={LabelSize.Small}>{label}</Label>}
            <BaseNumberInput
                defaultValue={defaultValue}
                placeholder={placeholder}
                onChange={(_, val) => onInput && onInput(val)}
                slotProps={{
                    root: ownerState => {
                        return {
                            className: `grid grid-cols-[1fr_8px] grid-rows-2 overflow-hidden rounded-lg text-main-text dark:text-main-text bg-background-secondary border border-solid hover:border-interactive-element-solid dark:hover:border-interactive-element-solid focus-visible:outline-0 p-1 ${ownerState.focused ? "border-interactive-element-solid dark:border-interactive-element-solid" : "border-interactive-element-right dark:border-interactive-element-right"}`,
                        }
                    },
                    input: {
                        className:
                            "col-start-1 col-end-2 row-start-1 row-end-3 text-sm font-sans leading-normal text-main-text dark:text-main-text bg-inherit border-0 rounded-[inherit] px-3 py-2 outline-0 focus-visible:outline-0 focus-visible:outline-none",
                    },
                    incrementButton: {
                        children: "▴",
                        className:
                            "font-[system-ui] flex flex-row flex-nowrap justify-center items-center p-0 w-[19px] h-[19px] col-start-3 col-end-3 row-start-1 row-end-2 border border-solid outline-none text-sm box-border leading-[1.2] rounded-t-md rounded-b-none bg-interactive-secondary border-interactive-element-right dark:border-interactive-element-right bg-background-secondary text-main-text transition-all duration-[120ms] hover:cursor-pointer hover:border-interactive-element-right hover:bg-interactive-hover",
                    },
                    decrementButton: {
                        children: "▾",
                        className:
                            "font-[system-ui] flex flex-row flex-nowrap justify-center items-center p-0 w-[19px] h-[19px] col-start-3 col-end-3 row-start-2 row-end-3 border border-solid outline-none text-sm box-border leading-[1.2] rounded-b-md rounded-t-none border-t-0 border-interactive-element-right dark:border-interactive-element-right bg-background-secondary text-main-text transition-all duration-[120ms] hover:cursor-pointer hover:border-interactive-element-right hover:bg-interactive-hover",
                    },
                }}
            />
        </>
    )
}

// <input
//     placeholder={placeholder}
//     defaultValue={defaultValue}
//     value={value}
//     onKeyPress={e => {
//         if (
//             e.key != null &&
//             numeric &&
//             !"1234567890,.".includes(e.key)
//         ) {
//             e.preventDefault()
//         }

//         if (validate && !validate(e.key)) e.preventDefault()
//     }}
//     onChange={e => {
//         if (onInput) onInput(e.target.value)
//     }}
//     className={`bg-interactive-background px-2 py-1 bg-[length:200%_100%] w-min rounded-md font-semibold cursor-pointer placeholder:italic ${className || ""
//         }`}
// />
export default NumberInput
