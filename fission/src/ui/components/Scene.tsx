import "./Scene.css"
import type React from "react"
import { useEffect, useRef } from "react"
import Stats from "stats.js"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"

let stats: Stats | null

class SceneProps {
    public useStats = false
}

const Scene: React.FC<SceneProps> = ({ useStats }) => {
    const refContainer = useRef<HTMLDivElement>(null)

    useEffect(() => {
        World.initWorld()

        if (refContainer.current) {
            const sr = World.sceneRenderer

            refContainer.current.innerHTML = ""
            refContainer.current.appendChild(sr.renderer.domElement)
            sr.updateCanvasSize()
            window.addEventListener("resize", () => {
                sr.updateCanvasSize()
            })

            if (useStats && !stats) {
                stats = new Stats()
                stats.dom.style.position = "fixed"
                stats.dom.style.top = "auto"
                stats.dom.style.left = "auto"
                stats.dom.style.bottom = "16px"
                stats.dom.style.right = "16px"
                refContainer.current.appendChild(stats.dom)
            }

            // Bit hacky but works
            class ComponentSceneObject extends SceneObject {
                public setup(): void {}
                public update(): void {
                    stats?.update()
                }
                public dispose(): void {}
            }
            const cso = new ComponentSceneObject()
            sr.registerSceneObject(cso)
        }
    }, [useStats])

    return (
        <div>
            <div ref={refContainer} className="scene-container"></div>
        </div>
    )
}

export default Scene
