import { test, describe, assert, expect } from "vitest"
import InputSystem, { EmptyModifierState, ModifierState } from "@/systems/input/InputSystem"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import DefaultInputs from "@/systems/input/DefaultInputs"

describe("Input Scheme Manager Checks", () => {
    test("Available Schemes", () => {
        assert(InputSchemeManager.availableInputSchemes[0].schemeName == DefaultInputs.ernie().schemeName)
        assert(InputSchemeManager.defaultInputSchemes.length >= 1)

        const startingLength = InputSchemeManager.availableInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)

        expect(InputSchemeManager.availableInputSchemes.length).toBe(startingLength + 1)
    })
    test("Add a Custom Scheme", () => {
        const startingLength = InputSchemeManager.availableInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)

        assert((InputSchemeManager.availableInputSchemes.length = startingLength + 1))
    })
    test("Get Random Names", () => {
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

describe("Input System Checks", () => {
    const inputSystem = new InputSystem()

    test("Brain Map Exists?", () => {
        assert(InputSystem.brainIndexSchemeMap != undefined)
    })

    test("Inputs are Zero", () => {
        expect(InputSystem.getInput("arcadeDrive", 0)).toBe(0)
        expect(InputSystem.getGamepadAxis(0)).toBe(0)
        expect(InputSystem.getInput("randomInputThatDoesNotExist", 1273)).toBe(0)
        expect(InputSystem.isKeyPressed("keyA")).toBe(false)
        expect(InputSystem.isKeyPressed("ajhsekff")).toBe(false)
        expect(InputSystem.isGamepadButtonPressed(1)).toBe(false)
    })

    test("Modifier State Comparison", () => {
        const allFalse: ModifierState = {
            alt: false,
            ctrl: false,
            shift: false,
            meta: false,
        }

        const differentState: ModifierState = {
            alt: false,
            ctrl: true,
            shift: false,
            meta: true,
        }

        inputSystem.Update(-1)

        expect(InputSystem.compareModifiers(allFalse, EmptyModifierState)).toBe(true)
        expect(InputSystem.compareModifiers(allFalse, InputSystem.currentModifierState)).toBe(true)
        expect(InputSystem.compareModifiers(differentState, InputSystem.currentModifierState)).toBe(false)
        expect(InputSystem.compareModifiers(differentState, differentState)).toBe(true)
        expect(InputSystem.compareModifiers(differentState, allFalse)).toBe(false)
    })
})
