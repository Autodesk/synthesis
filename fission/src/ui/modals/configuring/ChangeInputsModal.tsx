import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGamepad } from "react-icons/fa6"
import Stack, { StackDirection } from "@/ui/components/Stack"
import Label, { LabelSize } from "@/ui/components/Label"
import LabeledButton, { LabelPlacement } from "../../components/LabeledButton"
import InputSystem, { AxisInput, ButtonInput, ModifierState, EmptyModifierState } from "@/systems/input/InputSystem"
import Dropdown from "@/ui/components/Dropdown"
import Checkbox from "@/ui/components/Checkbox"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import Button from "@/ui/components/Button"
import { useModalControlContext } from "@/ui/ModalContext"
import { Box, Divider, styled } from "@mui/material"
import { AiOutlinePlus } from "react-icons/ai"

const AddIcon = <AiOutlinePlus size={"1.25rem"} />

const DividerStyled = styled(Divider)({
    borderColor: "white",
})

// capitalize first letter
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

const moveElementToTop = (arr: string[], element: string | undefined) => {
    if (element == undefined) {
        return arr
    }

    arr = arr.includes(element) ? [element, ...arr.filter(item => item !== element)] : arr
    return arr
}

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [useGamepad, setUseGamepad] = useState<boolean>(false)

    const [selectedInput, setSelectedInput] = useState<string>("")

    const [chosenKey, setChosenKey] = useState<string>("")
    const [chosenButton, setChosenButton] = useState<number>(-1)
    const [modifierState, setModifierState] = useState<ModifierState>(EmptyModifierState)

    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)
    const [useButtons, setUseButtons] = useState<UseButtonsState>({})

    // If there is a robot spawned, set it as the selected robot
    if (selectedScheme == null && InputSchemeManager.allInputSchemes.length > 0) {
        setTimeout(() => {
            if (!InputSystem.selectedScheme) {
                InputSystem.selectedScheme = InputSchemeManager.allInputSchemes[0]
            }

            setUseButtons({})
            setSelectedScheme(InputSystem.selectedScheme)
            setUseGamepad(InputSystem.selectedScheme?.usesGamepad ?? false)
        }, 1)
    }

    const axisInputs = selectedScheme?.inputs.filter((input): input is AxisInput => input instanceof AxisInput)

    // Stores the states for using buttons or a joystick axis for gamepad axis inputs
    type UseButtonsState = {
        [key: string]: boolean
    }

    if (axisInputs && Object.keys(useButtons).length == 0) {
        axisInputs.forEach(input => {
            useButtons[input.inputName] = input.useGamepadButtons
        })
    }

    // Assign keyboard inputs when a key is pressed
    if (!useGamepad && selectedInput && chosenKey && selectedScheme) {
        if (selectedInput.startsWith("pos")) {
            const input = selectedScheme?.inputs.find(
                input => input.inputName == selectedInput.substring(3)
            ) as AxisInput
            input.posKeyCode = chosenKey
            input.posKeyModifiers = modifierState
        } else if (selectedInput.startsWith("neg")) {
            const input = selectedScheme?.inputs.find(
                input => input.inputName == selectedInput.substring(3)
            ) as AxisInput
            input.negKeyCode = chosenKey
            input.negKeyModifiers = modifierState
        } else {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput) as ButtonInput
            input.keyCode = chosenKey
            input.keyModifiers = modifierState
        }

        selectedScheme.customized = true

        setChosenKey("")
        setSelectedInput("")
        setModifierState(EmptyModifierState)
    }
    // Assign gamepad button inputs when a button is pressed
    else if (selectedScheme && useGamepad && selectedInput && chosenButton != -1) {
        if (selectedInput.startsWith("pos")) {
            const input = selectedScheme?.inputs.find(
                input => input.inputName == selectedInput.substring(3)
            ) as AxisInput
            input.posGamepadButton = chosenButton
        } else if (selectedInput.startsWith("neg")) {
            const input = selectedScheme?.inputs.find(
                input => input.inputName == selectedInput.substring(3)
            ) as AxisInput
            input.negGamepadButton = chosenButton
        } else {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput) as ButtonInput

            input.gamepadButton = chosenButton
        }

        selectedScheme.customized = true

        setChosenButton(-1)
        setSelectedInput("")
    }

    // Assign gamepad axis inputs when a gamepad axis is selected
    if (useGamepad && selectedInput && chosenGamepadAxis != -1 && selectedScheme) {
        const selected = selectedScheme?.inputs.find(input => input.inputName == selectedInput) as AxisInput
        selected.gamepadAxisNumber = chosenGamepadAxis - 1

        selectedScheme.customized = true

        setChosenGamepadAxis(-1)
        setSelectedInput("")
    }

    useEffect(() => {
        const checkGamepadState = () => {
            if (InputSystem.gamepad !== null) {
                const pressedButtons = InputSystem.gamepad.buttons
                    .map((button, index) => (button.pressed ? index : null))
                    .filter(index => index !== null)
                    .map(index => index!)

                if (pressedButtons.length > 0) setChosenButton(pressedButtons[0])
                else setChosenButton(-1)
            }
            requestAnimationFrame(checkGamepadState)
        }

        checkGamepadState()
    })

    const KeyboardButtonSelection = (c: ButtonInput) => {
        return (
            <LabeledButton
                key={c.inputName}
                label={toTitleCase(c.inputName)}
                placement={LabelPlacement.Left}
                value={c.inputName == selectedInput ? "Press anything" : transformKeyName(c.keyCode, c.keyModifiers)}
                onClick={() => {
                    setSelectedInput(c.inputName)
                }}
            />
        )
    }

    const KeyboardAxisSelection = (c: AxisInput) => {
        return (
            <div key={c.inputName}>
                <Stack direction={StackDirection.Vertical} spacing={3}>
                    {/* Positive key */}
                    <Label size={LabelSize.Medium}>{toTitleCase(c.inputName)}</Label>
                    <LabeledButton
                        key={"pos" + c.inputName}
                        label={"(+)"}
                        placement={LabelPlacement.Left}
                        value={
                            "pos" + c.inputName == selectedInput
                                ? "Press anything"
                                : transformKeyName(c.posKeyCode, c.posKeyModifiers)
                        }
                        onClick={() => {
                            setSelectedInput("pos" + c.inputName)
                        }}
                    />
                    {/* Negative key */}
                    <LabeledButton
                        key={"neg" + c.inputName}
                        label={"(-)"}
                        placement={LabelPlacement.Left}
                        value={
                            "neg" + c.inputName == selectedInput
                                ? "Press anything"
                                : transformKeyName(c.negKeyCode, c.negKeyModifiers)
                        }
                        onClick={() => {
                            setSelectedInput("neg" + c.inputName)
                        }}
                    />
                </Stack>
            </div>
        )
    }

    const JoystickButtonSelection = (c: ButtonInput) => {
        return (
            <LabeledButton
                key={c.inputName}
                label={toTitleCase(c.inputName)}
                placement={LabelPlacement.Left}
                value={
                    c.inputName == selectedInput
                        ? "Press anything"
                        : c.gamepadButton == -1
                          ? "N/A"
                          : gamepadButtons[c.gamepadButton]
                }
                onClick={() => {
                    setSelectedInput(c.inputName)
                }}
            />
        )
    }

    const JoystickAxisSelection = (c: AxisInput) => {
        return (
            <Dropdown
                key={c.inputName}
                label={toTitleCase(c.inputName)}
                // Moves the selected option to the start of the array
                options={moveElementToTop(gamepadAxes, gamepadAxes[c.gamepadAxisNumber + 1])}
                onSelect={value => {
                    setSelectedInput(c.inputName)
                    setChosenGamepadAxis(gamepadAxes.indexOf(value))
                }}
            />
        )
    }

    const GamepadButtonAxisSelection = (c: AxisInput) => {
        return (
            <div>
                <Stack direction={StackDirection.Vertical} spacing={3}>
                    <Label size={LabelSize.Medium}>{toTitleCase(c.inputName)}</Label>
                    {/* // Positive gamepad button */}
                    <LabeledButton
                        key={"pos" + c.inputName}
                        label={"(+)"}
                        placement={LabelPlacement.Left}
                        value={
                            "pos" + c.inputName == selectedInput
                                ? "Press anything"
                                : c.posGamepadButton == -1
                                  ? "N/A"
                                  : gamepadButtons[c.posGamepadButton]
                        }
                        onClick={() => {
                            setSelectedInput("pos" + c.inputName)
                        }}
                    />
                    {/* // Negative gamepad button */}
                    <LabeledButton
                        key={"neg" + c.inputName}
                        label={"(-)"}
                        placement={LabelPlacement.Left}
                        value={
                            "neg" + c.inputName == selectedInput
                                ? "Press anything"
                                : c.negGamepadButton == -1
                                  ? "N/A"
                                  : gamepadButtons[c.negGamepadButton]
                        }
                        onClick={() => {
                            setSelectedInput("neg" + c.inputName)
                        }}
                    />
                </Stack>
            </div>
        )
    }

    return (
        <Modal
            name="Keybinds"
            icon={<FaGamepad />}
            modalId={modalId}
            onAccept={() => {
                InputSchemeManager.saveSchemes()
                InputSystem.selectedScheme = undefined
            }}
        >
            <>
                <Stack direction={StackDirection.Horizontal} spacing={25}>
                    <div>
                        <Stack direction={StackDirection.Vertical} spacing={10}>
                            <Dropdown
                                label={"Select Robot"}
                                // Moves the selected option to the start of the array
                                options={moveElementToTop(
                                    InputSchemeManager.allInputSchemes.map(s => s.schemeName),
                                    InputSystem?.selectedScheme?.schemeName
                                )}
                                onSelect={value => {
                                    const schemeData = InputSchemeManager.allInputSchemes.find(
                                        s => s.schemeName == value
                                    )
                                    if (!schemeData || schemeData == selectedScheme) return

                                    setSelectedScheme(undefined)
                                    InputSystem.selectedScheme = schemeData
                                }}
                            />
                            <Button
                                value={AddIcon}
                                onClick={() => {
                                    openModal("new-scheme")
                                }}
                            />

                            {selectedScheme ? (
                                <>
                                    <Checkbox
                                        label="Use Controller"
                                        defaultState={selectedScheme?.usesGamepad ?? false}
                                        onClick={val => {
                                            setUseGamepad(val)
                                            if (selectedScheme) selectedScheme.usesGamepad = val
                                        }}
                                    />
                                    <Box height={10} />
                                    <DividerStyled />
                                    <Box height={15}></Box>

                                    <Box display="flex" justifyContent="center" alignItems="center">
                                        <Button
                                            value={"Reset all to Defaults"}
                                            onClick={() => {
                                                openModal("reset-inputs")
                                            }}
                                        />
                                    </Box>
                                </>
                            ) : (
                                <Label>No robot selected.</Label>
                            )}
                        </Stack>
                    </div>
                    <div
                        className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[60vh] bg-background-secondary rounded-md p-2"
                        onKeyUp={e => {
                            setChosenKey(selectedInput ? e.code : "")
                            setModifierState({
                                ctrl: e.ctrlKey,
                                alt: e.altKey,
                                shift: e.shiftKey,
                                meta: e.metaKey,
                            })
                        }}
                    >
                        {selectedScheme ? (
                            <>
                                <Stack direction={StackDirection.Vertical} spacing={20}>
                                    {selectedScheme.inputs.map(c => {
                                        if (!useGamepad) {
                                            // Keyboard button
                                            if (c instanceof ButtonInput) {
                                                return KeyboardButtonSelection(c)
                                            }
                                            // Keyboard Axis
                                            else if (c instanceof AxisInput) {
                                                return KeyboardAxisSelection(c)
                                            }
                                        } else {
                                            // Joystick Button
                                            if (c instanceof ButtonInput) {
                                                return JoystickButtonSelection(c)
                                            }

                                            // Gamepad axis
                                            else if (c instanceof AxisInput) {
                                                return (
                                                    <div key={c.inputName}>
                                                        {useButtons[c.inputName]
                                                            ? GamepadButtonAxisSelection(c)
                                                            : // Gamepad joystick axis
                                                              JoystickAxisSelection(c)}

                                                        {/* // Button to switch between two buttons and a joystick axis */}
                                                        <Checkbox
                                                            label="Use Buttons"
                                                            defaultState={c.useGamepadButtons}
                                                            onClick={val => {
                                                                setUseButtons(prevState => ({
                                                                    ...prevState,
                                                                    [c.inputName]: val,
                                                                }))
                                                                c.useGamepadButtons = val
                                                            }}
                                                        />
                                                        {/* // Button to invert the joystick axis */}
                                                        <Checkbox
                                                            label="Joystick Inverted"
                                                            defaultState={c.joystickInverted}
                                                            onClick={val => {
                                                                c.joystickInverted = val
                                                            }}
                                                        />
                                                    </div>
                                                )
                                            }
                                        }
                                    })}
                                </Stack>
                            </>
                        ) : (
                            <Label>No robot selected.</Label>
                        )}
                    </div>
                </Stack>
            </>
        </Modal>
    )
}

export default ChangeInputsModal
