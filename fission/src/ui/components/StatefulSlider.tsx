import { Slider, Stack, Tooltip, Typography } from "@mui/material"
import Label from "./Label"
import { useEffect, useState } from "react"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & {
        label: string
        defaultValue: number
        onChange: (val: number) => void
        tooltip?: string
        showValue?: boolean
    }
> = props => {
    const [value, setValue] = useState(props.defaultValue)

    useEffect(() => {
        // when setting the values with a dropdown menu the sliders didn't update
        setValue(props.defaultValue)
    }, [props.defaultValue])

    const { showValue, ...sliderProps } = props
    return (
        <Tooltip title={props.tooltip ?? ""}>
            <Stack
                direction="column"
                gap={0.5}
                className="no-drag"
                sx={{
                    px: 2,
                    py: 0.5,
                    overflow: "hidden",
                    boxSizing: "border-box",
                    width: "100%",
                }}
            >
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Label size="sm" className="mr-12 whitespace-nowrap">
                        {props.label}
                    </Label>
                    {showValue !== false && (
                        <Typography variant="caption">{(value ?? 0).toFixed(2)}</Typography>
                    )}
                </Stack>
                <Slider
                    {...sliderProps}
                    value={value}
                    onChange={(_, value) => {
                        setValue(value as number)
                        props.onChange?.(value as number)
                    }}
                    sx={{
                        mx: 0,
                        width: "100%",
                        "& .MuiSlider-thumb": {
                            boxShadow: 2,
                        },
                    }}
                />
            </Stack>
        </Tooltip>
    )
}

export default StatefulSlider
