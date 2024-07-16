import { Box } from "@mui/material"
import { useEffect, useReducer } from "react"
import { SceneOverlayTag, SceneOverlayTagEvent } from "./SceneOverlayEvents"

const tagMap = new Map<number, SceneOverlayTag>()

function SceneOverlay() {

    // const [components, updateComponents] = useReducer(() => {
    //     return [...tagMap.values()].map(x => <h1 key={x.id}>{x.text}</h1>)
    // }, )

    useEffect(() => {
        const onTagUpdate = (e: Event) => {
            const tagEvent = e as SceneOverlayTagEvent
            const tag = tagEvent.tag
            tagMap.set(tag.id, tag)
        }

        SceneOverlayTagEvent.Listen(onTagUpdate)

        return () => {
            SceneOverlayTagEvent.RemoveListener(onTagUpdate)
        }
    }, [])

    return (
        <Box
            component="div"
            display="flex"
            sx={{
                position: "fixed",
                left: "0pt",
                top: "0pt",
                width: "100vw",
                height: "100vh",
                overflow: "hidden"
            }}
        >
            { <></> }
        </Box>
    )
}

export default SceneOverlay