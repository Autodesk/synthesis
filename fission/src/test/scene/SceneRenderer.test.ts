import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import * as THREE from "three"
import { Theme } from "@/ui/helpers/UseThemeHelpers"
import SceneObject from "@/systems/scene/SceneObject"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"

// Mock interfaces for testing
interface MockSceneObject extends Partial<SceneObject> {
    dispose: ReturnType<typeof vi.fn>
    update: ReturnType<typeof vi.fn>
    setup: ReturnType<typeof vi.fn>
    id: number
}

interface MockVec3 {
    GetX(): number
    GetY(): number
    GetZ(): number
}

// Mock WebGL Renderer to prevent WebGL context creation in CI environments
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

// Mock dependencies
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

vi.mock("@/ui/components/SceneOverlayEvents", () => ({
    SceneOverlayEvent: vi.fn(),
    SceneOverlayEventKey: {
        UPDATE: "UPDATE",
    },
}))

vi.mock("@/ui/components/TouchControls", () => ({
    TouchControlsEvent: vi.fn(),
    TouchControlsEventKeys: {
        PLACE_BUTTON: "PLACE_BUTTON",
    },
}))

vi.mock("@/ui/components/GlobalUIControls", () => ({
    globalAddToast: vi.fn(),
    globalOpenPanel: vi.fn(),
    globalOpenModal: vi.fn(),
    setAddToast: vi.fn(),
    setOpenPanel: vi.fn(),
    setOpenModal: vi.fn(),
}))

