import React, { ReactNode } from "react"
import { Button as BaseButton } from "@mui/base/Button"

export enum ButtonSize {
    Small,
    Medium,
    Large,
    XL,
}

export type ButtonProps = {
    value: ReactNode
    colorOverrideClass?: string
    sizeOverrideClass?: string
    size?: ButtonSize
    onClick?: () => void
    className?: string
    id?: string
    disabled?: boolean
}

const Button: React.FC<ButtonProps> = ({
    value,
    colorOverrideClass,
    sizeOverrideClass,
    size,
    onClick,
    className,
    id,
    disabled,
}) => {
    let sizeClassNames = sizeOverrideClass

    if (!size) size = ButtonSize.Medium as ButtonSize

    if (!sizeClassNames) {
        switch (size) {
            case ButtonSize.Small:
                sizeClassNames = "w-fit h-fit px-4 py-1"
                break
            case ButtonSize.Medium:
                sizeClassNames = "w-fit h-fit px-6 py-1.5"
                break
            case ButtonSize.Large:
                sizeClassNames = "w-fit h-fit px-8 py-2"
                break
            case ButtonSize.XL:
                sizeClassNames = "w-fit h-fit px-10 py-2"
                break
        }
    }

    return (
        <BaseButton
            onClick={onClick}
            className={`
                ${colorOverrideClass || "bg-gradient-to-r from-interactive-element-left via-interactive-element-right to-interactive-element-left bg-[length:200%_100%] active:bg-right"}  
                ${sizeClassNames} 
                rounded-sm 
                font-semibold 
                cursor-pointer 
                duration-200 
                border-none 
                focus-visible:outline-0 
                focus:outline-0 
                transform 
                transition-transform 
                hover:scale-[1.03] 
                active:scale-[1.06] 
                ${className || ""}
            `}
            id={id}
            disabled={disabled ?? false}
            style={{ userSelect: "none", MozUserSelect: "none", msUserSelect: "none", WebkitUserSelect: "none" }}
        >
            {value}
        </BaseButton>
    )
}

export default Button
