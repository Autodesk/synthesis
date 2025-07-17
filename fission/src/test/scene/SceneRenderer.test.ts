import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import * as THREE from "three"
import { Theme } from "@/ui/helpers/UseThemeHelpers"
import SceneObject from "@/systems/scene/SceneObject"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"

interface MockSceneObject extends Partial<SceneObject> {
    dispose: ReturnType<typeof vi.fn>
    update: ReturnType<typeof vi.fn>
    setup: ReturnType<typeof vi.fn>
    id: number
}

interface MockVec3 {
    /* eslint-disable @typescript-eslint/naming-convention */
    GetX(): number
    GetY(): number
    GetZ(): number
    /* eslint-enable @typescript-eslint/naming-convention */
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

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            mainCamera: {
                position: { x: 0, y: 0, z: 5 },
                updateMatrixWorld: vi.fn(),
            },
            pixelToWorldSpace: vi.fn(() => new THREE.Vector3(0, 0, 0)),
        },
        physicsSystem: {
            rayCast: vi.fn(() => null),
            getBodyAssociation: vi.fn(() => null),
        },
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
    let sceneRenderer: SceneRenderer
    const originalConsoleLog = console.log

    beforeEach(() => {
        vi.clearAllMocks()
        sceneRenderer = new SceneRenderer()
        console.log = vi.fn()
    })

    afterEach(() => {
        if (sceneRenderer) {
            sceneRenderer.destroy()
        }
        console.log = originalConsoleLog
    })

    describe("Scene Object Management", () => {
        test("should setup and remove scene objects", () => {
            const mockSceneObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const id = sceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)

            sceneRenderer.removeSceneObject(id)
            expect(sceneRenderer.sceneObjects.has(id)).toBe(false)
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

            sceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            sceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)
            expect(sceneRenderer.sceneObjects.size).toBe(2)

            sceneRenderer.removeAllSceneObjects()
            expect(sceneRenderer.sceneObjects.size).toBe(0)
            expect(mockSceneObject1.dispose).toHaveBeenCalled()
            expect(mockSceneObject2.dispose).toHaveBeenCalled()
        })
    })

    describe("Geometry Creation", () => {
        test("should create sphere with custom material", () => {
            const customMaterial = new THREE.MeshBasicMaterial({ color: 0xff0000 })
            const sphere = sceneRenderer.createSphere(1.0, customMaterial)
            expect(sphere.material).toBe(customMaterial)
        })

        test("should create sphere with default material", () => {
            const sphere = sceneRenderer.createSphere(1.0)
            expect(sphere.material).toBeInstanceOf(THREE.MeshToonMaterial)
        })

        test("should create box with default material and correct position", () => {
            // eslint-disable-next-line @typescript-eslint/naming-convention
            const mockVec3: MockVec3 = { GetX: () => 2, GetY: () => 3, GetZ: () => 4 }
            const box = sceneRenderer.createBox(mockVec3 as unknown as Parameters<typeof sceneRenderer.createBox>[0])
            expect(box.material).toBeInstanceOf(THREE.MeshToonMaterial)
            expect(box.geometry.attributes.position.array[0]).toBe(1)
            expect(box.geometry.attributes.position.array[1]).toBe(1.5)
            expect(box.geometry.attributes.position.array[2]).toBe(2)
        })

        test("should create toon material", () => {
            const material = sceneRenderer.createToonMaterial(0xff0000, 3)
            expect(material).toBeInstanceOf(THREE.MeshToonMaterial)
            expect(material.color.getHex()).toBe(0xff0000)
        })
    })

    describe("Coordinate Conversion", () => {
        test("should convert center screen to world space", () => {
            const centerX = window.innerWidth / 2
            const centerY = window.innerHeight / 2
            const worldPos = sceneRenderer.pixelToWorldSpace(centerX, centerY)

            expect(worldPos.x).toBe(0)
            expect(worldPos.y).toBe(0)
        })

        test("should convert world to pixel space", () => {
            const pixelPos = sceneRenderer.worldToPixelSpace(new THREE.Vector3(0, 0, 0))
            expect(pixelPos[0]).toBeCloseTo(1861, 0)
            expect(pixelPos[1]).toBeCloseTo(1261, 0)
        })
    })

    describe("Canvas Management", () => {
        test("should update camera aspect ratio based on window size", () => {
            // Windows size is already set to 1920x1080
            sceneRenderer.updateCanvasSize()

            const aspectRatio = 1920 / 1080
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)
            expect(sceneRenderer.mainCamera.fov).toBeCloseTo(110 / aspectRatio)
        })

        test("should handle wide aspect ratios correctly", () => {
            Object.defineProperty(window, "innerWidth", { value: 3840 })
            Object.defineProperty(window, "innerHeight", { value: 1080 })

            sceneRenderer.updateCanvasSize()

            const aspectRatio = 3840 / 1080
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            expect(sceneRenderer.mainCamera.fov).toBeCloseTo(110 / aspectRatio)
        })

        test("should handle tall aspect ratios correctly", () => {
            Object.defineProperty(window, "innerWidth", { value: 800 })
            Object.defineProperty(window, "innerHeight", { value: 1200 })

            sceneRenderer.updateCanvasSize()

            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(800 / 1200)
            expect(sceneRenderer.mainCamera.fov).toBeCloseTo(110 / (16 / 9))
        })
    })

    describe("Lighting", () => {
        test("should switch between directional and CSM lighting modes", () => {
            sceneRenderer.changeLighting(false)
            const directionalLight = sceneRenderer.scene.children.find(child => child instanceof THREE.DirectionalLight)
            expect(directionalLight).toBeInstanceOf(THREE.DirectionalLight)

            sceneRenderer.changeLighting(true)
            const noDirectionalLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            )
            expect(noDirectionalLight).toBeUndefined()

            sceneRenderer.changeLighting(false)
            const newDirectionalLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            )
            expect(newDirectionalLight).toBeInstanceOf(THREE.DirectionalLight)
        })

        test("should handle null material gracefully", () => {
            sceneRenderer.changeLighting(true)

            expect(() => sceneRenderer.setupMaterial(null as unknown as THREE.Material)).not.toThrow()
        })
    })

    describe("Scene Management", () => {
        test("should add object to scene and verify it exists", () => {
            const mockObject = new THREE.Object3D()
            mockObject.name = "TestObject"

            sceneRenderer.addObject(mockObject)

            expect(sceneRenderer.scene.children).toContain(mockObject)
            expect(sceneRenderer.scene.getObjectByName("TestObject")).toBe(mockObject)
        })

        test("should remove object from scene and verify it no longer exists", () => {
            const mockObject = new THREE.Object3D()
            mockObject.name = "TestObjectToRemove"

            sceneRenderer.addObject(mockObject)
            sceneRenderer.removeObject(mockObject)

            expect(sceneRenderer.scene.children).not.toContain(mockObject)
            expect(sceneRenderer.scene.getObjectByName("TestObjectToRemove")).toBeUndefined()
        })

        test("should handle adding same object multiple times gracefully", () => {
            const mockObject = new THREE.Object3D()

            const initialChildCount = sceneRenderer.scene.children.length

            sceneRenderer.addObject(mockObject)
            sceneRenderer.addObject(mockObject)

            expect(sceneRenderer.scene.children.length).toBe(initialChildCount + 1)
        })

        test("should handle removing non-existent object gracefully", () => {
            const mockObject = new THREE.Object3D()
            const initialChildCount = sceneRenderer.scene.children.length

            // Try to remove an object that was never added
            sceneRenderer.removeObject(mockObject)

            // Scene should remain unchanged
            expect(sceneRenderer.scene.children.length).toBe(initialChildCount)
        })

        test("should maintain scene hierarchy when adding child objects", () => {
            const parentObject = new THREE.Object3D()
            parentObject.name = "Parent"

            const childObject = new THREE.Object3D()
            childObject.name = "Child"

            parentObject.add(childObject)

            // Add parent to scene
            sceneRenderer.addObject(parentObject)

            // Verify both parent and child are accessible through the scene
            expect(sceneRenderer.scene.getObjectByName("Parent")).toBe(parentObject)
            expect(sceneRenderer.scene.getObjectByName("Child")).toBe(childObject)
            expect(childObject.parent).toBe(parentObject)
        })

        test("should handle removing parent object and its children", () => {
            const parentObject = new THREE.Object3D()
            parentObject.name = "ParentToRemove"

            const childObject = new THREE.Object3D()
            childObject.name = "ChildToRemove"

            parentObject.add(childObject)
            sceneRenderer.addObject(parentObject)
            sceneRenderer.removeObject(parentObject)

            expect(sceneRenderer.scene.getObjectByName("ParentToRemove")).toBeUndefined()
            expect(sceneRenderer.scene.getObjectByName("ChildToRemove")).toBeUndefined()
        })
    })

    describe("Skybox", () => {
        test("should update skybox colors", () => {
            const mockTheme: Partial<Theme> = {
                Background: {
                    color: {
                        r: 0.5,
                        g: 0.7,
                        b: 0.9,
                        a: 1.0,
                    },
                    above: [],
                },
            }

            sceneRenderer.updateSkyboxColors(mockTheme as unknown as Theme)

            // Find the skybox in the scene
            const skybox = sceneRenderer.scene.children.find(
                child =>
                    child instanceof THREE.Mesh &&
                    child.material instanceof THREE.ShaderMaterial &&
                    child.geometry instanceof THREE.SphereGeometry
            ) as THREE.Mesh<THREE.SphereGeometry, THREE.ShaderMaterial>

            expect(skybox.material.uniforms.rColor.value).toBe(0.5)
            expect(skybox.material.uniforms.gColor.value).toBe(0.7)
            expect(skybox.material.uniforms.bColor.value).toBe(0.9)
        })
    })

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

            sceneRenderer.registerGizmoSceneObject(mockGizmo as unknown as GizmoSceneObject)

            expect(sceneRenderer.gizmosOnMirabuf.has(123)).toBe(true)
            expect(sceneRenderer.gizmosOnMirabuf.get(123)).toBe(mockGizmo)
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

            const initialMapSize = sceneRenderer.gizmosOnMirabuf.size

            sceneRenderer.registerGizmoSceneObject(mockGizmo as unknown as GizmoSceneObject)

            expect(sceneRenderer.gizmosOnMirabuf.size).toBe(initialMapSize)
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

            sceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            sceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)
            expect(sceneRenderer.sceneObjects.size).toBe(2)

            sceneRenderer.update(0.016)

            expect(mockSceneObject1.update).toHaveBeenCalledTimes(1)
            expect(mockSceneObject2.update).toHaveBeenCalledTimes(1)
            expect(sceneRenderer.currentCameraControls.update).toHaveBeenCalledWith(0.016)
            expect(sceneRenderer.screenInteractionHandler.update).toHaveBeenCalledWith(0.016)
        })
    })

    describe("Camera Controls", () => {
        test("should set camera controls", () => {
            const initialControls = sceneRenderer.currentCameraControls

            sceneRenderer.setCameraControls("Orbit")

            expect(initialControls.dispose).toHaveBeenCalled()
            expect(sceneRenderer.currentCameraControls).toBeDefined()
            expect(sceneRenderer.currentCameraControls).not.toBe(initialControls)
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

            const sceneObjectId = sceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)
            const robotObjectId = sceneRenderer.registerSceneObject(mockRobotObject as unknown as MirabufSceneObject)

            expect(sceneRenderer.sceneObjects.size).toBe(2)

            sceneRenderer.removeAllFields()

            // Both objects should still be there - neither should be disposed
            expect(sceneRenderer.sceneObjects.size).toBe(2)
            expect(sceneRenderer.sceneObjects.has(sceneObjectId)).toBe(true)
            expect(sceneRenderer.sceneObjects.has(robotObjectId)).toBe(true)

            expect(mockSceneObject.dispose).not.toHaveBeenCalled()
            expect(mockRobotObject.dispose).not.toHaveBeenCalled()
        })
    })
})
