import { Button, Divider, TextField } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import { useCallback } from "react"
import { useEffect, useLayoutEffect, useState } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { ModalImplProps } from "@/components/Modal.tsx"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import Label from "@/components/Label.tsx"
import Checkbox from "@/components/Checkbox.tsx"
import { LabelWithTooltip } from "@/components/StyledComponents.tsx"
import { waitUntil } from "@/util/Utility.ts"
import SessionStorage from "@/util/SessionStorage.ts"
import { DEFAULT_MULTIPLAYER_PORT } from "@/systems/preferences/PreferenceTypes.ts"
import { multiplayerLogger as console } from "@/systems/multiplayer/MultiplayerSystem.ts"

export interface MultiplayerInitProps {
    displayName: string
    roomId?: string
    url: string
}
interface MultiplayerStartMenuCustomProps {
    startWorldCallback: (initData: MultiplayerInitProps) => Promise<boolean>
}

const DEFAULT_HOST = "127.0.0.1"

const MultiplayerStartModal: React.FC<ModalImplProps<void, MultiplayerStartMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const [room, setRoom] = useState<string>("")
    const [host, setHost] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerHost"))
    const [port, setPort] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerPort").toString())
    const [secure, setSecure] = useState(PreferencesSystem.getUserPreference("MultiplayerSecure"))
    const [name, setName] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerUsername"))
    const [testState, setTestState] = useState<"pass" | "fail" | "progress" | null>(null)
    const [showCheckCertButton, setShowCheckCertButton] = useState<boolean>(false)

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        setRoom(params.get("room") ?? "")
        setName(n => params.get("name") ?? n)
    }, [])

    const validateServer = useCallback((): string | undefined => {
        const parsedPort = parseInt(port)
        if (isNaN(parsedPort) || parsedPort <= 0 || parsedPort > 65535) {
            globalAddToast("warning", "Invalid Port", "Must be an integer between 0 and 65535")
            return
        }
        const url = `${secure ? "wss" : "ws"}://${host || DEFAULT_HOST}:${parsedPort}`
        if (URL.canParse != null && !URL.canParse(url)) {
            globalAddToast("warning", "Cannot Parse URL", url)
            return
        }
        PreferencesSystem.setUserPreference("MultiplayerPort", parsedPort)
        PreferencesSystem.setUserPreference("MultiplayerSecure", secure)
        PreferencesSystem.setUserPreference("MultiplayerHost", host)
        PreferencesSystem.savePreferences()

        return url
    }, [host, port, secure])

    const validate = useCallback(
        (useRoom: boolean): MultiplayerInitProps | undefined => {
            if (name.length <= 3) {
                globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                return
            }

            if (useRoom && room.length !== 6) {
                globalAddToast("warning", "Invalid Room", "Must be 6 characters")
                return
            }
            const url = validateServer()
            if (url == null) return

            return {
                displayName: name,
                roomId: useRoom ? room : undefined,
                url: url,
            }
        },
        [name, room, validateServer]
    )

    const promptCert = useCallback(async (url: string): Promise<boolean> => {
        const shouldAttemptCert = confirm(
            "This issue may be caused by an unrecognized certificate. Would you like to try manually accepting the certificate?\n\nThis will open a new tab, you will need to manually accept the certificate for your server, as it is self-signed. \n\nIf the page completely fails to load, it is not a certificate error, but rather an inaccessible server.\n\nAfter proceeding, close the tab and press 'Test Connection' again"
        )
        if (!shouldAttemptCert) return false
        const httpURL = url.replace("wss://", "https://") + "/cert"
        const windowHandle = window.open(httpURL, "_blank", "popup")
        if (windowHandle) {
            await waitUntil(() => windowHandle?.closed, 300)
            SessionStorage.saveOnce("autoOpenTo", "multiplayer")
            SessionStorage.saveOnce("autoToast", {
                type: "info",
                lines: ["Multiplayer Certificate Update", "Try connecting again!"],
            })
            window.location.reload()
        } else {
            globalAddToast("warning", "Could not open a new tab. Please visit the page manually", httpURL)
        }
        return false
    }, [])

    const connectionTest = useCallback(async () => {
        const url = validateServer()
        if (url == null) return
        const success = await withTimeout(
            new Promise<boolean>(resolve => {
                console.group("Connection Test")
                setTestState("progress")
                const ws = new WebSocket(url)
                ws.onopen = () => {
                    console.log("Test socket open")
                    resolve(true)
                    ws.close(4000, "test connection succeeded")
                }
                ws.onerror = async ev => {
                    console.error("Test socket error", ev)

                    // NOTE: Chrome is evil and for "security" this will always fail on Chrome. It works as intended on firefox
                    const reachable = await fetch(url.replace(/wss?:\/\//, "http://"), { mode: "no-cors" })
                        .then(() => true)
                        .catch(() => false)

                    if (reachable) {
                        if (secure) {
                            globalAddToast(
                                "warning",
                                "WebSocket connection failed!",
                                "Server reachable, try manually accepting the certificate"
                            )
                            await promptCert(url)
                        } else {
                            globalAddToast(
                                "warning",
                                "WebSocket connection failed!",
                                "Server reachable, check secure flag"
                            )
                        }
                    } else {
                        if (secure) {
                            globalAddToast("error", "Connection failed!", "Try pressing 'Load Certificate'")
                            setShowCheckCertButton(true)
                        } else {
                            globalAddToast("error", "Connection failed!")
                            setShowCheckCertButton(false)
                        }
                    }

                    resolve(false)
                }
                ws.onclose = () => {
                    console.log("Test socket closed")
                    resolve(false)
                    console.groupEnd()
                }
                ws.onmessage = () => {
                    console.log("Test socket message")
                }
            }),
            "Connection timed out",
            10000
        ).finally(() => {
            console.groupEnd()
        })

        if (success) {
            globalAddToast("success", "WebSocket connected!")
        }
        setTestState(success ? "pass" : "fail")
    }, [validateServer, promptCert, secure])

    const { startWorldCallback } = modal!.props.custom
    useLayoutEffect(() => {
        configureScreen(
            modal!,
            { title: "Start Multiplayer", hideAccept: true, hideCancel: true, allowClickAway: false },
            {}
        )
    }, [])
    return (
        <Stack direction="column" gap={2}>
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
                <LabelWithTooltip labelText="Port" tooltipText="The port the server is running on. Default 9001" />
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
            <Checkbox
                label={"Secure?"}
                tooltip="Should use Websockets over TLS? This should be enabled unless the server is running in insecure mode"
                checked={secure}
                onClick={checked => setSecure(checked)}
            />
            <Button
                disabled={testState === "progress"}
                variant={"outlined"}
                color={testState === "pass" ? "success" : "secondary"}
                onClick={connectionTest}
                className="w-full my-1"
            >
                {testState == "pass" ? "Connection OK!" : testState == "progress" ? "Testing..." : "Test Connection"}
            </Button>
            {secure && (
                <Button
                    disabled={!showCheckCertButton || testState !== "fail"}
                    variant={"outlined"}
                    color={"info"}
                    onClick={async () => {
                        const url = validateServer()
                        if (!url) return
                        await promptCert(url)
                    }}
                    className="w-full my-1"
                >
                    Load Certificate
                </Button>
            )}
            <Divider />
            <Stack gap={0.5}>
                <Label size={"sm"}>Display Name</Label>
                <TextField
                    type={"text"}
                    value={name}
                    placeholder="Dozer"
                    inputProps={{
                        onInput: e => {
                            setName(e.currentTarget.value.replace(/\W/, "").slice(0, 12))
                        },
                    }}
                />
            </Stack>
            <Button
                value={"Create Game"}
                onClick={async () => {
                    const initData = validate(false)
                    if (initData == null) return

                    const success = await withTimeout(startWorldCallback(initData), "Multiplayer create timed out")

                    if (success) {
                        closeModal(CloseType.Accept)
                    }
                }}
                className="w-full my-1"
            >
                Create Game
            </Button>
            <Divider />
            <Stack>
                <Label size={"sm"}>Room Code</Label>
                <TextField
                    value={room}
                    placeholder="ABC123"
                    inputProps={{
                        onInput: e => {
                            setRoom(
                                e.currentTarget.value
                                    .toUpperCase()
                                    .replace(/[^A-Z\d]/g, "")
                                    .slice(0, 6)
                            )
                        },
                    }}
                />
            </Stack>

            <Button
                disabled={room.length !== 6}
                onClick={async () => {
                    const initData = validate(true)
                    if (initData == null) return

                    const success = await withTimeout(startWorldCallback(initData), "Multiplayer join timed out")
                    if (success) {
                        closeModal(CloseType.Accept)
                    }
                }}
                className={`w-full mt-1 mb-3`}
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
