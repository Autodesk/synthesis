import React, { useEffect, useState } from "react"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import World from "@/systems/World"
import { ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import Checkbox from "@/ui/components/Checkbox"
import Slider from "@/ui/components/Slider"
import { AiOutlineDoubleRight } from "react-icons/ai"

const RAY_MAX_LENGTH = 20.0

const PUNCH_DEFAULT = false
const PUNCH_FORCE_DEFAULT = 40.0
const PUNCH_FORCE_MAX = 200.0
const PUNCH_FORCE_MIN = 20.0

const MARK_DEFAULT = true
const MARK_RADIUS_DEFAULT = 0.05
const MARK_RADIUS_MAX = 0.1
const MARK_RADIUS_MIN = 0.01

const MARK_RADIUS_SLIDER_STEP = 0.01

function affect(
    e: MouseEvent,
    punch: boolean,
    mark: boolean,
    punchForce: number,
    markRadius: number,
    markers: THREE.Mesh[]
) {
    const origin = World.SceneRenderer.mainCamera.position

    const worldSpace = World.SceneRenderer.PixelToWorldSpace(e.clientX, e.clientY)
    const dir = worldSpace.sub(origin).normalize().multiplyScalar(RAY_MAX_LENGTH)

    const res = World.PhysicsSystem.RayCast(ThreeVector3_JoltVec3(origin), ThreeVector3_JoltVec3(dir))

    if (res) {
        if (mark) {
            const ballMesh = World.SceneRenderer.CreateSphere(
                markRadius,
                World.SceneRenderer.CreateToonMaterial(0xd6564d)
            )
            World.SceneRenderer.scene.add(ballMesh)
            const hitPoint = res.point
            ballMesh.position.set(hitPoint.GetX(), hitPoint.GetY(), hitPoint.GetZ())
            markers.push(ballMesh)
        }

        if (punch) {
            World.PhysicsSystem.GetBody(res.data.mBodyID).AddImpulse(
                ThreeVector3_JoltVec3(dir.normalize().multiplyScalar(punchForce)),
                res.point
            )
        }
    }
}

const PokerPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [punch, setPunch] = useState(PUNCH_DEFAULT)
    const [punchForce, setPunchForce] = useState(PUNCH_FORCE_DEFAULT)
    const [mark, setMark] = useState(MARK_DEFAULT)
    const [markRadius, setMarkRadius] = useState(MARK_RADIUS_DEFAULT)

    const [markers, _] = useState<THREE.Mesh[]>([])

    useEffect(() => {
        const onClick = (e: MouseEvent) => {
            affect(e, punch, mark, punchForce, markRadius, markers)
        }

        World.SceneRenderer.renderer.domElement.addEventListener("click", onClick)

        return () => {
            World.SceneRenderer.renderer.domElement.removeEventListener("click", onClick)
        }
    }, [mark, markRadius, punch, punchForce, markers])

    useEffect(() => {
        return () => {
            markers.forEach(x => {
                x.geometry.dispose()
                World.SceneRenderer.scene.remove(x)
            })
        }
    }, [markers])

    return (
        <Panel
            openLocation="bottom-right"
            name={"The Poker"}
            icon={<AiOutlineDoubleRight />}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Close"
        >
            <Checkbox label="Punch?" defaultState={PUNCH_DEFAULT} onClick={x => setPunch(x)} />
            <Slider
                label="Punch Force"
                min={PUNCH_FORCE_MIN}
                max={PUNCH_FORCE_MAX}
                value={punchForce}
                onChange={(_, x) => setPunchForce(x as number)}
            />
            <Checkbox label="Mark?" defaultState={MARK_DEFAULT} onClick={x => setMark(x)} />
            <Slider
                label="Mark Radius"
                min={MARK_RADIUS_MIN}
                max={MARK_RADIUS_MAX}
                step={MARK_RADIUS_SLIDER_STEP}
                value={markRadius}
                onChange={(_, x) => setMarkRadius(x as number)}
            />
        </Panel>
    )
}

export default PokerPanel
