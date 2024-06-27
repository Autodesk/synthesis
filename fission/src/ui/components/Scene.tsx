import "./Scene.css"
import { useEffect, useRef } from "react"
import Stats from "stats.js"
import { OrbitControls } from "three/addons/controls/OrbitControls.js"
import SceneObject from "@/systems/scene/SceneObject"
import World from "@/systems/World"

let stats: Stats | null

let controls: OrbitControls

class SceneProps {
    public useStats = false
}

function Scene({ useStats }: SceneProps) {
    const refContainer = useRef<HTMLDivElement>(null)

    useEffect(() => {
        World.InitWorld()

        if (refContainer.current) {
            console.debug("Adding ThreeJs to DOM")

            const sr = World.SceneRenderer
            sr.renderer.domElement.style.width = "100%"
            sr.renderer.domElement.style.height = "100%"

            refContainer.current.innerHTML = ""
            refContainer.current.appendChild(sr.renderer.domElement)
            window.addEventListener("resize", () => {
                sr.UpdateCanvasSize()
            })

            if (useStats && !stats) {
                console.log("Adding stat")
                stats = new Stats()
                stats.dom.style.position = "absolute"
                stats.dom.style.top = "0px"
                refContainer.current.appendChild(stats.dom)
            }

            controls = new OrbitControls(sr.mainCamera, sr.renderer.domElement)
            controls.update()

            // Bit hacky but works
            class ComponentSceneObject extends SceneObject {
                public Setup(): void {}
                public Update(): void {
                    stats?.update()
                    controls?.update()
                }
                public Dispose(): void {}
            }
            const cso = new ComponentSceneObject()
            sr.RegisterSceneObject(cso)
        }
    }, [useStats])

    return (
        <div>
            <div ref={refContainer}></div>
        </div>
    )
}

export default Scene
