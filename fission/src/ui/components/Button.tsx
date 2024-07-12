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
}

const Button: React.FC<ButtonProps> = ({ value, colorOverrideClass, sizeOverrideClass, size, onClick, className }) => {
    let sizeClassNames = sizeOverrideClass

    if (!size) size = ButtonSize.Medium as ButtonSize

    if (!sizeClassNames) {
        switch (size) {
            case ButtonSize.Small:
                sizeClassNames = "px-4 py-1"
                break
            case ButtonSize.Medium:
                sizeClassNames = "px-6 py-1.5"
                break
            case ButtonSize.Large:
                sizeClassNames = "px-8 py-2"
                break
            case ButtonSize.XL:
                sizeClassNames = "px-10 py-2"
                break
        }
    }

    return (
        <BaseButton
            onClick={onClick}
            className={`${
                colorOverrideClass
                    ? colorOverrideClass
                    : "bg-gradient-to-r from-interactive-element-left via-interactive-element-right to-interactive-element-left bg-[length:200%_100%] active:bg-right"
            } w-fit h-fit ${sizeClassNames} rounded-sm font-semibold cursor-pointer duration-200 border-none focus-visible:outline-0 focus:outline-0 ${
                className || ""
            }`}
        >
            {value}
        </BaseButton>
    )
}

export default Button
