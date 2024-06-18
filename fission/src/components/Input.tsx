import React from "react"
import Label, { LabelSize } from "./Label"
import { Input as BaseInput } from "@mui/base/Input"

type InputProps = {
    placeholder: string
    defaultValue?: string
    label?: string
    onInput?: (value: string) => void
    className?: string
}

const Input: React.FC<InputProps> = ({
    placeholder,
    defaultValue,
    label,
    onInput,
    className,
}) => {
    return (
        <>
            {label && <Label size={LabelSize.Small}>{label}</Label>}
            <BaseInput
                defaultValue={defaultValue}
                placeholder={placeholder}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    onInput && onInput(e.target.value)
                }
                className={className}
                slotProps={{
                    input: {
                        className: `w-full text-sm font-normal font-sans leading-5 px-3 py-2 rounded-lg shadow-md text-main-text bg-background-secondary focus-visible:outline-0 border border-solid border-interactive-element-right dark:border-interactive-element-right hover:border-interactive-element-solid dark:hover:border-interactive-element-solid focus:border-interactive-solid dark:focus:border-interactive-element-solid`,
                        // className: `w-full text-sm font-normal font-sans leading-5 px-3 py-2 rounded-lg shadow-md focus:shadow-outline-interactive-element-left dark:focus:shadow-outline-interactive-element-left dark:outline-interactive-element-right focus:shadow-lg border border-solid border-interactive-element-right hover:border-interactive-element-left dark:hover:border-interactive-element-left focus:border-interactive-element-left dark:focus:border-interactive-element-left dark:border-slate-600 bg-white dark:bg-slate-900 text-slate-900 dark:text-slate-300 focus-visible:outline-0`
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
export default Input
