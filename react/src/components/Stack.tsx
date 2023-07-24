import React, { ReactNode } from "react"

export enum StackDirection {
    Horizontal,
    Vertical,
}

type StackProps = {
    children?: ReactNode
    className?: string
    direction: StackDirection
    spacing?: number
    justify?: "between" | "around" | "evenly"
}

const Stack: React.FC<StackProps> = ({
    className,
    children,
    direction,
    spacing,
    justify,
}) => {
    const directionClassName =
        direction == StackDirection.Horizontal ? "flex-row" : "flex-col"
    if (!justify) justify = "between"
    if (spacing == null) spacing = 10

    return (
        <div
            className={`flex ${directionClassName} justify-${justify} gap-[${spacing}px] w-full ${className}`}
        >
            {" "}
            {children}
        </div>
    )
}

export default Stack
