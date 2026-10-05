import * as THREE from "three"
import { beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { CameraMode, CustomTargetControls } from "@/systems/scene/CameraControls"
import ScreenInteractionHandler, {
    type InteractionType,
    SECONDARY_MOUSE_INTERACTION,
} from "@/systems/scene/ScreenInteractionHandler"

/** A stand-in focus target that reports a fixed world position, without needing a real loaded mirabuf model. */
function createMockFocusProvider(
    miraType: MiraType,
    position: THREE.Vector3 = new THREE.Vector3()
): MirabufSceneObject {
    const provider = Object.create(MirabufSceneObject.prototype) as MirabufSceneObject
    vi.spyOn(provider, "miraType", "get").mockReturnValue(miraType)
    const withLoadFocusTransform = provider as unknown as { loadFocusTransform: (mat: THREE.Matrix4) => void }
    withLoadFocusTransform.loadFocusTransform = vi.fn((mat: THREE.Matrix4) => {
        mat.identity().setPosition(position)
    })
    return provider
}

describe("CustomTargetControls", () => {
    let camera: THREE.PerspectiveCamera
    let interactionHandler: ScreenInteractionHandler
    let controls: CustomTargetControls

    beforeEach(() => {
        camera = new THREE.PerspectiveCamera(75, 1, 0.1, 1000)
        camera.position.set(0, 0, 5)

        const mockElement = document.createElement("div")
        interactionHandler = new ScreenInteractionHandler(mockElement)

        controls = new CustomTargetControls(camera, interactionHandler)
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

    describe("Camera Mode", () => {
        test("face mode is only available when focused on a robot, not a field", () => {
            const field = createMockFocusProvider(MiraType.FIELD)
            controls.focusProvider = field
            controls.mode = CameraMode.FACE
            expect(controls.mode).toBe(CameraMode.FOLLOW)

            const robot = createMockFocusProvider(MiraType.ROBOT)
            controls.focusProvider = robot
            controls.mode = CameraMode.FACE
            expect(controls.mode).toBe(CameraMode.FACE)
        })

        test("falls back to Follow mode if the focus changes to a field while in Face mode", () => {
            const robot = createMockFocusProvider(MiraType.ROBOT)
            controls.focusProvider = robot
            controls.mode = CameraMode.FACE

            const field = createMockFocusProvider(MiraType.FIELD)
            controls.focusProvider = field

            expect(controls.mode).toBe(CameraMode.FOLLOW)
        })
    })

    describe("Face Mode", () => {
        test("camera sits at the captured position and looks toward the focused robot", () => {
            const robot = createMockFocusProvider(MiraType.ROBOT, new THREE.Vector3(0, 0, 0))
            controls.focusProvider = robot

            const positionBeforeFaceMode = camera.position.clone()
            controls.mode = CameraMode.FACE
            controls.update(1 / 60)

            // Entering Face mode should not itself relocate the camera.
            expect(camera.position.distanceTo(positionBeforeFaceMode)).toBeCloseTo(0)

            // Verify that the camera is looking toward the robot's position
            const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
            const towardRobot = new THREE.Vector3(0, 0, 0).sub(camera.position).normalize()
            expect(forward.dot(towardRobot)).toBeCloseTo(1, 2)
        })

        test("zooming moves the camera along the view axis toward the robot, clamped to zoom bounds", () => {
            const robot = createMockFocusProvider(MiraType.ROBOT, new THREE.Vector3(0, 0, 0))
            controls.focusProvider = robot
            controls.mode = CameraMode.FACE
            controls.update(1 / 60)

            const initialDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))

            controls.interactionMove({ interactionType: 0, scale: -1 })
            controls.update(1 / 60)
            const zoomedInDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(zoomedInDistance).toBeLessThan(initialDistance)

            controls.interactionMove({ interactionType: 0, scale: 1 })
            controls.update(1 / 60)
            const zoomedOutDistance = camera.position.distanceTo(new THREE.Vector3(0, 0, 0))
            expect(zoomedOutDistance).toBeGreaterThan(zoomedInDistance)
        })
    })

    describe("Secondary Drag", () => {
        test("drops focus and returns to Follow mode, even while Locked onto a robot", () => {
            const robot = createMockFocusProvider(MiraType.ROBOT, new THREE.Vector3(1, 2, 3))
            controls.focusProvider = robot
            controls.mode = CameraMode.LOCKED

            controls.interactionStart({ interactionType: SECONDARY_MOUSE_INTERACTION, position: [0, 0] })
            controls.interactionMove({ interactionType: SECONDARY_MOUSE_INTERACTION, movement: [0.1, 0.1] })

            expect(controls.mode).toBe(CameraMode.FOLLOW)
            expect(controls.focusProvider).toBeUndefined()

            controls.interactionEnd({ interactionType: SECONDARY_MOUSE_INTERACTION, position: [10, 10] })
        })
    })
})
