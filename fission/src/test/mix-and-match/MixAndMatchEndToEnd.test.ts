import { afterAll, beforeAll, describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { hasMixAndMatchSession, readSessionFromAssembly } from "@/mix-and-match/MixAndMatchDocument"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import type SceneObject from "@/systems/scene/SceneObject"
import { ROBOT_MODELS } from "@/test/GetAssets"

const physicsSystem = new PhysicsSystem()
const sceneObjects = new Map<number, SceneObject>()
let nextSceneObjectId = 1

const mockSceneRenderer = {
    sceneObjects,
    scene: { add: vi.fn(), remove: vi.fn() },
    gizmosOnMirabuf: new Map(),
    registerSceneObject: vi.fn((obj: SceneObject) => {
        const id = nextSceneObjectId++
        obj.id = id
        sceneObjects.set(id, obj)
        obj.setup()

        return id
    }),
    removeSceneObject: vi.fn((id: number) => {
        const obj = sceneObjects.get(id)
        if (sceneObjects.delete(id)) obj?.dispose()
    }),
    createSphere: vi.fn(() => ({ material: {}, geometry: {}, position: {}, rotation: {} })),
    createToonMaterial: vi.fn(() => ({ color: 0x123456 })),
    setupMaterial: vi.fn(),
    currentCameraControls: { focusProvider: undefined, controlsType: "Target", locked: false },
    worldToPixelSpace: vi.fn(() => [0, 0]),
    mirabufSceneObjects: { getField: vi.fn() },
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return physicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
        get simulationSystem() {
            return {
                registerMechanism: vi.fn(),
                unregisterMechanism: vi.fn(),
                getSimulationLayer: vi.fn(() => ({ setBrain: vi.fn(), drivers: [], stimuli: [] })),
            }
        },
        get multiplayerSystem() {
            return undefined
        },
        get analyticsSystem() {
            return undefined
        },
    },
}))

vi.mock("@/ui/components/SceneOverlayEvents", () => ({
    SceneOverlayTag: vi.fn(() => ({ dispose: vi.fn(), color: undefined })),
}))

vi.mock("@/systems/simulation/synthesis_brain/SynthesisBrain", () => ({
    default: vi.fn(() => ({ inputSchemeName: "TestScheme", clearControls: vi.fn() })),
}))

const STEP = 1 / 60
const STEPS = 90

/**
 * The end-to-end check the whole feature rests on: place two parts, weld them, and confirm the
 * finished build carries a session a later robot load can read back and replay.
 *
 * Finishing does not yet merge the parts into one physical `mirabuf.Assembly` — see
 * MIX_AND_MATCH_RESTACK.md item 8 — so this does not assert the two parts move as one rigid body.
 */
describe("Mix and Match End to End", () => {
    let dozerRef: string

    beforeAll(async () => {
        const info = await MirabufCachingService.cacheRemote(ROBOT_MODELS.DOZER, MiraType.ROBOT)
        expect(info).toBeDefined()
        dozerRef = info!.hash
    })

    afterAll(() => {
        MixAndMatchMode.exit()
    })

    test("Two Welded Parts Finish Into A Replayable Session", async () => {
        await MixAndMatchMode.enter()
        expect(physicsSystem.isPaused).toBe(true)

        const frameId = await MixAndMatchMode.spawnPart(dozerRef)
        const podId = await MixAndMatchMode.spawnPart(dozerRef)
        expect(frameId).toBeDefined()
        expect(podId).toBeDefined()

        const scene = MixAndMatchMode.scene!
        const frame = scene.get(frameId!)!
        const pod = scene.get(podId!)!

        // Both parts share one robot layer, so a finished robot doesn't collide with itself and a
        // build isn't capped by the size of the robot layer pool.
        const frameLayer = physicsSystem.getBody(frame.getRootNodeId()!)!.GetObjectLayer()
        pod.getAllBodyIds().forEach(bodyId => {
            expect(physicsSystem.getBody(bodyId)!.GetObjectLayer()).toBe(frameLayer)
        })

        expect(await MixAndMatchMode.weld(frameId!, podId!)).toBe(true)

        const assembly = frame.mirabufInstance.parser.assembly

        expect(await MixAndMatchMode.finish()).toBe(true)
        expect(physicsSystem.isPaused).toBe(false)

        for (let i = 0; i < STEPS; i++) physicsSystem.update(STEP)

        // The finished build carries its own session, so it can be re-opened and replayed later while
        // still reading as an ordinary robot to everything else.
        expect(hasMixAndMatchSession(assembly)).toBe(true)
        const session = readSessionFromAssembly(assembly)!
        expect(session.timeline.filter(entry => entry.type === "spawn")).toHaveLength(2)
        expect(session.timeline.filter(entry => entry.type === "weld")).toHaveLength(1)
    })

    test("Reopening A Finished Build Replays It", async () => {
        const saved = readSessionFromAssembly(
            [...sceneObjects.values()]
                .map(obj => (obj as MirabufSceneObject).mirabufInstance?.parser?.assembly)
                .find(candidate => candidate && hasMixAndMatchSession(candidate))!
        )!

        MixAndMatchMode.exit()
        await MixAndMatchMode.enter(saved)

        const build = MixAndMatchMode.build!
        expect(build.timeline).toEqual(saved.timeline)
        expect(build.state.components.size).toBe(2)
        expect(MixAndMatchMode.scene!.components.size).toBe(2)

        const welded = [...build.state.components.values()].filter(component => component.weld)
        expect(welded).toHaveLength(1)
    })
})
