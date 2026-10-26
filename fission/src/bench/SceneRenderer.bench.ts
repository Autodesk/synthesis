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

interface Scene {
    renderer: SceneRenderer
    gl: WebGLRenderingContext | WebGL2RenderingContext
}

function makeScene(assemblies: mirabuf.Assembly[]): Scene {
    const renderer = new SceneRenderer()

    for (const assembly of assemblies) {
        const instance = new MirabufInstance(new MirabufParser(assembly))
        instance.addToScene(renderer.scene)
    }

    renderer.updateCanvasSize()
    const gl = renderer.renderer.getContext()

    for (let i = 0; i < 5; i++) {
        renderer.update(STANDARD_FRAME_DELTA)
        gl.finish()
    }
    return { renderer, gl }
}

const empty = makeScene([])
const oneRobot = makeScene([dozer])
const robotAndField = makeScene([dozer, field2023])
const twoRobotsAndField = makeScene([dozer, multiJoint, field2023])

function renderFrame({ renderer, gl }: Scene) {
    renderer.update(STANDARD_FRAME_DELTA)
    gl.finish()
}

// Skip on firefox: WebGL is unreliable in the firefox instance under GitHub Actions
// (same guard as MirabufRealLoad.test.ts).
describe.skipIf(server.browser === "firefox")("SceneRenderer — full frame render", () => {
    bench("render — empty scene (ground + skybox)", () => renderFrame(empty))
    bench("render — 1 robot (Dozer)", () => renderFrame(oneRobot))
    bench("render — 1 robot + field (2023)", () => renderFrame(robotAndField))
    bench("render — 2 robots + field (2023)", () => renderFrame(twoRobotsAndField))
})
