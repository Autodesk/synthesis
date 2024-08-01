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

const Input: React.FC<InputProps> = ({ placeholder, defaultValue, label, onInput, className }) => {
    return (
        <>
            {label && <Label size={LabelSize.Small}>{label}</Label>}
            <BaseInput
                defaultValue={defaultValue}
                placeholder={placeholder}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => onInput && onInput(e.target.value)}
                className={className}
                slotProps={{
                    input: {
                        className: `w-full text-sm font-normal font-sans leading-5 px-3 py-2 rounded-lg shadow-md text-main-text bg-background-secondary focus-visible:outline-0 border border-solid border-interactive-element-right dark:border-interactive-element-right hover:border-interactive-element-solid dark:hover:border-interactive-element-solid focus:border-interactive-solid dark:focus:border-interactive-element-solid`,
                    },
                }}
            />
        </>
    )
}

export default Input
