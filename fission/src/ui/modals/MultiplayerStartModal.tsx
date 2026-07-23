import { Button, Divider, TextField } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import {useCallback} from "react"
import { useEffect, useLayoutEffect, useState } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { ModalImplProps } from "@/components/Modal.tsx"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { CloseType, useUIContext } from "../helpers/UIProviderHelpers"
import Label from "@/components/Label.tsx";
import Checkbox from "@/components/Checkbox.tsx";
import {LabelWithTooltip} from "@/components/StyledComponents.tsx";
import {waitUntil} from "@/util/Utility.ts";
import SessionStorage from "@/util/SessionStorage.ts";


export interface MultiplayerInitProps {
    displayName:string
    roomId?: number
    url:string
}
interface MultiplayerStartMenuCustomProps {
    startWorldCallback: (initData:MultiplayerInitProps) => Promise<boolean>
}

const DEFAULT_PORT = 9001
const DEFAULT_HOST = "127.0.0.1"

const MultiplayerStartModal: React.FC<ModalImplProps<void, MultiplayerStartMenuCustomProps>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const [room, setRoom] = useState<string>("")
    const [host, setHost] = useState<string>("")
    const [port, setPort] = useState<string>("")
    const [secure, setSecure] = useState(true)
    const [name, setName] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerUsername"))
    const [testSuccess, setTestSuccess] = useState<boolean>(false)
    const [testInProgress, setTestInProgress] = useState<boolean>(false)

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        setRoom(params.get("room") ?? "")
        setName((n) => params.get("name") ?? n)
    }, [])

    const validateServer = useCallback((): string|undefined => {
        const parsedPort = port.trim().length == 0 ? DEFAULT_PORT : parseInt(port)
        if (isNaN(parsedPort) || parsedPort <= 0 || parsedPort > 65535) {
            globalAddToast("warning", "Invalid Port", "Must be an integer between 0 and 65535")
            return
        }
        const url = `${(secure ? "wss" : "ws")}://${host || DEFAULT_HOST}:${parsedPort}`
        if (URL.canParse != null && !URL.canParse(url)) {
            globalAddToast("warning", "Cannot Parse URL", url)
            return
        }
        return url

    }, [host, port, secure])

    const validate = useCallback((requireRoom:boolean): MultiplayerInitProps|undefined => {
        if (name.length <= 3) {
            globalAddToast("warning", "Invalid Name", "Must be at least 3 characters")
            return
        }

        const parsedRoom = requireRoom ? parseInt(room) : undefined
        if (parsedRoom != null && (isNaN(parsedRoom) || parsedRoom < 0)) {
            globalAddToast("warning", "Invalid Room", "Must be non-negative integer")
            return
        }
        const url = validateServer()
        if (url == null) return

        return {
            displayName: name,
            roomId: parsedRoom,
            url: url
        }

    }, [name, room, validateServer])

    const connectionTest = useCallback(async () => {
        const url = validateServer()
        if (url == null) return

        const promptCert= async ():Promise<boolean>  => {
            const shouldAttemptCert = confirm("Would you like to try manually accepting the certificate?\n\nThis will open a new tab, after clicking proceed, the page will say it failed to load. At this point, close the popup to resume.")
            if (!shouldAttemptCert) return false
            const httpURL = url.replace("wss://", "https://")
            const windowHandle = window.open(httpURL, "_blank", "popup")
            if (windowHandle) {
                await waitUntil(() => windowHandle?.closed, 300)
                SessionStorage.saveOnce("autoOpenTo", "multiplayer")
                SessionStorage.saveOnce("autoToast", {type: "info", lines:["Multiplayer Certificate Update", "Try connecting again!"]})
                window.location.reload()
            } else {
                globalAddToast("warning", "Could not open a new tab. Please visit the page manually", httpURL)
            }
            return false
        }

        const success = await withTimeout(new Promise<boolean>((resolve) => {
            setTestInProgress(true)
            const ws = new WebSocket(url)
            ws.onopen = (ev) => {
                console.log("WS Open", ev)
                resolve(true)
                ws.close(4000, "test connection succeeded")
            }
            ws.onerror = async (ev) => {
                console.error("WS Error", ev)
                const reachable = await fetch(url.replace(/wss?:\/\//, "http://"), {mode: "no-cors"}).then(() => true).catch((err:Error) => {console.log(err); return /ERR_INVALID_HTTP_RESPONSE/.test(err.message)})

                console.log("Accessible", reachable)
                if (reachable) {
                    if (secure) {
                        globalAddToast("warning", "WebSocket connection failed!", "Server reachable, try manually accepting the certificate")
                        await promptCert()
                    } else {
                        globalAddToast("warning", "WebSocket connection failed!", "Server reachable, check secure flag")
                    }
                } else {
                    globalAddToast("error", "Connection failed!")
                }

                resolve(false)
            }
            ws.onclose = (ev) => {
                console.warn("WS Close", ev)
            }
            ws.onmessage = (ev) => {
                console.error("WS Message", ev)
            }

        }), "Connection timed out", 10000)

        if (success) {
            globalAddToast("success", "WebSocket connected!")
        }
        setTestInProgress(false)
        setTestSuccess(success)
    },[validateServer])

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
            <LabelWithTooltip labelText="Port" tooltipText="The port the server is running on. Default 9001"/>
            <TextField
                value={port}
                placeholder={DEFAULT_PORT.toString()}
                inputProps={{
                    onInput: e => {
                        setPort(e.currentTarget.value.replace(/\D/, ""))
                    },
                }}
            />
            </Stack>
            <Checkbox label={"Secure?"} tooltip="Should use Websockets over TLS? This should be enabled unless the server is running in insecure mode" checked={secure} onClick={(checked) => setSecure(checked)}/>
            <Button
                disabled={testInProgress}
                variant={"outlined"}
                color={testSuccess ? "success" : "secondary"}
                onClick={connectionTest}
                className="w-full my-1"
            >
                {testSuccess
                    ? "Connection OK!"
                    : testInProgress
                        ? "Testing..."
                        : "Test Connection"}
            </Button>
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
                placeholder="000000"
                inputProps={{
                    onInput: e => {
                        setRoom(e.currentTarget.value.replace(/\D/, ""))
                    },
                }}
            />
            </Stack>

            <Button
                disabled={room.length == 0}
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
