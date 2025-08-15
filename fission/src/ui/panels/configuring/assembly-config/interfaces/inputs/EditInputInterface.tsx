import { Box, Divider, MenuItem, Select, Stack } from "@mui/material"
import { Button } from "@/ui/components/StyledComponents"
import type React from "react"
import { useEffect, useState } from "react"
import InputSystem from "@/systems/input/InputSystem"
import { EMPTY_MODIFIER_STATE, type ModifierState } from "@/systems/input/InputTypes"
import AxisInput from "@/systems/input/inputs/AxisInput"
import ButtonInput from "@/systems/input/inputs/ButtonInput"
import type Input from "@/systems/input/inputs/Input"
import type { KeyCode } from "@/systems/input/KeyboardTypes"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

// Converts camelCase to Title Case for the inputs modal
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

interface EditInputProps {
    input: Input
    useGamepad: boolean
    useTouchControls: boolean
    onInputChanged: () => void
}

const EditInputInterface: React.FC<EditInputProps> = ({ input, useGamepad, useTouchControls, onInputChanged }) => {
    const [selectedInput, setSelectedInput] = useState<string>("")
    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)
    const [chosenTouchControlsAxis, setChosenTouchControlsAxis] = useState<number>(-1)
    const [chosenKey, setChosenKey] = useState<KeyCode>("")
    const [modifierState, setModifierState] = useState<ModifierState>(EMPTY_MODIFIER_STATE)
    const [chosenButton, setChosenButton] = useState<number>(-1)
    const [useGamepadButtons, setUseGamepadButtons] = useState<boolean>(
        input instanceof AxisInput ? input.useGamepadButtons : false
    )

    /** Select any key on the keyboard */
    const KeyboardButtonSelection = () => {
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
                                ? "Press anything"
                                : transformKeyName(input.keyCode, input.keyModifiers)}
                        </Button>
                    </Box>
                </Stack>
                <Divider />
            </>
        )
    }

    /** Select an axis between two keyboard keys */
    const KeyboardAxisSelection = () => {
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
                                ? "Press anything"
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
                                ? "Press anything"
                                : transformKeyName(input.negKeyCode, input.negKeyModifiers)}
                        </Button>
                    </Stack>
                </Stack>
                <Divider />
            </>
        )
    }

    /** Select any button on a controller */
    const JoystickButtonSelection = () => {
        if (!(input instanceof ButtonInput)) throw new Error("Input not button type")
        return (
            <>
                <Stack direction="row" gap={10} alignItems="center" justifyContent="space-between" width="98%">
                    <Label size="md">{toTitleCase(input.inputName)}</Label>
                    <Button
                        key={input.inputName}
                        value={
                            input.inputName === selectedInput
                                ? "Press anything"
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
    const JoystickAxisSelection = () => {
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
    const GamepadButtonAxisSelection = () => {
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
                                ? "Press anything"
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
                                ? "Press anything"
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

    const TouchControlsAxisSelection = () => {
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

    /** Show the correct selection mode based on input type and how it's configured */
    const inputConfig = () => {
        if (useGamepad) {
            // Joystick Button
            if (input instanceof ButtonInput) {
                return JoystickButtonSelection()
            }

            // Gamepad axis
            else if (input instanceof AxisInput) {
                return (
                    <div key={input.inputName}>
                        {input.useGamepadButtons
                            ? GamepadButtonAxisSelection()
                            : // Gamepad joystick axis
                              JoystickAxisSelection()}

                        {/* // Button to switch between two buttons and a joystick axis */}
                        <Checkbox
                            label="Use Gamepad Buttons"
                            checked={useGamepadButtons}
                            onClick={checked => {
                                input.useGamepadButtons = checked
                                setUseGamepadButtons(checked)
                            }}
                        />
                        {/* // Button to invert the joystick axis */}
                        <Checkbox
                            label="Invert Joystick"
                            checked={input.joystickInverted}
                            onClick={checked => {
                                input.joystickInverted = checked
                            }}
                        />
                        <Divider />
                    </div>
                )
            }
        } else if (useTouchControls) {
            // here
            if (input instanceof AxisInput) {
                return (
                    <div key={input.inputName}>
                        {TouchControlsAxisSelection()}
                        {/* // Button to invert the joystick axis */}
                        <Checkbox
                            label="Invert Joystick"
                            checked={input.joystickInverted}
                            onClick={checked => {
                                input.joystickInverted = checked
                            }}
                        />
                        <Divider />
                    </div>
                )
            }
        } else {
            // Keyboard button
            if (input instanceof ButtonInput) {
                return KeyboardButtonSelection()
            }
            // Keyboard Axis
            else if (input instanceof AxisInput) {
                return KeyboardAxisSelection()
            }
        }
    }

    useEffect(() => {
        const checkGamepadState = () => {
            if (InputSystem.gamepad !== null) {
                const pressedButtons = InputSystem.gamepad.buttons
                    .map((button, index) => (button.pressed ? index : null))
                    .filter(index => index !== null)
                    .map(index => index!)

                if (pressedButtons.length > 0) setChosenButton(pressedButtons[0])
                else if (chosenButton !== -1) setChosenButton(-1)
            }
            requestAnimationFrame(checkGamepadState)
        }

        checkGamepadState()
    })

    /** Input detection for setting inputs */
    useEffect(() => {
        // // Assign keyboard inputs when a key is pressed
        if (!useGamepad && !useTouchControls && selectedInput && chosenKey) {
            if (selectedInput.startsWith("pos")) {
                if (!(input instanceof AxisInput)) return
                input.posKeyCode = chosenKey
                input.posKeyModifiers = modifierState
            } else if (selectedInput.startsWith("neg")) {
                if (!(input instanceof AxisInput)) return

                input.negKeyCode = chosenKey
                input.negKeyModifiers = modifierState
            } else {
                if (!(input instanceof ButtonInput)) return

                input.keyCode = chosenKey
                input.keyModifiers = modifierState
            }

            setChosenKey("")
            setSelectedInput("")
            setModifierState(EMPTY_MODIFIER_STATE)
            onInputChanged()
        }
        // Assign gamepad button inputs when a button is pressed
        else if (useGamepad && selectedInput && chosenButton !== -1) {
            if (selectedInput.startsWith("pos")) {
                if (!(input instanceof AxisInput)) return

                input.posGamepadButton = chosenButton
            } else if (selectedInput.startsWith("neg")) {
                if (!(input instanceof AxisInput)) return

                input.negGamepadButton = chosenButton
            } else {
                if (!(input instanceof ButtonInput)) return

                input.gamepadButton = chosenButton
            }

            onInputChanged()

            setChosenButton(-1)
            setSelectedInput("")
        }

        // Assign gamepad axis inputs when a gamepad axis is selected
        if (useGamepad && selectedInput && chosenGamepadAxis !== -1) {
            if (!(input instanceof AxisInput)) return

            input.gamepadAxisNumber = chosenGamepadAxis - 1

            onInputChanged()
            setChosenGamepadAxis(-1)
            setSelectedInput("")
        }

        if (useTouchControls && selectedInput && chosenTouchControlsAxis !== -1) {
            if (!(input instanceof AxisInput)) return

            input.touchControlAxis = chosenTouchControlsAxis

            onInputChanged()
            setChosenTouchControlsAxis(-1)
            setSelectedInput("")
        }
    }, [
        chosenKey,
        chosenButton,
        chosenGamepadAxis,
        input,
        modifierState,
        onInputChanged,
        selectedInput,
        useGamepad,
        useTouchControls,
        chosenTouchControlsAxis,
    ])

    return (
        <Box
            onKeyUp={e => {
                e.preventDefault()
                if (selectedInput != "") setChosenKey(selectedInput ? (e.code as KeyCode) : "")
                setModifierState({
                    ctrl: e.ctrlKey,
                    alt: e.altKey,
                    shift: e.shiftKey,
                    meta: e.metaKey,
                })
            }}
        >
            {inputConfig()}
        </Box>
    )
}

export default EditInputInterface
