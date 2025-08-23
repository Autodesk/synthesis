import { Box, Divider, MenuItem, Select, Stack } from "@mui/material"
import type React from "react"
import Label from "@/components/Label.tsx"
import { Button, SynthesisIcons } from "@/components/StyledComponents.tsx"
import type { ModifierState } from "@/systems/input/InputTypes.ts"
import AxisInput from "@/systems/input/inputs/AxisInput.ts"
import ButtonInput from "@/systems/input/inputs/ButtonInput.ts"
import type Input from "@/systems/input/inputs/Input.ts"
import type { KeyCode } from "@/systems/input/KeyboardTypes.ts"

const toTitleCase = (camelCase: string) => {
    const result = camelCase.replace(/([A-Z])/g, " $1")
    const finalResult = result.charAt(0).toUpperCase() + result.slice(1)
    return finalResult
}

// Special characters only
const codeToCharacterMap: Partial<Record<KeyCode, string>> = {
    Slash: "/",
    Comma: ",",
    Period: ".",
    BracketLeft: "{",
    BracketRight: "}",
    Backquote: "`",
    Minus: "-",
    Equal: "=",
    Backslash: "\\",
    Semicolon: ";",
    Quote: '"',
}

const gamepadButtons: string[] = [
    "A",
    "B",
    "X",
    "Y",
    "Left Bumper",
    "Right Bumper",
    "Back",
    "Start",
    "Left Stick",
    "Right Stick",
    "UNKNOWN",
    "UNKNOWN2",
    "Dpad Up",
    "Dpad Down",
    "Dpad Left",
    "Dpad Right",
]

const gamepadAxes: string[] = ["N/A", "Left X", "Left Y", "Right X", "Right Y"]
const touchControlsAxes: string[] = ["N/A", "Left X", "Left Y", "Right X", "Right Y"]

// Converts a key code to displayable character (ex: KeyA -> "A")
const keyCodeToCharacter = (code: KeyCode) => {
    if (code.startsWith("Key")) return code.charAt(3)

    if (code.startsWith("Digit")) return code.charAt(5)

    if (code in codeToCharacterMap) return codeToCharacterMap[code]

    if (code.startsWith("Gamepad")) return gamepadButtons[parseInt(code.substring(8))]

    return code
}

const transformKeyName = (keyCode: KeyCode, keyModifiers: ModifierState) => {
    let prefix = ""
    if (keyModifiers) {
        if (keyModifiers.meta) prefix += "Meta + "
        if (keyModifiers.shift) prefix += "Shift + "
        if (keyModifiers.ctrl) prefix += "Ctrl + "
        if (keyModifiers.alt) prefix += "Alt + "
    }

    const displayName = prefix + keyCodeToCharacter(keyCode)
    if (displayName === "") return "N/A"

    return displayName
}

interface InputSelectionProps {
    input: Input
    setSelectedInput: React.Dispatch<React.SetStateAction<string>>
    selectedInput: string
}

export const KeyboardButtonSelection: React.FC<InputSelectionProps> = ({ input, setSelectedInput, selectedInput }) => {
    if (!(input instanceof ButtonInput)) throw new Error("Input not button type")

    return (
        <>
            <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
                <Label size="md">{toTitleCase(input.inputName)}</Label>

                <Box>
                    <Button
                        key={input.inputName}
                        onClick={() => {
                            setSelectedInput(input.inputName)
                        }}
                    >
                        {input.inputName === selectedInput
                            ? "..."
                            : transformKeyName(input.keyCode, input.keyModifiers)}
                    </Button>
                </Box>
            </Stack>
            <Divider />
        </>
    )
}

export const KeyboardAxisSelection: React.FC<InputSelectionProps> = ({ input, setSelectedInput, selectedInput }) => {
    if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

    return (
        <>
            <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
                <Label size="md">{toTitleCase(input.inputName)}</Label>

                <Stack direction="row" gap="10px" alignItems={"center"}>
                    {SynthesisIcons.ADD}
                    {/* Positive key */}
                    <Button
                        key={`pos${input.inputName}`}
                        variant="contained"
                        onClick={() => {
                            setSelectedInput(`pos${input.inputName}`)
                        }}
                    >
                        {`pos${input.inputName}` === selectedInput
                            ? "..."
                            : transformKeyName(input.posKeyCode, input.posKeyModifiers)}
                    </Button>
                    {SynthesisIcons.MINUS}
                    {/* Negative key */}
                    <Button
                        key={`neg${input.inputName}`}
                        variant="contained"
                        onClick={() => {
                            setSelectedInput(`neg${input.inputName}`)
                        }}
                    >
                        {`neg${input.inputName}` === selectedInput
                            ? "..."
                            : transformKeyName(input.negKeyCode, input.negKeyModifiers)}
                    </Button>
                </Stack>
            </Stack>
            <Divider />
        </>
    )
}

