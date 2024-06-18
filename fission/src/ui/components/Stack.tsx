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
    justify?:
        | "normal"
        | "start"
        | "end"
        | "center"
        | "between"
        | "around"
        | "evenly"
        | "stretch"
    align?:
        | "normal"
        | "center"
        | "start"
        | "end"
        | "between"
        | "around"
        | "evenly"
        | "baseline"
        | "stretch"
}

const Stack: React.FC<StackProps> = ({
    className,
    children,
    direction,
    spacing,
    justify,
    align,
}) => {
    const directionClassName =
        direction == StackDirection.Horizontal ? "flex-row" : "flex-col"
    if (!justify) justify = "between"
    if (!align) align = "center"
    if (spacing == null) spacing = 10

    return (
        <div
            className={`flex ${directionClassName} justify-${justify} align-${align} gap-[${spacing}px] w-full ${className}`}
            style={{
                gap: `${spacing}px`,
                justifyContent: `${
                    justify == "start"
                        ? "flex-start"
                        : justify == "end"
                          ? "flex-end"
                          : justify == "between"
                            ? "space-between"
                            : justify == "around"
                              ? "space-around"
                              : justify == "evenly"
                                ? "space-evenly"
                                : justify
                }`,
                alignContent: `${
                    align == "start"
                        ? "flex-start"
                        : align == "end"
                          ? "flex-end"
                          : align == "between"
                            ? "space-between"
                            : align == "around"
                              ? "space-around"
                              : align == "evenly"
                                ? "space-evenly"
                                : align
                }`,
            }}
        >
            {" "}
            {children}
        </div>
    )
}

export default Stack
