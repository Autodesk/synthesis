import { styled, Typography } from "@mui/material"
import { Box } from "@mui/system"
import { useEffect, useReducer } from "react"

const handleMap = new Map<number, ProgressHandle>()

const TypoStyled = styled(Typography)((_) => ({
    fontFamily: "Artifakt",
    textAlign: "center"
}))

function ProgressNotifications() {

    const [progressElements, updateProgressElements] = useReducer(() => {
        return handleMap.size > 0
            ?
                [...handleMap.entries()].map(([id, handle]) => { return (
                    <Box
                        key={id}
                        component={"div"}
                        display={"flex"}
                        flexDirection={"column"}
                        sx={{
                            backgroundColor: "#000000",
                            borderWidth: "0rem",
                            borderRadius: "0.5rem",
                            overflow: "hidden"
                        }}
                    >
                        <Box
                            component={"div"}
                            display={"flex"}
                            flexDirection={"column"}
                            sx={{
                                paddingY: "0.75rem",
                                paddingX: "1.5rem"
                            }}
                        >
                            <TypoStyled fontWeight={"700"} fontSize={"1rem"}>{handle.title}</TypoStyled>
                            { handle.message.length > 0 ? <TypoStyled fontSize={"0.75rem"}>{handle.message}</TypoStyled> : <></> }
                        </Box>
                        <Box
                            key={"bar"}
                            component={"div"}
                            sx={{
                                backgroundColor: 
                                    handle.status == ProgressHandleStatus.inProgress ? "#ffc21a" // Autodesk Gold
                                    : handle.status == ProgressHandleStatus.Done ? "#2bc275"  // Autodesk Plant
                                    : "#d74e26", // Autodesk Clay
                                bottom: "0pt",
                                left: "0pt",
                                width: `${Math.floor(Math.min(100, Math.max(0, handle.progress * 100)))}%`,
                                height: "0.5rem",
                            }}
                        />
                    </Box>
                )})
            : undefined
    }, undefined)

    useEffect(() => {
        const onHandleUpdate = (e: Event) => {
            const handle = (e as ProgressEvent).handle;
            if (handle.status > 0) {
                setTimeout(() => handleMap.delete(handle.handleId) && updateProgressElements(), 1000)
                console.debug('Deleting')
            }
            handleMap.set(handle.handleId, handle)
            updateProgressElements()
        }

        window.addEventListener(ProgressEvent.EVENT_KEY, onHandleUpdate)
        return () => {
            window.removeEventListener(ProgressEvent.EVENT_KEY, onHandleUpdate)
        }
    }, [updateProgressElements])

    return (
        <Box
            component={"div"}
            display={"flex"}
            position={"fixed"}
            sx={{
                bottom: "0.5rem",
                left: "50vw",
                transform: "translate(-50%, 0)",
                maxWidth: "50vw",
                flexWrap: "wrap",
                gap: "0.5rem"
            }}
        >
            { progressElements ?? <></> }
        </Box>
    )
}

let nextHandleId = 0

export enum ProgressHandleStatus {
    inProgress = 0,
    Done = 1,
    Error = 2
}

export class ProgressHandle {
    
    private _handleId: number
    private _title: string
    public message: string = ""
    public progress: number = 0.0
    public status: ProgressHandleStatus = ProgressHandleStatus.inProgress

    public get handleId() { return this._handleId }
    public get title() { return this._title }

    public constructor(title: string) {
        this._handleId = nextHandleId++
        this._title = title
    }

    public Update(message: string, progress: number, status?: ProgressHandleStatus) {
        this.message = message
        this.progress = progress
        status && (this.status = status)

        this.Push()
    }

    public Push() {
        window.dispatchEvent(new ProgressEvent(this))
    }
}

class ProgressEvent extends Event {
    public static readonly EVENT_KEY = 'ProgressEvent'

    public handle: ProgressHandle

    public constructor(handle: ProgressHandle) {
        super(ProgressEvent.EVENT_KEY)

        this.handle = handle
    }
}

export default ProgressNotifications