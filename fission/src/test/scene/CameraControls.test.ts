import * as THREE from "three"
import { beforeEach, describe, expect, test } from "vitest"
import { CustomOrbitControls } from "@/systems/scene/CameraControls"
import ScreenInteractionHandler, { type InteractionType } from "@/systems/scene/ScreenInteractionHandler"

describe("CustomOrbitControls", () => {
    let camera: THREE.PerspectiveCamera
    let interactionHandler: ScreenInteractionHandler
    let controls: CustomOrbitControls

    beforeEach(() => {
        camera = new THREE.PerspectiveCamera(75, 1, 0.1, 1000)
        camera.position.set(0, 0, 5)

        const mockElement = document.createElement("div")
        interactionHandler = new ScreenInteractionHandler(mockElement)

        controls = new CustomOrbitControls(camera, interactionHandler)
    })

    describe("Camera Position and Update", () => {
        test("sets simple coordinates correctly", () => {
            controls.setImmediateCoordinates({ theta: Math.PI / 2, phi: 0, r: 2.0 })
            controls.update(1 / 60)

            expect(camera.position.x).toBeCloseTo(2)
            expect(camera.position.y).toBeCloseTo(0)
            expect(camera.position.z).toBeCloseTo(0)
        })

        test("sets complex coordinates correctly", () => {
            controls.setImmediateCoordinates({ theta: Math.PI / 3, phi: Math.PI / 6, r: 4.0 })
            controls.update(1 / 60)

            expect(camera.position.distanceTo(new THREE.Vector3(0, 0, 0))).toBeCloseTo(4)

            expect(camera.position.x).toBeCloseTo(3)
            expect(camera.position.y).toBeCloseTo(-2)
            expect(camera.position.z).toBeCloseTo(1.732)
        })

        test("clamps extreme values", () => {
            // Test r (zoom) bounds - values should be clamped
            controls.setImmediateCoordinates({ r: 1000 })
            controls.update(1 / 60)
            const maxR = controls.getCurrentCoordinates().r

            controls.setImmediateCoordinates({ r: 0.001 })
            controls.update(1 / 60)
            const minR = controls.getCurrentCoordinates().r

            expect(maxR).toBeLessThan(1000)
            expect(minR).toBeGreaterThan(0.001)
            expect(minR).toBeLessThan(maxR)

            // Test phi (vertical) bounds
            controls.setImmediateCoordinates({ phi: Math.PI })
            controls.update(1 / 60)
            const maxPhi = controls.getCurrentCoordinates().phi

            controls.setImmediateCoordinates({ phi: -Math.PI })
            controls.update(1 / 60)
            const minPhi = controls.getCurrentCoordinates().phi

            expect(maxPhi).toBeLessThan(Math.PI)
            expect(minPhi).toBeGreaterThan(-Math.PI)
            expect(minPhi).toBeLessThan(maxPhi)
        })
    })

    describe("Mouse Interaction", () => {
        const simulateMouseInteraction = (options: {
            startPosition: [number, number]
            movement?: [number, number]
            scale?: number
            updateFrames: number
            endPosition: [number, number]
            interactionType?: InteractionType
        }) => {
            const { startPosition, movement, scale, updateFrames, endPosition, interactionType = 0 } = options

            controls.interactionStart({
                interactionType,
                position: startPosition,
            })

            if (movement || scale !== undefined) {
                controls.interactionMove({
                    interactionType,
                    movement,
                    scale,
                })
            }

            for (let i = 0; i < updateFrames; i++) {
                controls.update(1 / 60)
            }

            controls.interactionEnd({
                interactionType,
                position: endPosition,
            })

            for (let i = 0; i < 10; i++) {
                controls.update(1 / 60)
            }
        }

        beforeEach(() => {
            controls.setImmediateCoordinates({ theta: 0, phi: 0, r: 5 })
            controls.update(1 / 60)
        })

        test("simulate mouse drag", () => {
            const initialCoords = controls.getCurrentCoordinates()

            simulateMouseInteraction({
                startPosition: [100, 100],
                movement: [0.28, -0.105],
                updateFrames: 60,
                endPosition: [180, 70],
            })

            expect(controls.getCurrentCoordinates()).not.toEqual(initialCoords)

            expect(camera.position.distanceTo(new THREE.Vector3(0, 0, 0))).toBeCloseTo(5)
        })

        test("should zoom in and out correctly", () => {
            const initialDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(initialDistance).toBeCloseTo(5, 0)

            simulateMouseInteraction({
                scale: -1.0,
                updateFrames: 1,
                startPosition: [100, 100],
                endPosition: [100, 100],
            })

            const zoomedInDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))

            simulateMouseInteraction({
                scale: 2.0,
                updateFrames: 1,
                startPosition: [100, 100],
                endPosition: [100, 100],
            })

            const finalDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))

            expect(zoomedInDistance).toBeCloseTo(4, 0)
            expect(finalDistance).toBeCloseTo(5.4, 0)
        })

        test("should not update when disabled", () => {
            const initialPosition = camera.position.clone()

            controls.enabled = false

            simulateMouseInteraction({
                startPosition: [100, 100],
                movement: [0.175, 0.35],
                updateFrames: 30,
                endPosition: [150, 200],
            })

            expect(camera.position.distanceTo(initialPosition)).toBeCloseTo(0)
        })
    })
})
