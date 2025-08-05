import { Divider, TextField } from "@mui/material"
import { Stack } from "@mui/system"
import React, {useEffect, useLayoutEffect, useState} from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { SynthesisIcons } from "../components/StyledComponents"
import type {ModalImplProps} from "@/components/Modal.tsx";

interface MultiplayerStartMenuCustomProps {
    startWorldCallback: (name: string, roomId?: string) => void
}

const MultiplayerStartModal: React.FC<ModalImplProps<void, MultiplayerStartMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const [room, setRoom] = useState<string>("")
    const [name, setName] = useState<string>(PreferencesSystem.getGlobalPreference("MultiplayerUsername"))
    let isValidName: boolean = name.length >= 3

    useLayoutEffect(() => {
        configureScreen(modal!, { title: "Start Multiplayer", hideAccept: true, hideCancel: true, allowClickAway: false }, {})
    }, [])

    useEffect(() => {
        isValidName = name.length >= 3
    }, [name, isValidName])
    return (
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
                >Create Game</Button>
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
