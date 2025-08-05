import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            mainCamera: {
                fov: 75,
                aspect: 1,
                near: 0.1,
                far: 1000,
                position: { x: 0, y: 0, z: 5, distanceTo: vi.fn(() => 5) },
            },
            renderer: { domElement: {} },
            registerGizmoSceneObject: vi.fn(),
            addObject: vi.fn(),
            removeObject: vi.fn(),
            isAnyGizmoDragging: vi.fn(() => false),
            currentCameraControls: { enabled: true },
            gizmosOnMirabuf: new Map(),
        },
        physicsSystem: {
            getBody: vi.fn(() => ({
                GetWorldTransform: vi.fn(() => ({
                    GetTranslation: vi.fn(() => ({
                        GetX: () => 0,
                        GetY: () => 0,
                        GetZ: () => 0,
                    })),
                    GetRotation: vi.fn(() => ({
                        GetX: () => 0,
                        GetY: () => 0,
                        GetZ: () => 0,
                        GetW: () => 1,
                    })),
                    GetQuaternion: vi.fn(() => ({
                        GetX: () => 0,
                        GetY: () => 0,
                        GetZ: () => 0,
                        GetW: () => 1,
                    })),
                })),
            })),
            setBodyPositionAndRotation: vi.fn(),
        },
    },
}))

vi.mock("three/examples/jsm/controls/TransformControls.js", () => ({
    TransformControls: vi.fn().mockImplementation(() => ({
        setMode: vi.fn(),
        getHelper: vi.fn(() => ({
            updateMatrixWorld: vi.fn(),
        })),
        setSpace: vi.fn(),
        attach: vi.fn(),
        detach: vi.fn(),
        setSize: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dragging: false,
        enabled: true,
        translationSnap: null,
        rotationSnap: null,
        setScaleSnap: vi.fn(),
        axis: "XYZ",
        mode: "translate",
        object: null,
    })),
}))

describe("GizmoSceneObject", () => {
    let gizmoSceneObject: GizmoSceneObject
    let mockMesh: THREE.Mesh
    let mockParentObject: MirabufSceneObject

    beforeEach(() => {
        vi.clearAllMocks()

        mockMesh = new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), new THREE.MeshBasicMaterial({ color: 0x00ff00 }))

        mockParentObject = {
            id: "test-parent-id",
            mirabufInstance: {
                parser: {
                    rigidNodes: [{ id: "node1" as RigidNodeId }, { id: "node2" as RigidNodeId }],
                },
            },
            mechanism: {
                getBodyByNodeId: vi.fn(() => "mock-body-id"),
            },
            disablePhysics: vi.fn(),
            enablePhysics: vi.fn(),
            updateMeshTransforms: vi.fn(),
        } as unknown as MirabufSceneObject

        gizmoSceneObject = new GizmoSceneObject("translate", 1.0, mockMesh, mockParentObject)
        gizmoSceneObject.setup()
    })

    afterEach(() => {
        gizmoSceneObject?.dispose()
    })

    describe("setTransform()", () => {
        test("should apply transformation with position, rotation, and scale correctly", () => {
            const position = new THREE.Vector3(1, 2, 3)
            const rotation = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 0, 1), Math.PI / 2)
            const scale = new THREE.Vector3(1.5, 1.5, 1.5)

            const transform = new THREE.Matrix4().compose(position, rotation, scale)

            gizmoSceneObject.setTransform(transform)

            const extractedPos = new THREE.Vector3()
            const extractedRot = new THREE.Quaternion()
            const extractedScale = new THREE.Vector3()

            gizmoSceneObject.obj.matrix.decompose(extractedPos, extractedRot, extractedScale)

            expect(extractedPos.x).toBeCloseTo(position.x)
            expect(extractedPos.y).toBeCloseTo(position.y)
            expect(extractedPos.z).toBeCloseTo(position.z)

            expect(extractedScale.x).toBeCloseTo(scale.x)
            expect(extractedScale.y).toBeCloseTo(scale.y)
            expect(extractedScale.z).toBeCloseTo(scale.z)

            expect(extractedRot.x).toBeCloseTo(rotation.x)
            expect(extractedRot.y).toBeCloseTo(rotation.y)
            expect(extractedRot.z).toBeCloseTo(rotation.z)
            expect(extractedRot.w).toBeCloseTo(rotation.w)
        })
    })

    describe("updateNodeTransform()", () => {
        test("should handle missing parent gracefully", () => {
            const noParentGizmo = new GizmoSceneObject("translate", 1.0, mockMesh)
            const nodeId = "node1" as RigidNodeId

            expect(() => noParentGizmo.updateNodeTransform(nodeId)).not.toThrow()

            noParentGizmo.dispose()
        })
    })

    describe("update()", () => {
        test("should update gizmo size and scale with camera distance", () => {
            gizmoSceneObject.gizmo.object = mockMesh

            gizmoSceneObject.update()
            const setSize = gizmoSceneObject.gizmo.setSize as ReturnType<typeof vi.fn>
            const initialSize = setSize.mock.calls[0][0]

            const gizmoMainCamera = gizmoSceneObject["_mainCamera"] as unknown as {
                position: { distanceTo: ReturnType<typeof vi.fn> }
            }
            gizmoMainCamera.position.distanceTo.mockReturnValue(10)

            setSize.mockClear()
            gizmoSceneObject.update()

            const newSize = setSize.mock.calls[0][0]

            expect(newSize).toBeLessThan(initialSize)
        })

        test("should update node transforms for all rigid nodes when dragging", () => {
            gizmoSceneObject.gizmo.dragging = true
            gizmoSceneObject.gizmo.object = mockMesh

            const updateNodeTransformSpy = vi.spyOn(gizmoSceneObject, "updateNodeTransform")

            gizmoSceneObject.update()

            expect(updateNodeTransformSpy).toHaveBeenCalledWith("node1")
            expect(updateNodeTransformSpy).toHaveBeenCalledWith("node2")
            expect(updateNodeTransformSpy).toHaveBeenCalledTimes(2)

            updateNodeTransformSpy.mockRestore()
        })

        test("should disable physics if dragging", () => {
            gizmoSceneObject.gizmo.dragging = true
            gizmoSceneObject.gizmo.object = mockMesh

            gizmoSceneObject.update()

            expect(mockParentObject.disablePhysics).toHaveBeenCalled()
            expect(mockParentObject.updateMeshTransforms).toHaveBeenCalled()
        })

        test("should handle gizmo without parent object gracefully", () => {
            const noParentGizmo = new GizmoSceneObject("translate", 1.0, mockMesh)
            noParentGizmo.setup()
            noParentGizmo.gizmo.object = mockMesh

            expect(() => noParentGizmo.update()).not.toThrow()

            noParentGizmo.dispose()
        })
    })
})
