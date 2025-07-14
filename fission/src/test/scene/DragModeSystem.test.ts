import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import DragModeSystem from "@/systems/scene/DragModeSystem"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import * as THREE from "three"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { PRIMARY_MOUSE_INTERACTION, InteractionType } from "@/systems/scene/ScreenInteractionHandler"

// Mock World to provide minimal required interface
vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return this._physicsSystem
        },
        set physicsSystem(value) {
            this._physicsSystem = value
        },
        _physicsSystem: null,
        sceneRenderer: {
            mainCamera: {
                position: { x: 0, y: 0, z: 5, set: vi.fn() },
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

import World from "@/systems/World"

// Type for World with writable physicsSystem
type WorldWithPhysicsSystem = typeof World & {
    physicsSystem: PhysicsSystem | null | undefined
}

describe("DragModeSystem Integration Tests", () => {
    let dragModeSystem: DragModeSystem
    let physicsSystem: PhysicsSystem

    beforeEach(() => {
        // Create real physics system
        physicsSystem = new PhysicsSystem()
        ;(World as WorldWithPhysicsSystem).physicsSystem = physicsSystem

        // Create real drag mode system
        dragModeSystem = new DragModeSystem()

        // Position camera for tests (using mock structure)
        World.sceneRenderer.mainCamera.position.x = 0
        World.sceneRenderer.mainCamera.position.y = 0
        World.sceneRenderer.mainCamera.position.z = 5
    })

    afterEach(() => {
        dragModeSystem.destroy()
        physicsSystem.destroy()
        ;(World as WorldWithPhysicsSystem).physicsSystem = null as unknown as PhysicsSystem
    })

    describe("Basic Functionality", () => {
        test("should enable and disable correctly", () => {
            const dispatchEventSpy = vi.spyOn(window, "dispatchEvent")

            // Enable
            dragModeSystem.enabled = true
            expect(dragModeSystem.enabled).toBe(true)
            expect(dispatchEventSpy).toHaveBeenCalledWith(
                expect.objectContaining({
                    type: "dragModeToggled",
                    detail: { enabled: true },
                })
            )

            // Disable
            dragModeSystem.enabled = false
            expect(dragModeSystem.enabled).toBe(false)
            expect(dispatchEventSpy).toHaveBeenCalledWith(
                expect.objectContaining({
                    type: "dragModeToggled",
                    detail: { enabled: false },
                })
            )

            dispatchEventSpy.mockRestore()
        })

        test("should handle update when enabled", () => {
            dragModeSystem.enabled = true
            expect(() => dragModeSystem.update(0.016)).not.toThrow()
        })

        test("should handle update when disabled", () => {
            dragModeSystem.enabled = false
            expect(() => dragModeSystem.update(0.016)).not.toThrow()
        })

        test("should handle disable drag mode event", () => {
            dragModeSystem.enabled = true

            // Simulate the event
            window.dispatchEvent(new CustomEvent("disableDragMode"))

            expect(dragModeSystem.enabled).toBe(false)
        })
    })

    describe("Cleanup", () => {
        test("should cleanup properly on destroy", () => {
            const removeEventListenerSpy = vi.spyOn(window, "removeEventListener")

            dragModeSystem.enabled = true
            dragModeSystem.destroy()

            expect(dragModeSystem.enabled).toBe(false)
            expect(removeEventListenerSpy).toHaveBeenCalledWith("disableDragMode", expect.any(Function))

            removeEventListenerSpy.mockRestore()
        })

        test("should stop dragging on destroy", () => {
            dragModeSystem.enabled = true

            // Verify we had enabled state before
            expect(dragModeSystem.enabled).toBe(true)

            dragModeSystem.destroy()

            expect(dragModeSystem.enabled).toBe(false)
        })
    })

    describe("Physics Integration", () => {
        test("should actually drag and move a cube", () => {
            // Create a physics cube
            const vertices = new Float32Array([
                -0.5, -0.5, -0.5, 0.5, -0.5, -0.5, 0.5, 0.5, -0.5, -0.5, 0.5, -0.5, -0.5, -0.5, 0.5, 0.5, -0.5, 0.5,
                0.5, 0.5, 0.5, -0.5, 0.5, 0.5,
            ])

            const shapeResult = physicsSystem.createConvexHull(vertices)
            expect(shapeResult.HasError()).toBe(false)

            const shape = shapeResult.Get()
            const body = physicsSystem.createBody(shape, 1.0, new THREE.Vector3(0, 0, 0), new THREE.Quaternion())
            const bodyId = body.GetID()
            physicsSystem.addBodyToSystem(bodyId, true)

            // Create a mock MirabufSceneObject and associate it with the body
            const mockSceneObject = {
                miraType: MiraType.ROBOT,
                loadFocusTransform: vi.fn(),
            }

            const mockAssociation = {
                sceneObject: mockSceneObject,
                isGamePiece: false,
            }

            // Mock the physics system to return our association
            const originalGetBodyAssociation = physicsSystem.getBodyAssociation
            physicsSystem.getBodyAssociation = vi.fn().mockReturnValue(mockAssociation)

            // Mock the raycasting to return our cube when clicked
            const originalRayCast = physicsSystem.rayCast
            physicsSystem.rayCast = vi.fn().mockReturnValue({
                data: { mBodyID: bodyId },
                point: { GetX: () => 0, GetY: () => 0, GetZ: () => 0 },
            })

            // Get initial position
            const physicsBody = physicsSystem.getBody(bodyId)
            const initialPos = physicsBody.GetPosition()
            const initialPosition = { x: initialPos.GetX(), y: initialPos.GetY(), z: initialPos.GetZ() }

            // Enable drag mode
            dragModeSystem.enabled = true

            // Simulate mouse click to start dragging
            const startInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            // Access the interaction handler directly
            const screenHandler = World.sceneRenderer.screenInteractionHandler
            if (screenHandler && screenHandler.interactionStart) {
                screenHandler.interactionStart(startInteraction)
            }

            // Simulate mouse movement to drag the cube
            const moveInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                movement: [100, 0] as [number, number],
            }

            if (screenHandler && screenHandler.interactionMove) {
                screenHandler.interactionMove(moveInteraction)
            }

            // Update the drag system to apply forces
            dragModeSystem.update(0.016)

            // Update physics to apply the forces
            physicsSystem.update(0.016)

            // Check that the cube has moved
            const finalPos = physicsBody.GetPosition()
            const finalPosition = { x: finalPos.GetX(), y: finalPos.GetY(), z: finalPos.GetZ() }

            // The cube should have moved from its initial position
            const hasMovedX = Math.abs(finalPosition.x - initialPosition.x) > 0.01
            const hasMovedY = Math.abs(finalPosition.y - initialPosition.y) > 0.01
            const hasMovedZ = Math.abs(finalPosition.z - initialPosition.z) > 0.01

            expect(hasMovedX || hasMovedY || hasMovedZ).toBe(true)

            // Simulate mouse release to stop dragging
            const endInteraction = {
                interactionType: PRIMARY_MOUSE_INTERACTION as InteractionType,
                position: [400, 300] as [number, number],
            }

            if (screenHandler && screenHandler.interactionEnd) {
                screenHandler.interactionEnd(endInteraction)
            }

            // Restore original methods
            physicsSystem.getBodyAssociation = originalGetBodyAssociation
            physicsSystem.rayCast = originalRayCast

            // Cleanup
            physicsSystem.destroyBodyIds(bodyId)
            shape.Release()
        })
    })

    describe("Event Handling", () => {
        test("should handle window events", () => {
            dragModeSystem.enabled = true

            // Test that the system responds to window events
            const beforeState = dragModeSystem.enabled
            window.dispatchEvent(new CustomEvent("disableDragMode"))

            expect(beforeState).toBe(true)
            expect(dragModeSystem.enabled).toBe(false)
        })

        test("should handle mouse wheel events during drag", () => {
            dragModeSystem.enabled = true

            // Create a mock wheel event
            const wheelEvent = new WheelEvent("wheel", {
                deltaY: 100,
                bubbles: true,
                cancelable: true,
            })

            // This should not throw even without active drag
            expect(() => {
                window.dispatchEvent(wheelEvent)
            }).not.toThrow()
        })
    })
})
