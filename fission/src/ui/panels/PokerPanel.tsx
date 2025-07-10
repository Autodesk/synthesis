import World from "@/systems/World"
import { JoltVec3_JoltRVec3, ThreeVector3_JoltVec3 } from "@/util/TypeConversions"
import { Checkbox, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"

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
                JoltVec3_JoltRVec3(res.point)
            )
        }
    }
}

const PokerPanel: React.FC = () => {
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
            for (const marker of markers) {
                marker.geometry.dispose()
                World.SceneRenderer.scene.remove(marker)
            }
        }
    }, [markers])

    return (
        <Stack>
            <FormControlLabel
                control={<Checkbox defaultChecked={PUNCH_DEFAULT} onChange={x => setPunch(x.target.checked)} />}
                label="Punch?"
            />
            <Stack>
                <Typography>Punch Force</Typography>
                <Slider
                    min={PUNCH_FORCE_MIN}
                    max={PUNCH_FORCE_MAX}
                    value={punchForce}
                    onChange={(_, x) => setPunchForce(x as number)}
                />
            </Stack>
            <FormControlLabel
                control={<Checkbox defaultChecked={MARK_DEFAULT} onChange={x => setMark(x.target.checked)} />}
                label="Mark?"
            />
            <Stack>
                <Typography>Mark Radius</Typography>
                <Slider
                    min={MARK_RADIUS_MIN}
                    max={MARK_RADIUS_MAX}
                    step={MARK_RADIUS_SLIDER_STEP}
                    value={markRadius}
                    onChange={(_, x) => setMarkRadius(x as number)}
                />
            </Stack>
        </Stack>
    )
}

export default PokerPanel
