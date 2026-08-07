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
 * finished build becomes one merged robot - not two `MirabufSceneObject`s left standing - that
 * carries a session a later robot load can read back and replay.
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

    test("Two Welded Parts Finish Into One Merged Robot", async () => {
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

        const combinedBodyCount = frame.getAllBodyIds().length + pod.getAllBodyIds().length

        expect(await MixAndMatchMode.weld(frameId!, podId!)).toBe(true)
        expect(await MixAndMatchMode.finish()).toBe(true)
        expect(physicsSystem.isPaused).toBe(false)

        // The build-time scene objects are gone; exactly one merged robot took their place. (Its own
        // intake sensor scene object, registered as a side effect of Dozer having one configured,
        // rides along same as it would for any freshly spawned robot.)
        expect(scene.components.size).toBe(0)
        const mergedCandidates = [...sceneObjects.values()].filter(
            (obj): obj is MirabufSceneObject => "mirabufInstance" in obj
        )
        expect(mergedCandidates).toHaveLength(1)
        const merged = mergedCandidates[0]
        // One body fewer than the sum: the weld fuses frame's and pod's root bodies into one shared
        // RigidNode instead of leaving them as two, which is the whole point of merging.
        expect(merged.getAllBodyIds().length).toBe(combinedBodyCount - 1)

        for (let i = 0; i < STEPS; i++) physicsSystem.update(STEP)

        // The finished build carries its own session, so it can be re-opened and replayed later while
        // still reading as an ordinary robot to everything else.
        const assembly = merged.mirabufInstance.parser.assembly
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

    test("Exports A Build Without Ending It", async () => {
        const build = MixAndMatchMode.build!
        const scene = MixAndMatchMode.scene!
        const cachedBefore = MirabufCachingService.getAll(MiraType.ROBOT).length

        expect(await MixAndMatchMode.exportBuild()).toBe(true)

        // A snapshot, not an exit: the build-time scene is untouched.
        expect(MixAndMatchMode.isActive).toBe(true)
        expect(scene.components.size).toBe(2)
        expect(build.state.components.size).toBe(2)

        // Cached under the same tagged mira the build would produce on finish.
        const cachedAfter = MirabufCachingService.getAll(MiraType.ROBOT)
        expect(cachedAfter.length).toBe(cachedBefore + 1)
        const exported = cachedAfter.at(-1)!
        const assembly = await MirabufCachingService.get(exported.hash)
        expect(hasMixAndMatchSession(assembly!)).toBe(true)
    })

    test("Resumes A Build From A Saved Mira By Cache Hash", async () => {
        expect(await MixAndMatchMode.exportBuild()).toBe(true)
        const exportedHash = MirabufCachingService.getAll(MiraType.ROBOT).at(-1)!.hash

        expect(await MixAndMatchMode.resumeFrom(exportedHash)).toBe(true)

        const build = MixAndMatchMode.build!
        expect(build.state.components.size).toBe(2)
        expect(MixAndMatchMode.scene!.components.size).toBe(2)

        const welded = [...build.state.components.values()].filter(component => component.weld)
        expect(welded).toHaveLength(1)
    })

    test("Refuses To Resume From A Mira With No Saved Build", async () => {
        const info = await MirabufCachingService.cacheRemote(ROBOT_MODELS.DOZER, MiraType.ROBOT)
        const buildBefore = MixAndMatchMode.build

        expect(await MixAndMatchMode.resumeFrom(info!.hash)).toBe(false)

        // Refused before touching the current build.
        expect(MixAndMatchMode.build).toBe(buildBefore)
    })

    test("Refuses To Export While A Component Is Stranded", async () => {
        const build = MixAndMatchMode.build!
        const scene = MixAndMatchMode.scene!
        const cachedBefore = MirabufCachingService.getAll(MiraType.ROBOT).length

        // Third part is spawned but never welded to the other two, so the build resolves into two
        // separate weld trees instead of one connected robot.
        const strandedId = await MixAndMatchMode.spawnPart(dozerRef)
        expect(strandedId).toBeDefined()

        expect(await MixAndMatchMode.exportBuild()).toBe(false)

        // Nothing exported: neither the cache nor the build/scene changed as a result of the attempt.
        expect(MirabufCachingService.getAll(MiraType.ROBOT).length).toBe(cachedBefore)
        expect(build.state.components.size).toBe(3)
        expect(scene.components.size).toBe(3)

        await MixAndMatchMode.deleteComponent(strandedId!)
    })
})
