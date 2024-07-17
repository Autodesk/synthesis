import { styled, Typography } from "@mui/material"
import { Box } from "@mui/system"
import { useEffect, useMemo, useReducer, useState } from "react"
import { ProgressHandle, ProgressHandleStatus, ProgressEvent } from "./ProgressNotificationData"
import { easeOutQuad } from "@/util/EasingFunctions"

const handleMap = new Map<number, ProgressHandle>()

const TypoStyled = styled(Typography)(_ => ({
    fontFamily: "Artifakt",
    textAlign: "center",
}))

interface NotificationProps {
    handle: ProgressHandle
}

function Timer(startTime: number, elapse: number) {
    const [time, setTime] = useState<number>(0)

    useEffect(() => {
        const update = () => {
            const n = Math.min(1.0, Math.max(0.0, (Date.now() - startTime) / elapse))

            setTime(n)
        }

        const interval = setInterval(update, 5)

        return () => {
            clearInterval(interval)
        }
    }, [startTime, elapse])

    return time
}

function ProgressNotification({ handle }: NotificationProps) {

    const [progressData, setProgressData] = useState<[lastValue: number, currentValue: number, lastUpdate: number]>([0, 0, Date.now()])

    const timer = Timer(progressData[2], 500)

    useEffect(() => {
        setProgressData([progressData[1], handle.progress, Date.now()])
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [handle.progress])

    useEffect(() => {
        console.debug(`Updated: [${progressData[0]}, ${progressData[1]}, ${progressData[2]}]`)
    }, [progressData])

    const interpolatedProgress = Math.min(1, Math.max(0,
        progressData[0] + (progressData[1] - progressData[0]) * easeOutQuad(timer)
    ))

    console.debug(`[${progressData[0]}, ${progressData[1]}, ${progressData[2]}]`)
    console.debug(interpolatedProgress.toFixed(2))

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
                    width: `${interpolatedProgress * 100}%`,
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
