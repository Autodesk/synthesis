import React from "react"
import Button from "./Button"
import Label, { LabelSize } from "./Label"
import Stack, { StackDirection } from "./Stack"

export enum LabelPlacement {
    LEFT,
    RIGHT,
    TOP,
    BOTTOM,
}

type LabeledButtonProps = {
    label: string
    value: string
    placement: LabelPlacement
    labelSize?: LabelSize
    onClick?: () => void
    labelClassName?: string
    buttonClassName?: string
}

const LabeledButton: React.FC<LabeledButtonProps> = ({
    label,
    value,
    placement,
    labelSize,
    onClick,
    labelClassName,
    buttonClassName,
}) => {
    const buttonComponent = <Button key={"button"} value={value} onClick={onClick} className={buttonClassName} />
    const labelComponent = (
        <Label key={"label"} size={labelSize || LabelSize.SMALL} className={labelClassName}>
            {label}
        </Label>
    )

    const labelBefore = placement == LabelPlacement.LEFT || placement == LabelPlacement.TOP
    const isHorizontal = placement == LabelPlacement.LEFT || placement == LabelPlacement.RIGHT

    return (
        <Stack
            direction={isHorizontal ? StackDirection.HORIZONTAL : StackDirection.VERTICAL}
            justify={"between"}
            className="items-center"
        >
            {labelBefore && [labelComponent, buttonComponent]}
            {!labelBefore && [buttonComponent, labelComponent]}
        </Stack>
    )
}

export default LabeledButton
