import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGamepad } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Label, { LabelSize } from "../../components/Label"
import LabeledButton, { LabelPlacement } from "../../components/LabeledButton"
import InputSystem, { AxisInput, ButtonInput, ModifierState, EmptyModifierState } from "@/systems/input/InputSystem"
import Dropdown from "@/ui/components/Dropdown"
import Checkbox from "@/ui/components/Checkbox"
import DefaultInputs, { InputScheme } from "@/systems/input/DefaultInputs"
import Button from "@/ui/components/Button"

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
const gamepadAxes: string[] = ["N/A", "Left X", "Left Y", "Right X", "Right Y"];

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

const moveElementToTop = (arr: string[], element: string) => {
    if (element == null) {
        return arr;
    }
    
    arr = arr.includes(element) ? [element, ...arr.filter(item => item !== element)] : arr;
    return arr;
};

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined);
    const [useGamepad, setUseGamepad] = useState<boolean>(false)

    const [selectedInput, setSelectedInput] = useState<string>("")

    const [chosenKey, setChosenKey] = useState<string>("")
    const [chosenButton, setChosenButton] = useState<number>(-1)
    const [modifierState, setModifierState] = useState<ModifierState>(EmptyModifierState)

    const [chosenGamepadAxis, setChosenGamepadAxis] = useState<number>(-1)
    const [chosenResetScheme, setChosenResetScheme] = useState<string>("WASD")
    
    // If there is a robot spawned, set it as the selected robot
    if (selectedScheme == null && InputSystem.allInputs.size > 0) {
        setTimeout(() => {
            if (!InputSystem.selectedScheme)
                InputSystem.selectedScheme = InputSystem.allInputs.values().next().value

            setSelectedScheme(InputSystem.selectedScheme);
            setUseGamepad(InputSystem.selectedScheme?.usesGamepad ?? false);
        }, 1);
    }

    const axisInputs = selectedScheme?.inputs.filter((input): input is AxisInput => input instanceof AxisInput);

    // TODO: try removing this, might not be needed
    type UseButtonsState = {
        [key: string]: boolean;
    };

    const [useButtons, setUseButtons] = useState<UseButtonsState>({})

    if (axisInputs && Object.keys(useButtons).length == 0) {
        axisInputs.forEach(input => {
            useButtons[input.inputName] = input.useGamepadButtons;
        });
    }

    if (!useGamepad && selectedInput && chosenKey) {
        if (selectedInput.startsWith("pos")) {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posKeyCode = chosenKey;
            input.posKeyModifiers = modifierState;
        }
        else if (selectedInput.startsWith("neg")) {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.negKeyCode = chosenKey;
            input.negKeyModifiers = modifierState;
        }
        else {
            const input =selectedScheme?.inputs.find(input => input.inputName == selectedInput) as ButtonInput;
            input.keyCode = chosenKey
            input.keyModifiers = modifierState
        }

        setChosenKey("")
        setSelectedInput("")
        setModifierState(EmptyModifierState)
    }
    else if (useGamepad && selectedInput && chosenButton != -1) {
        if (selectedInput.startsWith("pos")) {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.posGamepadButton = chosenButton;
        }
        else if (selectedInput.startsWith("neg")) {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput.substring(3)) as AxisInput;
            input.negGamepadButton = chosenButton;
        }
        else {
            const input = selectedScheme?.inputs.find(input => input.inputName == selectedInput) as ButtonInput;

            input.gamepadButton = chosenButton
        }

        setChosenButton(-1);
        setSelectedInput("")
    }

    if (useGamepad && selectedInput && chosenGamepadAxis != -1) {
        const selected = selectedScheme?.inputs.find(input => input.inputName == selectedInput) as AxisInput
        selected.gamepadAxisNumber = chosenGamepadAxis - 1;

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
           {(Array.from(InputSystem.allInputs.keys()).length > 0) ? (
                        <>
            <Stack direction={StackDirection.Horizontal}>
                <div>
                        <Dropdown
                        label={"Select Robot"}
                        // Moves the selected option to the start of the array
                        options={Array.from(InputSystem.allInputs.keys())}
                        onSelect={(value) => {
                            const newScheme = InputSystem.allInputs.get(value);
                            if (newScheme == selectedScheme)
                                return;

                            setSelectedScheme(undefined);
                            InputSystem.selectedScheme = newScheme;
                        }}
                    />
                    {selectedScheme ? (
                        <>
                    <Checkbox label="Gamepad Controller" defaultState={selectedScheme?.usesGamepad ?? false} onClick={ (val) => {
                                setUseGamepad(val);
                                if (selectedScheme)
                                    selectedScheme.usesGamepad = val;
                            }
                        } 
                    />
                    <Label size = {LabelSize.Medium}>Reset to Defaults</Label>
                    <Stack direction={StackDirection.Horizontal}>
                        <div>
                        <Dropdown
                            label={"Choose Scheme"}
                            // Moves the selected option to the start of the array
                            options={DefaultInputs.ALL_INPUT_SCHEMES.map((scheme) => scheme.schemeName)}
                            onSelect={(value) => {
                                setChosenResetScheme(value);
                            }}
                        />
                        </div>
                        <div>
                        <Button
                            value={
                                "Set"
                            }
                            onClick={() => {
                                const scheme = DefaultInputs.ALL_INPUT_SCHEMES.find(s => s.schemeName == chosenResetScheme);
                                if (!selectedScheme || !scheme)
                                    return;

                                scheme.inputs.forEach(newInput => {
                                    const currentInput = selectedScheme.inputs.find(i => i.inputName == newInput.inputName);

                                    if (currentInput) {
                                        const inputIndex = selectedScheme.inputs.indexOf(currentInput);

                                        selectedScheme.inputs.splice(inputIndex, 1);
                                        selectedScheme.inputs.push(newInput.getCopy());
                                    }
                                })
                                selectedScheme.usesGamepad = scheme.usesGamepad;

                                setSelectedScheme(undefined);
                            }}
                        />
                        </div>
                        </Stack>
                        </>
                    ) : (
                        <Label>No robot selected.</Label>
                    )}
                </div>
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
                    {selectedScheme ? (
                        <>
                            <Label size={LabelSize.Large}>Robot Controls</Label>
                                {selectedScheme.inputs.map(c => {
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
                                                    {/* Positive key */}
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
                                                    {/* Negative key */}
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

                                        // Gamepad axis
                                        else if (c instanceof AxisInput) {
                                            return (
                                                <div key={c.inputName}>
                                                    {useButtons[c.inputName] ? (
                                                        <div>
                                                        {/* // Positive gamepad button */}
                                                            <LabeledButton
                                                        key={"pos"+c.inputName}
                                                        label={toTitleCase(c.inputName) + " (+)"}
                                                        placement={LabelPlacement.Left}
                                                        value={
                                                            "pos" + c.inputName == selectedInput
                                                                ? "Press anything"
                                                                : (c.posGamepadButton == -1) ? "N/A" : gamepadButtons[c.posGamepadButton]
                                                        }
                                                        onClick={() => {
                                                            setSelectedInput("pos" + c.inputName)
                                                        }}
                                                    />
                                                        {/* // Negative gamepad button */}
                                                        <LabeledButton
                                                        key={"neg"+c.inputName}
                                                        label={toTitleCase(c.inputName) + " (-)"}
                                                        placement={LabelPlacement.Left}
                                                        value={
                                                            "neg" + c.inputName == selectedInput
                                                                ? "Press anything"
                                                                : (c.negGamepadButton == -1) ? "N/A" : gamepadButtons[c.negGamepadButton]
                                                        }
                                                        onClick={() => {
                                                            setSelectedInput("neg" + c.inputName)
                                                        }}
                                                    />
                                                        </div>
                                                    ) : (
                                                        // Gamepad joystick axis
                                                        <Dropdown
                                                            key={c.inputName}
                                                            label={toTitleCase(c.inputName)}
                                                            // Moves the selected option to the start of the array
                                                            options={moveElementToTop(gamepadAxes, gamepadAxes[c.gamepadAxisNumber + 1])}
                                                            onSelect={(value) => {
                                                                setSelectedInput(c.inputName); 
                                                                setChosenGamepadAxis(gamepadAxes.indexOf(value));
                                                            }}
                                                        />
                                                    )}
                                                    {/* // Button to switch between two buttons and a joystick axis */}
                                                    <Checkbox label="Use Buttons" defaultState={c.useGamepadButtons} onClick={ 
                                                        (val) => {
                                                            setUseButtons(prevState => ({
                                                                ...prevState,
                                                                [c.inputName]: val
                                                            }));
                                                            c.useGamepadButtons = val;
                                                    }}/>
                                                    {/* // Button to invert the joystick axis */}
                                                    <Checkbox label="Joystick Inverted" defaultState={c.joystickInverted} onClick={ 
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
                        <Label>No robot selected.</Label>
                    )}
                </div>
            </Stack>
            </>
            ) : (
                <Label size={LabelSize.Medium}>Spawn a robot to edit controls</Label>
            )}
        </Modal>
    )
}

export default ChangeInputsModal