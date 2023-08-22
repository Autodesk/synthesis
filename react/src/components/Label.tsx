import React, { ReactNode } from "react"

export enum LabelSize {
    Small,
    Medium,
    Large,
    XL,
}

const labelSizeToClassName = (size?: LabelSize) => {
    switch (size) {
        case LabelSize.Small:
            return "text-sm"
        case LabelSize.Medium:
            return "text-xl"
        case LabelSize.Large:
            return "text-2xl"
        case LabelSize.XL:
            return "text-4xl"
        default:
            return "text-base"
    }
}

type LabelProps = {
    size?: LabelSize
    children?: ReactNode
    className?: string
}

const Label: React.FC<LabelProps> = ({ children, size, className }) => (
    <span
        className={`text-main-text h-min ${labelSizeToClassName(size)} ${className}`}
    >
        {children}
    </span>
)

export default Label
