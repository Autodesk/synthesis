import React, { useCallback, useEffect, useRef, useState } from "react"
import Button, { ButtonSize } from "./Button"
import Label, { LabelSize } from "./Label"
import Stack, { StackDirection } from "./Stack"
import { Random } from '@/util/Random';

type SelectButtonProps = {
    colorClass?: string
    size?: ButtonSize
    placeholder?: string
    onSelect?: (value: string) => void
    className?: string
}

const SelectButton: React.FC<SelectButtonProps> = ({
    colorClass,
    size,
    placeholder,
    onSelect,
    className,
}) => {
    const [value, setValue] = useState<string>()
    const [selecting, setSelecting] = useState<boolean>(false)
    const timeoutRef = useRef<NodeJS.Timeout>()

    const onReceiveSelection = useCallback(
        (value: string) => {
            // TODO remove this when communication works
            clearTimeout(timeoutRef.current)
            setValue(value)
            setSelecting(false)
            if (onSelect) onSelect(value)
        },
        [setValue, setSelecting, onSelect]
    )

    useEffect(() => {
        // simulate receiving a selection from Synthesis
        if (selecting) {
            timeoutRef.current = setTimeout(
                () => {
                    if (selecting) {
                        const v = `node_${Math.floor(
                            Random() * 10
                        ).toFixed(0)}`
                        onReceiveSelection(v)
                    }
                },
                Math.floor(Random() * 2_750) + 250
            )
        }
    }, [selecting, onReceiveSelection])

    // should send selecting state when clicked and then receive string value to set selecting to false

    return (
        <Stack direction={StackDirection.Horizontal}>
            <Label size={LabelSize.Medium}>
                {value || placeholder || "Click to select"}
            </Label>
            <Button
                value={selecting ? "..." : "Select"}
                colorClass={selecting ? "bg-background-secondary" : colorClass}
                size={size}
                onClick={() => {
                    // send selecting state
                    if (selecting) {
                        // cancel selection
                        onReceiveSelection("")
                    } else {
                        setSelecting(true)
                    }
                }}
                className={className}
            />
        </Stack>
    )
}

export default SelectButton
