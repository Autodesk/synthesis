import { FormControlLabel, Slider, Tooltip } from "@mui/material"
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
            <FormControlLabel
                label={props.label}
                labelPlacement="top"
                control={
                    <Slider
                        {...props}
                        value={value}
                        onChange={(_, value) => {
                            setValue(value as number)
                            props.onChange?.(value as number)
                        }}
                    ></Slider>
                }
            />
        </Tooltip>
    )
}

export default StatefulSlider
