import { randomColor } from "@/util/Random"
import { useThemeContext } from "../ThemeProvider"
import { Box, Button, FormControlLabel, Stack, Switch, TextField, Typography } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { GiPerspectiveDiceSixFacesOne } from "react-icons/gi"
import Label from "../components/Label"
import { useUIContext } from "../UIProvider"
import { PanelImplProps } from "../components/Panel"
import StatefulCheckbox from "../components/StatefulCheckbox"

export const ThemeEditorPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { mode, setMode, primaryColor, secondaryColor, setPrimaryColor, setSecondaryColor } = useThemeContext()
    const { configureScreen } = useUIContext()

    const [tempPrimary, setTempPrimary] = useState(primaryColor)
    const [tempSecondary, setTempSecondary] = useState(secondaryColor)

    useEffect(() => {
        configureScreen(panel!, { title: "Theme Editor" }, {})
    }, [])

    return (
        <Stack gap={4}>
            <Label size="md">Theme Editor</Label>
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
            <StatefulCheckbox
                label="Dark Mode"
                checked={mode === "dark"}
                onClick={checked => setMode(checked ? "dark" : "light")}
            />
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
