import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGamepad } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Label, { LabelSize } from "../../components/Label"
import LabeledButton, { LabelPlacement } from "../../components/LabeledButton"
import InputSystem, { AxisInput, ButtonInput, ModifierState, emptyModifierState } from "@/systems/input/InputSystem"
import Dropdown from "@/ui/components/Dropdown"
import Checkbox from "@/ui/components/Checkbox"

// capitalize first letter
// TODO: assumes all inputs are keyboard buttons
const transformKeyName = (keyCode: string, keyModifiers: ModifierState) => {
    let prefix = ""
    if (keyModifiers) {
        if (keyModifiers.meta) prefix += "Meta + "
        if (keyModifiers.shift) prefix += "Shift + "
        if (keyModifiers.ctrl) prefix += "Ctrl + "
        if (keyModifiers.alt) prefix += "Alt + "
    }

    const displayName = prefix +  keyCodeToCharacter(keyCode)
    if (displayName == "") return "N/A";
    return displayName;
}
    
// Converts camelCase to Title Case for the inputs modal
const toTitleCase = (camelCase: string) => {
const result = camelCase.replace(/([A-Z])/g, " $1");
const finalResult = result.charAt(0).toUpperCase() + result.slice(1);
    return finalResult;
}

const codeToCharacterMap: { [code: string]: string } = {
    "Slash": "/",
    "Comma": ",",
    "Period": ".",
    "BracketLeft": "{",
    "BracketRight": "}",
    "BackQuote": "`",
    "Minus": "-",
    "Equal": "=",
    "Backslash": "\\",
    "Semicolon": ";",
    "Quote": "\""
};

const gamepadButtons: string[] = [ "A", "B", "X", "Y",  "Left Bumper", "Right Bumper", "Back", "Start", "Left Stick", "Right Stick", "UNKNOWN", "UNKNOWN2", "Dpad Up", "Dpad Down", "Dpad Left", "Dpad Right"];
const gamepadAxes: string[] = ["Left X", "Left Y", /* for some reason triggers aren't registering??? "LeftTrigger", "RightTrigger", */ "Right X", "Right Y"];

// Converts a key code to displayable character (ex: KeyA -> "A")
const keyCodeToCharacter = (code: string) => {
    if (code.startsWith("Key"))
        return code.charAt(3);

    if (code.startsWith("Digit"))
        return code.charAt(5);

    if (code in codeToCharacterMap)
        return codeToCharacterMap[code];

    if (code.startsWith("Gamepad"))
        return gamepadButtons[parseInt(code.substring(8))];

    return code;
}

