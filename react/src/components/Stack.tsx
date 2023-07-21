import React from 'react';

export enum StackDirection {
    Horizontal,
    Vertical
}

type StackProps = {
    direction: StackDirection;
    justify?: "between" | "around" | "evenly";
}

const Stack: React.FC<StackProps> = ({ children, direction, justify }) => {
    const directionClassName = direction == StackDirection.Horizontal ? "flex-row" : "flex-col";
    if (!justify) justify = "between";

    return (
        <div className={`flex ${directionClassName} justify-${justify}`} > {children}</div >
    );
}

export default Stack;
