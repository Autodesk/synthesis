import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem, { type SynthesisEventListener } from "@/systems/EventSystem.ts"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { CameraMode, CustomTargetControls } from "@/systems/scene/CameraControls"
import DragModeSystem from "@/systems/scene/DragModeSystem"
import { type InteractionType, PRIMARY_MOUSE_INTERACTION } from "@/systems/scene/ScreenInteractionHandler"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"

vi.mock("@/systems/World", () => ({
    default: {
        // biome-ignore lint/style/useNamingConvention: Mocking private class property
        _physicsSystem: null,
        sceneRenderer: {
            mainCamera: {
                position: { x: 0, y: 0, z: 5 },
                quaternion: { x: 0, y: 0, z: 0, w: 1 },
                fov: 75,
                aspect: 1,
                near: 0.1,
                far: 1000,
            },
            scene: {
                add: vi.fn(),
                remove: vi.fn(),
            },
            currentCameraControls: {
                enabled: true,
                coords: { theta: 0, phi: 0, r: 5 },
                focus: {
                    elements: new Array(16).fill(0),
                    makeTranslation: vi.fn(),
                    copy: vi.fn(),
                },
                focusProvider: undefined,
            },
            pixelToWorldSpace: (x: number, y: number) => ({
                x: x / 100,
                y: y / 100,
                z: 0,
                sub: vi.fn().mockReturnThis(),
                normalize: vi.fn().mockReturnThis(),
                multiplyScalar: vi.fn().mockReturnThis(),
            }),
            screenInteractionHandler: {
                interactionStart: vi.fn(),
                interactionMove: vi.fn(),
                interactionEnd: vi.fn(),
            },
            renderer: {
                domElement: {
                    parentElement: {
                        querySelector: vi.fn().mockReturnValue({
                            addEventListener: vi.fn(),
                            removeEventListener: vi.fn(),
                        }),
                    },
                },
            },
        },
    },
}))

type WorldWithPhysicsSystem = typeof World & {
    physicsSystem: PhysicsSystem | null | undefined
}

