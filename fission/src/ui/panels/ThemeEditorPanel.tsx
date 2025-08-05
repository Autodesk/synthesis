import { randomColor } from "@/util/Random"
import { useThemeContext } from "../helpers/ThemeProviderHelpers"
import { Box, Button, Stack, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import { GiPerspectiveDiceSixFacesOne } from "react-icons/gi"
import Label from "../components/Label"
import { useUIContext } from "../helpers/UIProviderHelpers"
import type { PanelImplProps } from "../components/Panel"
import Checkbox from "../components/Checkbox"

const ColorEditor: React.FC<{ label: string; color: string; setColor: (_c: string) => void }> = ({
    label,
    color,
    setColor,
}) => {
    return (
        <Stack direction="row" gap={2}>
            <TextField
                label={label}
                variant="outlined"
                defaultValue={color}
                onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                    setColor(event.target.value)
                }}
            />
            <Box
                sx={{
                    height: 55,
                    aspectRatio: 1,
                    borderRadius: 1,
                    bgcolor: `${color}`,
                }}
            />
        </Stack>
    )
}

export const ThemeEditorPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const {
        mode,
        setMode,
        primaryColor,
        secondaryColor,
        blueAllianceColor,
        redAllianceColor,
        setPrimaryColor,
        setSecondaryColor,
        setBlueAllianceColor,
        setRedAllianceColor,
    } = useThemeContext()
    const { configureScreen } = useUIContext()

    const [tempPrimary, setTempPrimary] = useState(primaryColor)
    const [tempSecondary, setTempSecondary] = useState(secondaryColor)
    const [tempBlue, setTempBlue] = useState(blueAllianceColor)
    const [tempRed, setTempRed] = useState(redAllianceColor)

    useEffect(() => {
        const onBeforeAccept = () => {
            setPrimaryColor(tempPrimary)
            setSecondaryColor(tempSecondary)
        }

        configureScreen(panel!, { title: "Theme Editor" }, { onBeforeAccept })
    }, [tempPrimary, tempSecondary, configureScreen, panel, setPrimaryColor, setSecondaryColor])

    return (
        <Stack gap={4}>
            <Label size="md">Theme Editor</Label>
            <ColorEditor label="Primary Color" color={tempPrimary} setColor={setTempPrimary} />
            <ColorEditor label="Secondary Color" color={tempSecondary} setColor={setTempSecondary} />
            <ColorEditor label="Blue Alliance" color={tempBlue} setColor={setTempBlue} />
            <ColorEditor label="Red Alliance" color={tempRed} setColor={setTempRed} />
            <Checkbox
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
                    setTempBlue("#0066b3")
                    setTempRed("#ed1c24")
                }}
            >
                Reset
            </Button>
            <Button
                onClick={() => {
                    setPrimaryColor(tempPrimary)
                    setSecondaryColor(tempSecondary)
                    setBlueAllianceColor(tempBlue)
                    setRedAllianceColor(tempRed)
                }}
            >
                Apply
            </Button>
        </Stack>
    )
}
