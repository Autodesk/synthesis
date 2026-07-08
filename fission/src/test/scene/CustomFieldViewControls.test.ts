import * as THREE from "three"
import { beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { CameraPoint } from "@/systems/preferences/PreferenceTypes"
import {
    CameraMode,
    CustomFieldViewControls,
    CustomTargetControls,
    getTargetControls,
} from "@/systems/scene/CameraControls"
import ScreenInteractionHandler, {
    PRIMARY_MOUSE_INTERACTION,
    SECONDARY_MOUSE_INTERACTION,
} from "@/systems/scene/ScreenInteractionHandler"
import World from "@/systems/World"

/** A stand-in mirabuf object that reports a fixed world position, without needing a real loaded model. */
function createMockSceneObject(miraType: MiraType, position: THREE.Vector3): MirabufSceneObject {
    const object = Object.create(MirabufSceneObject.prototype) as MirabufSceneObject
    vi.spyOn(object, "miraType", "get").mockReturnValue(miraType)
    const withPositionTransform = object as unknown as {
        getPositionTransform: (vec?: THREE.Vector3) => THREE.Vector3
    }
    withPositionTransform.getPositionTransform = vi
        .fn()
        .mockImplementation((vec: THREE.Vector3 = new THREE.Vector3()) => vec.copy(position))
    return object
}

/** Registers a single camera point on the field and anchors the controls to it. */
function anchorToPoint(controls: CustomFieldViewControls, field: MirabufSceneObject, point: CameraPoint): void {
    vi.spyOn(field, "fieldPreferences", "get").mockReturnValue({
        cameraPoints: [point],
    } as unknown as MirabufSceneObject["fieldPreferences"])
    controls.selectPoint(field, 0)
}

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            mirabufSceneObjects: {
                getAll: vi.fn().mockReturnValue([]),
            },
            currentCameraControls: undefined,
            setCameraControls: vi.fn(),
        },
    },
}))

