import * as THREE from "three"
import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import MirabufInstance from "../../mirabuf/MirabufInstance"
import type MirabufParser from "../../mirabuf/MirabufParser"
import { ParseErrorSeverity } from "../../mirabuf/MirabufParser"

const mockSceneRenderer = {
    createToonMaterial: vi.fn(() => new THREE.MeshStandardMaterial({ color: 0x123456 })),
    setupMaterial: vi.fn(),
}

vi.mock("@/systems/World", () => ({
    default: {
        get sceneRenderer() {
            return mockSceneRenderer
        },
    },
}))

describe("MirabufInstance", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    let parser: MirabufParser
    let scene: THREE.Scene

    beforeEach(() => {
        parser = {
            errors: [],
            assembly: {
                data: {
                    materials: {
                        appearances: {
                            mat1: { albedo: { A: 255, R: 10, G: 20, B: 30 }, roughness: 0.5, metallic: 0.1 },
                        },
                    },
                    parts: {
                        partDefinitions: {
                            def1: {
                                info: { GUID: "def1" },
                                bodies: [
                                    {
                                        info: { GUID: "body1" },
                                        triangleMesh: {
                                            mesh: {
                                                verts: [0, 0, 0, 1, 1, 1, 2, 2, 2],
                                                normals: [0, 0, 1, 0, 1, 0, 1, 0, 0],
                                                uv: [0, 0, 1, 1, 2, 2],
                                                indices: [0, 1, 2],
                                            },
                                        },
                                    },
                                ],
                            },
                        },
                        partInstances: {
                            inst1: {
                                partDefinitionReference: "def1",
                                info: { GUID: "inst1" },
                            },
                        },
                    },
                },
            },
            globalTransforms: new Map([["inst1", new THREE.Matrix4()]]),
        } as unknown as MirabufParser
        scene = new THREE.Scene()

        console.log = vi.fn()
        console.error = vi.fn()
        console.warn = vi.fn()
        console.debug = vi.fn()
    })

    afterEach(() => {
        vi.clearAllMocks()
        console.log = originalConsoleLog
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.debug = originalConsoleDebug
    })

    test("throws if parser has unimportable errors", () => {
        const badParser = { ...parser, errors: [[ParseErrorSeverity.UNIMPORTABLE, "fail"]] } as MirabufParser
        expect(() => new MirabufInstance(badParser)).toThrow()
    })

    test("AddToScene adds all batches to scene", () => {
        const instance = new MirabufInstance(parser)
        const addSpy = vi.spyOn(scene, "add")
        instance.addToScene(scene)
        expect(addSpy).toHaveBeenCalled()
        expect(scene.children.length).toBeGreaterThan(0)
    })

    test("Dispose removes all batches and clears materials", () => {
        const instance = new MirabufInstance(parser)
        instance.addToScene(scene)
        expect(scene.children.length).toBeGreaterThan(0)
        instance.dispose(scene)
        expect(scene.children.length).toBe(0)
        expect(instance.batches.length).toBe(0)
        expect(instance.meshes.size).toBe(0)
        expect(instance.materials.size).toBe(0)
    })

    test("Dispose is idempotent and safe to call multiple times", () => {
        const instance = new MirabufInstance(parser)
        instance.addToScene(scene)
        expect(() => {
            instance.dispose(scene)
            instance.dispose(scene)
            instance.dispose(scene)
        }).not.toThrow()
        expect(scene.children.length).toBe(0)
        expect(instance.batches.length).toBe(0)
        expect(instance.meshes.size).toBe(0)
        expect(instance.materials.size).toBe(0)
    })
})
