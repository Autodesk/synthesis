import { Box, Divider, IconButton, Stack, TextField, Tooltip } from "@mui/material"
import Label from "@/components/Label.tsx"
import { Button, SynthesisIcons } from "@/components/StyledComponents.tsx"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import type { MultiplayerInitProps } from "@/modals/multiplayer/MultiplayerStartModal.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import { withTimeout } from "@/util/Utility.ts"
import type { RoomInfo } from "@/systems/multiplayer/bindings/RoomInfo.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import { startMultiplayerWorld } from "@/ui/helpers/StartMultiplayerWorld.ts"

interface RoomModalProps {
    initialRoomList: RoomInfo[]
    url: string
    onBack: () => void
}

const RoomModal: React.FC<RoomModalProps> = ({ initialRoomList, url, onBack }) => {
    const { closeModal } = useUIContext()
    const [name, setName] = useState<string>(PreferencesSystem.getUserPreference("MultiplayerUsername"))
    const [roomList, setRoomList] = useState(initialRoomList)
    const [updatingRoomList, setUpdatingRoomList] = useState(false)
    const wsRef = useRef<MultiplayerWebsocket | null>(null)
    const usernameRef = useRef<HTMLInputElement>(null)

    useEffect(() => {
        setRoomList(initialRoomList)
    }, [initialRoomList])

    const validName = useMemo(() => name.length >= 3, [name])

    useEffect(() => {
        const params = new URLSearchParams(window.location.search)
        setName(n => params.get("name") ?? n)
    }, [])

    useEffect(() => {
        wsRef.current?.close()
        wsRef.current = null
    }, [url])

    const updateRoomList = useCallback(async () => {
        setUpdatingRoomList(true)
        return withTimeout(
            new Promise(resolve => {
                if (wsRef.current == null) {
                    wsRef.current = new MultiplayerWebsocket(url)
                    wsRef.current.onOpen = () => {
                        wsRef.current!.sendServer({ type: "requestrooms" })
                    }
                } else {
                    wsRef.current.sendServer({ type: "requestrooms" })
                }

                wsRef.current.onServerMessage = msg => {
                    if (msg.type === "roomlist") {
                        setRoomList(msg.rooms)
                        resolve(true)
                    }
                }
            }),
            "Refreshing rooms timed out",
            5000
        ).finally(() => setTimeout(() => setUpdatingRoomList(false), 150))
    }, [url])

    const validate = useCallback(
        (room: string | undefined, keepAssets: boolean): MultiplayerInitProps | undefined => {
            if (name.length < 3) {
                globalAddToast("warning", "Invalid Username", "Must be at least 3 characters")
                usernameRef.current?.querySelector("input")?.focus()
                return
            }

            if (room != null && room.length !== 6) {
                globalAddToast("warning", "Invalid Room", "Must be 6 characters")
                return
            }
            if (url == null) return

            return {
                displayName: name,
                ws: MultiplayerWebsocket.init(room ?? null, name, wsRef.current ?? new MultiplayerWebsocket(url)),
                keepAssets: keepAssets ?? false,
            }
        },
        [name, url]
    )

    const joinRoom = useCallback(
        async (roomId: string | undefined, keepAssets: boolean) => {
            const initData = validate(roomId, keepAssets)
            if (initData == null) return

            const success = await withTimeout(startMultiplayerWorld(initData), "Multiplayer connect timed out")
            if (success) {
                closeModal(CloseType.ACCEPT)
            } else {
                globalAddToast("warning", "Could not join room")
                wsRef.current?.close()
                wsRef.current = null
                await updateRoomList()
            }
        },
        [validate, updateRoomList, closeModal]
    )

    return (
        <Stack direction="column" gap={2} className="overflow-y-auto rounded-md p-2 min-w-[300px]">
            <Stack gap={0.5}>
                <Label size={"sm"}>Display Name</Label>
                <TextField
                    type={"text"}
                    ref={usernameRef}
                    value={name}
                    sx={{ transition: "all 0.5s ease" }}
                    placeholder="Dozer"
                    color={validName ? "success" : "warning"}
                    inputProps={{
                        onInput: e => {
                            setName(e.currentTarget.value.replace(/\W/, "").slice(0, 12))
                        },
                    }}
                />
            </Stack>

            <Divider />
            <Stack direction="row" justifyContent="space-between" alignItems={"center"}>
                <Label size={"md"}>Rooms</Label>
                <IconButton
                    onClick={updateRoomList}
                    sx={{ animation: updatingRoomList ? "continuous-spin 1s linear infinite" : undefined }}
                    color={"secondary"}
                >
                    <SynthesisIcons.REFRESH_LARGE />
                </IconButton>
            </Stack>
            {roomList.map(room => (
                <Box sx={{ bgcolor: "background.paper", p: 2, borderRadius: 5, width: "100%" }} key={room.id}>
                    <Stack direction={"row"} justifyContent="space-between" gap={2}>
                        <Stack direction="column" gap={1}>
                            <Label size={"md"}>{room.host != null ? `${room.host}'s Room` : "Empty Room"}</Label>
                            <Label size={"sm"}>{room.id}</Label>
                        </Stack>
                        <Tooltip
                            title={
                                room.locked
                                    ? "Cannot join a locked room. The server operator must unlock it for you to join"
                                    : null
                            }
                            placement="right"
                            arrow
                        >
                            <Box display={"flex"}>
                                <Button
                                    disabled={room.locked}
                                    variant={"outlined"}
                                    color={"primary"}
                                    onClick={() => joinRoom(room.id, false)}
                                >
                                    {room.locked ? "Locked" : "Join"}
                                </Button>
                            </Box>
                        </Tooltip>
                    </Stack>
                </Box>
            ))}
            <Tooltip title={"Create a new room"} placement="right" arrow>
                <Box display={"flex"}>
                    <Button color="primary" onClick={() => joinRoom(undefined, false)} className="w-full my-1">
                        New Room
                    </Button>
                </Box>
            </Tooltip>
            <Tooltip title={"Create a new room and keep existing assets"} placement="right" arrow>
                <Box display={"flex"}>
                    <Button color="primary" onClick={() => joinRoom(undefined, true)} className="w-full my-1">
                        Convert to Room
                    </Button>
                </Box>
            </Tooltip>
            <Divider />
            <Button color={"secondary"} variant={"outlined"} onClick={onBack}>
                Back
            </Button>
        </Stack>
    )
}

export default RoomModal
