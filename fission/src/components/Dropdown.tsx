import React, { ReactNode, useState } from "react"
import { FaChevronDown, FaChevronUp } from "react-icons/fa"
import Label, { LabelSize } from "./Label"

type DropdownProps = {
    children?: ReactNode
    label?: string
    className?: string
    options: string[]
    onSelect: (opt: string) => void
}

const Dropdown: React.FC<DropdownProps> = ({
    label,
    className,
    options,
    onSelect,
}) => {
    const [expanded, setExpanded] = useState(false)
    const [optionList, setOptionList] = useState(options)

    type DropdownOptionProps = {
        value: string
        children?: ReactNode
        className?: string
    }

    const DropdownOption: React.FC<DropdownOptionProps> = ({
        children,
        value,
        className,
    }) => (
        <span
            onClick={() => {
                const newOptions = options.filter(item => item !== value)
                newOptions.unshift(value)
                setOptionList(newOptions)
                if (onSelect) onSelect(value)
            }}
            className={`block relative duration-100 hover:backdrop-brightness-90 w-full h-full px-2 py-2 ${className}`}
        >
            {children}
        </span>
    )

    return (
        <>
            {label && <Label size={LabelSize.Medium}>{label}</Label>}
            <div
                onClick={() => setExpanded(!expanded)}
                className={`relative flex flex-col gap-2 select-none cursor-pointer bg-gradient-to-r from-interactive-element-left to-interactive-element-right w-full rounded-md ${className}`}
            >
                <DropdownOption value={optionList[0]}>
                    {optionList[0]}
                    {optionList.length > 1 && (
                        <div className="absolute right-2 top-1/2 -translate-y-1/2 flex flex-col h-1/2 content-center items-center">
                            <FaChevronUp className="h-1/2" />
                            <FaChevronDown className="h-1/2" />
                        </div>
                    )}
                </DropdownOption>
                {expanded &&
                    optionList.slice(1).map(o => (
                        <DropdownOption key={o} value={o}>
                            {o}
                        </DropdownOption>
                    ))}
            </div>
        </>
    )
}

export default Dropdown
