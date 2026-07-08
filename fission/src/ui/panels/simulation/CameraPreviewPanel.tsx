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
const PANEL_CHROME_H = 150

const CameraPreviewPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const canvasRef = useRef<HTMLCanvasElement | null>(null)
    const contentRef = useRef<HTMLDivElement | null>(null)
    const [cameras, setCameras] = useState<RobotCameraSceneObject[]>(resolveCameras)
    const [selectedIndex, setSelectedIndex] = useState(0)
    const [contentHeight, setContentHeight] = useState(0)

    useEffect(() => {
        configureScreen(
            panel!,
            {
                title: "Camera Preview",
                hideCancel: true,
                acceptText: "Close",
                width: PANEL_WIDTH,
                height: contentHeight ? contentHeight + PANEL_CHROME_H : undefined,
            },
            {}
        )
    }, [contentHeight])

    useEffect(() => {
        // cameras don't render if no consumer
        RobotCameraSceneObject.addPreviewConsumer()
        return () => RobotCameraSceneObject.removePreviewConsumer()
    }, [])

    useEffect(() => EventSystem.listen("RobotCamerasChangeEvent", () => setCameras(resolveCameras())), [])

    useEffect(() => {
        const el = contentRef.current
        if (!el) return
        const observer = new ResizeObserver(() => setContentHeight(el.offsetHeight))
        observer.observe(el)
        return () => observer.disconnect()
    }, [])

    const index = Math.min(selectedIndex, Math.max(0, cameras.length - 1))
    const selected = cameras[index]

    const scale = selected ? Math.min(BOX_W / Math.max(1, selected.width), BOX_H / Math.max(1, selected.height)) : 1
    const displayW = selected ? Math.round(selected.width * scale) : BOX_W
    const displayH = selected ? Math.round(selected.height * scale) : BOX_H

    useEffect(() => {
        let handle = requestAnimationFrame(function draw() {
            const dst = canvasRef.current
            const src = selected?.frameCanvas

            if (dst && src) {
                if (dst.width !== selected.width || dst.height !== selected.height) {
                    dst.width = selected.width
                    dst.height = selected.height
                }
                const ctx = dst.getContext("2d")
                if (ctx) {
                    ctx.clearRect(0, 0, dst.width, dst.height)
                    ctx.drawImage(src, 0, 0)
                }
            }
            handle = requestAnimationFrame(draw)
        })
        return () => cancelAnimationFrame(handle)
    }, [selected])

    if (cameras.length === 0) {
        return (
            <Label size="md" className="text-center">
                No cameras configured. Add one under Configure → USB Cameras.
            </Label>
        )
    }

    return (
        <div ref={contentRef} style={{ display: "flex", flexDirection: "column", alignItems: "center", gap: "16px" }}>
            {cameras.length > 1 && (
                <div
                    style={{
                        display: "flex",
                        flexDirection: "row",
                        flexWrap: "wrap",
                        justifyContent: "center",
                        gap: "8px",
                        width: "100%",
                    }}
                >
                    {cameras.map((cam, i) => (
                        <Button
                            key={cam.deviceName}
                            onClick={() => setSelectedIndex(i)}
                            sx={i === index ? { outline: "2px solid #2684ff" } : undefined}
                        >
                            {cam.deviceName}
                        </Button>
                    ))}
                </div>
            )}

            {selected && (
                <div style={{ display: "flex", flexDirection: "column", alignItems: "center", gap: "8px" }}>
                    <Label size="sm">{selected.displayName}</Label>
                    <canvas
                        ref={canvasRef}
                        width={selected.width}
                        height={selected.height}
                        style={{
                            display: "block",
                            width: `${displayW}px`,
                            height: `${displayH}px`,
                            flexShrink: 0,
                            borderRadius: "8px",
                            background: "#000",
                        }}
                    />
                </div>
            )}
        </div>
    )
}

export default CameraPreviewPanel
