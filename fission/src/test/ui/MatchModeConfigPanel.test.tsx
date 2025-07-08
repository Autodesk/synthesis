import { fireEvent, getByText, render, waitFor, act } from "@testing-library/react"
import { assert, afterEach, beforeEach, describe, test, vi } from "vitest"
import MatchModeConfigPanel from "@/ui/panels/configuring/MatchModeConfigPanel"
import { PanelControlProvider } from "@/ui/PanelContext"
import { ModalControlProvider } from "@/ui/ModalContext"

describe("MatchModeConfigPanel", () => {
    // Mock console methods to suppress output during tests
    const originalConsoleError = console.error
    const originalConsoleLog = console.log
    const originalConsoleWarn = console.warn

    let container: HTMLElement

    beforeEach(async () => {
        // Suppress console output during tests
        console.error = vi.fn()
        console.warn = vi.fn()
        console.log = vi.fn()

        container = createTestContainer()
    })

    afterEach(() => {
        // Restore original console methods
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.log = originalConsoleLog

        container.remove()
    })

    function createTestContainer() {
        // Create mock context providers
        const mockPanelControl = {
            openPanel: () => {},
            closePanel: () => {},
            closeAllPanels: () => {},
        }

        const mockModalControl = {
            openModal: () => {},
            closeModal: () => {},
            activeModalId: null,
        }

        return render(
            <PanelControlProvider {...mockPanelControl}>
                <ModalControlProvider {...mockModalControl}>
                    <MatchModeConfigPanel panelId="test-panel" />
                </ModalControlProvider>
            </PanelControlProvider>
        ).container
    }

    function getMatchModeCount(container: HTMLElement): number {
        // Find the element that contains the count by looking for text that matches pattern "X Match Mode"
        const elements = container.querySelectorAll("*")
        for (const element of elements) {
            const text = element.textContent?.trim()
            if (text && /^\d+\s+Match\s+Mode/.test(text)) {
                const match = text.match(/(\d+)/)
                return match ? parseInt(match[1]) : 0
            }
        }
        return 0
    }

    async function waitForInitialLoad(container: HTMLElement): Promise<number> {
        // Wait for the component to load the initial configs from the public directory
        // This should show something like "4 Match Modes" instead of "0 Match Modes"
        return waitFor(
            () => {
                const count = getMatchModeCount(container)
                if (count > 0) return count
                throw new Error("Still waiting for initial configs to load")
            },
            { timeout: 5000 }
        )
    }

    async function testUploadMatchModeConfig(json: unknown, validJSON: boolean) {
        const initialCount = await waitForInitialLoad(container)

        const testJsonString = JSON.stringify(json)
        const testFile = new File([testJsonString], "test.json", { type: "application/json" })

        const fileInput = container.querySelector("input[type='file']")
        assert(fileInput != undefined)

        // Upload the file (wrapped in act to handle React state updates)
        act(() => {
            fireEvent.change(fileInput, { target: { files: [testFile] } })
        })

        await new Promise(resolve => setTimeout(resolve, 100))

        const finalCount = getMatchModeCount(container)
        if (validJSON) {
            assert(
                finalCount === initialCount + 1,
                `Expected count to increase from ${initialCount} to ${initialCount + 1}, but got ${finalCount}`
            )
        } else {
            assert(finalCount === initialCount, `Expected count to remain ${initialCount}, but got ${finalCount}`)
        }
    }

    test("Render MatchModeConfigPanel", () => {
        const container = createTestContainer()
        const matchModeConfigTitle = getByText(container, "Match Mode Config")
        const matchModeConfigButton = getByText(container, "Upload File")
        assert(matchModeConfigTitle != undefined)
        assert(matchModeConfigButton != undefined)
    })

    test("Upload Valid MatchModeConfig", async () => {
        const testJson = {
            id: "test-json",
            name: "Valid MatchModeConfig",
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 10,
        }

        await testUploadMatchModeConfig(testJson, true)
    })

    test("Upload Valid MatchModeConfig - Two configs", async () => {
        const testJson = {
            id: "test",
            name: "Valid MatchModeConfig",
            autonomousTime: 10,
            teleopTime: 42,
            endgameTime: 30,
        }
        const testJson2 = {
            id: "test2",
            name: "Valid MatchModeConfig2",
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 20,
        }

        await testUploadMatchModeConfig(testJson, true)
        await testUploadMatchModeConfig(testJson2, true)
    })

    test("Upload Invalid MatchModeConfig - Missing id field", async () => {
        const invalidJson = {
            // Missing required 'id' field
            name: "Invalid MatchModeConfig",
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 15,
        }

        await testUploadMatchModeConfig(invalidJson, false)
    })

    test("Upload Invalid MatchModeConfig - Missing name field", async () => {
        const invalidJson = {
            id: "test",
            // Missing required 'name' field
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 15,
        }

        await testUploadMatchModeConfig(invalidJson, false)
    })

    test("Upload Valid MatchModeConfig - Missing autonomousTime field", async () => {
        const validJson = {
            id: "test",
            name: "Valid MatchModeConfig",
            // Missing optional 'autonomousTime' field - autonomousTime will default to default value
            teleopTime: 135,
            endgameTime: 15,
        }

        await testUploadMatchModeConfig(validJson, true)
    })

    test("Upload Valid MatchModeConfig - Missing teleopTime and endgameTime fields", async () => {
        const validJson = {
            id: "test",
            name: "Valid MatchModeConfig",
            autonomousTime: 10,
        }

        await testUploadMatchModeConfig(validJson, true)
    })

    test("Upload two configs with same id", async () => {
        const validJson = {
            id: "test",
            name: "Valid MatchModeConfig",
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 15,
        }
        const jsonWithSameId = {
            id: "test",
            name: "Valid MatchModeConfig",
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 15,
        }

        await testUploadMatchModeConfig(validJson, true) // Valid config should be uploaded
        await testUploadMatchModeConfig(jsonWithSameId, false) // Has the same id as the first config, should not be uploaded
    })
})
