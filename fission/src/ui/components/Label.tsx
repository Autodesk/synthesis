import React, { ReactNode } from "react"

export enum LabelSize {
    SMALL,
    MEDIUM,
    LARGE,
    XL,
}

const labelSizeToClassName = (size?: LabelSize) => {
    switch (size) {
        case LabelSize.SMALL:
            return "text-sm"
        case LabelSize.MEDIUM:
            return "text-xl"
        case LabelSize.LARGE:
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
        style={{ userSelect: "none", MozUserSelect: "none", msUserSelect: "none", WebkitUserSelect: "none" }}
    >
        {children}
    </span>
)

export default Label