//var chosenButton: number = -1;

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [useGamepad, setUseGamepad] = useState<boolean>(InputSystem.useGamepad)
    const [loadedRobot, setLoadedRobot] = useState<string>("")
    const [selectedInput, setSelectedInput] = useState<string>("")

    const [chosenKey, setChosenKey] = useState<string>("")
    const [chosenButton, setChosenButton] = useState<number>(-1)
    const [modifierState, setModifierState] = useState<ModifierState>(emptyModifierState)

    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)

    const axisInputs = InputSystem.allInputs.filter((input): input is AxisInput => input instanceof AxisInput);

    // TODO: try removing this, might not be needed
    type UseButtonsState = {
        [key: string]: boolean;
    };
    
    const [useButtons, setUseButtons] = useState<UseButtonsState>(
        axisInputs.reduce<UseButtonsState>((acc, input) => {
            acc[input.inputName] = input.useJoystickButtons;
            return acc;
        }, {})
    );

    useEffect(() => {
        // TODO: use the actual loaded robot(s)
        setTimeout(() => setLoadedRobot("Dozer v9"), 1)
    })

    if (!useGamepad && selectedInput && chosenKey) {
        if (selectedInput.startsWith("pos")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posKeyCode = chosenKey;
            input.posKeyModifiers = modifierState;
        }
        else if (selectedInput.startsWith("neg")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.negKeyCode = chosenKey;
            input.negKeyModifiers = modifierState;
        }
        else {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput) as ButtonInput;

            input.keyCode = chosenKey
            input.keyModifiers = modifierState
        }

        setChosenKey("")
        setSelectedInput("")
        setModifierState(emptyModifierState)
    }
    else if (useGamepad && selectedInput && chosenButton != -1) {
        if (selectedInput.startsWith("pos")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posJoystickButton = chosenButton;
        }
        else if (selectedInput.startsWith("neg")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.negJoystickButton = chosenButton;
        }
        else {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput) as ButtonInput;

            input.gamepadButton = chosenButton
        }

        setChosenButton(-1);
        setSelectedInput("")
    }

    if (useGamepad && selectedInput && chosenGamepadAxis != -1) {
        const selected = InputSystem.allInputs.find(input => input.inputName == selectedInput) as AxisInput
        selected.gamepadAxisNumber = chosenGamepadAxis;

        setChosenGamepadAxis(-1)
        setSelectedInput("")
    }

      useEffect(() => {
        const checkGamepadState = () => {
          if (InputSystem.gamepad !== null) {
              const pressedButtons = InputSystem.gamepad.buttons
                .map((button, index) => (button.pressed ? index : null))
                .filter((index) => index !== null)
                .map((index) => index!);

                if (pressedButtons.length > 0)
                    setChosenButton(pressedButtons[0]);
                else setChosenButton(-1);
            }
          requestAnimationFrame(checkGamepadState);
        };

        checkGamepadState();
      });

    return (
        <Modal name="Keybinds" icon={<FaGamepad />} modalId={modalId}>
            <Checkbox label="Gamepad Controller" defaultState={InputSystem.useGamepad} onClick={ (val) => {
                        setUseGamepad(val);
                        InputSystem.useGamepad = val;
                    }
                } 
            />
            <Stack direction={StackDirection.Horizontal}>
                <div
                    className="w-max"
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
                    {loadedRobot ? (
                        <>
                            <Label size={LabelSize.Large}>Robot Controls</Label>
                                {InputSystem.robotInputs.map(c => {
                                    if (!useGamepad) {

                                        // Keyboard button
                                        if (c instanceof ButtonInput) {
                                            return (
                                                <LabeledButton
                                                    key={c.inputName}
                                                    label={toTitleCase(c.inputName)}
                                                    placement={LabelPlacement.Left}
                                                    value={
                                                        c.inputName == selectedInput
                                                            ? "Press anything"
                                                                : transformKeyName(c.keyCode, c.keyModifiers)
                                                    }
                                                    onClick={() => {
                                                        setSelectedInput(c.inputName)
                                                    }}
                                                />
                                            )
                                        }

                                        // Keyboard Axis
                                        else if (c instanceof AxisInput) {
                                            return ( 
                                                <div key={c.inputName}>
                                                    <LabeledButton
                                                        key={"pos"+c.inputName}
                                                        label={toTitleCase(c.inputName) + " (+)"}
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
                                                    <LabeledButton
                                                        key={"neg" + c.inputName}
                                                        label={toTitleCase(c.inputName) + " (-)"}
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
                                                </div>
                                            )
                                        }
                                    }
                                    else {
                                        // Joystick Button
                                        if (c instanceof ButtonInput) {
                                            <LabeledButton
                                                    key={c.inputName}
                                                    label={toTitleCase(c.inputName)}
                                                    placement={LabelPlacement.Left}
                                                    value={
                                                        c.inputName == selectedInput
                                                            ? "Press anything"
        
                                                            : (c.gamepadButton == -1) ? "N/A" : gamepadButtons[c.gamepadButton]
                                                    }
                                                    onClick={() => {
                                                        setSelectedInput(c.inputName)
                                                    }}
                                                />
                                         }

                                        // Joystick Axis
                                        else if (c instanceof AxisInput) {
                                            return (
                                                <div key={c.inputName}>
                                                    {useButtons[c.inputName] ? (
                                                        <div>
                                                            <LabeledButton
                                                        key={"pos"+c.inputName}
                                                        label={toTitleCase(c.inputName) + " (+)"}
                                                        placement={LabelPlacement.Left}
                                                        value={
                                                            "pos" + c.inputName == selectedInput
                                                                ? "Press anything"
                                                                : (c.posJoystickButton == -1) ? "N/A" : gamepadButtons[c.posJoystickButton]
                                                        }
                                                        onClick={() => {
                                                            setSelectedInput("pos" + c.inputName)
                                                        }}
                                                    />
                                                        <LabeledButton
                                                        key={"neg"+c.inputName}
                                                        label={toTitleCase(c.inputName) + " (-)"}
                                                        placement={LabelPlacement.Left}
                                                        value={
                                                            "neg" + c.inputName == selectedInput
                                                                ? "Press anything"
                                                                : (c.negJoystickButton == -1) ? "N/A" : gamepadButtons[c.negJoystickButton]
                                                        }
                                                        onClick={() => {
                                                            setSelectedInput("neg" + c.inputName)
                                                        }}
                                                    />
                                                        </div>
                                                    ) : (
                                                        <Dropdown
                                                            key={c.inputName}
                                                            label={toTitleCase(c.inputName)}
                                                            // Moves the selected option to the start of the array
                                                            options={gamepadAxes.sort(function(x, y) { return x === gamepadAxes[c.gamepadAxisNumber] ? -1 : y === gamepadAxes[c.gamepadAxisNumber] ? 1 : 0; })}
                                                            onSelect={(value) => {
                                                                setSelectedInput(c.inputName); 
                                                                setChosenGamepadAxis(gamepadAxes.indexOf(value));
                                                            }}
                                                        />
                                                    )}
                                                    <Checkbox label="Use Buttons" defaultState={c.useJoystickButtons} onClick={ 
                                                        (val) => {
                                                            setUseButtons(prevState => ({
                                                                ...prevState,
                                                                [c.inputName]: val
                                                            }));
                                                            c.useJoystickButtons = val;
                                                    }}/>
                                                    <Checkbox label="Inverted" defaultState={c.joystickInverted} onClick={ 
                                                        (val) => {
                                                            c.joystickInverted = val;
                                                    }}/>
                                                </div>
                                            )
                                        }
                                    }
                                }
                            )}
                        </>
                    ) : (
                        <Label>No robot loaded.</Label>
                    )}
                </div>
                <div
                    className="w-max"
                    onKeyUp={e => {
                        setChosenKey(selectedInput ? e.key : "")
                        setModifierState({
                            ctrl: e.ctrlKey,
                            alt: e.altKey,
                            shift: e.shiftKey,
                            meta: e.metaKey,
                        })
                    }}
                >
                    <Label size={LabelSize.Large}>Global Controls</Label>

                    {/* {InputSystem.globalInputs.map(c => {
                    if (!useGamepad) {

                        // Keyboard button
                        return (
                            <LabeledButton
                                key={c.inputName}
                                label={toTitleCase(c.inputName)}
                                placement={LabelPlacement.Left}
                                value={
                                    c.inputName == selectedInput
                                        ? "Press anything"
                                        // TODO: assumes all inputs are keyboard buttons
                                        : transformKeyName(c as ButtonInput)
                                }
                                onClick={() => {
                                    setSelectedInput(c.inputName)
                                }}
                            />
                        )
                }
                }
                    )} */}
                </div>
            </Stack>
        </Modal>
    )
}

export default ChangeInputsModal