import { server } from "@vitest/browser/context"
import { bench, describe } from "vitest"
import MirabufInstance from "@/mirabuf/MirabufInstance"
import MirabufParser from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import World from "@/systems/World"
import { getMiraAssembly } from "@/test/GetAssets"

const STANDARD_FRAME_DELTA = 1 / 60

const [dozer, multiJoint, field2023] = (await Promise.all([
    getMiraAssembly("DOZER"),
    getMiraAssembly("MULTI_JOINT"),
    getMiraAssembly(2023),
])) as [mirabuf.Assembly, mirabuf.Assembly, mirabuf.Assembly]

World.initWorld()

function makeRenderer(assemblies: mirabuf.Assembly[]): SceneRenderer {
    const renderer = new SceneRenderer()
    for (const assembly of assemblies) {
        const instance = new MirabufInstance(new MirabufParser(assembly))
        instance.addToScene(renderer.scene)
    }
    renderer.updateCanvasSize()
    return renderer
}

const emptyRenderer = makeRenderer([])
const oneRobotRenderer = makeRenderer([dozer])
const robotAndFieldRenderer = makeRenderer([dozer, field2023])
const twoRobotsAndFieldRenderer = makeRenderer([dozer, multiJoint, field2023])

// Skip on firefox: WebGL is unreliable in the firefox instance under GitHub Actions
// (same guard as MirabufRealLoad.test.ts).
describe.skipIf(server.browser === "firefox")("SceneRenderer — full frame render", () => {
    bench("render — empty scene (ground + skybox)", () => {
        emptyRenderer.update(STANDARD_FRAME_DELTA)
    })

    bench("render — 1 robot (Dozer)", () => {
        oneRobotRenderer.update(STANDARD_FRAME_DELTA)
    })

    bench("render — 1 robot + field (2023)", () => {
        robotAndFieldRenderer.update(STANDARD_FRAME_DELTA)
    })

    bench("render — 2 robots + field (2023)", () => {
        twoRobotsAndFieldRenderer.update(STANDARD_FRAME_DELTA)
    })
})
