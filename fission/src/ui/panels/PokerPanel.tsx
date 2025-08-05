import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type * as THREE from "three"
import World from "@/systems/World"
import { convertJoltVec3ToJoltRVec3, convertThreeVector3ToJoltVec3 } from "@/util/TypeConversions"
import Checkbox from "../components/Checkbox"
import type { PanelImplProps } from "../components/Panel"
import StatefulSlider from "../components/StatefulSlider"
import { useUIContext } from "../helpers/UIProviderHelpers"

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

const PokerPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [punch, setPunch] = useState(PUNCH_DEFAULT)
    const [punchForce, setPunchForce] = useState(PUNCH_FORCE_DEFAULT)
    const [mark, setMark] = useState(MARK_DEFAULT)
    const [markRadius, setMarkRadius] = useState(MARK_RADIUS_DEFAULT)

    const [markers, _] = useState<THREE.Mesh[]>([])

    useEffect(() => {
        const onClick = (e: MouseEvent) => {
            affect(e, punch, mark, punchForce, markRadius, markers)
        }

        console.log(punch, mark)

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)

        return () => {
            World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
        }
    }, [mark, markRadius, punch, punchForce, markers])

    useEffect(() => {
        return () => {
            for (const marker of markers) {
                marker.geometry.dispose()
                World.sceneRenderer.scene.remove(marker)
            }
        }
    }, [markers])

    useEffect(() => {
        configureScreen(panel!, { title: "The Poker", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <Stack>
            <Checkbox label="Punch?" checked={punch} onClick={setPunch} />
            <StatefulSlider
                label="Punch Force"
                min={PUNCH_FORCE_MIN}
                max={PUNCH_FORCE_MAX}
                defaultValue={punchForce}
                onChange={x => setPunchForce(x as number)}
            />
            <Checkbox label="Mark?" checked={mark} onClick={setMark} />
            <StatefulSlider
                label="Mark Radius"
                min={MARK_RADIUS_MIN}
                max={MARK_RADIUS_MAX}
                step={MARK_RADIUS_SLIDER_STEP}
                defaultValue={markRadius}
                onChange={x => setMarkRadius(x as number)}
            />
        </Stack>
    )
}

export default PokerPanel
