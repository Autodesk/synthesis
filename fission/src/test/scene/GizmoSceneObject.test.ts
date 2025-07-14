import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import * as THREE from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { RigidNodeId } from "@/mirabuf/MirabufParser"

// Mock dependencies
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
                    GetTranslation: vi.fn(() => ({ x: 0, y: 0, z: 0 })),
                    GetRotation: vi.fn(() => ({ x: 0, y: 0, z: 0, w: 1 })),
                })),
            })),
            setBodyPositionAndRotation: vi.fn(),
        },
    },
}))

vi.mock("@/systems/input/InputSystem", () => ({
    default: {
        isKeyPressed: vi.fn(() => false),
    },
}))

vi.mock("@/util/TypeConversions", () => ({
    convertJoltMat44ToThreeMatrix4: vi.fn(() => new THREE.Matrix4()),
    convertThreeVector3ToJoltRVec3: vi.fn(() => ({ x: 0, y: 0, z: 0 })),
    convertThreeQuaternionToJoltQuat: vi.fn(() => ({ x: 0, y: 0, z: 0, w: 1 })),
}))

// Mock TransformControls
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

        // Create mock mesh
        mockMesh = new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), new THREE.MeshBasicMaterial({ color: 0x00ff00 }))

        // Create mock parent object
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
    })

    afterEach(() => {
        if (gizmoSceneObject) {
            gizmoSceneObject.dispose()
        }
    })

    describe("setTransform()", () => {
        beforeEach(() => {
            gizmoSceneObject = new GizmoSceneObject("translate", 1.0, mockMesh, mockParentObject)
        })

        test("should apply translation transformation correctly", () => {
            const targetPosition = new THREE.Vector3(5, 10, -3)
            const transform = new THREE.Matrix4().makeTranslation(targetPosition.x, targetPosition.y, targetPosition.z)

            gizmoSceneObject.setTransform(transform)

            // Test that position is applied correctly
            expect(gizmoSceneObject.obj.position.x).toBeCloseTo(targetPosition.x)
            expect(gizmoSceneObject.obj.position.y).toBeCloseTo(targetPosition.y)
            expect(gizmoSceneObject.obj.position.z).toBeCloseTo(targetPosition.z)

            // Test that the position vector matches
            expect(gizmoSceneObject.obj.position.equals(targetPosition)).toBe(true)
        })

        test("should apply rotation transformation correctly", () => {
            const rotationAngle = Math.PI / 4 // 45 degrees
            const rotationAxis = new THREE.Vector3(0, 1, 0) // Y-axis
            const transform = new THREE.Matrix4().makeRotationAxis(rotationAxis, rotationAngle)

            gizmoSceneObject.setTransform(transform)

            // Test that rotation is applied correctly
            expect(gizmoSceneObject.obj.rotation.y).toBeCloseTo(rotationAngle)
            expect(Math.abs(gizmoSceneObject.obj.rotation.x)).toBeLessThan(0.001)
            expect(Math.abs(gizmoSceneObject.obj.rotation.z)).toBeLessThan(0.001)
        })

        test("should apply combined translation and rotation correctly", () => {
            const position = new THREE.Vector3(2, 4, 6)
            const rotation = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(1, 0, 0), Math.PI / 6)
            const scale = new THREE.Vector3(1, 1, 1)

            const transform = new THREE.Matrix4().compose(position, rotation, scale)

            gizmoSceneObject.setTransform(transform)

            // Test position
            expect(gizmoSceneObject.obj.position.x).toBeCloseTo(position.x)
            expect(gizmoSceneObject.obj.position.y).toBeCloseTo(position.y)
            expect(gizmoSceneObject.obj.position.z).toBeCloseTo(position.z)

            // Test rotation (convert quaternion to euler for comparison)
            const expectedEuler = new THREE.Euler().setFromQuaternion(rotation)
            expect(gizmoSceneObject.obj.rotation.x).toBeCloseTo(expectedEuler.x)
            expect(gizmoSceneObject.obj.rotation.y).toBeCloseTo(expectedEuler.y)
            expect(gizmoSceneObject.obj.rotation.z).toBeCloseTo(expectedEuler.z)
        })

        test("should apply scaling transformation correctly", () => {
            const position = new THREE.Vector3(0, 0, 0)
            const rotation = new THREE.Quaternion(0, 0, 0, 1)
            const scale = new THREE.Vector3(2, 0.5, 3)

            const transform = new THREE.Matrix4().compose(position, rotation, scale)

            gizmoSceneObject.setTransform(transform)

            // Test that matrix contains correct scale values
            const testMatrix = gizmoSceneObject.obj.matrix
            const extractedScale = new THREE.Vector3()
            testMatrix.decompose(new THREE.Vector3(), new THREE.Quaternion(), extractedScale)

            expect(extractedScale.x).toBeCloseTo(scale.x)
            expect(extractedScale.y).toBeCloseTo(scale.y)
            expect(extractedScale.z).toBeCloseTo(scale.z)
        })

        test("should handle identity transformation", () => {
            const identityTransform = new THREE.Matrix4()

            gizmoSceneObject.setTransform(identityTransform)

            // Should set to origin position and zero rotation
            expect(gizmoSceneObject.obj.position.x).toBeCloseTo(0)
            expect(gizmoSceneObject.obj.position.y).toBeCloseTo(0)
            expect(gizmoSceneObject.obj.position.z).toBeCloseTo(0)
            expect(gizmoSceneObject.obj.rotation.x).toBeCloseTo(0)
            expect(gizmoSceneObject.obj.rotation.y).toBeCloseTo(0)
            expect(gizmoSceneObject.obj.rotation.z).toBeCloseTo(0)
        })

        test("should update object matrix correctly", () => {
            const position = new THREE.Vector3(1, 2, 3)
            const rotation = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 0, 1), Math.PI / 2)
            const scale = new THREE.Vector3(1.5, 1.5, 1.5)

            const transform = new THREE.Matrix4().compose(position, rotation, scale)

            gizmoSceneObject.setTransform(transform)

            // Test that the object's matrix matches the input transform
            const objMatrix = gizmoSceneObject.obj.matrix
            const extractedPos = new THREE.Vector3()
            const extractedRot = new THREE.Quaternion()
            const extractedScale = new THREE.Vector3()

            objMatrix.decompose(extractedPos, extractedRot, extractedScale)

            // Test position and scale with component-wise comparison
            expect(extractedPos.x).toBeCloseTo(position.x)
            expect(extractedPos.y).toBeCloseTo(position.y)
            expect(extractedPos.z).toBeCloseTo(position.z)

            expect(extractedScale.x).toBeCloseTo(scale.x)
            expect(extractedScale.y).toBeCloseTo(scale.y)
            expect(extractedScale.z).toBeCloseTo(scale.z)

            // Test rotation with component-wise comparison (quaternions can have precision issues)
            expect(extractedRot.x).toBeCloseTo(rotation.x)
            expect(extractedRot.y).toBeCloseTo(rotation.y)
            expect(extractedRot.z).toBeCloseTo(rotation.z)
            expect(extractedRot.w).toBeCloseTo(rotation.w)
        })
    })

    describe("updateNodeTransform()", () => {
        beforeEach(() => {
            gizmoSceneObject = new GizmoSceneObject("translate", 1.0, mockMesh, mockParentObject)
        })

        test("should handle missing parent gracefully", () => {
            const noParentGizmo = new GizmoSceneObject("translate", 1.0, mockMesh)
            const nodeId = "node1" as RigidNodeId

            // Should not throw
            noParentGizmo.updateNodeTransform(nodeId)

            noParentGizmo.dispose()
        })
    })

    describe("update()", () => {
        beforeEach(() => {
            gizmoSceneObject = new GizmoSceneObject("translate", 1.0, mockMesh, mockParentObject)
            gizmoSceneObject.setup()
        })

        test("should update gizmo size", () => {
            gizmoSceneObject.gizmo.object = mockMesh

            gizmoSceneObject.update()

            expect(gizmoSceneObject.gizmo.setSize).toHaveBeenCalled()
        })

        test("should update parent object transforms when dragging", () => {
            gizmoSceneObject.gizmo.dragging = true
            gizmoSceneObject.gizmo.object = mockMesh

            gizmoSceneObject.update()

            expect(mockParentObject.disablePhysics).toHaveBeenCalled()
            expect(mockParentObject.updateMeshTransforms).toHaveBeenCalled()
        })

        test("should handle missing object gracefully", () => {
            const mockGizmo = gizmoSceneObject.gizmo as unknown as { object: null }
            mockGizmo.object = null
            const consoleSpy = vi.spyOn(console, "error").mockImplementation(() => {})

            gizmoSceneObject.update()

            expect(consoleSpy).toHaveBeenCalledWith("No object added to gizmo")
            consoleSpy.mockRestore()
        })
    })
})
