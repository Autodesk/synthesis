import { describe, test, expect, beforeEach, vi, afterEach } from "vitest"

// Mock all the system dependencies before importing World
vi.mock("@/systems/physics/PhysicsSystem", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

vi.mock("@/systems/scene/SceneRenderer", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

vi.mock("@/systems/simulation/SimulationSystem", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

vi.mock("@/systems/input/InputSystem", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

vi.mock("@/systems/analytics/AnalyticsSystem", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

vi.mock("@/systems/scene/DragModeSystem", () => ({
    default: vi.fn(() => ({
        Update: vi.fn(),
        Destroy: vi.fn(),
    })),
}))

// Mock THREE.Clock
vi.mock("three", async () => {
    const actual = await vi.importActual("three")
    return {
        ...actual,
        Clock: vi.fn(() => ({
            getDelta: vi.fn().mockReturnValue(0.016), // ~60fps
        })),
    }
})

// Import World after setting up mocks
import World from "@/systems/World"

describe("World Tests", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        // Ensure World is not alive before each test
        if (World.isAlive) {
            World.DestroyWorld()
        }
        World.resetAccumTimes()
    })

    afterEach(() => {
        // Clean up after each test
        if (World.isAlive) {
            World.DestroyWorld()
        }
    })

    describe("Initial State", () => {
        test("World should not be alive initially", () => {
            expect(World.isAlive).toBeFalsy()
        })

        test("accumTimes should have initial values", () => {
            const accumTimes = World.accumTimes
            expect(accumTimes.frames).toBe(0)
            expect(accumTimes.sceneTime).toBe(0)
            expect(accumTimes.physicsTime).toBe(0)
            expect(accumTimes.simulationTime).toBe(0)
            expect(accumTimes.inputTime).toBe(0)
            expect(accumTimes.totalTime).toBe(0)
        })

        test("currentDeltaT should be 0 initially", () => {
            expect(World.currentDeltaT).toBe(0)
        })
    })

    describe("Getters before initialization", () => {
        test("system getters should return undefined before initialization", () => {
            expect(World.SceneRenderer).toBeUndefined()
            expect(World.PhysicsSystem).toBeUndefined()
            expect(World.SimulationSystem).toBeUndefined()
            expect(World.InputSystem).toBeUndefined()
            expect(World.AnalyticsSystem).toBeUndefined()
            expect(World.DragModeSystem).toBeUndefined()
        })
    })

    describe("InitWorld", () => {
        test("InitWorld should initialize all systems and set isAlive to true", () => {
            World.InitWorld()

            expect(World.isAlive).toBeTruthy()
            expect(World.SceneRenderer).toBeDefined()
            expect(World.PhysicsSystem).toBeDefined()
            expect(World.SimulationSystem).toBeDefined()
            expect(World.InputSystem).toBeDefined()
            expect(World.DragModeSystem).toBeDefined()
        })

        test("InitWorld should handle AnalyticsSystem initialization failure gracefully", async () => {
            // Import the mocked modules to access the mock functions
            const AnalyticsSystemMock = (await import("@/systems/analytics/AnalyticsSystem")).default

            // Mock AnalyticsSystem to throw an error for this test
            vi.mocked(AnalyticsSystemMock).mockImplementationOnce(() => {
                throw new Error("Analytics initialization failed")
            })

            World.InitWorld()

            expect(World.isAlive).toBeTruthy()
            expect(World.AnalyticsSystem).toBeUndefined()
        })

        test("InitWorld should not reinitialize if already alive", () => {
            World.InitWorld()
            const firstSceneRenderer = World.SceneRenderer

            World.InitWorld() // Call again

            expect(World.SceneRenderer).toBe(firstSceneRenderer)
        })
    })

    describe("DestroyWorld", () => {
        test("DestroyWorld should destroy all systems and set isAlive to false", () => {
            World.InitWorld()
            const sceneRenderer = World.SceneRenderer
            const physicsSystem = World.PhysicsSystem
            const simulationSystem = World.SimulationSystem
            const inputSystem = World.InputSystem
            const dragModeSystem = World.DragModeSystem

            World.DestroyWorld()

            expect(World.isAlive).toBeFalsy()
            expect(sceneRenderer.Destroy).toHaveBeenCalled()
            expect(physicsSystem.Destroy).toHaveBeenCalled()
            expect(simulationSystem.Destroy).toHaveBeenCalled()
            expect(inputSystem.Destroy).toHaveBeenCalled()
            expect(dragModeSystem.Destroy).toHaveBeenCalled()
        })

        test("DestroyWorld should handle AnalyticsSystem destruction if it exists", () => {
            World.InitWorld()
            const analyticsSystem = World.AnalyticsSystem

            World.DestroyWorld()

            if (analyticsSystem) {
                expect(analyticsSystem.Destroy).toHaveBeenCalled()
            }
        })

        test("DestroyWorld should not do anything if World is not alive", () => {
            expect(World.isAlive).toBeFalsy()

            // This should not throw or cause issues
            World.DestroyWorld()

            expect(World.isAlive).toBeFalsy()
        })
    })

    describe("resetAccumTimes", () => {
        test("resetAccumTimes should reset all timing values to 0", () => {
            World.InitWorld()
            World.UpdateWorld() // This should accumulate some time
            expect(World.accumTimes.frames).not.toBe(0)

            World.resetAccumTimes()

            const accumTimes = World.accumTimes
            expect(accumTimes.frames).toBe(0)
            expect(accumTimes.sceneTime).toBe(0)
            expect(accumTimes.physicsTime).toBe(0)
            expect(accumTimes.simulationTime).toBe(0)
            expect(accumTimes.inputTime).toBe(0)
            expect(accumTimes.totalTime).toBe(0)
        })
    })

    describe("UpdateWorld", () => {
        beforeEach(() => {
            World.InitWorld()
        })

        test("UpdateWorld should update all systems", () => {
            const sceneRenderer = World.SceneRenderer
            const physicsSystem = World.PhysicsSystem
            const simulationSystem = World.SimulationSystem
            const inputSystem = World.InputSystem
            const dragModeSystem = World.DragModeSystem
            const analyticsSystem = World.AnalyticsSystem

            World.UpdateWorld()

            expect(sceneRenderer.Update).toHaveBeenCalledWith(0.016)
            expect(physicsSystem.Update).toHaveBeenCalledWith(0.016)
            expect(simulationSystem.Update).toHaveBeenCalledWith(0.016)
            expect(inputSystem.Update).toHaveBeenCalledWith(0.016)
            expect(dragModeSystem.Update).toHaveBeenCalledWith(0.016)

            if (analyticsSystem) {
                expect(analyticsSystem.Update).toHaveBeenCalledWith(0.016)
            }
        })

        test("UpdateWorld should update currentDeltaT", () => {
            World.UpdateWorld()
            expect(World.currentDeltaT).toBe(0.016)
        })

        test("UpdateWorld should increment frame count", () => {
            const initialFrames = World.accumTimes.frames

            World.UpdateWorld()

            expect(World.accumTimes.frames).toBe(initialFrames + 1)
        })

        test("UpdateWorld should accumulate timing data", () => {
            const initialAccumTimes = { ...World.accumTimes }

            World.UpdateWorld()

            const newAccumTimes = World.accumTimes
            expect(newAccumTimes.frames).toBeGreaterThan(initialAccumTimes.frames)
            expect(newAccumTimes.totalTime).toBeGreaterThanOrEqual(initialAccumTimes.totalTime)
        })
    })

    describe("Getters after initialization", () => {
        beforeEach(() => {
            World.InitWorld()
        })

        test("all system getters should return valid instances after initialization", () => {
            expect(World.SceneRenderer).toBeDefined()
            expect(World.PhysicsSystem).toBeDefined()
            expect(World.SimulationSystem).toBeDefined()
            expect(World.InputSystem).toBeDefined()
            expect(World.DragModeSystem).toBeDefined()
            // AnalyticsSystem might be undefined if initialization fails, so we check if it exists
            const analyticsSystem = World.AnalyticsSystem
            if (analyticsSystem) {
                expect(analyticsSystem).toBeDefined()
            }
        })

        test("accumTimes getter should return timing object", () => {
            const accumTimes = World.accumTimes
            expect(accumTimes).toHaveProperty("frames")
            expect(accumTimes).toHaveProperty("sceneTime")
            expect(accumTimes).toHaveProperty("physicsTime")
            expect(accumTimes).toHaveProperty("simulationTime")
            expect(accumTimes).toHaveProperty("inputTime")
            expect(accumTimes).toHaveProperty("totalTime")
        })
    })

    describe("Lifecycle", () => {
        test("complete lifecycle: init -> update -> destroy", () => {
            // Initialize
            expect(World.isAlive).toBeFalsy()
            World.InitWorld()
            expect(World.isAlive).toBeTruthy()

            // Update a few times
            World.UpdateWorld()
            World.UpdateWorld()
            expect(World.accumTimes.frames).toBe(2)

            // Destroy
            World.DestroyWorld()
            expect(World.isAlive).toBeFalsy()
        })

        test("multiple init/destroy cycles should work correctly", () => {
            // First cycle
            World.InitWorld()
            expect(World.isAlive).toBeTruthy()
            World.DestroyWorld()
            expect(World.isAlive).toBeFalsy()

            // Second cycle
            World.InitWorld()
            expect(World.isAlive).toBeTruthy()
            World.DestroyWorld()
            expect(World.isAlive).toBeFalsy()
        })
    })
})
