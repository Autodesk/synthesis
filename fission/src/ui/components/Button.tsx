import React from "react"

export enum ButtonSize {
    Small,
    Medium,
    Large,
    XL,
}

type ButtonProps = {
    value: string
    colorClass?: string
    size?: ButtonSize
    onClick?: () => void
    className?: string
}

const Button: React.FC<ButtonProps> = ({
    value,
    colorClass,
    size,
    onClick,
    className,
}) => {
    let sizeClassNames

    if (!size) size = ButtonSize.Medium as ButtonSize

    switch (size) {
        case ButtonSize.Small:
            sizeClassNames = "px-2 py-1"
            break
        case ButtonSize.Medium:
            sizeClassNames = "px-4 py-1"
            break
        case ButtonSize.Large:
            sizeClassNames = "px-8 py-2"
            break
        case ButtonSize.XL:
            sizeClassNames = "px-10 py-2"
            break
    }

    return (
        <input
            type="button"
            value={value}
            onClick={onClick}
            className={`${
                colorClass
                    ? colorClass
                    : "bg-gradient-to-r from-interactive-element-left via-interactive-element-right to-interactive-element-left bg-[length:200%_100%] active:bg-right"
            } w-min ${sizeClassNames} rounded-sm font-semibold cursor-pointer duration-200 ${
                className || ""
            }`}
        />
    )
}

export default Button
