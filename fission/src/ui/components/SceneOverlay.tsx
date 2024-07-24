import { Box } from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import {
    SceneOverlayTag,
    SceneOverlayEvent,
    SceneOverlayEventKey,
    SceneOverlayTagEvent,
    SceneOverlayTagEventKey,
} from "./SceneOverlayEvents"
import Label, { LabelSize } from "./Label"

const tagMap = new Map<number, SceneOverlayTag>()

function SceneOverlay() {
    /* State to determine if the overlay is disabled */
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
                <Label size={LabelSize.Large}>{x.text()}</Label>
            </div>
        ))
    }, [])

    /* Creating listener for tag events to update tagMap and rerender overlay */
    useEffect(() => {
        const onTagAdd = (e: Event) => {
            tagMap.set((e as SceneOverlayTagEvent).tag.id, (e as SceneOverlayTagEvent).tag)
        }

        const onTagRemove = (e: Event) => {
            tagMap.delete((e as SceneOverlayTagEvent).tag.id)
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
        SceneOverlayTagEvent.Listen(SceneOverlayTagEventKey.ADD, onTagAdd)
        SceneOverlayTagEvent.Listen(SceneOverlayTagEventKey.REMOVE, onTagRemove)

        // listening for updates to the overlay every frame
        SceneOverlayEvent.Listen(SceneOverlayEventKey.UPDATE, onUpdate)

        // listening for disabling and enabling scene tags
        SceneOverlayEvent.Listen(SceneOverlayEventKey.DISABLE, onDisable)
        SceneOverlayEvent.Listen(SceneOverlayEventKey.ENABLE, onEnable)

        // disposing all the tags and listeners when the scene is destroyed
        return () => {
            SceneOverlayTagEvent.RemoveListener(SceneOverlayTagEventKey.ADD, onTagAdd)
            SceneOverlayTagEvent.RemoveListener(SceneOverlayTagEventKey.REMOVE, onTagRemove)
            SceneOverlayEvent.RemoveListener(SceneOverlayEventKey.UPDATE, onUpdate)
            SceneOverlayEvent.RemoveListener(SceneOverlayEventKey.DISABLE, onDisable)
            SceneOverlayEvent.RemoveListener(SceneOverlayEventKey.ENABLE, onEnable)
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
