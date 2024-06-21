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

    return prefix +  keyCodeToCharacter(keyCode);
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
    "Backslash": "\\", //TODO
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

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [useGamepad, setUseGamepad] = useState<boolean>(InputSystem.useGamepad)
    const [loadedRobot, setLoadedRobot] = useState<string>("")
    const [selectedInput, setSelectedInput] = useState<string>("")

    const [chosenButton, setChosenButton] = useState<string>("")
    const [modifierState, setModifierState] = useState<ModifierState>(emptyModifierState)

    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)

    useEffect(() => {
        // TODO: use the actual loaded robot(s)
        setTimeout(() => setLoadedRobot("Dozer v9"), 1)
    })

    if (selectedInput && chosenButton) {
        if (selectedInput.startsWith("pos")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posKeyCode = chosenButton;
            input.posKeyModifiers = modifierState;
        }
        else if (selectedInput.startsWith("neg")) {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posKeyCode = chosenButton;
            input.posKeyModifiers = modifierState;
        }
        else {
            const input = InputSystem.allInputs.find(input => input.inputName == selectedInput) as ButtonInput;

            input.keyCode = chosenButton
            input.keyModifiers = modifierState
        }

        setChosenButton("")
        setSelectedInput("")
        setModifierState(emptyModifierState)
    }

    if (selectedInput && chosenGamepadAxis != -1) {
        // TODO: assumes all inputs are keyboard buttons
        const selected = InputSystem.allInputs.find(input => input.inputName == selectedInput) as AxisInput
        selected.gamepadAxisNumber = chosenGamepadAxis;

        setChosenGamepadAxis(-1)
        setSelectedInput("")
    }

    useEffect(() => {
        const checkGamepadState = () => {
          if (useGamepad && InputSystem._gpIndex !== null) {
            const gamepad = navigator.getGamepads()[InputSystem._gpIndex];
            if (gamepad) {
              const pressedButtons = gamepad.buttons
                .map((button, index) => (button.pressed ? index : null))
                .filter((index) => index !== null)
                .map((index) => index!);

                if (pressedButtons.length > 0)
                    setChosenButton("Gamepad " + pressedButtons[0]);
                else setChosenButton("");
            }
          }
          else setChosenButton("");
          requestAnimationFrame(checkGamepadState);
        };
    
        checkGamepadState();
      }, [InputSystem._gpIndex]);

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
                        setChosenButton(selectedInput ? e.code : "")
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
                                {Object.values(InputSystem.robotInputs).map(c => {
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
        
                                                            // TODO: assumes all inputs are keyboard buttons
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
            
                                                                // TODO: assumes all inputs are keyboard buttons
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
            
                                                                // TODO: assumes all inputs are keyboard buttons
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
        
                                                            // TODO: assumes all inputs are keyboard buttons
                                                            : (c.gamepadButton == -1) ? "" : gamepadButtons[c.gamepadButton]
                                                    }
                                                    onClick={() => {
                                                        setSelectedInput(c.inputName)
                                                    }}
                                                />
                                         }

                                        // Joystick Axis
                                        else if (c instanceof AxisInput) {
                                            return (
                                                <Dropdown
                                                    key={c.inputName}
                                                    label={toTitleCase(c.inputName)}
                                                    // Moves the selected option to the start of the array
                                                    options={gamepadAxes.sort(function(x,y){ return x == gamepadAxes[c.gamepadAxisNumber] ? -1 : y == gamepadAxes[c.gamepadAxisNumber] ? 1 : 0; })}
                                                    onSelect={(value) => {
                                                        setSelectedInput(c.inputName); 
                                                        setChosenGamepadAxis(gamepadAxes.indexOf(value));
                                                    }}
                                                />
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
                        setChosenButton(selectedInput ? e.key : "")
                        setModifierState({
                            ctrl: e.ctrlKey,
                            alt: e.altKey,
                            shift: e.shiftKey,
                            meta: e.metaKey,
                        })
                    }}
                >
                    <Label size={LabelSize.Large}>Global Controls</Label>

                    {/* {Object.values(InputSystem.globalInputs).map(c => {
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