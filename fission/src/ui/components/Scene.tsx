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
            sr.renderer.domElement.style.width = "100%"
            sr.renderer.domElement.style.height = "100%"

            refContainer.current.innerHTML = ""
            refContainer.current.appendChild(sr.renderer.domElement)
            window.addEventListener("resize", () => {
                sr.updateCanvasSize()
            })

            if (useStats && !stats) {
                stats = new Stats()
                stats.dom.style.position = "absolute"
                stats.dom.style.top = "0px"
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
            <div ref={refContainer}></div>
        </div>
    )
}

export default Scene
