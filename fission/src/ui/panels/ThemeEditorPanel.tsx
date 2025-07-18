import { randomColor } from "@/util/Random"
import { useThemeContext } from "../ThemeProvider"
import { Box, Button, Stack, TextField, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { GiPerspectiveDiceSixFacesOne } from "react-icons/gi"

export const ThemeEditorPanel: React.FC = () => {
    const { mode, toggleColorMode, primaryColor, secondaryColor, setPrimaryColor, setSecondaryColor } =
        useThemeContext()

    const [tempPrimary, setTempPrimary] = useState(primaryColor)
    const [tempSecondary, setTempSecondary] = useState(secondaryColor)

    return (
        <Stack gap={4}>
            <Typography variant="h4">Theme Editor</Typography>
            <Stack direction="row" gap={2}>
                <TextField
                    label="Primary Color"
                    variant="outlined"
                    defaultValue={tempPrimary}
                    onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                        setTempPrimary(event.target.value)
                    }}
                />
                <Box
                    sx={{
                        height: 55,
                        aspectRatio: 1,
                        borderRadius: 1,
                        bgcolor: `${tempPrimary}`,
                    }}
                />
            </Stack>
            <Stack direction="row" gap={2}>
                <TextField
                    label="Secondary Color"
                    variant="outlined"
                    defaultValue={tempSecondary}
                    onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                        setTempSecondary(event.target.value)
                    }}
                />
                <Box
                    sx={{
                        height: 55,
                        aspectRatio: 1,
                        borderRadius: 1,
                        bgcolor: `${tempSecondary}`,
                    }}
                />
            </Stack>
            <Button
                startIcon={<GiPerspectiveDiceSixFacesOne />}
                onClick={() => {
                    setTempPrimary(randomColor())
                    setTempSecondary(randomColor())
                }}
            >
                Randomize
            </Button>
            <Button
                onClick={() => {
                    setPrimaryColor(tempPrimary)
                    setSecondaryColor(tempSecondary)
                }}
            >
                Apply
            </Button>
        </Stack>
    )
}
