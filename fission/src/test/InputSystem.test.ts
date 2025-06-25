import { test, describe, assert, expect } from "vitest"
import InputSystem, { AxisInput, ButtonInput, EmptyModifierState, ModifierState } from "@/systems/input/InputSystem"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import DefaultInputs from "@/systems/input/DefaultInputs"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

describe("Input Scheme Manager Checks", () => {
    test("Available Schemes", () => {
        assert(InputSchemeManager.availableInputSchemes[0].schemeName == DefaultInputs.ernie().schemeName)
        assert(InputSchemeManager.defaultInputSchemes.length >= 1)
    })

    test("Add a Custom Scheme", () => {
        const startingLength = InputSchemeManager.availableInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)

        expect(InputSchemeManager.availableInputSchemes.length).toBe(startingLength + 1)
    })

    test("Change Custom Scheme Values", () => {
        const scheme = DefaultInputs.newBlankScheme
        scheme.schemeName = "Test Scheme"
        expect(scheme.schemeName).toBe("Test Scheme")
        InputSchemeManager.addCustomScheme(scheme)
        scheme.inputs[0].inputName = "Test Input"
        scheme.inputs.forEach(input => {
            if (input instanceof ButtonInput) {
                input.keyCode = "KeyA"
                expect(input.keyCode).toBe("KeyA")
            } else if (input instanceof AxisInput) {
                input.posGamepadButton = 0
                expect(input.posGamepadButton).toBe(0)
            }
        })
    })

    test("Saving Schemes", () => {
        const startingLength = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes").length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme)
        InputSchemeManager.saveSchemes()
        const newLength = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes").length
        expect(newLength).toBe(startingLength + 1)
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

    test("Keyboard Input", () => {
        function testKeyPress(key: string) {
            // Simulate key press
            document.dispatchEvent(new KeyboardEvent("keydown", { code: key }))

            // Check if the key is registered as pressed
            expect(InputSystem.isKeyPressed(key)).toBe(true)

            // Simulate key release
            document.dispatchEvent(new KeyboardEvent("keyup", { code: key }))

            // Check if the key is no longer registered as pressed
            expect(InputSystem.isKeyPressed(key)).toBe(false)
        }

        testKeyPress("keyA")
        testKeyPress("KeyK")
        testKeyPress("KeyR")
        testKeyPress("RightShift")
        testKeyPress("LeftControl")
        testKeyPress("Enter")
        testKeyPress("Escape")
        testKeyPress("Space")
    })

    test("Arcade Drive", () => {
        InputSystem.brainIndexSchemeMap.set(0, DefaultInputs.ernie())
        inputSystem.Update(-1) // Initialize the input system

        function testArcadeInput(inputMap: string, key: string, expectedValue: number) {
            document.dispatchEvent(new KeyboardEvent("keydown", { code: key }))
            expect(InputSystem.getInput(inputMap, 0)).toBe(expectedValue)
            document.dispatchEvent(new KeyboardEvent("keyup", { code: key }))
            expect(InputSystem.getInput(inputMap, 0)).toBe(0)
        }

        testArcadeInput("arcadeDrive", "KeyW", 1) // Forward
        testArcadeInput("arcadeDrive", "KeyS", -1) // Backward
        testArcadeInput("arcadeTurn", "KeyD", 1) // Right
        testArcadeInput("arcadeTurn", "KeyA", -1) // Left
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
