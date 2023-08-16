import React, { useState } from "react"
import Modal, { ModalPropsImpl } from "../../components/Modal"
import { FaGamepad } from "react-icons/fa6"
import Stack, { StackDirection } from "../../components/Stack"
import Label, { LabelSize } from "../../components/Label"
import Button from "../../components/Button"

type ModifierState = {
    alt?: boolean
    ctrl?: boolean
    shift?: boolean
    meta?: boolean
}

type Control = {
    name: string
    key: string
    modifiers?: ModifierState
}

const globalControls: { [key: string]: Control } = {
    "Intake": { name: "Intake", key: "E" },
    "Shoot Gamepiece": { name: "Shoot Gamepiece", key: "Q" },
    "Enable God Mode": { name: "Enable God Mode", key: "G" }
}

const robotControls: { [key: string]: Control } = {
    "Arcade Forward": { name: "Arcade Forward", key: "W" },
    "Arcade Backward": { name: "Arcade Backward", key: "A" },
    "Arcade Left": { name: "Arcade Left", key: "S" },
    "Arcade Right": { name: "Arcade Right", key: "D" },
}


// capitalize first letter
const transformKeyName = (control: Control) => {
    let suffix = ""
    if (control.modifiers) {
        if (control.modifiers.meta) suffix += " + Meta";
        if (control.modifiers.shift) suffix += " + Shift";
        if (control.modifiers.ctrl) suffix += " + Ctrl";
        if (control.modifiers.alt) suffix += " + Alt";
    }
    return control.key[0].toUpperCase() + control.key.substring(1) + suffix;
}

const ChangeInputsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const [loadedRobot, setLoadedRobot] = useState<string>("");
    const [selectedInput, setSelectedInput] = useState<string>("");
    const [chosenKey, setChosenKey] = useState<string>("");
    const [modifierState, setModifierState] = useState<ModifierState>({})

    if (selectedInput && chosenKey) {
        const selected = globalControls[selectedInput];
        selected.key = chosenKey;
        selected.modifiers = modifierState;
        setChosenKey("");
        setSelectedInput("");
        setModifierState({})
    }

    return (
        <Modal name="Keybinds" icon={<FaGamepad />} modalId={modalId}>
            <Stack direction={StackDirection.Horizontal}>
                <div onKeyUp={e => {
                    setChosenKey(selectedInput ? e.key : "");
                    setModifierState({ ctrl: e.ctrlKey, alt: e.altKey, shift: e.shiftKey, meta: e.metaKey })
                }}>
                    {loadedRobot ? (
                        <>
                            <Label size={LabelSize.Large}>Robot Controls</Label>
                            {Object.values(robotControls).map(c => (
                                <Stack direction={StackDirection.Horizontal}>
                                    <Label>{c.name}</Label>
                                    <Button value={c.name == selectedInput ? "Press anything" : transformKeyName(c)} onClick={() => { setSelectedInput(c.name) }} />
                                </Stack>
                            ))}
                        </>
                    ) : (
                        <Label>No robot loaded.</Label>
                    )}
                </div>
                <div onKeyUp={e => {
                    setChosenKey(selectedInput ? e.key : "");
                    setModifierState({ ctrl: e.ctrlKey, alt: e.altKey, shift: e.shiftKey, meta: e.metaKey })
                }}>
                    <Label size={LabelSize.Large}>Global Controls</Label>
                    {Object.values(globalControls).map(c => (
                        <Stack direction={StackDirection.Horizontal}>
                            <Label>{c.name}</Label>
                            <Button value={c.name == selectedInput ? "Press anything" : transformKeyName(c)} onClick={() => { setSelectedInput(c.name) }} />
                        </Stack>
                    ))}
                </div>
            </Stack>
        </Modal>
    )
}

export default ChangeInputsModal
