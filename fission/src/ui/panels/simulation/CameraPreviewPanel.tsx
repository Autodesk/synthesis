import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useMemo, useRef } from "react"
import RobotCameraSceneObject from "@/mirabuf/RobotCameraSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const CameraPreviewPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const canvasRefs = useRef<Map<string, HTMLCanvasElement | null>>(new Map())

    useEffect(() => {
        configureScreen(panel!, { title: "Camera Preview", hideCancel: true, acceptText: "Close" }, {})
    }, [])

    // Marks a frame consumer so the cameras actually render while this panel is open.
    useEffect(() => {
        RobotCameraSceneObject.addPreviewConsumer()
        return () => RobotCameraSceneObject.removePreviewConsumer()
    }, [])

    const cameras = useMemo<RobotCameraSceneObject[]>(
        () => World.sceneRenderer.mirabufSceneObjects.getRobots().flatMap(r => [...r.cameras]),
        []
    )

    useEffect(() => {
        let handle = requestAnimationFrame(function draw() {
            cameras.forEach(cam => {
                const dst = canvasRefs.current.get(cam.deviceName)
                const src = cam.frameCanvas
                if (!dst || !src) return
                if (dst.width !== src.width || dst.height !== src.height) {
                    dst.width = src.width
                    dst.height = src.height
                }
                dst.getContext("2d")?.drawImage(src, 0, 0)
            })
            handle = requestAnimationFrame(draw)
        })
        return () => cancelAnimationFrame(handle)
    }, [cameras])

    return (
        <Stack gap={3} alignItems="center">
            {cameras.length === 0 ? (
                <Label size="md" className="text-center">
                    No cameras configured. Add one under Configure → USB Cameras.
                </Label>
            ) : (
                cameras.map(cam => (
                    <Stack key={cam.deviceName} gap={1} alignItems="center">
                        <Label size="sm">{cam.displayName}</Label>
                        <canvas
                            ref={el => {
                                canvasRefs.current.set(cam.deviceName, el)
                            }}
                            style={{
                                display: "block",
                                width: "320px",
                                // Pin display height so the canvas can't render at its full
                                // bitmap height and slip under the panel footer.
                                aspectRatio: `${cam.width} / ${cam.height}`,
                                borderRadius: "8px",
                                background: "#000",
                            }}
                        />
                    </Stack>
                ))
            )}
        </Stack>
    )
}

export default CameraPreviewPanel
