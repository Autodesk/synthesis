import React from "react"
import Label, { LabelSize } from "./Label"

type InputProps = {
    placeholder: string
    value?: string
    defaultValue?: string
    numeric?: boolean
    validate?: (s: string) => boolean
    label?: string
    onInput?: (value: string) => void
    className?: string
}

const Input: React.FC<InputProps> = ({
    placeholder,
    value,
    defaultValue,
    numeric,
    validate,
    label,
    onInput,
    className,
}) => {
    return (
        <>
            {label && <Label size={LabelSize.Small}>{label}</Label>}
            <input
                placeholder={placeholder}
                defaultValue={defaultValue}
                value={value}
                onKeyPress={e => {
                    if (
                        e.key != null &&
                        numeric &&
                        !"1234567890,.".includes(e.key)
                    ) {
                        e.preventDefault()
                    }

                    if (validate && !validate(e.key)) e.preventDefault()
                }}
                onChange={e => {
                    if (onInput) onInput(e.target.value)
                }}
                className={`bg-gray-700 px-2 py-1 bg-[length:200%_100%] w-min rounded-md font-semibold cursor-pointer placeholder:italic ${
                    className || ""
                }`}
            />
        </>
    )
}

export default Input
