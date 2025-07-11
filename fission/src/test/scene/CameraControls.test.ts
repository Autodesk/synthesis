import { expect, test, vi, beforeEach, describe } from "vitest"
import { CustomOrbitControls } from "@/systems/scene/CameraControls"
import * as THREE from "three"
import ScreenInteractionHandler from "@/systems/scene/ScreenInteractionHandler"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

// Mock dependencies
vi.mock("@/systems/preferences/PreferencesSystem", () => ({
    default: {
        getGlobalPreference: vi.fn().mockReturnValue(1.0),
    },
}))

vi.mock("@/mirabuf/MirabufSceneObject", () => ({
    default: vi.fn().mockImplementation(() => ({
        loadFocusTransform: vi.fn(),
    })),
}))

describe("CustomOrbitControls", () => {
    let camera: THREE.PerspectiveCamera
    let interactionHandler: ScreenInteractionHandler
    let controls: CustomOrbitControls

    beforeEach(() => {
        // Setup camera
        camera = new THREE.PerspectiveCamera(75, 1, 0.1, 1000)
        camera.position.set(0, 0, 5)

        // Setup interaction handler with mock DOM element
        const mockElement = document.createElement("div")
        interactionHandler = new ScreenInteractionHandler(mockElement)

        // Create controls
        controls = new CustomOrbitControls(camera, interactionHandler)
    })

    describe("Basic Properties", () => {
        test("should initialize with default values", () => {
            expect(controls.enabled).toBe(true)
            expect(controls.controlsType).toBe("Orbit")
            expect(controls.locked).toBe(false)
            expect(controls.focusProvider).toBeUndefined()
            expect(camera.fov).toBe(75)
            expect(camera.aspect).toBe(1)
            expect(camera.near).toBe(0.1)
            expect(camera.far).toBe(1000)
            expect(camera.position.equals(new THREE.Vector3(0, 0, 5))).toBe(true)
        })
    })

    describe("Coordinate Management", () => {
        test("should get current coordinates", () => {
            const coords = controls.getCurrentCoordinates()
            expect(coords).toHaveProperty("theta")
            expect(coords).toHaveProperty("phi")
            expect(coords).toHaveProperty("r")
        })

        test("should set immediate coordinates", () => {
            controls.setImmediateCoordinates({ theta: 1.0, phi: 0.5, r: 5.0 })

            const coords = controls.getCurrentCoordinates()
            expect(coords.theta).toBe(1.0)
            expect(coords.phi).toBe(0.5)
            expect(coords.r).toBe(5.0)
        })

        test("should clamp phi values within bounds", () => {
            const maxPhi = Math.PI / 2.1
            const minPhi = -Math.PI / 2.1

            controls.setImmediateCoordinates({ phi: Math.PI }) // Too high
            expect(controls.getCurrentCoordinates().phi).toBeLessThanOrEqual(maxPhi)

            controls.setImmediateCoordinates({ phi: -Math.PI }) // Too low
            expect(controls.getCurrentCoordinates().phi).toBeGreaterThanOrEqual(minPhi)
        })

        test("should clamp r values within bounds", () => {
            const maxZoom = 40.0
            const minZoom = 0.1

            controls.setImmediateCoordinates({ r: 100 }) // Too high
            expect(controls.getCurrentCoordinates().r).toBeLessThanOrEqual(maxZoom)

            controls.setImmediateCoordinates({ r: 0.01 }) // Too low
            expect(controls.getCurrentCoordinates().r).toBeGreaterThanOrEqual(minZoom)
        })
    })

    describe("Camera Position and Update", () => {
        test("should update camera position when coordinates change", () => {
            // Set initial position
            const initialPosition = camera.position.clone()

            // Set target coordinates to a different position
            controls.setTargetCoordinates({ theta: Math.PI / 2, phi: 0, r: 5.0 })

            // Update multiple times to reach target
            for (let i = 0; i < 120; i++) {
                controls.update(1 / 60)
            }

            // Camera should have moved from initial position
            expect(camera.position.equals(initialPosition)).toBe(false)

            // Camera should be moving towards the expected distance (within reasonable range)
            const distance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(distance).toBeGreaterThan(3.0) // Should be somewhere between 3 and 6
            expect(distance).toBeLessThan(6.0)
        })

        test("should update camera rotation when coordinates change", () => {
            const initialRotation = camera.rotation.clone()

            // Set target coordinates with specific rotation
            controls.setTargetCoordinates({ theta: Math.PI / 4, phi: -Math.PI / 4, r: 3.0 })

            // Update multiple times to reach target
            for (let i = 0; i < 120; i++) {
                controls.update(1 / 60)
            }

            // Camera rotation should have changed
            expect(camera.rotation.equals(initialRotation)).toBe(false)
        })

        test("should handle immediate coordinate changes", () => {
            // Set immediate coordinates
            controls.setImmediateCoordinates({ theta: 0, phi: 0, r: 2.0 })
            controls.update(1 / 60)

            // Camera should be approximately at expected position
            const distance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(distance).toBeGreaterThan(1.5) // Should be around 2
            expect(distance).toBeLessThan(2.5)

            // Camera should be generally looking towards origin (within reasonable tolerance)
            const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
            const directionToOrigin = new THREE.Vector3(0, 0, 0).sub(camera.position).normalize()
            const dotProduct = forward.dot(directionToOrigin)
            expect(dotProduct).toBeGreaterThan(0.8) // Reasonable alignment check
        })

        test("should zoom in and out correctly", () => {
            // Start at distance 5
            controls.setImmediateCoordinates({ r: 5.0 })
            controls.update(1 / 60)

            const initialDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(initialDistance).toBeGreaterThan(4.0) // Should be around 5
            expect(initialDistance).toBeLessThan(6.0)

            // Zoom in to distance 2
            controls.setTargetCoordinates({ r: 2.0 })
            for (let i = 0; i < 120; i++) {
                controls.update(1 / 60)
            }

            const finalDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(finalDistance).toBeGreaterThan(1.5) // Should be around 2-3
            expect(finalDistance).toBeLessThan(4.0)
            expect(finalDistance).toBeLessThan(initialDistance) // Should be closer than before
        })

        test("should orbit around focus point", () => {
            // Set focus point at origin
            const focusMatrix = new THREE.Matrix4().makeTranslation(0, 0, 0)
            controls.focus = focusMatrix

            // Start at theta = 0
            controls.setImmediateCoordinates({ theta: 0, phi: 0, r: 3.0 })
            controls.update(1 / 60)
            const position1 = camera.position.clone()

            // Move to theta = PI/2 (90 degrees)
            controls.setTargetCoordinates({ theta: Math.PI / 2 })
            for (let i = 0; i < 120; i++) {
                controls.update(1 / 60)
            }
            const position2 = camera.position.clone()

            // Both positions should be approximately same distance from origin
            const distance1 = position1.distanceTo(new THREE.Vector3(0, 0, 0))
            const distance2 = position2.distanceTo(new THREE.Vector3(0, 0, 0))
            const distanceDifference = Math.abs(distance1 - distance2)
            expect(distanceDifference).toBeLessThan(0.5) // Should be similar distances

            // Positions should be different (camera moved)
            expect(position1.equals(position2)).toBe(false)

            // Both should be roughly the expected distance
            expect(distance1).toBeGreaterThan(2.0)
            expect(distance1).toBeLessThan(4.0)
            expect(distance2).toBeGreaterThan(2.0)
            expect(distance2).toBeLessThan(4.0)
        })

        test("should respect coordinate bounds", () => {
            // Test r (zoom) bounds
            controls.setImmediateCoordinates({ r: 100 }) // Too far
            controls.update(1 / 60)
            expect(controls.getCurrentCoordinates().r).toBeLessThanOrEqual(40.0)

            controls.setImmediateCoordinates({ r: 0.01 }) // Too close
            controls.update(1 / 60)
            expect(controls.getCurrentCoordinates().r).toBeGreaterThanOrEqual(0.1)

            // Test phi (vertical) bounds
            controls.setImmediateCoordinates({ phi: Math.PI }) // Too high
            controls.update(1 / 60)
            expect(controls.getCurrentCoordinates().phi).toBeLessThanOrEqual(Math.PI / 2.1)

            controls.setImmediateCoordinates({ phi: -Math.PI }) // Too low
            controls.update(1 / 60)
            expect(controls.getCurrentCoordinates().phi).toBeGreaterThanOrEqual(-Math.PI / 2.1)
        })

        test("should not update when disabled", () => {
            controls.setImmediateCoordinates({ theta: 0, phi: 0, r: 3.0 })
            controls.update(1 / 60)

            const initialPosition = camera.position.clone()

            // Disable controls and try to update
            controls.enabled = false
            controls.setTargetCoordinates({ theta: Math.PI, phi: Math.PI / 4, r: 5.0 })
            controls.update(1 / 60)

            // Camera should not move significantly when disabled
            const finalPosition = camera.position.clone()
            const distance = initialPosition.distanceTo(finalPosition)
            expect(distance).toBeLessThan(0.1)
        })

        test("should clamp delta time for stability", () => {
            controls.setImmediateCoordinates({ theta: 0, phi: 0, r: 3.0 })
            controls.update(1 / 60)

            const initialPosition = camera.position.clone()

            // Test with very large delta time (should be clamped)
            controls.setTargetCoordinates({ theta: Math.PI / 2 })
            controls.update(2.0) // Very large delta time

            // Should not cause instability or huge jumps
            const finalPosition = camera.position.clone()
            const distance = initialPosition.distanceTo(finalPosition)
            expect(distance).toBeLessThan(10.0) // Reasonable movement
        })

        test("should load focus transform from provider when enabled", () => {
            const mockProvider = { loadFocusTransform: vi.fn() }
            controls.focusProvider = mockProvider as unknown as MirabufSceneObject

            controls.update(1 / 60)

            expect(mockProvider.loadFocusTransform).toHaveBeenCalled()
        })
    })

    describe("Animation", () => {
        test("should call animateToOrientation without errors", () => {
            const targetTheta = 1.5
            const targetPhi = 0.5

            // Mock requestAnimationFrame to avoid infinite recursion
            let callCount = 0
            vi.stubGlobal(
                "requestAnimationFrame",
                vi.fn(cb => {
                    callCount++
                    if (callCount < 10) {
                        // Limit calls to prevent infinite recursion
                        setTimeout(() => cb(performance.now() + callCount * 16), 0)
                    }
                })
            )

            // Should not throw errors when starting animation
            expect(() => controls.animateToOrientation(targetTheta, targetPhi, 100)).not.toThrow()

            // Verify animation was initiated
            expect(callCount).toBeGreaterThan(0)
        })
    })

    describe("Disposal", () => {
        test("should dispose without errors", () => {
            expect(() => controls.dispose()).not.toThrow()
        })
    })
})
