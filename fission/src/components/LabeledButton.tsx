import React from "react"
import Button from "./Button"
import Label, { LabelSize } from "./Label"
import Stack, { StackDirection } from "./Stack"

export enum LabelPlacement {
    Left,
    Right,
    Top,
    Bottom,
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
    const buttonComponent = (
        <Button
            key={"button"}
            value={value}
            onClick={onClick}
            className={buttonClassName}
        />
    )
    const labelComponent = (
        <Label
            key={"label"}
            size={labelSize || LabelSize.Small}
            className={labelClassName}
        >
            {label}
        </Label>
    )

    const labelBefore =
        placement == LabelPlacement.Left || placement == LabelPlacement.Top
    const isHorizontal =
        placement == LabelPlacement.Left || placement == LabelPlacement.Right

    return (
        <Stack
            direction={
                isHorizontal
                    ? StackDirection.Horizontal
                    : StackDirection.Vertical
            }
            justify={"between"}
            className="items-center"
        >
            {labelBefore && [labelComponent, buttonComponent]}
            {!labelBefore && [buttonComponent, labelComponent]}
        </Stack>
    )
}

export default LabeledButton
