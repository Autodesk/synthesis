import { assert, describe, expect, test } from "vitest"
import InputSystem, { EmptyModifierState, ModifierState } from "@/systems/input/InputSystem"

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
