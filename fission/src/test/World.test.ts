import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"

// Mock all the system dependencies before importing World

vi.mock("@/systems/analytics/AnalyticsSystem", () => ({
    default: vi.fn(() => ({
        update: vi.fn(),
        destroy: vi.fn(),
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

const systems = [
    "sceneRenderer",
    "physicsSystem",
    "simulationSystem",
    "inputSystem",
    "analyticsSystem",
    "dragModeSystem",
] as const satisfies (keyof typeof World)[]

describe("World Tests", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        // Ensure World is not alive before each test
        if (World.isAlive) {
            World.destroyWorld()
        }
        World.resetAccumTimes()
    })

    afterEach(() => {
        // Clean up after each test
        if (World.isAlive) {
            World.destroyWorld()
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
            systems.forEach(system => {
                expect(World[system]).toBeUndefined()
            })
        })
    })

    describe("InitWorld", () => {
        test("InitWorld should initialize all systems and set isAlive to true", () => {
            World.initWorld()

            expect(World.isAlive).toBeTruthy()
            systems.forEach(system => {
                expect(World[system]).toBeDefined()
            })
        })

        test("InitWorld should handle AnalyticsSystem initialization failure gracefully", async () => {
            // Import the mocked modules to access the mock functions
            const AnalyticsSystemMock = (await import("@/systems/analytics/AnalyticsSystem")).default

            // Mock AnalyticsSystem to throw an error for this test
            vi.mocked(AnalyticsSystemMock).mockImplementationOnce(() => {
                throw new Error("Analytics initialization failed")
            })

            World.initWorld()

            expect(World.isAlive).toBeTruthy()
            expect(World.analyticsSystem).toBeUndefined()
        })

        test("InitWorld should not reinitialize if already alive", () => {
            World.initWorld()
            const firstSceneRenderer = World.sceneRenderer

            World.initWorld() // Call again

            expect(World.sceneRenderer).toBe(firstSceneRenderer)
        })
    })

    describe("DestroyWorld", () => {
        test("DestroyWorld should destroy all systems and set isAlive to false", () => {
            World.initWorld()
            systems.forEach(system => {
                vi.spyOn(World[system]!, "destroy")
            })
            World.destroyWorld()

            expect(World.isAlive).toBeFalsy()
            systems.forEach(system => {
                expect(World[system]!.destroy).toHaveBeenCalled()
            })
        })

        test("DestroyWorld should handle AnalyticsSystem destruction if it exists", () => {
            World.initWorld()
            const analyticsSystem = World.analyticsSystem

            World.destroyWorld()

            if (analyticsSystem) {
                expect(analyticsSystem.destroy).toHaveBeenCalled()
            }
        })

        test("DestroyWorld should not do anything if World is not alive", () => {
            expect(World.isAlive).toBeFalsy()

            // This should not throw or cause issues
            World.destroyWorld()

            expect(World.isAlive).toBeFalsy()
        })
    })

    describe("resetAccumTimes", () => {
        test("resetAccumTimes should reset all timing values to 0", () => {
            World.initWorld()
            World.updateWorld() // This should accumulate some time
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
            World.initWorld()
        })

        test("UpdateWorld should update all systems", () => {
            systems.forEach(system => {
                vi.spyOn(World[system]!, "update")
            })

            World.updateWorld()

            systems.forEach(system => {
                expect(World[system]?.update).toHaveBeenCalledWith(0.016)
            })
        })

        test("UpdateWorld should update currentDeltaT", () => {
            World.updateWorld()
            expect(World.currentDeltaT).toBe(0.016)
        })

        test("UpdateWorld should increment frame count", () => {
            const initialFrames = World.accumTimes.frames

            World.updateWorld()

            expect(World.accumTimes.frames).toBe(initialFrames + 1)
        })

        test("UpdateWorld should accumulate timing data", () => {
            const initialAccumTimes = { ...World.accumTimes }

            World.updateWorld()

            const newAccumTimes = World.accumTimes
            expect(newAccumTimes.frames).toBeGreaterThan(initialAccumTimes.frames)
            expect(newAccumTimes.totalTime).toBeGreaterThanOrEqual(initialAccumTimes.totalTime)
        })
    })

    describe("Getters after initialization", () => {
        beforeEach(() => {
            World.initWorld()
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
            World.initWorld()
            expect(World.isAlive).toBeTruthy()

            // Update a few times
            World.updateWorld()
            World.updateWorld()
            expect(World.accumTimes.frames).toBe(2)

            // Destroy
            World.destroyWorld()
            expect(World.isAlive).toBeFalsy()
        })

        test("multiple init/destroy cycles should work correctly", () => {
            // First cycle
            World.initWorld()
            expect(World.isAlive).toBeTruthy()
            World.destroyWorld()
            expect(World.isAlive).toBeFalsy()

            // Second cycle
            World.initWorld()
            expect(World.isAlive).toBeTruthy()
            World.destroyWorld()
            expect(World.isAlive).toBeFalsy()
        })
    })
})
