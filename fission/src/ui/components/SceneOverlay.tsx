import { Box } from "@mui/material"
import { useEffect, useReducer } from "react"
import { SceneOverlayTag, SceneOverlayTagAddEvent, SceneOverlayTagRemoveEvent } from "./SceneOverlayEvents"

const tagMap = new Map<number, SceneOverlayTag>()

function SceneOverlay() {
    /* h1 text for each tagMap tag */
    const [components, updateComponents] = useReducer(() => {
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

        // listening for tags being added and removed
        SceneOverlayTagAddEvent.Listen(onTagAdd)
        SceneOverlayTagRemoveEvent.Listen(onTagRemove)

        // listening for updates to the overlay every frame
        SceneOverlayUpdateEvent.Listen(onUpdate)

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

export class SceneOverlayUpdateEvent extends Event {
    private static readonly EVENT_KEY = "SceneOverlayUpdateEvent"

    public constructor() {
        super(SceneOverlayUpdateEvent.EVENT_KEY)

        window.dispatchEvent(this)
    }

    public static Listen(func: (e: Event) => void) {
        window.addEventListener(SceneOverlayUpdateEvent.EVENT_KEY, func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener(SceneOverlayUpdateEvent.EVENT_KEY, func)
    }
}
