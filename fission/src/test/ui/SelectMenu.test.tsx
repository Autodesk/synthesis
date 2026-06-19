import { userEvent } from "@vitest/browser/context"
import { assert, beforeEach, describe, expect, test } from "vitest"
import { render } from "vitest-browser-react"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"

enum ConfigMode {
    INTAKE,
    EJECTOR,
    MOTORS,
    CONTROLS,
    SCORING_ZONES,
}

class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode

    constructor(name: string, configMode: ConfigMode) {
        super(name, name)
        this.configMode = configMode
    }
}

const robotModes = [
    new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE),
    new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR),
    new ConfigModeSelectionOption("Sequential Joints", ConfigMode.MOTORS),
    new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS),
]

let selectedOption: ConfigModeSelectionOption | undefined
let itemDeleted: ConfigModeSelectionOption | undefined
let addClicked: boolean

let screen: Awaited<ReturnType<typeof render>>

describe("Select Menu", () => {
    // Re-render the select menu before each test
    beforeEach(async () => {
        screen = await render(
            <SelectMenu
                options={robotModes}
                onOptionSelected={o => {
                    selectedOption = o as ConfigModeSelectionOption
                }}
                onDelete={o => {
                    itemDeleted = o as ConfigModeSelectionOption
                }}
                deleteCondition={o => o != robotModes[0]}
                defaultHeaderText="Test Select Menu"
                onAddClicked={() => {
                    addClicked = true
                }}
            />
        )
    })

    test("Navigate Menu", async () => {
        const controlsButton = screen.baseElement.querySelector("#select-button-Controls")
        assert(controlsButton != undefined)

        await userEvent.click(controlsButton)
        expect(selectedOption).toBe(robotModes[3])

        const backButton = screen.baseElement.querySelector("#select-menu-back-button")
        assert(backButton != undefined)

        await userEvent.click(backButton)
        expect(selectedOption).toBe(undefined)
    })

    test("Conditional Delete", async () => {
        const deleteButton = screen.baseElement.querySelector("#select-menu-delete-button")
        assert(deleteButton != undefined)

        await userEvent.click(deleteButton)
        expect(itemDeleted).toBe(robotModes[1])
    })

    test("Add Item", async () => {
        const addButton = screen.baseElement.querySelector("#select-menu-add-button")
        assert(addButton != undefined)

        await userEvent.click(addButton)
        expect(addClicked).toBe(true)
    })
})
