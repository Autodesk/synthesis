import { Button, Divider, TextField } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import { useEffect, useLayoutEffect, useState } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { ModalImplProps } from "@/components/Modal.tsx"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"

interface MultiplayerStartMenuCustomProps {
    startWorldCallback: (name: string, roomId?: string) => Promise<boolean>
}

const MultiplayerStartModal: React.FC<ModalImplProps<void, MultiplayerStartMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const [room, setRoom] = useState<string>("")
    const [name, setName] = useState<string>(PreferencesSystem.getGlobalPreference("MultiplayerUsername"))
    let isValidName: boolean = name.length >= 3
    const { startWorldCallback } = modal!.props.custom
    useLayoutEffect(() => {
        configureScreen(
            modal!,
            { title: "Start Multiplayer", hideAccept: true, hideCancel: true, allowClickAway: false },
            {}
        )
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
                onClick={async () => {
                    if (!isValidName) {
                        globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                        return
                    }
                    const success = await withTimeout(startWorldCallback(name), "Multiplayer create timed out")

                    if (success) {
                        closeModal(CloseType.Accept)
                    }
                }}
                className="w-full my-1"
            >
                Create Game
            </Button>
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
                disabled={room.length != 6}
                onClick={async () => {
                    if (!isValidName) {
                        globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                        return
                    }

                    const success = await withTimeout(startWorldCallback(name, room), "Multiplayer join timed out")
                    if (success) {
                        closeModal(CloseType.Accept)
                    }
                }}
                className={`w-full mt-1 mb-3 ${room.length != 6 && "brightness-50"}`}
            >
                Join Game
            </Button>
        </Stack>
    )
}

async function withTimeout(promise: Promise<boolean>, timeoutMessage: string, duration: number = 5000) {
    let timeout: NodeJS.Timeout
    return await Promise.race([
        promise,
        new Promise<boolean>(res => {
            timeout = setTimeout(() => {
                globalAddToast("warning", timeoutMessage)
                res(false)
            }, duration)
        }),
    ]).then(v => {
        clearTimeout(timeout)
        return v
    })
}

export default MultiplayerStartModal
