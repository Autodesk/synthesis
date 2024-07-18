import { styled, Typography } from "@mui/material"
import { Box } from "@mui/system"
import { useEffect, useReducer, useState } from "react"
import { ProgressHandle, ProgressHandleStatus, ProgressEvent } from "./ProgressNotificationData"
import { easeOutQuad } from "@/util/EasingFunctions"

interface ProgressData {
    lastValue: number
    currentValue: number
    lastUpdate: number
}

const handleMap = new Map<number, ProgressHandle>()

const TypoStyled = styled(Typography)(_ => ({
    fontFamily: "Artifakt",
    textAlign: "center",
}))

interface NotificationProps {
    handle: ProgressHandle
}

function Interp(elapse: number, progressData: ProgressData) {
    const [time, setTime] = useState<number>(0)

    useEffect(() => {
        console.debug(`Updated: [${progressData.lastValue}, ${progressData.currentValue}, ${progressData.lastUpdate}]`)

        const update = () => {
            const n = Math.min(1.0, Math.max(0.0, (Date.now() - progressData.lastUpdate) / elapse))

            setTime(n)
        }

        const interval = setInterval(update, 5)
        const timeout = setTimeout(() => clearInterval(interval), elapse)

        return () => {
            clearTimeout(timeout)
            clearInterval(interval)
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [progressData])

    return progressData.lastValue + (progressData.currentValue - progressData.lastValue) * easeOutQuad(time)
}

function ProgressNotification({ handle }: NotificationProps) {

    const [progressData, setProgressData] = useState<ProgressData>({ lastValue: 0, currentValue: 0, lastUpdate: Date.now() })

    const interpProgress = Interp(500, progressData)

    useEffect(() => {
        setProgressData({ lastValue: progressData.currentValue, currentValue: handle.progress, lastUpdate: Date.now() })
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [handle.progress])

    console.debug(`[${progressData.lastValue}, ${progressData.currentValue}, ${progressData.lastUpdate}]`)
    console.debug(interpProgress.toFixed(2))

    return (
        <Box
            key={handle.handleId}
            component={"div"}
            display={"flex"}
            flexDirection={"column"}
            sx={{
                backgroundColor: "#000000",
                borderWidth: "0rem",
                borderRadius: "0.5rem",
                overflow: "hidden",
            }}
        >
            <Box
                component={"div"}
                display={"flex"}
                flexDirection={"column"}
                sx={{
                    paddingY: "0.75rem",
                    paddingX: "1.5rem",
                }}
            >
                <TypoStyled fontWeight={"700"} fontSize={"1rem"}>
                    {handle.title}
                </TypoStyled>
                {handle.message.length > 0 ? (
                    <TypoStyled fontSize={"0.75rem"}>{handle.message}</TypoStyled>
                ) : (
                    <></>
                )}
            </Box>
            <Box
                key={"bar"}
                component={"div"}
                sx={{
                    backgroundColor:
                        handle.status == ProgressHandleStatus.inProgress
                            ? "#ffc21a" // Autodesk Gold
                            : handle.status == ProgressHandleStatus.Done
                              ? "#2bc275" // Autodesk Plant
                              : "#d74e26", // Autodesk Clay
                    bottom: "0pt",
                    left: "0pt",
                    width: `${interpProgress * 100}%`,
                    height: "0.5rem",
                }}
            />
        </Box>
    )
}

function ProgressNotifications() {
    const [progressElements, updateProgressElements] = useReducer(() => {
        return handleMap.size > 0
            ? [...handleMap.entries()].map(([_, handle]) =>
                <ProgressNotification handle={handle} key={handle.handleId} />
            ) : undefined
    }, undefined)

    useEffect(() => {
        const onHandleUpdate = (e: ProgressEvent) => {
            const handle = e.handle
            if (handle.status > 0) {
                setTimeout(() => handleMap.delete(handle.handleId) && updateProgressElements(), 2000)
            }
            handleMap.set(handle.handleId, handle)
            updateProgressElements()
        }

        ProgressEvent.AddListener(onHandleUpdate)
        return () => {
            ProgressEvent.RemoveListener(onHandleUpdate)
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
                gap: "0.5rem",
            }}
        >
            {progressElements ?? <></>}
        </Box>
    )
}

export default ProgressNotifications