describe("DragModeSystem Integration Tests", () => {
    let dragModeSystem: DragModeSystem
    let physicsSystem: PhysicsSystem

    beforeEach(() => {
        physicsSystem = new PhysicsSystem()
        const mockWorld = World as unknown as WorldWithPhysicsSystem
        mockWorld.physicsSystem = physicsSystem

        const camera = new THREE.PerspectiveCamera(75, 1, 0.1, 1000)
        camera.position.set(0, 0, 5)
        const sceneRenderer = mockWorld.sceneRenderer as { mainCamera: THREE.PerspectiveCamera }
        sceneRenderer.mainCamera = camera

        dragModeSystem = new DragModeSystem()
    })

    afterEach(() => {
        dragModeSystem.destroy()
        physicsSystem.destroy()
    })

    describe("Event Handling", () => {
        test("takes its enabled state from the command event", () => {
            EventSystem.dispatch("SetDragModeEvent", { enabled: true })
            expect(dragModeSystem.enabled).toBe(true)

            EventSystem.dispatch("SetDragModeEvent", { enabled: false })
            expect(dragModeSystem.enabled).toBe(false)
        })

        // The top bar button mirrors the same command it dispatches, so every caller has to go
        // through the event rather than assigning `enabled` directly or the button goes stale.
        test("the command reaches listeners alongside the system", () => {
            const listener = vi.fn<SynthesisEventListener<"SetDragModeEvent">>()
            const unlisten = EventSystem.listen("SetDragModeEvent", listener)

            EventSystem.dispatch("SetDragModeEvent", { enabled: true })

            expect(listener).toHaveBeenLastCalledWith(expect.objectContaining({ enabled: true }))
            expect(dragModeSystem.enabled).toBe(true)

            unlisten()
        })
    })

    describe("Cleanup", () => {
        test("should cleanup properly on destroy", () => {
            const removeEventListenerSpy = vi.spyOn(window, "removeEventListener")

            dragModeSystem.enabled = true
            dragModeSystem.destroy()

            expect(dragModeSystem.enabled).toBe(false)
            expect(removeEventListenerSpy).toHaveBeenCalledWith("SetDragModeEvent", expect.any(Function))

            removeEventListenerSpy.mockRestore()
        })
    })

    describe("Physics Integration", () => {
        function setupDraggableCube(spawnActive: boolean = true) {
            // Create a physics cube which will then be a draggable game piece
            const vertices = new Float32Array([
                -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, 0.5, -0.5, 0.5,
                0.5, 0.5, 0.5, -0.5, 0.5, 0.5,
            ])

            const shapeResult = physicsSystem.createConvexHull(vertices)
            expect(shapeResult.HasError()).toBe(false)

            const shape = shapeResult.Get()
            const body = physicsSystem.createBody(shape, 1.0, new THREE.Vector3(0, 0, 0), new THREE.Quaternion())
            const bodyId = body.GetID()
            physicsSystem.addBodyToSystem(bodyId, spawnActive)

            // Create a mock MirabufSceneObject that properly passes `instanceof` checks
            const mockSceneObject = Object.create(MirabufSceneObject.prototype)
            mockSceneObject.loadFocusTransform = vi.fn()
            vi.spyOn(mockSceneObject, "miraType", "get").mockReturnValue(MiraType.FIELD)

            const mockAssociation = Object.create(RigidNodeAssociate.prototype)
            mockAssociation.sceneObject = mockSceneObject
            mockAssociation.rigidNode = { isGamePiece: true }

            const originalGetBodyAssociation = physicsSystem.getBodyAssociation
            physicsSystem.getBodyAssociation = vi.fn().mockReturnValue(mockAssociation)

            const originalRayCast = physicsSystem.rayCast
            physicsSystem.rayCast = vi.fn().mockImplementation(() => ({
                data: { mBodyID: bodyId },
                point: new JOLT.Vec3(0, 0, 0),
            }))

            const physicsBody = physicsSystem.getBody(bodyId)!
            dragModeSystem.enabled = true

            return {
                physicsBody,
                cleanup: () => {
                    physicsSystem.getBodyAssociation = originalGetBodyAssociation
                    physicsSystem.rayCast = originalRayCast
                    physicsSystem.destroyBodiesById(bodyId)
                    shape.Release()
                },
            }
        }

        test("should drag and move a cube when mouse is moved", () => {
            const { physicsBody, cleanup } = setupDraggableCube()

            const initialPos = physicsBody.GetPosition()
            const initialPosition = { x: initialPos.GetX(), y: initialPos.GetY(), z: initialPos.GetZ() }

            // Simulate mouse click to start dragging
            const startInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            const screenHandler = World.sceneRenderer.screenInteractionHandler
            screenHandler?.interactionStart?.(startInteraction)

            // Simulate mouse movement to drag the cube
            const moveInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                movement: [100, 0] as [number, number],
            }

            if (screenHandler && screenHandler.interactionMove) {
                screenHandler.interactionMove(moveInteraction)
            }

            // Update the drag system and physics system to apply forces
            for (let i = 0; i < 10; i++) {
                dragModeSystem.update(0.016)
                physicsSystem.update(0.016)
            }

            // Check that the cube has moved from its initial position
            const afterDragPos = physicsBody.GetPosition()
            const moved =
                Math.abs(afterDragPos.GetX() - initialPosition.x) > 0.2 ||
                Math.abs(afterDragPos.GetY() - initialPosition.y) > 0.2 ||
                Math.abs(afterDragPos.GetZ() - initialPosition.z) > 0.2
            expect(moved).toBe(true)

            // Simulate mouse release to stop dragging
            const endInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            screenHandler.interactionEnd?.(endInteraction)

            cleanup()
        })

        test("should wake and drag a sleeping game piece", () => {
            const { physicsBody, cleanup } = setupDraggableCube(false)
            expect(physicsBody.IsActive()).toBe(false)

            const initialPos = physicsBody.GetPosition()
            const initialPosition = { x: initialPos.GetX(), y: initialPos.GetY(), z: initialPos.GetZ() }

            const screenHandler = World.sceneRenderer.screenInteractionHandler
            screenHandler?.interactionStart?.({
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            })
            screenHandler?.interactionMove?.({
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                movement: [100, 0] as [number, number],
            })

            for (let i = 0; i < 10; i++) {
                dragModeSystem.update(0.016)
                physicsSystem.update(0.016)
            }

            expect(physicsBody.IsActive()).toBe(true)
            const afterDragPos = physicsBody.GetPosition()
            const moved =
                Math.abs(afterDragPos.GetX() - initialPosition.x) > 0.2 ||
                Math.abs(afterDragPos.GetY() - initialPosition.y) > 0.2 ||
                Math.abs(afterDragPos.GetZ() - initialPosition.z) > 0.2
            expect(moved).toBe(true)

            screenHandler.interactionEnd?.({
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            })

            cleanup()
        })

        test("should move cube away from camera when wheel scrolled during drag", () => {
            const { physicsBody, cleanup } = setupDraggableCube()

            const camera = World.sceneRenderer.mainCamera
            const cameraPosition = camera.position.clone()

            // Start dragging first
            const startInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            const screenHandler = World.sceneRenderer.screenInteractionHandler
            screenHandler?.interactionStart?.(startInteraction)

            // Update to establish drag state
            for (let i = 0; i < 5; i++) {
                dragModeSystem.update(0.016)
                physicsSystem.update(0.016)
            }

            const beforeScrollPos = physicsBody.GetPosition()
            const beforeScrollPosition = new THREE.Vector3(
                beforeScrollPos.GetX(),
                beforeScrollPos.GetY(),
                beforeScrollPos.GetZ()
            )
            const distanceBeforeScroll = cameraPosition.distanceTo(beforeScrollPosition)

            // Simulate mouse wheel scroll down
            const wheelEvent = new WheelEvent("wheel", {
                deltaY: 10000,
                bubbles: true,
                cancelable: true,
            })
            const wheelEventHandler = dragModeSystem["_wheelEventHandler"]
            wheelEventHandler?.(wheelEvent)

            for (let i = 0; i < 10; i++) {
                dragModeSystem.update(0.016)
                physicsSystem.update(0.016)
            }

            const afterWheelPos = physicsBody.GetPosition()
            const afterScrollPosition = new THREE.Vector3(
                afterWheelPos.GetX(),
                afterWheelPos.GetY(),
                afterWheelPos.GetZ()
            )
            const distanceAfterScroll = cameraPosition.distanceTo(afterScrollPosition)

            expect(Math.abs(distanceAfterScroll - distanceBeforeScroll)).toBeGreaterThan(0.5)

            // End dragging
            const endInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            screenHandler.interactionEnd?.(endInteraction)

            cleanup()
        })

        describe("Camera Controls Integration", () => {
            const clickInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            function installTargetControls(mode: CameraMode = CameraMode.FOLLOW): CustomTargetControls {
                const dummyHandler = {} as unknown as ConstructorParameters<typeof CustomTargetControls>[1]
                const targetControls = new CustomTargetControls(World.sceneRenderer.mainCamera, dummyHandler)
                targetControls.mode = mode

                const mockSceneRenderer = World.sceneRenderer as unknown as {
                    currentCameraControls: CustomTargetControls
                }
                mockSceneRenderer.currentCameraControls = targetControls

                return targetControls
            }

            test("disables the active Target camera controls while dragging in Follow mode, and re-enables them on release", () => {
                const targetControls = installTargetControls()
                const { cleanup } = setupDraggableCube()
                const screenHandler = World.sceneRenderer.screenInteractionHandler

                screenHandler?.interactionStart?.(clickInteraction)
                expect(targetControls.enabled).toBe(false)

                screenHandler?.interactionEnd?.(clickInteraction)
                expect(targetControls.enabled).toBe(true)

                cleanup()
            })

            test("keeps Face mode camera controls enabled throughout a drag so it can keep tracking its target", () => {
                const targetControls = installTargetControls(CameraMode.FACE)
                const { cleanup } = setupDraggableCube()
                const screenHandler = World.sceneRenderer.screenInteractionHandler

                screenHandler?.interactionStart?.(clickInteraction)
                expect(targetControls.enabled).toBe(true)

                screenHandler?.interactionEnd?.(clickInteraction)
                cleanup()
            })
        })
    })
})
