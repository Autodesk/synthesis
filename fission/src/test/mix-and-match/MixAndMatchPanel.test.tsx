import { act, getByText, render, waitFor } from "@testing-library/react"
import React from "react"
import * as THREE from "three"
import { afterEach, assert, beforeAll, beforeEach, describe, expect, test, vi } from "vitest"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import type SceneObject from "@/systems/scene/SceneObject"
import { mockConsole } from "@/test/mocks/Common"
import { ROBOT_MODELS } from "@/test/GetAssets"
import { Panel } from "@/ui/components/Panel"
import type { CloseType, PanelPosition, UIScreen } from "@/ui/helpers/UIProviderHelpers"
import MixAndMatchPanel from "@/ui/panels/mix-and-match/MixAndMatchPanel"
import { UICallback } from "@/ui/UICallbacks"
import { UIProvider } from "@/ui/UIProvider"

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
    pixelToWorldSpace: vi.fn(() => new THREE.Vector3()),
    mainCamera: new THREE.PerspectiveCamera(),
    renderer: { domElement: document.createElement("canvas") },
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

// Keeps the library list to just what this test caches, instead of the whole default manifest.
vi.mock("@/mirabuf/DefaultAssetLoader", () => ({
    default: {
        get robots() {
            return []
        },
        get fields() {
            return []
        },
    },
}))

describe("MixAndMatchPanel", () => {
    let container: HTMLElement
    let dozerRef: string

    beforeAll(async () => {
        const info = await MirabufCachingService.cacheRemote(ROBOT_MODELS.DOZER, MiraType.ROBOT)
        dozerRef = info!.hash
    })

    beforeEach(() => {
        mockConsole()
    })

    afterEach(() => {
        MixAndMatchMode.exit()
        vi.restoreAllMocks()
        container?.remove()
    })

    function renderPanel() {
        const panel = {
            id: "mix-and-match",
            content: MixAndMatchPanel,
            props: { type: "panel" as const, configured: true, position: "right" as PanelPosition, custom: undefined },
            parent: {} as UIScreen<unknown, unknown>,
            onClose: new UICallback<[CloseType], void>(),
            onCancel: new UICallback<[void], void>(),
            onAccept: new UICallback<[unknown], void>(),
            onBeforeAccept: new UICallback<[void], unknown>(),
        }

        return render(
            <UIProvider>
                <Panel panel={panel} parent={undefined}>
                    {React.createElement(panel.content)}
                </Panel>
            </UIProvider>
        ).container
    }

    test("Mounting The Panel Enters Build Mode", async () => {
        await act(async () => {
            container = renderPanel()
        })

        await waitFor(() => assert(MixAndMatchMode.isActive, "Build mode did not start"))

        expect(physicsSystem.isPaused).toBe(true)
        // The cache is shared across test files, so the library count isn't fixed; only that the part
        // this test cached is offered.
        assert(container.textContent?.includes("Part Library ("), "Library section missing")
        assert(container.textContent?.includes("Dozer"), "Cached part not offered")
        assert(getByText(container, "Placed Parts (0)") != undefined)
        assert(getByText(container, "Add a part to start building") != undefined)
    })

    test("Placing A Part Updates The Panel", async () => {
        await act(async () => {
            container = renderPanel()
        })
        await waitFor(() => assert(MixAndMatchMode.isActive, "Build mode did not start"))

        await act(async () => {
            await MixAndMatchMode.spawnPart(dozerRef)
        })

        assert(getByText(container, "Placed Parts (1)") != undefined)
        expect(MixAndMatchMode.build!.timeline.map(entry => entry.type)).toEqual(["spawn"])
    })

    test("Unmounting The Panel Leaves Build Mode And Unpauses", async () => {
        let rendered: ReturnType<typeof render>
        await act(async () => {
            rendered = render(
                <UIProvider>
                    <Panel
                        panel={{
                            id: "mix-and-match",
                            content: MixAndMatchPanel,
                            props: {
                                type: "panel" as const,
                                configured: true,
                                position: "right" as PanelPosition,
                                custom: undefined,
                            },
                            parent: {} as UIScreen<unknown, unknown>,
                            onClose: new UICallback<[CloseType], void>(),
                            onCancel: new UICallback<[void], void>(),
                            onAccept: new UICallback<[unknown], void>(),
                            onBeforeAccept: new UICallback<[void], unknown>(),
                        }}
                        parent={undefined}
                    >
                        {React.createElement(MixAndMatchPanel)}
                    </Panel>
                </UIProvider>
            )
        })
        await waitFor(() => assert(MixAndMatchMode.isActive, "Build mode did not start"))

        await act(async () => {
            rendered!.unmount()
        })

        expect(MixAndMatchMode.isActive).toBe(false)
        expect(physicsSystem.isPaused).toBe(false)
    })
})
