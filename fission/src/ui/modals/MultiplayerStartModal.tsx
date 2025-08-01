import { Divider, TextField } from "@mui/material"
import { Stack } from "@mui/system"
import React, { useEffect, useState } from "react"
import Button from "@/components/Button.tsx"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { SynthesisIcons } from "../components/StyledComponents"
import { useModalControlContext } from "../helpers/UseModalManager"

const MultiplayerStartModal: React.FC<
    ModalPropsImpl & {
        startWorldCallback: (name: string, roomId?: string) => void
    }
> = ({ modalId, startWorldCallback }) => {
    const { closeModal } = useModalControlContext()
    const [room, setRoom] = useState<string>("")
    const [name, setName] = useState<string>(PreferencesSystem.getGlobalPreference("MultiplayerUsername"))
    let isValidName: boolean = name.length >= 3

    useEffect(() => {
        isValidName = name.length >= 3
    }, [name, isValidName])
    return (
        <Modal
            name={"Start Multiplayer"}
            icon={SynthesisIcons.PEOPLE}
            modalId={modalId}
            middleEnabled={false}
            cancelEnabled={false}
            acceptEnabled={false}
            allowClickAway={false}
        >
            <Stack direction="column">
                <TextField
                    type={"text"}
                    value={name}
                    sx={{ input: { color: "#ffffff" } }}
                    placeholder="Name"
                    inputProps={{
                        onInput: e => {
                            setName(e.currentTarget.value.replace(/\W/, "").slice(0, 12))
                        },
                    }}
                />
                <Button
                    value={"Create Game"}
                    onClick={() => {
                        if (!isValidName) {
                            globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                            return
                        }
                        closeModal()
                        startWorldCallback(name)
                    }}
                    className="w-full my-1"
                />
                <Divider />
                <TextField
                    type={"text"}
                    value={room}
                    sx={{ input: { color: "#ffffff" } }}
                    placeholder="000000"
                    inputProps={{
                        onInput: e => {
                            setRoom(e.currentTarget.value.replace(/\D/, "").slice(0, 6)) // 6-digit numbers
                        },
                    }}
                />
                <Button
                    value={"Join Game"}
                    disabled={room.length != 6}
                    // cl={room.length != 6 ? "bg-gray-600":undefined}
                    onClick={() => {
                        if (!isValidName) {
                            globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                            return
                        }
                        closeModal()
                        startWorldCallback(name, room)
                    }}
                    className={`w-full mt-1 mb-3 ${room.length != 6 && "brightness-50"}`}
                />
            </Stack>
        </Modal>
    )
}

export default MultiplayerStartModal
