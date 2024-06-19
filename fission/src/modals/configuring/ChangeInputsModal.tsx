import React, { useEffect, useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaGamepad } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Label, { LabelSize } from "../../components/Label"
import LabeledButton, { LabelPlacement } from "../../components/LabeledButton"
import InputSystem, { emptyModifierState } from "@/systems/input/InputSystem"

// capitalize first letter
const transformKeyName = (control: Input) => {
    let prefix = ""
    if (control.modifiers) {
        if (control.modifiers.meta) prefix += "Meta + "
        if (control.modifiers.shift) prefix += "Shift + "
        if (control.modifiers.ctrl) prefix += "Ctrl + "
        if (control.modifiers.alt) prefix += "Alt + "
    }
    return prefix + control.keybind[0].toUpperCase() + control.keybind.substring(1)
}

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [loadedRobot, setLoadedRobot] = useState<string>("")
    const [selectedInput, setSelectedInput] = useState<string>("")
    const [chosenKey, setChosenKey] = useState<string>("")
    const [modifierState, setModifierState] = useState<ModifierState>(emptyModifierState)

    useEffect(() => {
        setTimeout(() => setLoadedRobot("Dozer v9"), 1)
    })

    if (selectedInput && chosenKey) {
        const selected = InputSystem.allInputs[selectedInput]
        selected.keybind = chosenKey
        selected.modifiers = modifierState
        setChosenKey("")
        setSelectedInput("")
        setModifierState(emptyModifierState)
    }

    return (
        <Modal name="Keybinds" icon={<FaGamepad />} modalId={modalId}>
            <Stack direction={StackDirection.Horizontal}>
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
                    {loadedRobot ? (
                        <>
                            <Label size={LabelSize.Large}>Robot Controls</Label>
                            {Object.values(InputSystem.robotInputs).map(c => (
                                <LabeledButton
                                    key={c.name}
                                    label={InputSystem.toTitleCase(c.name)}
                                    placement={LabelPlacement.Left}
                                    value={
                                        c.name == selectedInput
                                            ? "Press anything"
                                            : transformKeyName(c)
                                    }
                                    onClick={() => {
                                        setSelectedInput(c.name)
                                    }}
                                />
                            ))}
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
                    {Object.values(InputSystem.globalInputs).map(c => (
                        <LabeledButton
                            key={c.name}
                            label={InputSystem.toTitleCase(c.name)}
                            placement={LabelPlacement.Left}
                            value={
                                c.name == selectedInput
                                    ? "Press anything"
                                    : transformKeyName(c)
                            }
                            onClick={() => {
                                setSelectedInput(c.name)
                            }}
                        />
                    ))}
                </div>
            </Stack>
        </Modal>
    )
}

export default ChangeInputsModal
