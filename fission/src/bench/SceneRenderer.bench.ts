import { server } from "@vitest/browser/context"
import { bench, describe } from "vitest"
import MirabufInstance from "@/mirabuf/MirabufInstance"
import MirabufParser from "@/mirabuf/MirabufParser"
import type { mirabuf } from "@/proto/mirabuf"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import World from "@/systems/World"
import { getMiraAssembly } from "@/test/GetAssets"

const STANDARD_FRAME_DELTA = 1 / 60

// Top-level await: assemblies load once before any bench runs. beforeAll does not fire
// in Vitest browser bench mode (same workaround as the other .bench.ts files).
const [dozer, multiJoint, field2023] = (await Promise.all([
    getMiraAssembly("DOZER"),
    getMiraAssembly("MULTI_JOINT"),
    getMiraAssembly(2023),
])) as [mirabuf.Assembly, mirabuf.Assembly, mirabuf.Assembly]

// MirabufInstance sets its materials up against World.sceneRenderer during construction,
// so a live World is required even though each scenario renders on its own SceneRenderer.
World.initWorld()

interface Scene {
    renderer: SceneRenderer
    gl: WebGLRenderingContext | WebGL2RenderingContext
}

// Build a standalone renderer with the given assemblies' geometry added directly to its
// scene. We bypass MirabufSceneObject/physics on purpose: this isolates the per-frame
// *render* cost (draw calls, shadow map, postprocessing) — what PerformanceMonitor reacts
// to and what dominates a real frame. Each scenario owns its own WebGL context so the
// scenes stay independent and directly comparable.
function makeScene(assemblies: mirabuf.Assembly[]): Scene {
    const renderer = new SceneRenderer()
    for (const assembly of assemblies) {
        const instance = new MirabufInstance(new MirabufParser(assembly))
        instance.addToScene(renderer.scene)
    }
    renderer.updateCanvasSize()

    const gl = renderer.renderer.getContext()
    // Warm up: the first frames compile shaders and allocate GPU buffers (hundreds of ms).
    // Render a few and drain the GPU so that one-time cost never lands in a sample.
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

// composer.render() only queues GPU commands; gl.finish() blocks until they complete, so
// the timed region reflects the true frame cost instead of command-submission noise.
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
