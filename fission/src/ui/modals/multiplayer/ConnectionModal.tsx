import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { Stack } from "@mui/system"
import Label from "@/components/Label.tsx"
import { Button, TextField } from "@mui/material"
import { DEFAULT_MULTIPLAYER_PORT } from "@/systems/preferences/PreferenceTypes.ts"
import Checkbox from "@/components/Checkbox.tsx"
import { CustomTooltip } from "@/components/StyledComponents.tsx"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import { waitUntil, withTimeout } from "@/util/Utility.ts"
import SessionStorage from "@/util/SessionStorage.ts"
import type { RoomInfo } from "@/systems/multiplayer/bindings/RoomInfo.ts"

const DEFAULT_HOST = "127.0.0.1"

async function promptCert(url: string): Promise<boolean> {
    const shouldAttemptCert = confirm(
        "This issue may be caused by an unrecognized certificate. Would you like to try manually accepting the certificate?\n\nThis will open a new tab, you will need to manually accept the certificate for your server, as it is self-signed. \n\nIf the page completely fails to load, it is not a certificate error, but rather an inaccessible server.\n\nAfter proceeding, close the tab and press 'Test Connection' again"
    )
    if (!shouldAttemptCert) return false
    const urlObj = new URL(url)
    urlObj.protocol = "https:"
    urlObj.pathname = "/cert"
    const windowHandle = window.open(urlObj.href, "_blank", "popup")
    if (windowHandle) {
        await waitUntil(() => windowHandle?.closed, 300)
        SessionStorage.saveOnce("autoOpenTo", "multiplayer")
        SessionStorage.saveOnce("autoToast", {
            type: "info",
            lines: ["Multiplayer Certificate Update", "Try connecting again!"],
        })
        window.location.reload()
    } else {
        globalAddToast("warning", "Could not open a new tab. Please visit the page manually", urlObj.href)
    }
    return false
}

interface ConnectionModalProps {
    setRoomList: (roomList: RoomInfo[]) => void
    setURL: (url: string) => void
    onNext: () => void
}

const ConnectionModal: React.FC<ConnectionModalProps> = ({ setRoomList, setURL, onNext }) => {
    const [host, setHost] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerHost"))
    const [port, setPort] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerPort").toString())
    const [secure, setSecure] = useState(PreferencesSystem.getUserPreference("MultiplayerSecure"))
    const [testState, setTestState] = useState<"pass" | "fail" | "progress" | null>(null)
    const [showCheckCertButton, setShowCheckCertButton] = useState<boolean>(false)

    // biome-ignore lint/correctness/useExhaustiveDependencies: Should run whenever these change regardless of their values
    useEffect(() => {
        setTestState(null)
    }, [port, host, secure])

    const validateServer = useCallback(
        (silent: boolean): string | undefined => {
            const parsedPort = parseInt(port)
            if (isNaN(parsedPort) || parsedPort <= 0 || parsedPort > 65535) {
                !silent && globalAddToast("warning", "Invalid Port", "Must be an integer between 0 and 65535")
                return
            }
            const url = `${secure ? "wss" : "ws"}://${host || DEFAULT_HOST}:${parsedPort}`
            if (URL.canParse != null && !URL.canParse(url)) {
                !silent && globalAddToast("warning", "Cannot Parse URL", url)
                return
            }

            PreferencesSystem.setUserPreference("MultiplayerPort", parsedPort)
            PreferencesSystem.setUserPreference("MultiplayerSecure", secure)
            PreferencesSystem.setUserPreference("MultiplayerHost", host)
            PreferencesSystem.savePreferences()
            const cleanURL = new URL(url).href
            setURL(cleanURL)

            return cleanURL
        },
        [host, port, secure, setURL]
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
                    const ws = new MultiplayerWebsocket(url.toString())
                    ws.onOpen = () => {
                        resolve(true)
                        ws.sendServer({
                            type: "requestrooms",
                        })
                    }
                    ws.onError = async () => {
                        const urlObj = new URL(url)
                        urlObj.protocol = "http:"
                        // NOTE: Chrome is evil and for "security" this will always fail on Chrome. It works as intended on firefox
                        const reachable = await fetch(urlObj.href, { mode: "no-cors" })
                            .then(() => true)
                            .catch(() => false)

                        if (reachable && !silent) {
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
                                !silent &&
                                    globalAddToast("error", "Connection failed!", "Try pressing 'Load Certificate'")
                                setShowCheckCertButton(true)
                            } else {
                                !silent && globalAddToast("error", "Connection failed!")
                                setShowCheckCertButton(false)
                            }
                        }

                        resolve(false)
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
        [validateServer, setRoomList, secure]
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
                onClick={() => connectionTest(false)}
                className="w-full my-1"
            >
                {testState == "pass" ? "Connection OK!" : testState == "progress" ? "Testing..." : "Test Connection"}
            </Button>
            {secure && testState !== "pass" && (
                <Stack direction={"row"} gap={1}>
                    <Button
                        disabled={!showCheckCertButton || testState !== "fail"}
                        variant={"outlined"}
                        color={"info"}
                        onClick={async () => {
                            const url = validateServer(false)
                            if (!url) return
                            await promptCert(url)
                        }}
                        className="w-full my-1"
                    >
                        Load Certificate
                    </Button>
                    <CustomTooltip text={"Self-signed certificates on secure servers must be manually trusted"} />
                </Stack>
            )}
            <Button disabled={testState != "pass"} onClick={onNext} variant={"contained"} color={"primary"}>
                Next
            </Button>
        </Stack>
    )
}

export default ConnectionModal
