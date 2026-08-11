import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { Stack } from "@mui/system"
import Label from "@/components/Label.tsx"
import { Button, TextField } from "@mui/material"
import { DEFAULT_MULTIPLAYER_PORT } from "@/systems/preferences/PreferenceTypes.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { withTimeout } from "@/util/Utility.ts"
import type { RoomInfo } from "@/systems/multiplayer/bindings/RoomInfo.ts"
import MultiplayerWebtransport from "@/systems/multiplayer/MultiplayerWebtransport.ts"

const DEFAULT_HOST = "127.0.0.1"

interface ConnectionModalProps {
    setRoomList: (roomList: RoomInfo[]) => void
    setURL: (url: string) => void
    onNext: () => void
}

const ConnectionModal: React.FC<ConnectionModalProps> = ({ setRoomList, setURL, onNext }) => {
    const [host, setHost] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerHost"))
    const [port, setPort] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerPort").toString())
    const [testState, setTestState] = useState<"pass" | "fail" | "progress" | null>(null)

    // biome-ignore lint/correctness/useExhaustiveDependencies: Should run whenever these change regardless of their values
    useEffect(() => {
        setTestState(null)
    }, [port, host])

    const validateServer = useCallback(
        (silent: boolean): string | undefined => {
            const parsedPort = parseInt(port || `${DEFAULT_MULTIPLAYER_PORT}`)
            if (isNaN(parsedPort) || parsedPort <= 0 || parsedPort > 65535) {
                !silent && globalAddToast("warning", "Invalid Port", "Must be an integer between 0 and 65535")
                return
            }
            const url = `https://${host || DEFAULT_HOST}:${parsedPort}`
            if (URL.canParse != null && !URL.canParse(url)) {
                !silent && globalAddToast("warning", "Cannot Parse URL", url)
                return
            }

            PreferencesSystem.setUserPreference("MultiplayerPort", parsedPort)
            PreferencesSystem.setUserPreference("MultiplayerHost", host)
            PreferencesSystem.savePreferences()
            const cleanURL = new URL(url).href
            setURL(cleanURL)

            return cleanURL
        },
        [host, port, setURL]
    )

    // biome-ignore lint/correctness/useExhaustiveDependencies: Only trying to run this on load
    useEffect(() => {
        setTimeout(() => connectionTest(true))
    }, [])

    const connectionTest = useCallback(
        async (silent: boolean) => {
            const url = validateServer(silent)
            if (url == null) return
            const success = await withTimeout(
                new Promise<boolean>(resolve => {
                    console.groupCollapsed("Connection Test")
                    setTestState("progress")
                    MultiplayerWebtransport.create(url.toString()).then(ws => {
                        if (ws == null) return
                        ws.onOpen = () => {
                            resolve(true)
                            ws.sendServer({
                                type: "requestrooms",
                            })
                        }
                        ws.onClose = () => {
                            resolve(false)
                            console.groupEnd()
                        }
                        ws.onServerMessage = msg => {
                            console.log("Test server message", msg)
                            if (msg.type == "roomlist") {
                                setRoomList(msg.rooms)
                                resolve(true)
                            }
                        }
                        ws.onPeerMessage = msg => {
                            console.log("Test peer message", msg)
                        }
                    })
                }),
                "Connection timed out",
                10000
            ).finally(() => {
                console.groupEnd()
            })

            if (!silent && success) {
                globalAddToast("success", "WebSocket connected!")
            }
            setTestState(success ? "pass" : "fail")
        },
        [validateServer, setRoomList]
    )

    return (
        <Stack direction="column" gap={2} className="overflow-y-auto rounded-md p-2 min-w-[300px]">
            <Stack gap={0.5}>
                <Label size={"sm"}>Host</Label>
                <TextField
                    type={"text"}
                    value={host}
                    placeholder={DEFAULT_HOST}
                    inputProps={{
                        onInput: e => {
                            setHost(e.currentTarget.value.trim())
                        },
                    }}
                />
            </Stack>
            <Stack gap={0.5}>
                <Label size="sm">Port</Label>
                <TextField
                    value={port}
                    placeholder={DEFAULT_MULTIPLAYER_PORT.toString()}
                    inputProps={{
                        onInput: e => {
                            setPort(e.currentTarget.value.replace(/\D/, ""))
                        },
                    }}
                />
            </Stack>
            <Button
                disabled={testState === "progress"}
                variant={"outlined"}
                color={testState === "pass" ? "success" : "secondary"}
                onClick={() => connectionTest(false)}
                className="w-full my-1"
            >
                {testState == "pass" ? "Connection OK!" : testState == "progress" ? "Testing..." : "Test Connection"}
            </Button>
            <Button disabled={testState != "pass"} onClick={onNext} variant={"contained"} color={"primary"}>
                Next
            </Button>
        </Stack>
    )
}

export default ConnectionModal
