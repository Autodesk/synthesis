import React from "react";

export enum StackDirection {
    Horizontal,
    Vertical,
}

type StackProps = {
    direction: StackDirection;
    spacing?: number;
    justify?: "between" | "around" | "evenly";
};

const Stack: React.FC<StackProps> = ({ className, children, direction, spacing, justify }) => {
    const directionClassName =
        direction == StackDirection.Horizontal ? "flex-row" : "flex-col";
    if (!justify) justify = "between";

    return (
        <div className={`flex ${directionClassName} justify-${justify} gap-[${spacing}px] w-full ${className}`}>
            {" "}
            {children}
        </div>
    );
};

export default Stack;
