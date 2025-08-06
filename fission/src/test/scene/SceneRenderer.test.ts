import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import type SceneObject from "@/systems/scene/SceneObject"
import SceneRenderer, { STANDARD_CAMERA_FOV_X, STANDARD_CAMERA_FOV_Y } from "@/systems/scene/SceneRenderer"
import JOLT from "@/util/loading/JoltSyncLoader"

interface MockSceneObject {
    dispose: ReturnType<typeof vi.fn>
    update: ReturnType<typeof vi.fn>
    setup: ReturnType<typeof vi.fn>
    id: number
}

vi.mock("three", async () => {
    const actual = await vi.importActual<typeof import("three")>("three")
    return {
        ...actual,
        WebGLRenderer: vi.fn().mockImplementation(() => ({
            domElement: document.createElement("canvas"),
            setSize: vi.fn(),
            setClearColor: vi.fn(),
            setPixelRatio: vi.fn(),
            render: vi.fn(),
            dispose: vi.fn(),
            shadowMap: {
                enabled: true,
                type: actual.PCFSoftShadowMap,
            },
            capabilities: {
                maxTextureSize: 4096,
            },
            getSize: vi.fn().mockReturnValue(new actual.Vector2(1920, 1080)),
        })),
    }
})

vi.mock("@/systems/scene/SceneRenderer", () => ({
    default: {
        position: { x: 0, y: 0, z: 5 },
        updateMatrixWorld: vi.fn(),
    },
}))

vi.mock("@/systems/physics/PhysicsSystem", () => ({
    default: {
        rayCast: vi.fn(() => null),
        getBodyAssociation: vi.fn(() => null),
    },
}))

vi.mock("@/systems/scene/CameraControls", () => ({
    CustomOrbitControls: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
        update: vi.fn(),
    })),
}))

vi.mock("@/systems/scene/ScreenInteractionHandler", () => ({
    default: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
        update: vi.fn(),
        contextMenu: null,
    })),
}))

vi.mock("postprocessing", () => ({
    EffectComposer: vi.fn().mockImplementation(() => ({
        addPass: vi.fn(),
        render: vi.fn(),
        dispose: vi.fn(),
    })),
    EffectPass: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
    })),
    RenderPass: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
    })),
    SMAAEffect: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
    })),
    EdgeDetectionMode: {
        COLOR: "COLOR",
    },
}))

vi.mock("three/examples/jsm/csm/CSM.js", () => ({
    CSM: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
        remove: vi.fn(),
        update: vi.fn(),
        setupMaterial: vi.fn(),
        fade: true,
    })),
}))

Object.defineProperty(window, "innerWidth", {
    writable: true,
    configurable: true,
    value: 1920,
})

Object.defineProperty(window, "innerHeight", {
    writable: true,
    configurable: true,
    value: 1080,
})

Object.defineProperty(window, "devicePixelRatio", {
    writable: true,
    configurable: true,
    value: 1,
})

