import { Box, Typography } from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import {
    type SceneOverlayTag,
    SceneOverlayEvent,
    SceneOverlayEventKey,
    SceneOverlayTag,
    SceneOverlayTagEvent,
    SceneOverlayTagEventKey,
} from "./SceneOverlayEvents"
import ViewCube from "./ViewCube"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const tagMap = new Map<number, SceneOverlayTag>()

const SceneOverlay: React.FC = () => {
    /* State to determine if the overlay is disabled */
    const [isDisabled, setIsDisabled] = useState(false)

    /* State to determine if the ViewCube should be shown */
    const [showViewCube, setShowViewCube] = useState(PreferencesSystem.getGlobalPreference("ShowViewCube"))

    /* Check if the main menu modal is active */
    // TODO:
    const isMainMenuOpen = false

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
                <Typography variant="h3">{x.text()}</Typography>
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

        // listening for tags being added and removed
        SceneOverlayTagEvent.listen(SceneOverlayTagEventKey.ADD, onTagAdd)
        SceneOverlayTagEvent.listen(SceneOverlayTagEventKey.REMOVE, onTagRemove)

        // listening for updates to the overlay every frame
        SceneOverlayEvent.listen(SceneOverlayEventKey.UPDATE, onUpdate)

        // listening for disabling and enabling scene tags
        const unsubscribe = PreferencesSystem.addPreferenceEventListener("RenderSceneTags", e => {
            setIsDisabled(!e.prefValue)
            updateComponents()
        })

        // disposing all the tags and listeners when the scene is destroyed
        return () => {
            SceneOverlayTagEvent.removeListener(SceneOverlayTagEventKey.ADD, onTagAdd)
            SceneOverlayTagEvent.removeListener(SceneOverlayTagEventKey.REMOVE, onTagRemove)
            SceneOverlayEvent.removeListener(SceneOverlayEventKey.UPDATE, onUpdate)
            unsubscribe()
            tagMap.clear()
        }
    }, [])

    /* Update ViewCube visibility when preferences change */
    useEffect(() => {
        const removeListener = PreferencesSystem.addPreferenceEventListener("ShowViewCube", e =>
            setShowViewCube(e.prefValue)
        )

        return () => {
            removeListener()
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
