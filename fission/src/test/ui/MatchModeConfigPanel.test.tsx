import { act, fireEvent, getByText, render, waitFor } from "@testing-library/react"
import React from "react"
import { afterEach, assert, beforeEach, describe, type MockInstance, test, vi } from "vitest"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs"
import { Panel } from "@/ui/components/Panel"
import type { CloseType, PanelPosition, UIScreen } from "@/ui/helpers/UIProviderHelpers"
import MatchModeConfigPanel, { type MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { UICallback } from "@/ui/UICallbacks"
import { UIProvider } from "@/ui/UIProvider"
import { mockConsole } from "@/test/mocks/Common.ts"

describe("MatchModeConfigPanel", () => {
    let container: HTMLElement
    let getConfigsSpy: MockInstance<typeof DefaultMatchModeConfigs.getConfigs>

    beforeEach(async () => {
        // Suppress console output during tests
        mockConsole()

        // Clear local storage
        window.localStorage.setItem("match-mode-configs", JSON.stringify([]))

        getConfigsSpy = vi.spyOn(DefaultMatchModeConfigs, "getConfigs").mockResolvedValue([])

        // act() lets the getConfigs() promise settle so state updates before testing
        await act(async () => {
            container = createTestContainer()
        })
    })

    afterEach(() => {
        // Restore original console methods
        vi.restoreAllMocks()
    })

    function createTestContainer() {
        // Create mock context provider
        const panel = {
            id: "match-mode",
            content: MatchModeConfigPanel,
            props: {
                type: "panel" as const,
                configured: true,
                position: "center" as PanelPosition,
                custom: {},
            },
            parent: {} as UIScreen<unknown, unknown>,
            onClose: new UICallback<[CloseType], void>(),
            onCancel: new UICallback<[void], void>(),
            onAccept: new UICallback<[unknown], void>(),
            onBeforeAccept: new UICallback<[void], unknown>(),
        }
        return render(
            <UIProvider>
                <Panel panel={panel} parent={undefined}>
                    {React.createElement(panel.content)}
                </Panel>
            </UIProvider>
        ).container
    }

    function getMatchModeCount(container: HTMLElement): number {
        return container.querySelectorAll("[data-testid='match-mode-config']").length
    }

    async function uploadMatchModeConfig(container: HTMLElement, json: unknown) {
        const testJsonString = JSON.stringify(json)
        const testFile = new File([testJsonString], "test.json", { type: "application/json" })

        const fileInput = container.querySelector<HTMLInputElement>("input[type='file']")
        assert(fileInput != undefined)
        const readSpy = vi.spyOn(testFile, "text")

        // Upload the file (wrapped in act to handle React state updates)
        await act(async () => {
            fireEvent.change(fileInput, { target: { files: [testFile] } })
        })

        // Wait for the file to be read and processed
        await waitFor(() => assert(readSpy.mock.calls.length > 0, "File has not been read"))
        await readSpy.mock.results[0].value
        await act(async () => {})
    }

    async function testUploadMatchModeConfig(json: unknown, validJSON: boolean) {
        const initialCount = getMatchModeCount(container)

        await uploadMatchModeConfig(container, json)

        if (validJSON) {
            await waitFor(() =>
                assert(
                    getMatchModeCount(container) === initialCount + 1,
                    `Expected count to increase from ${initialCount} to ${initialCount + 1}, but got ${getMatchModeCount(container)}`
                )
            )
        } else {
            const finalCount = getMatchModeCount(container)
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

    test("Merges default configs that arrive after the panel is opened", async () => {
        const lateDefault: MatchModeConfig = {
            ...DefaultMatchModeConfigs.fallbackValues(),
            id: "late-default",
            name: "Late Default",
        }
        let deliverDefaults: (configs: MatchModeConfig[]) => void = () => {}
        getConfigsSpy.mockReturnValue(new Promise(resolve => (deliverDefaults = resolve)))

        const lateContainer = createTestContainer()
        assert(getMatchModeCount(lateContainer) === 0, "Nothing should be listed while the defaults are still pending")

        await uploadMatchModeConfig(lateContainer, { id: "early-upload", name: "Early Upload" })
        await waitFor(() => getByText(lateContainer, "Early Upload"))

        await act(async () => deliverDefaults([lateDefault]))

        await waitFor(() => {
            getByText(lateContainer, "Late Default")
            getByText(lateContainer, "Early Upload")
        })
        assert(
            getMatchModeCount(lateContainer) === 2,
            `Expected 2 configs, but got ${getMatchModeCount(lateContainer)}`
        )
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

    test("Upload Invalid MatchModeConfig - Invalid id data type", async () => {
        const invalidJson = {
            id: 123,
            name: "Invalid MatchModeConfig",
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

    test("Upload Valid MatchModeConfig - isDefault is true", async () => {
        const validJson = {
            id: "test",
            name: "Valid MatchModeConfig",
            isDefault: true,
            autonomousTime: 10,
            teleopTime: 20,
            endgameTime: 15,
        }

        await testUploadMatchModeConfig(validJson, true)
    })
})