vi.mock("@/ui/ToastContext", () => ({
    ToastType: {
        Info: "info",
        Error: "error",
        Success: "success",
        Warning: "warning",
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

vi.mock("@/mirabuf/MirabufSceneObject", () => ({
    default: vi.fn().mockImplementation(() => ({
        miraType: "ROBOT",
        dispose: vi.fn(),
        update: vi.fn(),
        setup: vi.fn(),
        getSupplierData: vi.fn(() => ({ title: "Test", items: [] })),
    })),
}))

vi.mock("@/systems/scene/GizmoSceneObject", () => ({
    default: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
        update: vi.fn(),
        setup: vi.fn(),
        hasParent: vi.fn(() => false),
        parentObjectId: 1,
        gizmo: { dragging: false },
    })),
}))

vi.mock("@/systems/scene/SceneObject", () => ({
    default: vi.fn().mockImplementation(() => ({
        dispose: vi.fn(),
        update: vi.fn(),
        setup: vi.fn(),
        id: 1,
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

// Mock DOM elements
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
        test("should register scene objects", () => {
            const mockSceneObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const id = sceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)
            expect(id).toBe(1)
            expect(mockSceneObject.id).toBe(1)
            expect(mockSceneObject.setup).toHaveBeenCalled()
            expect(sceneRenderer.sceneObjects.has(id)).toBe(true)
        })

        test("should remove scene objects", () => {
            const mockSceneObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const id = sceneRenderer.registerSceneObject(mockSceneObject as unknown as SceneObject)
            expect(sceneRenderer.sceneObjects.has(id)).toBe(true)

            sceneRenderer.removeSceneObject(id)
            expect(sceneRenderer.sceneObjects.has(id)).toBe(false)
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
        test("should create sphere with default material", () => {
            const sphere = sceneRenderer.createSphere(1.0)
            expect(sphere).toBeInstanceOf(THREE.Mesh)
            expect(sphere.geometry).toBeInstanceOf(THREE.SphereGeometry)
            expect(sphere.material).toBeInstanceOf(THREE.MeshToonMaterial)
        })

        test("should create sphere with custom material", () => {
            const customMaterial = new THREE.MeshBasicMaterial({ color: 0xff0000 })
            const sphere = sceneRenderer.createSphere(1.0, customMaterial)
            expect(sphere.material).toBe(customMaterial)
        })

        test("should create box with default material", () => {
            const mockVec3: MockVec3 = { GetX: () => 2, GetY: () => 3, GetZ: () => 4 }
            const box = sceneRenderer.createBox(mockVec3 as unknown as Parameters<typeof sceneRenderer.createBox>[0])
            expect(box).toBeInstanceOf(THREE.Mesh)
            expect(box.geometry).toBeInstanceOf(THREE.BoxGeometry)
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
        test("should convert pixel to world space", () => {
            const worldPos = sceneRenderer.pixelToWorldSpace(960, 540)
            expect(worldPos).toBeInstanceOf(THREE.Vector3)

            expect(worldPos.x).toBe(0)
            expect(worldPos.y).toBe(0)
        })

        test("should convert (0, 0) pixel to world space", () => {
            const worldPos = sceneRenderer.pixelToWorldSpace(0, 0)
            expect(worldPos).toBeInstanceOf(THREE.Vector3)

            expect(worldPos.x).toBeCloseTo(-0.4, 1)
            expect(worldPos.y).toBeCloseTo(0.2, 1)
        })

        test("should convert world to pixel space", () => {
            const worldPos = new THREE.Vector3(0, 0, 0)
            const pixelPos = sceneRenderer.worldToPixelSpace(worldPos)
            expect(Array.isArray(pixelPos)).toBe(true)
            expect(pixelPos[0]).toBeCloseTo(1861, 0)
            expect(pixelPos[1]).toBeCloseTo(1261, 0)
        })
    })

    describe("Canvas Management", () => {
        test("should update camera aspect ratio based on window size", () => {
            const initialAspect = sceneRenderer.mainCamera.aspect

            // Change to different aspect ratio
            Object.defineProperty(window, "innerWidth", { value: 1200 })
            Object.defineProperty(window, "innerHeight", { value: 800 })

            sceneRenderer.updateCanvasSize()

            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(1.5) // 1200/800
            expect(sceneRenderer.mainCamera.aspect).not.toBe(initialAspect)
        })

        test("should handle wide aspect ratios correctly", () => {
            Object.defineProperty(window, "innerWidth", { value: 3840 })
            Object.defineProperty(window, "innerHeight", { value: 1080 })

            sceneRenderer.updateCanvasSize()

            const aspectRatio = 3840 / 1080
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            // For wide aspect ratios, use standard FOV_X
            expect(sceneRenderer.mainCamera.fov).toBeCloseTo(110 / aspectRatio) // STANDARD_CAMERA_FOV_X / aspect
        })

        test("should handle tall aspect ratios correctly", () => {
            // Test portrait-like aspect ratio
            Object.defineProperty(window, "innerWidth", { value: 800 })
            Object.defineProperty(window, "innerHeight", { value: 1200 })

            sceneRenderer.updateCanvasSize()

            const aspectRatio = 800 / 1200
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            // For narrow aspect ratios, use standard FOV_Y
            expect(sceneRenderer.mainCamera.fov).toBeCloseTo(61.875) // STANDARD_CAMERA_FOV_Y
        })
    })

    describe("Lighting", () => {
        test("should create directional light with proper shadow settings", () => {
            sceneRenderer.changeLighting(false)

            const directionalLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            expect(directionalLight).toBeDefined()
            expect(directionalLight.castShadow).toBe(true)
        })

        test("should switch between directional and CSM lighting", () => {
            // Start with directional light
            sceneRenderer.changeLighting(false)
            const directionalLight = sceneRenderer.scene.children.find(child => child instanceof THREE.DirectionalLight)
            expect(directionalLight).toBeDefined()

            // Switch to CSM (fancy shadows)
            sceneRenderer.changeLighting(true)
            const noDirectionalLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            )
            expect(noDirectionalLight).toBeUndefined()
        })

        test("should update light intensity for directional light", () => {
            sceneRenderer.changeLighting(false)

            const initialLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            expect(initialLight.intensity).toBe(5) // Default from preferences

            sceneRenderer.setLightIntensity(0.5)

            const updatedLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            expect(updatedLight.intensity).toBe(0.5)
        })

        test("should create and position directional light when fancy shadows disabled", () => {
            sceneRenderer.changeLighting(false)

            const light = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            // Test that light exists and has correct properties
            expect(light).toBeDefined()
            expect(light).toBeInstanceOf(THREE.DirectionalLight)

            // Test that light is positioned away from origin (not at 0,0,0)
            const distanceFromOrigin = light.position.length()
            expect(distanceFromOrigin).toBeGreaterThan(10) // Should be positioned far enough away

            // Test that light has proper shadow settings
            expect(light.castShadow).toBe(true)

            // Test that light is actually added to the scene
            expect(sceneRenderer.scene.children).toContain(light)

            // Test that light has reasonable intensity (from preferences)
            expect(light.intensity).toBeGreaterThan(0)
        })

        test("should handle null material gracefully", () => {
            sceneRenderer.changeLighting(true)

            // Should not throw when passed null/undefined
            expect(() => sceneRenderer.setupMaterial(null as unknown as THREE.Material)).not.toThrow()
        })
    })

    describe("Scene Management", () => {
        test("should add object to scene and verify it exists", () => {
            const mockObject = new THREE.Object3D()
            mockObject.name = "TestObject"

            const initialChildCount = sceneRenderer.scene.children.length

            sceneRenderer.addObject(mockObject)

            // Verify object is actually in the scene
            expect(sceneRenderer.scene.children).toContain(mockObject)
            expect(sceneRenderer.scene.children.length).toBe(initialChildCount + 1)

            // Verify we can find it by name
            const foundObject = sceneRenderer.scene.getObjectByName("TestObject")
            expect(foundObject).toBe(mockObject)
        })

        test("should remove object from scene and verify it no longer exists", () => {
            const mockObject = new THREE.Object3D()
            mockObject.name = "TestObjectToRemove"

            // First add it
            sceneRenderer.addObject(mockObject)
            expect(sceneRenderer.scene.children).toContain(mockObject)

            const childCountAfterAdd = sceneRenderer.scene.children.length

            // Then remove it
            sceneRenderer.removeObject(mockObject)

            // Verify it's actually gone
            expect(sceneRenderer.scene.children).not.toContain(mockObject)
            expect(sceneRenderer.scene.children.length).toBe(childCountAfterAdd - 1)
            expect(sceneRenderer.scene.getObjectByName("TestObjectToRemove")).toBeUndefined()
        })

        test("should handle adding same object multiple times gracefully", () => {
            const mockObject = new THREE.Object3D()

            const initialChildCount = sceneRenderer.scene.children.length

            // Add the same object twice
            sceneRenderer.addObject(mockObject)
            sceneRenderer.addObject(mockObject)

            // Should only appear once in the scene
            expect(sceneRenderer.scene.children.length).toBe(initialChildCount + 1)
            expect(sceneRenderer.scene.children.filter(child => child === mockObject)).toHaveLength(1)
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

            // Create hierarchy
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

            // Verify both are in scene
            expect(sceneRenderer.scene.getObjectByName("ParentToRemove")).toBe(parentObject)
            expect(sceneRenderer.scene.getObjectByName("ChildToRemove")).toBe(childObject)

            // Remove parent
            sceneRenderer.removeObject(parentObject)

            // Both should be gone from scene access
            expect(sceneRenderer.scene.getObjectByName("ParentToRemove")).toBeUndefined()
            expect(sceneRenderer.scene.getObjectByName("ChildToRemove")).toBeUndefined()
        })

        test("should track scene composition over multiple operations", () => {
            const objects = [
                new THREE.Object3D(),
                new THREE.Mesh(new THREE.SphereGeometry(1), new THREE.MeshBasicMaterial()),
                new THREE.Group(),
            ]

            objects.forEach((obj, index) => {
                obj.name = `TestObject${index}`
            })

            const initialCount = sceneRenderer.scene.children.length

            // Add all objects
            objects.forEach(obj => sceneRenderer.addObject(obj))
            expect(sceneRenderer.scene.children.length).toBe(initialCount + objects.length)

            // Remove one object
            sceneRenderer.removeObject(objects[1])
            expect(sceneRenderer.scene.children.length).toBe(initialCount + objects.length - 1)
            expect(sceneRenderer.scene.children).not.toContain(objects[1])
            expect(sceneRenderer.scene.children).toContain(objects[0])
            expect(sceneRenderer.scene.children).toContain(objects[2])
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

            expect(skybox).toBeDefined()
            expect(skybox?.material.uniforms.rColor.value).toBe(0.5)
            expect(skybox?.material.uniforms.gColor.value).toBe(0.7)
            expect(skybox?.material.uniforms.bColor.value).toBe(0.9)
        })
    })

    describe("Gizmo Management", () => {
        test("should detect if any gizmo is dragging", () => {
            const isDragging = sceneRenderer.isAnyGizmoDragging()
            expect(typeof isDragging).toBe("boolean")
            expect(isDragging).toBe(false) // No gizmos should be dragging initially
        })

        test("should manage gizmo registration", () => {
            const mockGizmo = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                hasParent: vi.fn().mockReturnValue(true),
                parentObjectId: 123,
                gizmo: { dragging: false },
            }

            const gizmoId = sceneRenderer.registerGizmoSceneObject(mockGizmo as unknown as GizmoSceneObject)

            expect(gizmoId).toBeGreaterThan(0) // ID should be positive
            expect(sceneRenderer.gizmosOnMirabuf.has(123)).toBe(true)
            expect(sceneRenderer.gizmosOnMirabuf.get(123)).toBe(mockGizmo)
        })
    })

    describe("Update Loop", () => {
        test("should update all scene objects in correct order", () => {
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

            // Verify both objects are registered
            expect(sceneRenderer.sceneObjects.size).toBe(2)

            sceneRenderer.update(0.016)

            // Verify all scene objects were updated
            expect(mockSceneObject1.update).toHaveBeenCalledTimes(1)
            expect(mockSceneObject2.update).toHaveBeenCalledTimes(1)

            // Verify camera controls and screen handler were updated with correct deltaTime
            expect(sceneRenderer.currentCameraControls.update).toHaveBeenCalledWith(0.016)
            expect(sceneRenderer.screenInteractionHandler.update).toHaveBeenCalledWith(0.016)
        })

        test("should handle empty scene objects gracefully", () => {
            expect(sceneRenderer.sceneObjects.size).toBe(0)

            sceneRenderer.update(0.016)

            // Should still update camera controls and screen handler
            expect(sceneRenderer.currentCameraControls.update).toHaveBeenCalledWith(0.016)
            expect(sceneRenderer.screenInteractionHandler.update).toHaveBeenCalledWith(0.016)
        })
    })

    describe("Camera Controls", () => {
        test("should set camera controls", () => {
            const initialControls = sceneRenderer.currentCameraControls

            sceneRenderer.setCameraControls("Orbit")

            // Should dispose old controls and create new ones
            expect(initialControls.dispose).toHaveBeenCalled()
            expect(sceneRenderer.currentCameraControls).toBeDefined()
            expect(sceneRenderer.currentCameraControls).not.toBe(initialControls)
        })
    })

    describe("Field Management", () => {
        test("should not remove non-MirabufSceneObject instances when calling removeAllFields", () => {
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

            // Register objects
            const id1 = sceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            const id2 = sceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)

            expect(sceneRenderer.sceneObjects.size).toBe(2)

            // Call removeAllFields - should not remove anything since these aren't MirabufSceneObjects
            sceneRenderer.removeAllFields()

            // All objects should still be there
            expect(sceneRenderer.sceneObjects.size).toBe(2)
            expect(sceneRenderer.sceneObjects.has(id1)).toBe(true)
            expect(sceneRenderer.sceneObjects.has(id2)).toBe(true)

            // Verify no dispose calls were made
            expect(mockSceneObject1.dispose).not.toHaveBeenCalled()
            expect(mockSceneObject2.dispose).not.toHaveBeenCalled()
        })

        test("should handle empty scene when removing fields", () => {
            expect(sceneRenderer.sceneObjects.size).toBe(0)

            expect(() => sceneRenderer.removeAllFields()).not.toThrow()

            expect(sceneRenderer.sceneObjects.size).toBe(0)
        })
    })
})