describe("CustomFieldViewControls", () => {
    let camera: THREE.PerspectiveCamera
    let interactionHandler: ScreenInteractionHandler
    let controls: CustomFieldViewControls
    let field: MirabufSceneObject
    const fieldPosition = new THREE.Vector3(10, 0, 20)

    beforeEach(() => {
        vi.clearAllMocks()

        camera = new THREE.PerspectiveCamera(75, 1, 0.1, 1000)
        camera.position.set(0, 0, 5)

        const mockElement = document.createElement("div")
        interactionHandler = new ScreenInteractionHandler(mockElement)

        controls = new CustomFieldViewControls(camera, interactionHandler)

        field = createMockSceneObject(MiraType.FIELD, fieldPosition)
        vi.mocked(World.sceneRenderer.mirabufSceneObjects.getAll).mockReturnValue([field])
    })

    describe("selectPoint", () => {
        test("anchors the camera to the field-relative offset of the point", () => {
            const point: CameraPoint = { name: "Station 1", pos: [1, 2, 3], look: { type: "field" } }

            anchorToPoint(controls, field, point)
            controls.update(1 / 60)

            expect(camera.position.x).toBeCloseTo(fieldPosition.x + 1)
            expect(camera.position.y).toBeCloseTo(fieldPosition.y + 2)
            expect(camera.position.z).toBeCloseTo(fieldPosition.z + 3)
        })
    })

    describe("look direction", () => {
        test("a 'rotation' point uses its authored yaw/pitch directly", () => {
            const point: CameraPoint = {
                name: "Fixed",
                pos: [0, 0, 0],
                look: { type: "rotation", yaw: Math.PI / 4, pitch: -0.2 },
            }

            anchorToPoint(controls, field, point)
            controls.update(1 / 60)

            expect(camera.rotation.y).toBeCloseTo(Math.PI / 4)
            expect(camera.rotation.x).toBeCloseTo(-0.2)
        })

        test("a 'field' point aims the camera at the field's center", () => {
            const point: CameraPoint = { name: "Overview", pos: [5, 5, 0], look: { type: "field" } }

            anchorToPoint(controls, field, point)
            controls.update(1 / 60)

            const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
            const towardField = fieldPosition.clone().sub(camera.position).normalize()
            expect(forward.dot(towardField)).toBeCloseTo(1, 2)
        })
    })

    describe("focusRobot", () => {
        test("aims at a focused robot, without moving the camera", () => {
            const point: CameraPoint = { name: "Overview", pos: [5, 5, 0], look: { type: "field" } }
            anchorToPoint(controls, field, point)
            controls.update(1 / 60)
            const anchoredPosition = camera.position.clone()

            const robotPosition = new THREE.Vector3(-3, 0, -3)
            const robot = createMockSceneObject(MiraType.ROBOT, robotPosition)
            controls.focusRobot(robot)
            controls.update(1 / 60)

            expect(controls.focusedRobot).toBe(robot)
            const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
            const towardRobot = robotPosition.clone().sub(camera.position).normalize()
            expect(forward.dot(towardRobot)).toBeCloseTo(1, 2)
            expect(camera.position.distanceTo(anchoredPosition)).toBeCloseTo(0)
        })

        test("keeps its anchor fixed as the focused robot moves, only re-aiming (no chase)", () => {
            const point: CameraPoint = { name: "Overview", pos: [5, 5, 0], look: { type: "field" } }
            anchorToPoint(controls, field, point)

            const robotPosition = new THREE.Vector3(-3, 0, -3)
            const robot = createMockSceneObject(MiraType.ROBOT, robotPosition)
            controls.focusRobot(robot)
            controls.update(1 / 60)
            const anchoredPosition = camera.position.clone()

            // Robot drives away; the mock reports its live position via the shared vector reference.
            robotPosition.set(15, 0, 15)
            for (let i = 0; i < 60; i++) controls.update(1 / 60)

            // The anchor must not follow the robot...
            expect(camera.position.distanceTo(anchoredPosition)).toBeCloseTo(0)
            // ...but the camera should still be aimed at the robot's new position.
            const forward = new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
            const towardRobot = robotPosition.clone().sub(camera.position).normalize()
            expect(forward.dot(towardRobot)).toBeCloseTo(1, 2)
        })
    })

    describe("zoom", () => {
        test("scrolling zooms in and out from the look target", () => {
            const point: CameraPoint = { name: "Overview", pos: [5, 0, 5], look: { type: "field" } }
            anchorToPoint(controls, field, point)
            controls.update(1 / 60)
            const initialDistance = camera.position.distanceTo(fieldPosition)

            controls.interactionMove({ interactionType: PRIMARY_MOUSE_INTERACTION, scale: -1 })
            controls.update(1 / 60)
            const zoomedInDistance = camera.position.distanceTo(fieldPosition)

            controls.interactionMove({ interactionType: PRIMARY_MOUSE_INTERACTION, scale: 1 })
            controls.update(1 / 60)
            const zoomedOutDistance = camera.position.distanceTo(fieldPosition)

            expect(zoomedInDistance).toBeLessThan(initialDistance)
            expect(zoomedOutDistance).toBeGreaterThan(zoomedInDistance)
        })
    })

    describe("Target controls interaction", () => {
        let targetInteractionHandler: ScreenInteractionHandler
        let targetControls: CustomTargetControls
        const mockSceneRenderer = World.sceneRenderer as unknown as {
            currentCameraControls: CustomTargetControls | CustomFieldViewControls
        }

        beforeEach(() => {
            targetInteractionHandler = new ScreenInteractionHandler(document.createElement("div"))
            targetControls = new CustomTargetControls(camera, targetInteractionHandler)
        })

        test("a secondary-button drag hands control to Follow controls without a visible jump, then continues the pan", () => {
            const point: CameraPoint = { name: "Overview", pos: [0, 0, 5], look: { type: "field" } }
            anchorToPoint(controls, field, point)
            controls.update(1 / 60)
            const positionBeforeHandoff = camera.position.clone()

            mockSceneRenderer.currentCameraControls = targetControls

            controls.interactionStart({ interactionType: SECONDARY_MOUSE_INTERACTION, position: [100, 100] })
            controls.interactionMove({
                interactionType: SECONDARY_MOUSE_INTERACTION,
                movement: [10, 0],
            })

            expect(World.sceneRenderer.setCameraControls).toHaveBeenCalledWith("Target")
            expect(targetControls.mode).toBe(CameraMode.Follow)
            expect(targetControls.focusProvider).toBeUndefined()
            expect(camera.position.distanceTo(positionBeforeHandoff)).toBeLessThan(1)

            const positionAfterHandoffMove = camera.position.clone()
            targetControls.update(1 / 60)
            targetControls.interactionMove({ interactionType: SECONDARY_MOUSE_INTERACTION, movement: [10, 0] })
            targetControls.update(1 / 60)
            expect(camera.position.distanceTo(positionAfterHandoffMove)).toBeGreaterThan(0)
        })

        test("only returns the active controls when they are Target controls", () => {
            mockSceneRenderer.currentCameraControls = controls
            expect(getTargetControls()).toBeUndefined()

            mockSceneRenderer.currentCameraControls = targetControls
            expect(getTargetControls()).toBe(targetControls)
        })
    })
})
