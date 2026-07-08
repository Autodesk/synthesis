import type React from "react"
import { useEffect, useRef, useState } from "react"
import RobotCameraSceneObject from "@/mirabuf/RobotCameraSceneObject"
import EventSystem from "@/systems/EventSystem"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

function resolveCameras(): RobotCameraSceneObject[] {
    return World.sceneRenderer.mirabufSceneObjects.getRobots().flatMap(r => [...r.cameras])
}

const BOX_W = 480
const BOX_H = 360
const PANEL_WIDTH = BOX_W + 40
const PANEL_HEIGHT = `min(${BOX_H + 200}px, 85vh)`

const CameraPreviewPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const previewRef = useRef<HTMLDivElement | null>(null)
    const [cameras, setCameras] = useState<RobotCameraSceneObject[]>(resolveCameras)
    const [selectedIndex, setSelectedIndex] = useState(0)

    useEffect(() => {
        configureScreen(
            panel!,
            {
                title: "Camera Preview",
                hideCancel: true,
                acceptText: "Close",
                width: PANEL_WIDTH,
                height: PANEL_HEIGHT,
            },
            {}
        )
    }, [configureScreen, panel])

    useEffect(() => {
        // cameras don't render if no consumer
        RobotCameraSceneObject.addPreviewConsumer()
        return () => RobotCameraSceneObject.removePreviewConsumer()
    }, [])

    useEffect(() => EventSystem.listen("RobotCamerasChangeEvent", () => setCameras(resolveCameras())), [])

    const index = Math.min(selectedIndex, Math.max(0, cameras.length - 1))
    const selected = cameras[index]

    useEffect(() => {
        const preview = previewRef.current
        const canvas = selected?.frameCanvas
        if (!preview || !canvas) return

        Object.assign(canvas.style, {
            display: "block",
            position: "absolute",
            top: "50%",
            left: "50%",
            transform: "translate(-50%, -50%)",
            maxWidth: "100%",
            maxHeight: "100%",
            width: "auto",
            height: "auto",
            borderRadius: "8px",
            background: "#000",
        })
        preview.appendChild(canvas)

        return () => canvas.remove()
    }, [selected])

    if (cameras.length === 0) {
        return (
            <Label size="md" className="text-center">
                No cameras configured. Add one under Configure → USB Cameras.
            </Label>
        )
    }

    return (
        <div
            style={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                gap: "16px",
                width: "100%",
                height: "100%",
                minHeight: 0,
            }}
        >
            {cameras.length > 1 && (
                <div
                    style={{
                        display: "flex",
                        flexDirection: "row",
                        alignItems: "center",
                        justifyContent: "center",
                        gap: "8px",
                        width: "100%",
                        flexShrink: 0,
                        position: "relative",
                        zIndex: 1,
                    }}
                >
                    <Button onClick={() => setSelectedIndex((index - 1 + cameras.length) % cameras.length)}>
                        Previous
                    </Button>
                    <Label size="sm">
                        {index + 1} / {cameras.length}
                    </Label>
                    <Button onClick={() => setSelectedIndex((index + 1) % cameras.length)}>Next</Button>
                </div>
            )}

            {selected && (
                <div
                    style={{
                        display: "flex",
                        flex: "1 1 auto",
                        flexDirection: "column",
                        alignItems: "center",
                        gap: "8px",
                        width: "100%",
                        minHeight: 0,
                        overflow: "hidden",
                    }}
                >
                    <Label size="sm">{selected.displayName}</Label>
                    <div
                        ref={previewRef}
                        style={{
                            position: "relative",
                            flex: "1 1 0",
                            width: "100%",
                            minHeight: 0,
                            background: "#000",
                            borderRadius: "8px",
                        }}
                    />
                </div>
            )}
        </div>
    )
}

export default CameraPreviewPanel