describe("SceneRenderer", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        SceneRenderer.setup()
    })

    afterEach(() => {
        if (SceneRenderer) {
            SceneRenderer.destroy()
        }
    })

    describe("Scene Object Management", () => {
        test("should setup and remove scene objects", () => {
            const mockSceneObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const id = SceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)

            SceneRenderer.removeSceneObject(id)
            expect(SceneRenderer.sceneObjects.has(id)).toBe(false)
            expect(mockSceneObject.setup).toHaveBeenCalled()
            expect(mockSceneObject.dispose).toHaveBeenCalled()
        })

        test("should remove all scene objects", () => {
            const mockSceneObject1: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }
            const mockSceneObject2: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            SceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            SceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)
            expect(SceneRenderer.sceneObjects.size).toBe(2)

            SceneRenderer.removeAllSceneObjects()
            expect(SceneRenderer.sceneObjects.size).toBe(0)
            expect(mockSceneObject1.dispose).toHaveBeenCalled()
            expect(mockSceneObject2.dispose).toHaveBeenCalled()
        })
    })

    describe("Geometry Creation", () => {
        test("should create sphere with custom material", () => {
            const customMaterial = new THREE.MeshBasicMaterial({ color: 0xff0000 })
            const sphere = SceneRenderer.createSphere(1.0, customMaterial)
            expect(sphere.material).toBe(customMaterial)
        })

        test("should create sphere with default material", () => {
            const sphere = SceneRenderer.createSphere(1.0)
            expect(sphere.material).toBeInstanceOf(THREE.MeshToonMaterial)
        })

        test("should create box with default material and correct position", () => {
            const vec3 = new JOLT.Vec3(2, 3, 4)

            const box = SceneRenderer.createBox(vec3)
            expect(box.material).toBeInstanceOf(THREE.MeshToonMaterial)
            expect(box.geometry.attributes.position.array[0]).toBe(1)
            expect(box.geometry.attributes.position.array[1]).toBe(1.5)
            expect(box.geometry.attributes.position.array[2]).toBe(2)
        })

        test("should create toon material", () => {
            const material = SceneRenderer.createToonMaterial(0xff0000, 3)
            expect(material).toBeInstanceOf(THREE.MeshToonMaterial)
            expect(material.color.getHex()).toBe(0xff0000)
        })
    })

    describe("Coordinate Conversion", () => {
        test("should convert center screen to world space", () => {
            const centerX = window.innerWidth / 2
            const centerY = window.innerHeight / 2
            const worldPos = SceneRenderer.pixelToWorldSpace(centerX, centerY)

            expect(worldPos.x).toBe(0)
            expect(worldPos.y).toBe(0)
        })

        test("should convert world to pixel space", () => {
            const worldPos = new THREE.Vector3(0, 0, 0)

            SceneRenderer.updateCanvasSize()
            const pixelPos1920 = SceneRenderer.worldToPixelSpace(worldPos)

            Object.defineProperty(window, "innerWidth", { value: 800 })
            Object.defineProperty(window, "innerHeight", { value: 600 })
            SceneRenderer.updateCanvasSize()

            const pixelPos800 = SceneRenderer.worldToPixelSpace(worldPos)
            expect(pixelPos800[0]).not.toBe(pixelPos1920[0])
            expect(pixelPos800[1]).not.toBe(pixelPos1920[1])

            Object.defineProperty(window, "innerWidth", { value: 1920 })
            Object.defineProperty(window, "innerHeight", { value: 1080 })
            SceneRenderer.updateCanvasSize()
        })
    })

    describe("Canvas Management", () => {
        test("should update camera aspect ratio based on window size", () => {
            // Windows size is already set to 1920x1080
            SceneRenderer.updateCanvasSize()

            const aspectRatio = 1920 / 1080
            expect(SceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)
            expect(SceneRenderer.mainCamera.fov).toBeCloseTo(STANDARD_CAMERA_FOV_X / aspectRatio)
        })

        test("should handle wide aspect ratios correctly", () => {
            Object.defineProperty(window, "innerWidth", { value: 3840 })
            Object.defineProperty(window, "innerHeight", { value: 1080 })

            SceneRenderer.updateCanvasSize()

            const aspectRatio = 3840 / 1080
            expect(SceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            expect(SceneRenderer.mainCamera.fov).toBeCloseTo(STANDARD_CAMERA_FOV_X / aspectRatio)
        })

        test("should handle tall aspect ratios correctly", () => {
            Object.defineProperty(window, "innerWidth", { value: 800 })
            Object.defineProperty(window, "innerHeight", { value: 1200 })

            SceneRenderer.updateCanvasSize()

            expect(SceneRenderer.mainCamera.aspect).toBeCloseTo(800 / 1200)
            expect(SceneRenderer.mainCamera.fov).toBeCloseTo(STANDARD_CAMERA_FOV_Y)
        })
    })

    describe("Lighting", () => {
        test("should switch between directional and CSM lighting modes", () => {
            SceneRenderer.changeLighting(false)
            const directionalLight = SceneRenderer.scene.children.find(child => child instanceof THREE.DirectionalLight)
            expect(directionalLight).toBeInstanceOf(THREE.DirectionalLight)

            SceneRenderer.changeLighting(true)
            const noDirectionalLight = SceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            )
            expect(noDirectionalLight).toBeUndefined()

            SceneRenderer.changeLighting(false)
            const newDirectionalLight = SceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            )
            expect(newDirectionalLight).toBeInstanceOf(THREE.DirectionalLight)
        })

        test("should handle null material gracefully", () => {
            SceneRenderer.changeLighting(true)

            expect(() => SceneRenderer.setupMaterial(null as unknown as THREE.Material)).not.toThrow()
        })
    })

    // describe("Skybox", () => {
    // TODO:
    // test("should update skybox colors", () => {
    //     const mockTheme: Partial<Theme> = {
    //         Background: {
    //             color: {
    //                 r: 0.5,
    //                 g: 0.7,
    //                 b: 0.9,
    //                 a: 1.0,
    //             },
    //             above: [],
    //         },
    //     }
    //
    //     SceneRenderer.updateSkyboxColors(mockTheme as unknown as Theme)
    //
    //     // Find the skybox in the scene
    //     const skybox = SceneRenderer.scene.children.find(
    //         child =>
    //             child instanceof THREE.Mesh &&
    //             child.material instanceof THREE.ShaderMaterial &&
    //             child.geometry instanceof THREE.SphereGeometry
    //     ) as THREE.Mesh<THREE.SphereGeometry, THREE.ShaderMaterial>
    //
    //     expect(skybox.material.uniforms.rColor.value).toBe(0.5)
    //     expect(skybox.material.uniforms.gColor.value).toBe(0.7)
    //     expect(skybox.material.uniforms.bColor.value).toBe(0.9)
    // })
    // })

    describe("Gizmo Management", () => {
        test("should register gizmos with parents", () => {
            const mockGizmo = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                hasParent: vi.fn().mockReturnValue(true),
                parentObjectId: 123,
                gizmo: { dragging: false },
            }

            SceneRenderer.registerGizmoSceneObject(mockGizmo as unknown as GizmoSceneObject)

            expect(SceneRenderer.gizmosOnMirabuf.has(123)).toBe(true)
            expect(SceneRenderer.gizmosOnMirabuf.get(123)).toBe(mockGizmo)
        })

        test("should not register gizmos without parents", () => {
            const mockGizmo = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                hasParent: vi.fn().mockReturnValue(false),
                parentObjectId: undefined,
                gizmo: { dragging: false },
            }

            const initialMapSize = SceneRenderer.gizmosOnMirabuf.size

            SceneRenderer.registerGizmoSceneObject(mockGizmo as unknown as GizmoSceneObject)

            expect(SceneRenderer.gizmosOnMirabuf.size).toBe(initialMapSize)
        })
    })

    describe("Update Loop", () => {
        test("should update all scene objects", () => {
            const mockSceneObject1: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }
            const mockSceneObject2: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            SceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            SceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)
            expect(SceneRenderer.sceneObjects.size).toBe(2)

            SceneRenderer.update(0.016)

            expect(mockSceneObject1.update).toHaveBeenCalledTimes(1)
            expect(mockSceneObject2.update).toHaveBeenCalledTimes(1)
            expect(SceneRenderer.currentCameraControls.update).toHaveBeenCalledWith(0.016)
            expect(SceneRenderer.screenInteractionHandler.update).toHaveBeenCalledWith(0.016)
        })
    })

    describe("Camera Controls", () => {
        test("should set camera controls", () => {
            const initialControls = SceneRenderer.currentCameraControls

            SceneRenderer.setCameraControls("Orbit")

            expect(initialControls.dispose).toHaveBeenCalled()
            expect(SceneRenderer.currentCameraControls).toBeDefined()
            expect(SceneRenderer.currentCameraControls).not.toBe(initialControls)
        })
    })

    describe("Field Management", () => {
        test("should not remove non-field objects when calling removeAllFields", () => {
            // Mock regular scene object - should not be removed
            const mockSceneObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            // Mock robot object - should not be removed
            const mockRobotObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
                miraType: MiraType.ROBOT,
            }

            const sceneObjectId = SceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)
            const robotObjectId = SceneRenderer.registerSceneObject(mockRobotObject as unknown as MirabufSceneObject)

            expect(SceneRenderer.sceneObjects.size).toBe(2)

            SceneRenderer.removeAllFields()

            // Both objects should still be there - neither should be disposed
            expect(SceneRenderer.sceneObjects.size).toBe(2)
            expect(SceneRenderer.sceneObjects.has(sceneObjectId)).toBe(true)
            expect(SceneRenderer.sceneObjects.has(robotObjectId)).toBe(true)

            expect(mockSceneObject.dispose).not.toHaveBeenCalled()
            expect(mockRobotObject.dispose).not.toHaveBeenCalled()
        })
    })
})
