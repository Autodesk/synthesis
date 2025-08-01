import { fireEvent, render } from "@testing-library/react"
import { assert, beforeEach, describe, expect, test } from "vitest"
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

let container: HTMLElement

describe("Select Menu", () => {
    // Re-render the select menu before each test
    beforeEach(() => {
        container = render(
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
        ).container
    })

    test("Navigate Menu", () => {
        const controlsButton = container.querySelector("#select-button-Controls")
        assert(controlsButton != undefined)

        fireEvent.click(controlsButton)
        expect(selectedOption).toBe(robotModes[3])

        const backButton = container.querySelector("#select-menu-back-button")
        assert(backButton != undefined)

        fireEvent.click(backButton)
        expect(selectedOption).toBe(undefined)
    })

    test("Conditional Delete", () => {
        const deleteButton = container.querySelector("#select-menu-delete-button")
        assert(deleteButton != undefined)

        fireEvent.click(deleteButton)
        expect(itemDeleted).toBe(robotModes[1])
    })

    test("Add Item", async () => {
        const addButton = container.querySelector("#select-menu-add-button")
        assert(addButton != undefined)

        fireEvent.click(addButton)
        expect(addClicked).toBe(true)
    })
})
