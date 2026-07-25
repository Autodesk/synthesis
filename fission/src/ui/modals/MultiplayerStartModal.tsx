import {Button, Divider, TextField} from "@mui/material"
import {Box, Stack} from "@mui/system"
import type React from "react"
import {useCallback, useEffect, useLayoutEffect, useState} from "react"
import {globalAddToast} from "@/components/GlobalUIControls.ts"
import type {ModalImplProps} from "@/components/Modal.tsx"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import {CloseType, useUIContext} from "../helpers/UIProviderHelpers"
import Label from "@/components/Label.tsx"
import Checkbox from "@/components/Checkbox.tsx"
import {CustomTooltip} from "@/components/StyledComponents.tsx"
import {waitUntil} from "@/util/Utility.ts"
import SessionStorage from "@/util/SessionStorage.ts"
import {DEFAULT_MULTIPLAYER_PORT} from "@/systems/preferences/PreferenceTypes.ts"
import {multiplayerLogger as console} from "@/systems/multiplayer/MultiplayerSystem.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import {RoomInfo} from "@/systems/multiplayer/bindings/RoomInfo.ts";

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
    const [_room, setRoom] = useState<string>("")
    const [host, setHost] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerHost"))
    const [port, setPort] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerPort").toString())
    const [secure, setSecure] = useState(PreferencesSystem.getUserPreference("MultiplayerSecure"))
    const [name, setName] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerUsername"))
    const [roomList, setRoomList] = useState<RoomInfo[]>([])
    const [testState, setTestState] = useState<"pass" | "fail" | "progress" | null>(null)
    const [showCheckCertButton, setShowCheckCertButton] = useState<boolean>(false)

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        setRoom(params.get("room") ?? "")
        setName(n => params.get("name") ?? n)
    }, [])

    const validateServer = useCallback((silent:boolean): string | undefined => {
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

        return url
    }, [host, port, secure])

    const validate = useCallback(
        (room?: string): MultiplayerInitProps | undefined => {
            if (name.length <= 3) {
                globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
                return
            }

            if (room != null && room.length !== 6) {
                globalAddToast("warning", "Invalid Room", "Must be 6 characters")
                return
            }
            const url = validateServer(false)
            if (url == null) return

            return {
                displayName: name,
                roomId: room,
                url: url,
            }
        },
        [name, validateServer]
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

    useEffect(() => {
        setTimeout(() => connectionTest(true))
    }, [])

    const connectionTest = useCallback(async (silent:boolean) => {
        const url = validateServer(silent)
        if (url == null) return
        const success = await withTimeout(
            new Promise<boolean>(resolve => {
                console.groupCollapsed("Connection Test")
                setTestState("progress")
                const ws = new MultiplayerWebsocket(url)
                ws.onOpen = () => {
                    resolve(true)
                    ws.send({
                        type: "requestrooms",
                    })
                }
                ws.onError = async () => {
                    // NOTE: Chrome is evil and for "security" this will always fail on Chrome. It works as intended on firefox
                    const reachable = await fetch(url.replace(/wss?:\/\//, "http://"), { mode: "no-cors" })
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
                            !silent && globalAddToast("error", "Connection failed!", "Try pressing 'Load Certificate'")
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

    const joinRoom = useCallback(async (roomId:string) => {
        setRoom(roomId)
        const initData = validate(roomId)
        if (initData == null) return

        const success = await withTimeout(startWorldCallback(initData), "Multiplayer join timed out")
        if (success) {
            closeModal(CloseType.Accept)
        }
    }, [validate])

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
            {secure && (
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
                disabled={name.length < 3}
                value={"Create Game"}
                onClick={async () => {
                    const initData = validate()
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
            {roomList.map((room) => (
                <Box
                    sx={{ bgcolor: "background.paper", p: 2, borderRadius: 5, width: "100%" }}
                    key={room.id}
                >
                    <Stack direction={"row"} justifyContent="space-between" gap={2}>
                    <Stack direction="column" gap={1}>
                        <Label size={"md"}>
                            {room.authority != null? `${room.authority}'s Room` : "Server Room"}
                        </Label>
                        <Label size={"sm"}>{room.id}</Label>
                    </Stack>
                        <Button disabled={name.length < 3} variant={'outlined'} color={'secondary'} onClick={() => joinRoom(room.id)}>
                            Join
                        </Button>
                    </Stack>
                </Box>
            ))}
            <Divider />
            {/*<Stack>*/}
            {/*    <Label size={"sm"}>Room Code</Label>*/}
            {/*    <TextField*/}
            {/*        value={room}*/}
            {/*        placeholder="ABC123"*/}
            {/*        inputProps={{*/}
            {/*            onInput: e => {*/}
            {/*                setRoom(*/}
            {/*                    e.currentTarget.value*/}
            {/*                        .toUpperCase()*/}
            {/*                        .replace(/[^A-Z\d]/g, "")*/}
            {/*                        .slice(0, 6)*/}
            {/*                )*/}
            {/*            },*/}
            {/*        }}*/}
            {/*    />*/}
            {/*</Stack>*/}

            {/*<Button*/}
            {/*    disabled={room.length !== 6}*/}
            {/*    onClick={() => joinRoom(room)}*/}
            {/*    className={`w-full mt-1 mb-3`}*/}
            {/*>*/}
            {/*    Join Game*/}
            {/*</Button>*/}
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