/** Select any button on a controller */
export const JoystickButtonSelection: React.FC<InputSelectionProps> = ({ input, selectedInput, setSelectedInput }) => {
    if (!(input instanceof ButtonInput)) throw new Error("Input not button type")
    return (
        <>
            <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
                <Label size="md">{toTitleCase(input.inputName)}</Label>
                <Button
                    key={input.inputName}
                    value={
                        input.inputName === selectedInput
                            ? "..."
                            : input.gamepadButton === -1
                              ? "N/A"
                              : gamepadButtons[input.gamepadButton]
                    }
                    onClick={() => {
                        setSelectedInput(input.inputName)
                    }}
                />
            </Stack>
            <Divider />
        </>
    )
}

/** Dropdown to select a controller axis */
export const JoystickAxisSelection: React.FC<
    InputSelectionProps & {
        setChosenGamepadAxis: React.Dispatch<React.SetStateAction<number>>
    }
> = ({ input, setSelectedInput, setChosenGamepadAxis }) => {
    if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

    return (
        <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
            <Label size="md">{toTitleCase(input.inputName)}</Label>
            <Select
                key={input.inputName}
                value={gamepadAxes[input.gamepadAxisNumber + 1]}
                onChange={e => {
                    setSelectedInput(input.inputName)
                    setChosenGamepadAxis(gamepadAxes.indexOf(e.target.value))
                }}
            >
                {gamepadAxes.map(axis => (
                    <MenuItem key={`axis-${axis}`} value={axis}>
                        {axis}
                    </MenuItem>
                ))}
            </Select>
        </Stack>
    )
}

/** Select an axis between two controller buttons */
export const GamepadButtonAxisSelection: React.FC<InputSelectionProps> = ({
    input,
    selectedInput,
    setSelectedInput,
}) => {
    if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

    return (
        <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
            <Label size="md">{toTitleCase(input.inputName)}</Label>

            <Stack direction="row" gap="10px" alignItems={"center"}>
                {/* Positive gamepad button */}
                {SynthesisIcons.ADD}
                <Button
                    key={`pos${input.inputName}`}
                    value={
                        `pos${input.inputName}` === selectedInput
                            ? "..."
                            : input.posGamepadButton === -1
                              ? "N/A"
                              : gamepadButtons[input.posGamepadButton]
                    }
                    onClick={() => {
                        setSelectedInput(`pos${input.inputName}`)
                    }}
                />
                {/* // Negative gamepad button */}
                {SynthesisIcons.MINUS}
                <Button
                    key={`neg${input.inputName}`}
                    value={
                        `neg${input.inputName}` === selectedInput
                            ? "..."
                            : input.negGamepadButton === -1
                              ? "N/A"
                              : gamepadButtons[input.negGamepadButton]
                    }
                    onClick={() => {
                        setSelectedInput(`neg${input.inputName}`)
                    }}
                />
            </Stack>
        </Stack>
    )
}

export const TouchControlsAxisSelection: React.FC<
    InputSelectionProps & {
        setChosenTouchControlsAxis: React.Dispatch<React.SetStateAction<number>>
    }
> = ({ input, setSelectedInput, setChosenTouchControlsAxis }) => {
    if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

    return (
        <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
            <Label size="md">{toTitleCase(input.inputName)}</Label>
            <Select
                key={input.inputName}
                value={touchControlsAxes[input.touchControlAxis]}
                onChange={e => {
                    setSelectedInput(input.inputName)
                    setChosenTouchControlsAxis(touchControlsAxes.indexOf(e.target.value))
                }}
            >
                {touchControlsAxes.map(axis => (
                    <MenuItem key={`touch-axis-${axis}`} value={axis}>
                        {axis}
                    </MenuItem>
                ))}
            </Select>
        </Stack>
    )
}
