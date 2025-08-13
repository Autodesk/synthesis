import { Box, FormControlLabel, Slider, Stack, Tooltip, Typography } from "@mui/material"
import { useState } from "react"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & {
        label: string
        defaultValue: number
        onChange: (val: number) => void
        tooltip?: string
        format?: Intl.NumberFormatOptions
        unit?: string
    }
> = props => {
    const [value, setValue] = useState(props.defaultValue)
    
    const formatValue = (val: number) => {
        const formattedNumber = props.format 
            ? new Intl.NumberFormat('en-US', props.format).format(val)
            : val.toString()
        return props.unit ? `${formattedNumber} ${props.unit}` : formattedNumber
    }
    
    return (
        <Tooltip title={props.tooltip ?? ""}>
            <Box>
                <Stack direction="row" justifyContent="space-between" alignItems="center" mb={1}>
                    <Typography variant="body2" component="label">
                        {props.label}
                    </Typography>
                    <Typography 
                        variant="body2" 
                        component="span"
                        sx={{ 
                            fontWeight: 'medium',
                            color: 'primary.main',
                            minWidth: 'fit-content',
                            textAlign: 'right'
                        }}
                    >
                        {formatValue(value)}
                    </Typography>
                </Stack>
                <Slider
                    {...props}
                    value={value}
                    onChange={(_, value) => {
                        setValue(value as number)
                        props.onChange?.(value as number)
                    }}
                />
            </Box>
        </Tooltip>
    )
}

export default StatefulSlider
