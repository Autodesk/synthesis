import { Slider, Stack, Tooltip, Typography } from "@mui/material"
import { useState } from "react"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & {
        label: string
        defaultValue: number
        onChange: (val: number) => void
        tooltip?: string
    }
> = props => {
    const [value, setValue] = useState(props.defaultValue)
    return (
        <Tooltip title={props.tooltip ?? ""}>
            <Stack direction="column" gap={0.5} className="no-drag">
                <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                    <Typography variant="body2">{props.label}</Typography>
                    <Typography variant="caption">{value.toFixed(2)}</Typography>
                </Stack>
                <Slider
                    {...props}
                    value={value}
                    onChange={(_, value) => {
                        setValue(value as number)
                        props.onChange?.(value as number)
                    }}
                />
            </Stack>
        </Tooltip>
    )
}

export default StatefulSlider
