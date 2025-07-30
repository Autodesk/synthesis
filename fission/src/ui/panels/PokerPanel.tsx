import React, { useEffect, useState } from "react"
import * as THREE from "three"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import Slider from "@/ui/components/Slider"
import { convertJoltVec3ToJoltRVec3, convertThreeVector3ToJoltVec3 } from "@/util/TypeConversions"
import { SynthesisIcons } from "../components/StyledComponents"

const RAY_MAX_LENGTH = 20.0

const PUNCH_DEFAULT = false
const PUNCH_FORCE_DEFAULT = 40.0
const PUNCH_FORCE_MAX = 200.0
const PUNCH_FORCE_MIN = 1.0

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
    const origin = World.sceneRenderer.mainCamera.position

    const worldSpace = World.sceneRenderer.pixelToWorldSpace(e.clientX, e.clientY)
    const dir = worldSpace.sub(origin).normalize().multiplyScalar(RAY_MAX_LENGTH)

    const res = World.physicsSystem.rayCast(convertThreeVector3ToJoltVec3(origin), convertThreeVector3ToJoltVec3(dir))

    if (res) {
        if (mark) {
            const ballMesh = World.sceneRenderer.createSphere(
                markRadius,
                World.sceneRenderer.createToonMaterial(0xd6564d)
            )
            World.sceneRenderer.scene.add(ballMesh)
            const hitPoint = res.point
            ballMesh.position.set(hitPoint.GetX(), hitPoint.GetY(), hitPoint.GetZ())
            markers.push(ballMesh)
        }

        if (punch) {
            World.physicsSystem
                .getBody(res.data.mBodyID)
                .AddImpulse(
                    convertThreeVector3ToJoltVec3(dir.normalize().multiplyScalar(punchForce)),
                    convertJoltVec3ToJoltRVec3(res.point)
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

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)

        return () => {
            World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
        }
    }, [mark, markRadius, punch, punchForce, markers])

    useEffect(() => {
        return () => {
            markers.forEach(x => {
                x.geometry.dispose()
                World.sceneRenderer.scene.remove(x)
            })
        }
    }, [markers])

    return (
        <Panel
            openLocation="bottom-right"
            name={"The Poker"}
            icon={SynthesisIcons.OUTLINED_DOUBLE_RIGHT}
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
