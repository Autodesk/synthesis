import React, { useEffect, useState } from "react"
import InputSystem, {
    AxisInput,
    ButtonInput,
    EmptyModifierState,
    Input,
    ModifierState,
} from "@/systems/input/InputSystem"
import Dropdown from "@/ui/components/Dropdown"
import Checkbox from "@/ui/components/Checkbox"
import Button from "@/ui/components/Button"
import Label from "@/ui/components/Label"
import { Box } from "@mui/material"
import { SectionDivider, Spacer, SynthesisIcons } from "@/ui/components/StyledComponents"

// Converts camelCase to Title Case for the inputs modal
const toTitleCase = (camelCase: string) => {
    const result = camelCase.replace(/([A-Z])/g, " $1")
    const finalResult = result.charAt(0).toUpperCase() + result.slice(1)
    return finalResult
}

// Special characters only
const codeToCharacterMap: { [code: string]: string } = {
    Slash: "/",
    Comma: ",",
    Period: ".",
    BracketLeft: "{",
    BracketRight: "}",
    BackQuote: "`",
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

// Converts a key code to displayable character (ex: KeyA -> "A")
const keyCodeToCharacter = (code: string) => {
    if (code.startsWith("Key")) return code.charAt(3)

    if (code.startsWith("Digit")) return code.charAt(5)

    if (code in codeToCharacterMap) return codeToCharacterMap[code]

    if (code.startsWith("Gamepad")) return gamepadButtons[parseInt(code.substring(8))]

    return code
}

const transformKeyName = (keyCode: string, keyModifiers: ModifierState) => {
    let prefix = ""
    if (keyModifiers) {
        if (keyModifiers.meta) prefix += "Meta + "
        if (keyModifiers.shift) prefix += "Shift + "
        if (keyModifiers.ctrl) prefix += "Ctrl + "
        if (keyModifiers.alt) prefix += "Alt + "
    }

    const displayName = prefix + keyCodeToCharacter(keyCode)
    if (displayName == "") return "N/A"

    return displayName
}

interface EditInputProps {
    input: Input
    useGamepad: boolean
    onInputChanged: () => void
}

const EditInputInterface: React.FC<EditInputProps> = ({ input, useGamepad, onInputChanged }) => {
    const [selectedInput, setSelectedInput] = useState<string>("")
    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)
    const [chosenKey, setChosenKey] = useState<string>("")
    const [modifierState, setModifierState] = useState<ModifierState>(EmptyModifierState)
    const [chosenButton, setChosenButton] = useState<number>(-1)
    const [useGamepadButtons, setUseGamepadButtons] = useState<boolean>(
        input instanceof AxisInput ? input.useGamepadButtons : false
    )

    /** Select any key on the keyboard */
    const KeyboardButtonSelection = () => {
        if (!(input instanceof ButtonInput)) throw new Error("Input not button type")

        return (
            <>
                <Box
                    display="flex"
                    flexDirection={"row"}
                    gap="10px"
                    alignItems={"center"}
                    justifyContent={"space-between"}
                    width={"98%"}
                >
                    <Label>{toTitleCase(input.inputName)}</Label>

                    <Box>
                        <Button
                            key={input.inputName}
                            value={
                                input.inputName == selectedInput
                                    ? "Press anything"
                                    : transformKeyName(input.keyCode, input.keyModifiers)
                            }
                            onClick={() => {
                                setSelectedInput(input.inputName)
                            }}
                        />
                    </Box>
                </Box>
                {Spacer(8, 0)}
                <SectionDivider />
            </>
        )
    }

    /** Select an axis between two keyboard keys */
    const KeyboardAxisSelection = () => {
        if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

        return (
            <>
                <Box
                    display="flex"
                    flexDirection={"row"}
                    gap="10px"
                    alignItems={"center"}
                    justifyContent={"space-between"}
                    width={"98%"}
                >
                    <Label>{toTitleCase(input.inputName)}</Label>

                    <Box display="flex" gap="10px" alignItems={"center"}>
                        {SynthesisIcons.Add}
                        {/* Positive key */}
                        <Button
                            key={"pos" + input.inputName}
                            value={
                                "pos" + input.inputName == selectedInput
                                    ? "Press anything"
                                    : transformKeyName(input.posKeyCode, input.posKeyModifiers)
                            }
                            onClick={() => {
                                setSelectedInput("pos" + input.inputName)
                            }}
                        />
                        {Spacer()}
                        {SynthesisIcons.Minus}
                        {/* Negative key */}
                        <Button
                            key={"neg" + input.inputName}
                            value={
                                "neg" + input.inputName == selectedInput
                                    ? "Press anything"
                                    : transformKeyName(input.negKeyCode, input.negKeyModifiers)
                            }
                            onClick={() => {
                                setSelectedInput("neg" + input.inputName)
                            }}
                        />
                    </Box>
                </Box>
                {Spacer(8, 0)}
                <SectionDivider />
            </>
        )
    }

    /** Select any button on a controller */
    const JoystickButtonSelection = () => {
        if (!(input instanceof ButtonInput)) throw new Error("Input not button type")
        return (
            <>
                <Box
                    display="flex"
                    flexDirection={"row"}
                    gap="10px"
                    alignItems={"center"}
                    justifyContent={"space-between"}
                    width={"98%"}
                >
                    <Label>{toTitleCase(input.inputName)}</Label>
                    <Button
                        key={input.inputName}
                        value={
                            input.inputName == selectedInput
                                ? "Press anything"
                                : input.gamepadButton == -1
                                  ? "N/A"
                                  : gamepadButtons[input.gamepadButton]
                        }
                        onClick={() => {
                            setSelectedInput(input.inputName)
                        }}
                    />
                </Box>
                {Spacer(8, 0)}
                <SectionDivider />
            </>
        )
    }

    /** Dropdown to select a controller axis */
    const JoystickAxisSelection = () => {
        if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

        return (
            <>
                <Box
                    display="flex"
                    flexDirection={"row"}
                    gap="10px"
                    alignItems={"center"}
                    justifyContent={"space-between"}
                    width={"98%"}
                >
                    <Label>{toTitleCase(input.inputName)}</Label>
                    <Dropdown
                        key={input.inputName}
                        defaultValue={gamepadAxes[input.gamepadAxisNumber + 1]}
                        options={gamepadAxes}
                        onSelect={value => {
                            setSelectedInput(input.inputName)
                            setChosenGamepadAxis(gamepadAxes.indexOf(value))
                        }}
                    />
                </Box>
            </>
        )
    }

    /** Select an axis between two controller buttons */
    const GamepadButtonAxisSelection = () => {
        if (!(input instanceof AxisInput)) throw new Error("Input not axis type")

        return (
            <>
                <Box
                    display="flex"
                    flexDirection={"row"}
                    gap="10px"
                    alignItems={"center"}
                    justifyContent={"space-between"}
                    width={"98%"}
                >
                    <Label>{toTitleCase(input.inputName)}</Label>

                    <Box display="flex" gap="10px" alignItems={"center"}>
                        {/* Positive gamepad button */}
                        {SynthesisIcons.Add}
                        <Button
                            key={"pos" + input.inputName}
                            value={
                                "pos" + input.inputName == selectedInput
                                    ? "Press anything"
                                    : input.posGamepadButton == -1
                                      ? "N/A"
                                      : gamepadButtons[input.posGamepadButton]
                            }
                            onClick={() => {
                                setSelectedInput("pos" + input.inputName)
                            }}
                        />
                        {Spacer()}
                        {/* // Negative gamepad button */}
                        {SynthesisIcons.Minus}
                        <Button
                            key={"neg" + input.inputName}
                            value={
                                "neg" + input.inputName == selectedInput
                                    ? "Press anything"
                                    : input.negGamepadButton == -1
                                      ? "N/A"
                                      : gamepadButtons[input.negGamepadButton]
                            }
                            onClick={() => {
                                setSelectedInput("neg" + input.inputName)
                            }}
                        />
                    </Box>
                </Box>
            </>
        )
    }

    /** Show the correct selection mode based on input type and how it's configured */
    const inputConfig = () => {
        if (!useGamepad) {
            // Keyboard button
            if (input instanceof ButtonInput) {
                return KeyboardButtonSelection()
            }
            // Keyboard Axis
            else if (input instanceof AxisInput) {
                return KeyboardAxisSelection()
            }
        } else {
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
                            defaultState={useGamepadButtons}
                            onClick={val => {
                                input.useGamepadButtons = val
                                setUseGamepadButtons(val)
                            }}
                        />
                        {/* // Button to invert the joystick axis */}
                        <Checkbox
                            label="Invert Joystick"
                            defaultState={input.joystickInverted}
                            onClick={val => {
                                input.joystickInverted = val
                            }}
                        />
                        <SectionDivider />
                    </div>
                )
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
                else if (chosenButton != -1) setChosenButton(-1)
            }
            requestAnimationFrame(checkGamepadState)
        }

        checkGamepadState()
    })

    /** Input detection for setting inputs */
    useEffect(() => {
        // // Assign keyboard inputs when a key is pressed
        if (!useGamepad && selectedInput && chosenKey) {
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
            setModifierState(EmptyModifierState)
            onInputChanged()
        }
        // Assign gamepad button inputs when a button is pressed
        else if (useGamepad && selectedInput && chosenButton != -1) {
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
        if (useGamepad && selectedInput && chosenGamepadAxis != -1) {
            if (!(input instanceof AxisInput)) return

            input.gamepadAxisNumber = chosenGamepadAxis - 1

            onInputChanged()
            setChosenGamepadAxis(-1)
            setSelectedInput("")
        }
    }, [chosenKey, chosenButton, chosenGamepadAxis, input, modifierState, onInputChanged, selectedInput, useGamepad])

    return (
        <Box
            onKeyUp={e => {
                if (selectedInput != "") setChosenKey(selectedInput ? e.code : "")
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
