import { Box } from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import {
    SceneOverlayTag,
    SceneOverlayTagAddEvent,
    SceneOverlayTagRemoveEvent,
    SceneOverlayUpdateEvent,
    SceneOverlayDisableEvent,
    SceneOverlayEnableEvent,
} from "./SceneOverlayEvents"

const tagMap = new Map<number, SceneOverlayTag>()

function SceneOverlay() {
    const [isDisabled, setIsDisabled] = useState<boolean>(false)

    /* h1 text for each tagMap tag */
    const [components, updateComponents] = useReducer(() => {
        if (isDisabled) return <></> // if the overlay is disabled, return nothing

        return [...tagMap.values()].map(x => (
            <div
                key={x.id}
                style={{
                    position: "absolute",
                    left: x.position[0],
                    top: x.position[1],
                    backgroundColor: "rgba(0, 0, 0, 0.5)",
                    borderRadius: "8px",
                    padding: "8px",
                    whiteSpace: "nowrap",
                    transform: "translate(-50%, -100%)",
                }}
            >
                <h1 className="text-2xl text-white font-sans" style={{ fontWeight: "bold" }}>
                    {x.text}
                </h1>
            </div>
        ))
    }, [])

    /* Creating listener for tag events to update tagMap and rerender overlay */
    useEffect(() => {
        const onTagAdd = (e: Event) => {
            tagMap.set((e as SceneOverlayTagAddEvent).tag.id, (e as SceneOverlayTagAddEvent).tag)
        }

        const onTagRemove = (e: Event) => {
            tagMap.delete((e as SceneOverlayTagRemoveEvent).tag.id)
        }

        const onUpdate = (_: Event) => {
            updateComponents()
        }

        const onDisable = () => {
            setIsDisabled(true)
            updateComponents()
        }

        const onEnable = () => {
            setIsDisabled(false)
            updateComponents()
        }

        // listening for tags being added and removed
        SceneOverlayTagAddEvent.Listen(onTagAdd)
        SceneOverlayTagRemoveEvent.Listen(onTagRemove)

        // listening for updates to the overlay every frame
        SceneOverlayUpdateEvent.Listen(onUpdate)

        // listening for disabling and enabling scene tags
        SceneOverlayDisableEvent.Listen(onDisable)
        SceneOverlayEnableEvent.Listen(onEnable)

        // disposing all the tags and listeners when the scene is destroyed
        return () => {
            SceneOverlayTagAddEvent.RemoveListener(onTagAdd)
            SceneOverlayTagRemoveEvent.RemoveListener(onTagRemove)
            SceneOverlayUpdateEvent.RemoveListener(onUpdate)
            tagMap.clear()
        }
    }, [])

    /* Render the overlay as a box that spans the entire screen and does not intercept any user interaction */
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
                overflow: "hidden",
                pointerEvents: "none",
            }}
        >
            {components ?? <></>}
        </Box>
    )
}

export default SceneOverlay
