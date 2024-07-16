import { styled, Typography } from "@mui/material"
import { Box } from "@mui/system"
import { useEffect, useReducer } from "react"
import { ProgressHandle, ProgressHandleStatus, ProgressEvent } from "./ProgressNotificationData"

const handleMap = new Map<number, ProgressHandle>()

const TypoStyled = styled(Typography)(_ => ({
    fontFamily: "Artifakt",
    textAlign: "center",
}))

function ProgressNotifications() {
    const [progressElements, updateProgressElements] = useReducer(() => {
        return handleMap.size > 0
            ? [...handleMap.entries()].map(([id, handle]) => {
                  return (
                      <Box
                          key={id}
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
                                  width: `${Math.floor(Math.min(100, Math.max(0, handle.progress * 100)))}%`,
                                  height: "0.5rem",
                              }}
                          />
                      </Box>
                  )
              })
            : undefined
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
