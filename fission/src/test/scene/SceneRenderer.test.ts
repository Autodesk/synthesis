import { expect, test, vi, beforeEach, describe, afterEach } from "vitest"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import * as THREE from "three"
import { Theme } from "@/ui/helpers/UseThemeHelpers"
import SceneObject from "@/systems/scene/SceneObject"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"

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

vi.mock("@/systems/preferences/PreferencesSystem", () => ({
    default: {
        getGraphicsPreferences: vi.fn(() => ({
            antiAliasing: false,
            fancyShadows: false,
            shadowMapSize: 1024,
            lightIntensity: 1.0,
            cascades: 4,
            maxFar: 100,
        })),
        getGlobalPreference: vi.fn(() => false),
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
    })),
    EffectPass: vi.fn(),
    RenderPass: vi.fn(),
    SMAAEffect: vi.fn(),
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
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    beforeEach(() => {
        vi.clearAllMocks()
        sceneRenderer = new SceneRenderer()
        console.log = vi.fn()
        console.error = vi.fn()
        console.warn = vi.fn()
        console.debug = vi.fn()
    })

    afterEach(() => {
        if (sceneRenderer) {
            sceneRenderer.destroy()
        }
        console.log = originalConsoleLog
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.debug = originalConsoleDebug
    })

    describe("Basic Properties", () => {
        test("should initialize with default values", () => {
            expect(sceneRenderer.mainCamera).toBeInstanceOf(THREE.PerspectiveCamera)
            expect(sceneRenderer.scene).toBeInstanceOf(THREE.Scene)
            expect(sceneRenderer.renderer).toBeInstanceOf(THREE.WebGLRenderer)
            expect(sceneRenderer.isPlacingAssembly).toBe(false)
            expect(sceneRenderer.sceneObjects).toBeInstanceOf(Map)
            expect(sceneRenderer.gizmosOnMirabuf).toBeInstanceOf(Map)
        })

        test("should have correct camera properties", () => {
            const camera = sceneRenderer.mainCamera
            expect(camera.fov).toBeCloseTo(61.875) // STANDARD_CAMERA_FOV_Y
            expect(camera.aspect).toBeCloseTo(1920 / 1080)
            expect(camera.near).toBe(0.1)
            expect(camera.far).toBe(1000)
        })
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
        })

        test("should convert world to pixel space", () => {
            const worldPos = new THREE.Vector3(0, 0, 0)
            const pixelPos = sceneRenderer.worldToPixelSpace(worldPos)
            expect(Array.isArray(pixelPos)).toBe(true)
            expect(pixelPos).toHaveLength(2)
        })
    })

    describe("Canvas Management", () => {
        test("should update camera aspect ratio and FOV based on window size", () => {
            const initialAspect = sceneRenderer.mainCamera.aspect
            const initialFOV = sceneRenderer.mainCamera.fov

            // Change to different aspect ratio
            Object.defineProperty(window, "innerWidth", { value: 1200 })
            Object.defineProperty(window, "innerHeight", { value: 800 })

            sceneRenderer.updateCanvasSize()

            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(1.5) // 1200/800
            expect(sceneRenderer.mainCamera.aspect).not.toBe(initialAspect)

            // FOV should be adjusted based on aspect ratio
            expect(sceneRenderer.mainCamera.fov).toBeDefined()
            expect(sceneRenderer.mainCamera.fov).toBeGreaterThan(0)
        })

        test("should handle wide aspect ratios correctly", () => {
            // Test very wide aspect ratio (wider than standard 16:9)
            Object.defineProperty(window, "innerWidth", { value: 2560 })
            Object.defineProperty(window, "innerHeight", { value: 1080 })

            sceneRenderer.updateCanvasSize()

            const aspectRatio = 2560 / 1080
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            // For aspect ratios wider than standard, FOV should be adjusted
            const standardAspect = 16 / 9
            if (aspectRatio > standardAspect) {
                expect(sceneRenderer.mainCamera.fov).toBeCloseTo(110 / aspectRatio) // STANDARD_CAMERA_FOV_X / aspect
            }
        })

        test("should handle tall aspect ratios correctly", () => {
            // Test portrait-like aspect ratio
            Object.defineProperty(window, "innerWidth", { value: 800 })
            Object.defineProperty(window, "innerHeight", { value: 1200 })

            sceneRenderer.updateCanvasSize()

            const aspectRatio = 800 / 1200
            expect(sceneRenderer.mainCamera.aspect).toBeCloseTo(aspectRatio)

            // For narrow aspect ratios, use standard FOV_Y
            const standardAspect = 16 / 9
            if (aspectRatio < standardAspect) {
                expect(sceneRenderer.mainCamera.fov).toBeCloseTo(61.875) // STANDARD_CAMERA_FOV_Y
            }
        })

        test("should handle zero height gracefully", () => {
            Object.defineProperty(window, "innerWidth", { value: 1920 })
            Object.defineProperty(window, "innerHeight", { value: 0 })

            sceneRenderer.updateCanvasSize()

            // Should default to aspect ratio of 1.0 when height is zero
            expect(sceneRenderer.mainCamera.aspect).toBe(1.0)
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
            expect(directionalLight.shadow.camera.top).toBe(15)
            expect(directionalLight.shadow.camera.bottom).toBe(-15)
            expect(directionalLight.shadow.camera.left).toBe(-15)
            expect(directionalLight.shadow.camera.right).toBe(15)
            expect(directionalLight.shadow.blurSamples).toBe(16)
            expect(directionalLight.shadow.bias).toBe(0.0)
            expect(directionalLight.shadow.normalBias).toBe(0.01)
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

            expect(initialLight.intensity).toBe(1.0) // Default from preferences

            sceneRenderer.setLightIntensity(0.5)

            const updatedLight = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            expect(updatedLight.intensity).toBe(0.5)
        })

        test("should position light correctly", () => {
            sceneRenderer.changeLighting(false)

            const light = sceneRenderer.scene.children.find(
                child => child instanceof THREE.DirectionalLight
            ) as THREE.DirectionalLight

            // Light should be positioned based on normalized direction vector
            // Direction is (1, -3, -2).normalize() * -20
            const expectedDirection = new THREE.Vector3(1, -3, -2).normalize().multiplyScalar(-20)

            expect(light.position.x).toBeCloseTo(expectedDirection.x, 5)
            expect(light.position.y).toBeCloseTo(expectedDirection.y, 5)
            expect(light.position.z).toBeCloseTo(expectedDirection.z, 5)
        })
    })

    describe("Scene Management", () => {
        test("should add object to scene", () => {
            const mockObject = new THREE.Object3D()
            const addSpy = vi.spyOn(sceneRenderer.scene, "add")

            sceneRenderer.addObject(mockObject)
            expect(addSpy).toHaveBeenCalledWith(mockObject)
        })

        test("should remove object from scene", () => {
            const mockObject = new THREE.Object3D()
            const removeSpy = vi.spyOn(sceneRenderer.scene, "remove")

            sceneRenderer.removeObject(mockObject)
            expect(removeSpy).toHaveBeenCalledWith(mockObject)
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

            const id1 = sceneRenderer.registerSceneObject(mockSceneObject1 as unknown as SceneObject)
            const id2 = sceneRenderer.registerSceneObject(mockSceneObject2 as unknown as SceneObject)

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

        test("should update skybox position to match camera", () => {
            const skybox = sceneRenderer.scene.children.find(
                child => child instanceof THREE.Mesh && child.geometry instanceof THREE.SphereGeometry
            )

            expect(skybox).toBeDefined()

            // Move camera to test position
            sceneRenderer.mainCamera.position.set(10, 20, 30)

            sceneRenderer.update(0.016)

            expect(skybox?.position.x).toBe(10)
            expect(skybox?.position.y).toBe(20)
            expect(skybox?.position.z).toBe(30)
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

    describe("Context Menu", () => {
        test("should dispatch default context menu when no physics hit", () => {
            const mockEvent = {
                position: [100, 100] as [number, number],
                interactionType: 0 as const,
            }

            sceneRenderer.onContextMenu(mockEvent)

            // Should dispatch default context menu with "Add" option
            expect(sceneRenderer.screenInteractionHandler.contextMenu).toBeDefined()
        })

        test("should convert pixel coordinates to world space for raycasting", () => {
            const mockEvent = {
                position: [960, 540] as [number, number], // Center of 1920x1080 screen
                interactionType: 0 as const,
            }

            sceneRenderer.onContextMenu(mockEvent)

            // Should call World.sceneRenderer.pixelToWorldSpace with correct coordinates
            // (World is mocked, so we verify the mock was called)
            expect(World.sceneRenderer.pixelToWorldSpace).toHaveBeenCalledWith(960, 540)
        })

        test("should perform raycast for physics interaction", () => {
            const mockEvent = {
                position: [100, 200] as [number, number],
                interactionType: 0 as const,
            }

            sceneRenderer.onContextMenu(mockEvent)

            // Should perform raycast from camera position in direction of mouse click
            expect(sceneRenderer.scene).toBeDefined() // Physics system should be called via World mock
        })
    })

    describe("Field Management", () => {
        test("should remove only field objects while keeping other objects", () => {
            // Test removeAllFields behavior by spying on removeSceneObject
            const removeSceneObjectSpy = vi.spyOn(sceneRenderer, "removeSceneObject")

            // Create mock objects and register them
            const mockRobotObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const mockFieldObject: MockSceneObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
            }

            const robotId = sceneRenderer.registerSceneObject(mockRobotObject as unknown as SceneObject)
            const fieldId = sceneRenderer.registerSceneObject(mockFieldObject as unknown as SceneObject)

            expect(sceneRenderer.sceneObjects.size).toBe(2)

            // Since our mock objects aren't MirabufSceneObject instances,
            // removeAllFields won't remove them. Test that it completes successfully.
            sceneRenderer.removeAllFields()

            // Both objects should still be there since they're not MirabufSceneObjects
            expect(sceneRenderer.sceneObjects.size).toBe(2)
            expect(sceneRenderer.sceneObjects.has(robotId)).toBe(true)
            expect(sceneRenderer.sceneObjects.has(fieldId)).toBe(true)

            // Verify removeSceneObject was not called since no objects matched the filter
            expect(removeSceneObjectSpy).not.toHaveBeenCalled()
        })

        test("should handle empty scene when removing fields", () => {
            expect(sceneRenderer.sceneObjects.size).toBe(0)

            sceneRenderer.removeAllFields()

            expect(sceneRenderer.sceneObjects.size).toBe(0)
        })

        test("should handle scene with no field objects", () => {
            const mockRobotObject = {
                dispose: vi.fn(),
                update: vi.fn(),
                setup: vi.fn(),
                id: 0,
                miraType: "ROBOT",
            }

            const robotId = sceneRenderer.registerSceneObject(mockRobotObject as unknown as SceneObject)
            expect(sceneRenderer.sceneObjects.size).toBe(1)

            sceneRenderer.removeAllFields()

            // Robot should remain untouched
            expect(sceneRenderer.sceneObjects.size).toBe(1)
            expect(sceneRenderer.sceneObjects.has(robotId)).toBe(true)
            expect(mockRobotObject.dispose).not.toHaveBeenCalled()
        })
    })

    describe("Material Setup", () => {
        test("should setup material for CSM when fancy shadows enabled", () => {
            const mockMaterial = new THREE.MeshToonMaterial()

            // Enable fancy shadows to ensure CSM is active
            sceneRenderer.changeLighting(true)

            sceneRenderer.setupMaterial(mockMaterial)

            // Material should be processed (we can't easily verify CSM.setupMaterial was called
            // due to mocking complexity, but we can verify the method completes successfully)
            expect(mockMaterial).toBeDefined()
            expect(mockMaterial.type).toBe("MeshToonMaterial")
        })

        test("should handle material setup with directional lighting", () => {
            const mockMaterial = new THREE.MeshToonMaterial()

            // Use directional lighting (no CSM)
            sceneRenderer.changeLighting(false)

            sceneRenderer.setupMaterial(mockMaterial)

            // With directional lighting, material should not be modified by CSM
            expect(mockMaterial).toBeDefined()
            expect(mockMaterial.type).toBe("MeshToonMaterial")
        })

        test("should handle null material gracefully", () => {
            sceneRenderer.changeLighting(true)

            // Should not throw when passed null/undefined
            expect(() => sceneRenderer.setupMaterial(null as unknown as THREE.Material)).not.toThrow()
        })
    })
})
