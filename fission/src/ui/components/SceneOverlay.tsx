import { Stack } from "@mui/material"
import { useEffect, useReducer, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { useStateContext } from "../helpers/StateProviderHelpers"
import Label from "./Label"
import type { SceneOverlayTag } from "./SceneOverlayEvents"
import ViewCube from "./ViewCube"

const tagMap = new Map<number, SceneOverlayTag>()

const SceneOverlay: React.FC = () => {
    const { isMainMenuOpen } = useStateContext()
    /* State to determine if the overlay is disabled */
    const [isDisabled, setIsDisabled] = useState(false)

    /* State to determine if the ViewCube should be shown */
    const [showViewCube, setShowViewCube] = useState(PreferencesSystem.getGlobalPreference("ShowViewCube"))

    /* h1 text for each tagMap tag */
    const [components, updateComponents] = useReducer(() => {
        if (isDisabled) return null // if the overlay is disabled, return nothing

        return [...tagMap.values()].map(x => (
            <div
                key={x.id}
                style={{
                    position: "absolute",
                    left: x.position[0],
                    top: x.position[1],
                    backgroundColor: x.getCSSColor(),
                    borderRadius: "8px",
                    padding: "8px",
                    whiteSpace: "nowrap",
                    transform: "translate(-50%, -100%)",
                    color: "white",
                }}
            >
                <Label size="md">{x.text()}</Label>
            </div>
        ))
    }, [])

    /* Creating listener for tag events to update tagMap and rerender overlay */
    useEffect(() => {
        const unsubscribers: (() => void)[] = []

        // listening for tags being added and removed
        unsubscribers.push(EventSystem.listen("SceneOverlayTagAddEvent", tag => tagMap.set(tag.id, tag)))
        unsubscribers.push(EventSystem.listen("SceneOverlayTagRemoveEvent", tag => tagMap.delete(tag.id)))

        // listening for updates to the overlay every frame
        unsubscribers.push(EventSystem.listen("SceneOverlayUpdateEvent", () => updateComponents()))

        // listening for disabling and enabling scene tags
        unsubscribers.push(
            PreferencesSystem.addPreferenceEventListener("RenderSceneTags", e => {
                setIsDisabled(!e.prefValue)
                updateComponents()
            })
        )

        // disposing all the tags and listeners when the scene is destroyed
        return () => {
            unsubscribers.forEach(func => func())
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
        <Stack
            direction="row"
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
            {components}
            {showViewCube && !isMainMenuOpen && <ViewCube position={{ top: 20, right: 20 }} />}
        </Stack>
    )
}

export default SceneOverlay
