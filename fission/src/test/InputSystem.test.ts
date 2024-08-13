import { test, describe, assert, expect } from "vitest"
import InputSystem from "@/systems/input/InputSystem"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import DefaultInputs from "@/systems/input/DefaultInputs"

describe("Input scheme manager checks", () => {
    test("Available schemes", () => {
        assert(InputSchemeManager.availableInputSchemes[0].schemeName == DefaultInputs.ernie().schemeName)
        assert(InputSchemeManager.defaultInputSchemes.length >= 1)

        const startingLength = InputSchemeManager.availableInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)

        expect(InputSchemeManager.availableInputSchemes.length).toBe(startingLength + 1)
    })
    test("Add a custom scheme", () => {
        const startingLength = InputSchemeManager.availableInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)

        assert((InputSchemeManager.availableInputSchemes.length = startingLength + 1))
    })
    test("Get random names", () => {
        const names: string[] = []
        for (let i = 0; i < 20; i++) {
            const name = InputSchemeManager.randomAvailableName
            expect(names.includes(name)).toBe(false)
            assert(name != undefined)
            expect(name.length).toBeGreaterThan(0)

            const scheme = DefaultInputs.newBlankScheme
            scheme.schemeName = name

            InputSchemeManager.addCustomScheme(scheme)

            names.push(name)
        }
    })
})

describe("Input system checks", () => {
    new InputSystem()

    test("Brain map exists", () => {
        assert(InputSystem.brainIndexSchemeMap != undefined)
    })

    test("Inputs are zero", () => {
        expect(InputSystem.getInput("arcadeDrive", 0)).toBe(0)
        expect(InputSystem.getGamepadAxis(0)).toBe(0)
        expect(InputSystem.getInput("randomInputThatDoesNotExist", 1273)).toBe(0)
        expect(InputSystem.isKeyPressed("keyA")).toBe(false)
        expect(InputSystem.isKeyPressed("ajhsekff")).toBe(false)
        expect(InputSystem.isGamepadButtonPressed(1)).toBe(false)
    })
})
