import * as THREE from "three"
import { beforeEach, describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import PartPickingMode, { type PartSelection, setPointerNdc } from "@/systems/scene/PartPickingMode"

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            mainCamera: undefined,
            renderer: {
                domElement: {
                    addEventListener: vi.fn(),
                    removeEventListener: vi.fn(),
                    getBoundingClientRect: vi.fn(),
                },
            },
            scene: {
                add: vi.fn(),
                remove: vi.fn(),
            },
        },
    },
}))

class TestPickingMode extends PartPickingMode<PartSelection> {
    public constructor() {
        super(new THREE.Color(), vi.fn(), vi.fn())
    }

    protected handlePick(): void {}

    public pick(mousePos: [number, number]) {
        return this.pickPart(mousePos)
    }
}

function createBatch(): THREE.BatchedMesh {
    const geometry = new THREE.BoxGeometry(1, 1, 1)
    const batch = new THREE.BatchedMesh(1, geometry.attributes.position.count, geometry.index!.count)
    const geometryId = batch.addGeometry(geometry)
    const instanceId = batch.addInstance(geometryId)
    batch.setMatrixAt(instanceId, new THREE.Matrix4())
    return batch
}

function createSceneObject(mesh: THREE.BatchedMesh, guid: string): MirabufSceneObject {
    return {
        mirabufInstance: {
            batches: [mesh],
            meshes: new Map([[guid, [[mesh, 0]]]]),
        },
    } as unknown as MirabufSceneObject
}

describe("PartPickingMode", () => {
    beforeEach(() => {
        const sceneRenderer = World.sceneRenderer as unknown as {
            mainCamera: THREE.PerspectiveCamera
            renderer: { domElement: HTMLCanvasElement }
            screenInteractionHandler: {
                interactionStart: ((interaction: unknown) => void) | undefined
                interactionEnd: ((interaction: unknown) => void) | undefined
            }
            scene: THREE.Scene
        }
        const canvas = document.createElement("canvas")
        Object.defineProperty(canvas, "getBoundingClientRect", {
            value: () => ({ left: 100, top: 50, width: 400, height: 300 }),
        })
        sceneRenderer.mainCamera = new THREE.PerspectiveCamera(90, 4 / 3, 0.1, 100)
        sceneRenderer.mainCamera.position.set(0, 0, 5)
        sceneRenderer.mainCamera.lookAt(0, 0, 0)
        sceneRenderer.mainCamera.updateMatrixWorld()
        sceneRenderer.renderer.domElement = canvas
        sceneRenderer.screenInteractionHandler = {
            interactionStart: undefined,
            interactionEnd: undefined,
        }
        sceneRenderer.scene = new THREE.Scene()
    })

    test("maps client coordinates relative to the canvas into NDC", () => {
        const ndc = setPointerNdc(new THREE.Vector2(), [300, 200], {
            left: 100,
            top: 50,
            width: 400,
            height: 300,
        })

        expect(ndc.x).toBeCloseTo(0)
        expect(ndc.y).toBeCloseTo(0)

        setPointerNdc(ndc, [100, 50], { left: 100, top: 50, width: 400, height: 300 })
        expect(ndc.x).toBe(-1)
        expect(ndc.y).toBe(1)
    })

    test("retargets the pick index to replacement batches after apply", () => {
        const oldBatch = createBatch()
        const replacementBatch = createBatch()
        const oldObject = createSceneObject(oldBatch, "old-part")
        const replacementObject = createSceneObject(replacementBatch, "replacement-part")
        World.sceneRenderer.scene.add(replacementBatch)
        World.sceneRenderer.scene.updateMatrixWorld(true)

        const mode = new TestPickingMode()
        mode.enable(oldObject)
        mode.finishApply(replacementObject)

        expect(mode.sceneObject).toBe(replacementObject)
        expect(mode.pick([300, 200])?.guid).toBe("replacement-part")

        mode.destroy()
        oldBatch.dispose()
        replacementBatch.dispose()
    })

    test("clears selections when configuration is cancelled", () => {
        const batch = createBatch()
        const sceneObject = createSceneObject(batch, "part")
        const mode = new TestPickingMode()
        mode.enable(sceneObject)
        mode.pending.addPart("part", { guid: "part", highlight: { mesh: batch, instanceId: 0 } })

        mode.cancel()

        expect(mode.pendingCount).toBe(0)
        expect(mode.enabled).toBe(false)
        batch.dispose()
    })
})
