import { FormControlLabel, Slider } from "@mui/material"
import { useState } from "react"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & {
        label: string
        defaultValue: number
        onChange: (val: number) => void
    }
> = props => {
    const [value, setValue] = useState(props.defaultValue)
    return (
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
    )
}

export default StatefulSlider
