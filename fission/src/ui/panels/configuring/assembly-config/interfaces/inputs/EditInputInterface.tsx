import { Box, Divider } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import {
    GamepadButtonAxisSelection,
    JoystickAxisSelection,
    JoystickButtonSelection,
    KeyboardAxisSelection,
    KeyboardButtonSelection,
    TouchControlsAxisSelection,
} from "@/panels/configuring/assembly-config/interfaces/inputs/InputSelectionComponents.tsx"
import InputSystem from "@/systems/input/InputSystem"
import { EMPTY_MODIFIER_STATE, type ModifierState } from "@/systems/input/InputTypes"
import AxisInput from "@/systems/input/inputs/AxisInput"
import ButtonInput from "@/systems/input/inputs/ButtonInput"
import type Input from "@/systems/input/inputs/Input"
import type { KeyCode } from "@/systems/input/KeyboardTypes"
import Checkbox from "@/ui/components/Checkbox"

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
    const [joystickInverted, setJoystickInverted] = useState<boolean>(
        input instanceof AxisInput ? input.joystickInverted : false
    )

    /** Show the correct selection mode based on input type and how it's configured */
    const inputConfig = () => {
        if (useGamepad) {
            // Joystick Button
            if (input instanceof ButtonInput) {
                return (
                    <JoystickButtonSelection
                        input={input}
                        selectedInput={selectedInput}
                        setSelectedInput={setSelectedInput}
                    />
                )
            }

            // Gamepad axis
            else if (input instanceof AxisInput) {
                return (
                    <div key={input.inputName}>
                        {input.useGamepadButtons ? (
                            <GamepadButtonAxisSelection
                                input={input}
                                selectedInput={selectedInput}
                                setSelectedInput={setSelectedInput}
                            />
                        ) : (
                            // Gamepad joystick axis
                            <JoystickAxisSelection
                                input={input}
                                selectedInput={selectedInput}
                                setSelectedInput={setSelectedInput}
                                setChosenGamepadAxis={setChosenGamepadAxis}
                            />
                        )}

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
                            checked={joystickInverted}
                            onClick={checked => {
                                input.joystickInverted = checked
                                setJoystickInverted(checked)
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
                        <TouchControlsAxisSelection
                            input={input}
                            selectedInput={selectedInput}
                            setSelectedInput={setSelectedInput}
                            setChosenTouchControlsAxis={setChosenTouchControlsAxis}
                        />
                        {/* // Button to invert the joystick axis */}
                        <Checkbox
                            label="Invert Joystick"
                            checked={joystickInverted}
                            onClick={checked => {
                                input.joystickInverted = checked
                                setJoystickInverted(checked)
                            }}
                        />
                        <Divider />
                    </div>
                )
            }
        } else {
            // Keyboard button
            if (input instanceof ButtonInput) {
                return KeyboardButtonSelection({ input, setSelectedInput, selectedInput })
            }
            // Keyboard Axis
            else if (input instanceof AxisInput) {
                return KeyboardAxisSelection({ input, setSelectedInput, selectedInput })
            }
        }
    }

    useEffect(() => {
        let frameId: number
        let wasPressedLastFrame = false

        const checkGamepadState = () => {
            const activeGamepads = InputSystem.getConnectedGamepads()
            let currentlyPressedButton = -1

            for (const gamepad of activeGamepads) {
                const pressedIndex = gamepad.buttons.findIndex(button => button.pressed)
                if (pressedIndex !== -1) {
                    currentlyPressedButton = pressedIndex
                    break
                }
            }
            if (currentlyPressedButton !== -1) {
                if (!wasPressedLastFrame) {
                    setChosenButton(currentlyPressedButton)
                    wasPressedLastFrame = true
                }
            } else {
                wasPressedLastFrame = false
            }

            frameId = requestAnimationFrame(checkGamepadState)
        }

        frameId = requestAnimationFrame(checkGamepadState)
        return () => cancelAnimationFrame(frameId)
    }, [])

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
