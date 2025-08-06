import { assert, beforeEach, describe, expect, test, vi } from "vitest"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import {
    EMPTY_MODIFIER_STATE,
    type InputName,
    type KeyDescriptor,
    type ModifierState,
} from "@/systems/input/InputTypes"
import AxisInput from "@/systems/input/inputs/AxisInput"
import ButtonInput from "@/systems/input/inputs/ButtonInput"
import type { KeyCode } from "@/systems/input/KeyboardTypes.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"

describe("Input Scheme Manager Checks", () => {
    test("Available Schemes", () => {
        assert(InputSchemeManager.allInputSchemes[0].schemeName == DefaultInputs.ernie().schemeName)
        assert(InputSchemeManager.defaultInputSchemes.length >= 1)
    })

    test("Add a Custom Scheme", () => {
        const startingLength = InputSchemeManager.allInputSchemes.length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme(DriveType.ARCADE))

        expect(InputSchemeManager.allInputSchemes.length).toBe(startingLength + 1)
    })

    test("Change Custom Scheme Values", () => {
        const scheme = DefaultInputs.newBlankScheme(DriveType.ARCADE)
        scheme.schemeName = "Test Scheme"
        expect(scheme.schemeName).toBe("Test Scheme")
        InputSchemeManager.addCustomScheme(scheme)
        scheme.inputs[0].inputName = "joint 9999"
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
        const startingLength = PreferencesSystem.getGlobalPreference("InputSchemes").length
        InputSchemeManager.addCustomScheme(DefaultInputs.newBlankScheme(DriveType.ARCADE))
        InputSchemeManager.saveSchemes()
        const newLength = PreferencesSystem.getGlobalPreference("InputSchemes").length
        expect(newLength).toBe(startingLength + 1)
    })

    test("Get Random Names", () => {
        const names: string[] = []
        for (let i = 0; i < 20; i++) {
            const name = InputSchemeManager.randomAvailableName
            expect(names.includes(name)).toBe(false)
            assert(name != undefined)
            expect(name.length).toBeGreaterThan(0)

            const scheme = DefaultInputs.newBlankScheme(DriveType.ARCADE)
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
        expect(InputSystem.getInput("joint 987654", 1273)).toBe(0)
        expect(InputSystem.isKeyPressed("KeyA")).toBe(false)
        expect(InputSystem.isKeyPressed("ajhsekff" as KeyCode)).toBe(false)
        expect(InputSystem.isGamepadButtonPressed(1)).toBe(false)
    })

    test("Keyboard Input", () => {
        function testKeyPress(key: KeyCode) {
            // Simulate key press
            document.dispatchEvent(new KeyboardEvent("keydown", { code: key }))

            // Check if the key is registered as pressed
            expect(InputSystem.isKeyPressed(key)).toBe(true)

            // Simulate key release
            document.dispatchEvent(new KeyboardEvent("keyup", { code: key }))

            // Check if the key is no longer registered as pressed
            expect(InputSystem.isKeyPressed(key)).toBe(false)
        }

        testKeyPress("KeyA")
        testKeyPress("KeyK")
        testKeyPress("KeyR")
        testKeyPress("ShiftRight")
        testKeyPress("ControlLeft")
        testKeyPress("Enter")
        testKeyPress("Escape")
        testKeyPress("Space")
    })

    test("Arcade Drive", () => {
        InputSystem.setBrainIndexSchemeMapping(0, DefaultInputs.ernie())
        inputSystem.update(-1) // Initialize the input system

        function testArcadeInput(inputMap: InputName, key: string, expectedValue: number) {
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

        inputSystem.update(-1)

        expect(InputSystem.compareModifiers(allFalse, EMPTY_MODIFIER_STATE)).toBe(true)
        expect(InputSystem.compareModifiers(allFalse, InputSystem.currentModifierState)).toBe(true)
        expect(InputSystem.compareModifiers(differentState, InputSystem.currentModifierState)).toBe(false)
        expect(InputSystem.compareModifiers(differentState, differentState)).toBe(true)
        expect(InputSystem.compareModifiers(differentState, allFalse)).toBe(false)
    })
})

describe("Gamepad Input Check", () => {
    let fakeGamepad: Gamepad

    beforeEach(() => {
        fakeGamepad = {
            id: "Test Gamepad",
            index: 0,
            axes: [0.5, -0.5],
            buttons: [
                { pressed: true, value: 1.0 },
                { pressed: false, value: 0.0 },
            ],
            connected: true,
            mapping: "standard",
            timestamp: Date.now(),
        } as unknown as Gamepad

        vi.spyOn(navigator, "getGamepads").mockReturnValue([fakeGamepad, null, null, null])
        const ev = new Event("gamepadconnected") as GamepadEvent
        Object.defineProperty(ev, "gamepad", {
            value: fakeGamepad,
            writable: false,
            enumerable: true,
            configurable: true,
        })
        window.dispatchEvent(ev)
    })

    test("Reads axes correctly", () => {
        const sys = new InputSystem()
        sys.update(0)

        expect(InputSystem.getGamepadAxis(0)).toBe(0.5)
        expect(InputSystem.getGamepadAxis(1)).toBe(-0.5)
    })

    test("Applies dead-band", () => {
        const updatedGamepad = {
            ...fakeGamepad,
            axes: [0.1, -0.1],
        } as unknown as Gamepad

        vi.spyOn(navigator, "getGamepads").mockReturnValue([updatedGamepad, null, null, null])
        const sys = new InputSystem()
        sys.update(0)

        expect(InputSystem.getGamepadAxis(0)).toBe(0)
        expect(InputSystem.getGamepadAxis(1)).toBe(0)
    })

    test("Invalid axis indices return 0", () => {
        const updatedGamepad = {
            ...fakeGamepad,
            axes: [0.9],
        } as unknown as Gamepad
        vi.spyOn(navigator, "getGamepads").mockReturnValue([updatedGamepad, null, null, null])
        const sys = new InputSystem()
        sys.update(0)

        expect(InputSystem.getGamepadAxis(-1)).toBe(0)
        expect(InputSystem.getGamepadAxis(0)).toBe(0.9)
        expect(InputSystem.getGamepadAxis(1)).toBe(0)
    })

    test("Gamepad button pressed", () => {
        const sys = new InputSystem()
        sys.update(0)

        expect(InputSystem.isGamepadButtonPressed(0)).toBe(true)
        expect(InputSystem.isGamepadButtonPressed(1)).toBe(false)
        expect(InputSystem.isGamepadButtonPressed(2)).toBe(false) // Non-existent button
    })

    test("AxisInput inverts axis values (joystickInverted=true)", () => {
        vi.spyOn(InputSystem, "getGamepadAxis").mockReturnValue(0.6)

        const axis = new AxisInput("joint 1", undefined, undefined, 0, /* joystickInverted=true */ true, false)
        expect(axis.getValue(true, false)).toBe(-0.6)
    })

    test("Use gamepad buttons mode", () => {
        const axis = new AxisInput("joint 2", undefined, undefined, undefined, false, true, 1, 2)
        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockImplementation(b => b === 1)
        expect(axis.getValue(true, false)).toBe(1)

        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockImplementation(b => b === 2)
        expect(axis.getValue(true, false)).toBe(-1)

        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockReturnValue(false)
        expect(axis.getValue(true, false)).toBe(0)
    })

    test("End-to-end button-input", () => {
        const btn = new ButtonInput("joint 3", undefined, /*gamepadButton*/ 0)
        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockReturnValue(true)
        expect(btn.getValue(true)).toBe(1)

        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockReturnValue(false)
        expect(btn.getValue(true)).toBe(0)
    })

    test("Disconnect event", () => {
        // The connection event is implicitly tested by registering a fake gamepad
        window.dispatchEvent(Object.assign(new Event("gamepaddisconnected"), { gamepad: fakeGamepad }))
        expect(InputSystem["_gpIndex"]).toBeNull()
    })

    test("Get input with gamepad scheme", () => {
        const scheme = DefaultInputs.newBlankScheme(DriveType.ARCADE)
        scheme.usesGamepad = true
        scheme.inputs = [new ButtonInput("joint 4", undefined, 0)]
        InputSystem.setBrainIndexSchemeMapping(42, scheme)

        vi.spyOn(InputSystem, "isGamepadButtonPressed").mockReturnValue(true)
        expect(InputSystem.getInput("joint 4", 42)).toBe(1)
    })
})

describe("Default Input Scheme Checks", () => {
    test("Default schemes unique names", () => {
        const defaults = DefaultInputs.defaultInputCopies
        const names = defaults.map(scheme => scheme.schemeName)
        names.forEach(name => {
            expect.soft(names.filter(other => other == name).length, `Only one schema named ${name}`).toBe(1)
        })
    })
    test("Default schemes internally conflict-free", () => {
        DefaultInputs.defaultInputCopies.forEach(scheme => {
            const usedKeys = new Map<KeyDescriptor, number>()
            scheme.inputs.forEach(input => {
                input.keysUsed
                    .filter(key => key != null)
                    .forEach(key => usedKeys.set(key, (usedKeys.get(key) ?? 0) + 1))
                usedKeys.forEach((count, key) => {
                    expect.soft(count, `key ${key} used only once in scheme ${scheme.schemeName}`).toBe(1)
                })
            })
        })
    })
})
