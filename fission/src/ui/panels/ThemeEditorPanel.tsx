import { randomColor } from "@/util/Random"
import { useThemeContext } from "../helpers/ThemeProviderHelpers"
import { Box, Button, Stack, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { GiPerspectiveDiceSixFacesOne } from "react-icons/gi"
import Label from "../components/Label"
import { useUIContext } from "../helpers/UIProviderHelpers"
import { PanelImplProps } from "../components/Panel"
import StatefulCheckbox from "../components/StatefulCheckbox"

export const ThemeEditorPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { mode, setMode, primaryColor, secondaryColor, setPrimaryColor, setSecondaryColor } = useThemeContext()
    const { configureScreen } = useUIContext()

    const [tempPrimary, setTempPrimary] = useState(primaryColor)
    const [tempSecondary, setTempSecondary] = useState(secondaryColor)

    useEffect(() => {
        const onBeforeAccept = () => {
            setPrimaryColor(tempPrimary)
            setSecondaryColor(tempSecondary)
        }

        configureScreen(panel!, { title: "Theme Editor" }, { onBeforeAccept })
    }, [tempPrimary, tempSecondary])

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
                    // I intentionally decided not to apply the reset in case that's not what the user wants
                    setTempPrimary("#90caf9")
                    setTempSecondary("#ce93d8")
                }}
            >
                Reset
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
