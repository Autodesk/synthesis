import WorldSystem from "../WorldSystem"
import { InputScheme } from "./InputSchemeManager"

export type ModifierState = {
    alt: boolean
    ctrl: boolean
    shift: boolean
    meta: boolean
}
export const EmptyModifierState: ModifierState = { ctrl: false, alt: false, shift: false, meta: false }

// Represents any input
abstract class Input {
    public inputName: string

    constructor(inputName: string) {
        this.inputName = inputName
    }

    // Returns the current value of the input. Range depends on input type
    abstract getValue(useGamepad: boolean): number

    // Creates a copy to avoid modifying the default inputs by reference
    abstract getCopy(): Input
}

// A single button
class ButtonInput extends Input {
    public keyCode: string
    public keyModifiers: ModifierState

    public gamepadButton: number

    public constructor(inputName: string, keyCode?: string, gamepadButton?: number, keyModifiers?: ModifierState) {
        super(inputName)
        this.keyCode = keyCode ?? ""
        this.keyModifiers = keyModifiers ?? EmptyModifierState
        this.gamepadButton = gamepadButton ?? -1
    }

    // Returns 1 if pressed and 0 if not pressed
    getValue(useGamepad: boolean): number {
        // Gamepad button input
        if (useGamepad) {
            return InputSystem.isGamepadButtonPressed(this.gamepadButton) ? 1 : 0
        }

        // Keyboard button input
        return InputSystem.isKeyPressed(this.keyCode, this.keyModifiers) ? 1 : 0
    }

    getCopy(): Input {
        return new ButtonInput(this.inputName, this.keyCode, this.gamepadButton, this.keyModifiers)
    }
}

// An axis between two buttons (-1 to 1)
class AxisInput extends Input {
    public posKeyCode: string
    public posKeyModifiers: ModifierState
    public negKeyCode: string
    public negKeyModifiers: ModifierState

    public gamepadAxisNumber: number
    public joystickInverted: boolean
    public useGamepadButtons: boolean
    public posGamepadButton: number
    public negGamepadButton: number

    public constructor(
        inputName: string,
        posKeyCode?: string,
        negKeyCode?: string,
        gamepadAxisNumber?: number,
        joystickInverted?: boolean,
        useGamepadButtons?: boolean,
        posGamepadButton?: number,
        negGamepadButton?: number,
        posKeyModifiers?: ModifierState,
        negKeyModifiers?: ModifierState
    ) {
        super(inputName)

        this.posKeyCode = posKeyCode ?? ""
        this.posKeyModifiers = posKeyModifiers ?? EmptyModifierState
        this.negKeyCode = negKeyCode ?? ""
        this.negKeyModifiers = negKeyModifiers ?? EmptyModifierState

        this.gamepadAxisNumber = gamepadAxisNumber ?? -1
        this.joystickInverted = joystickInverted ?? false

        this.useGamepadButtons = useGamepadButtons ?? false
        this.posGamepadButton = posGamepadButton ?? -1
        this.negGamepadButton = negGamepadButton ?? -1
    }

    // For keyboard: returns 1 if positive pressed, -1 if negative pressed, or 0 if none or both are pressed
    // For gamepad axis: returns a range between -1 and 1 with a deadband in the middle
    getValue(useGamepad: boolean): number {
        // Gamepad joystick axis
        if (useGamepad) {
            if (!this.useGamepadButtons)
                return InputSystem.getGamepadAxis(this.gamepadAxisNumber) * (this.joystickInverted ? -1 : 1)

            // Gamepad button axis
            return (
                (InputSystem.isGamepadButtonPressed(this.posGamepadButton) ? 1 : 0) -
                (InputSystem.isGamepadButtonPressed(this.negGamepadButton) ? 1 : 0)
            )
        }

        // Keyboard button axis
        return (
            (InputSystem.isKeyPressed(this.posKeyCode, this.posKeyModifiers) ? 1 : 0) -
            (InputSystem.isKeyPressed(this.negKeyCode, this.negKeyModifiers) ? 1 : 0)
        )
    }

    getCopy(): Input {
        return new AxisInput(
            this.inputName,
            this.posKeyCode,
            this.negKeyCode,
            this.gamepadAxisNumber,
            this.joystickInverted,
            this.useGamepadButtons,
            this.posGamepadButton,
            this.negGamepadButton,
            this.posKeyModifiers,
            this.negKeyModifiers
        )
    }
}

class InputSystem extends WorldSystem {
    public static currentModifierState: ModifierState

    // A list of keys currently being pressed
    private static _keysPressed: { [key: string]: boolean } = {}

    private static _gpIndex: number | null
    public static gamepad: Gamepad | null

    // Maps a brain index to a certain input scheme
    public static brainIndexSchemeMap: Map<number, InputScheme> = new Map()

    constructor() {
        super()

        this.handleKeyDown = this.handleKeyDown.bind(this)
        document.addEventListener("keydown", this.handleKeyDown)

        this.handleKeyUp = this.handleKeyUp.bind(this)
        document.addEventListener("keyup", this.handleKeyUp)

        this.gamepadConnected = this.gamepadConnected.bind(this)
        window.addEventListener("gamepadconnected", this.gamepadConnected)

        this.gamepadDisconnected = this.gamepadDisconnected.bind(this)
        window.addEventListener("gamepaddisconnected", this.gamepadDisconnected)

        document.addEventListener("visibilitychange", () => {
            if (document.hidden) this.clearKeyData()
        })
    }

    public Update(_: number): void {
        InputSystem
        // Fetch current gamepad information
        if (InputSystem._gpIndex == null) InputSystem.gamepad = null
        else InputSystem.gamepad = navigator.getGamepads()[InputSystem._gpIndex]

        if (!document.hasFocus()) this.clearKeyData()

        InputSystem.currentModifierState = {
            ctrl: InputSystem.isKeyPressed("ControlLeft") || InputSystem.isKeyPressed("ControlRight"),
            alt: InputSystem.isKeyPressed("AltLeft") || InputSystem.isKeyPressed("AltRight"),
            shift: InputSystem.isKeyPressed("ShiftLeft") || InputSystem.isKeyPressed("ShiftRight"),
            meta: InputSystem.isKeyPressed("MetaLeft") || InputSystem.isKeyPressed("MetaRight"),
        }
    }

    public Destroy(): void {
        document.removeEventListener("keydown", this.handleKeyDown)
        document.removeEventListener("keyup", this.handleKeyUp)
        window.removeEventListener("gamepadconnected", this.gamepadConnected)
        window.removeEventListener("gamepaddisconnected", this.gamepadDisconnected)
    }

    // Called when any key is first pressed
    private handleKeyDown(event: KeyboardEvent) {
        console.log(event.code)
        InputSystem._keysPressed[event.code] = true
    }

    // Called when any key is released
    private handleKeyUp(event: KeyboardEvent) {
        InputSystem._keysPressed[event.code] = false
    }

    private clearKeyData() {
        for (const keyCode in InputSystem._keysPressed) delete InputSystem._keysPressed[keyCode]
    }

    // Called once when a gamepad is first connected
    private gamepadConnected(event: GamepadEvent) {
        console.log(
            "Gamepad connected at index %d: %s. %d buttons, %d axes.",
            event.gamepad.index,
            event.gamepad.id,
            event.gamepad.buttons.length,
            event.gamepad.axes.length
        )

        InputSystem._gpIndex = event.gamepad.index
    }

    // Called once when a gamepad is first disconnected
    private gamepadDisconnected(event: GamepadEvent) {
        console.log("Gamepad disconnected from index %d: %s", event.gamepad.index, event.gamepad.id)

        InputSystem._gpIndex = null
    }

    // Returns true if the given key is currently down
    public static isKeyPressed(key: string, modifiers?: ModifierState): boolean {
        if (modifiers != null && !InputSystem.compareModifiers(InputSystem.currentModifierState, modifiers))
            return false

        return !!InputSystem._keysPressed[key]
    }

    // If an input exists, return it's value
    public static getInput(inputName: string, brainIndex: number): number {
        const targetScheme = InputSystem.brainIndexSchemeMap.get(brainIndex)

        const targetInput = targetScheme?.inputs.find(input => input.inputName == inputName) as Input

        if (targetScheme == null || targetInput == null) return 0

        return targetInput.getValue(targetScheme.usesGamepad)
    }

    // Returns true if two modifier states are identical
    private static compareModifiers(state1: ModifierState, state2: ModifierState): boolean {
        if (!state1 || !state2) return false

        return (
            state1.alt == state2.alt &&
            state1.ctrl == state2.ctrl &&
            state1.meta == state2.meta &&
            state1.shift == state2.shift
        )
    }

    // Returns a number between -1 and 1 with a deadband
    public static getGamepadAxis(axisNumber: number): number {
        if (InputSystem.gamepad == null) return 0

        if (axisNumber < 0 || axisNumber >= InputSystem.gamepad.axes.length) return 0

        const value = InputSystem.gamepad.axes[axisNumber]

        // Return value with a deadband
        return Math.abs(value) < 0.15 ? 0 : value
    }

    // Returns true if a gamepad is connected and a certain button is pressed
    public static isGamepadButtonPressed(buttonNumber: number): boolean {
        if (InputSystem.gamepad == null) return false

        if (buttonNumber < 0 || buttonNumber >= InputSystem.gamepad.buttons.length) return false

        const button = InputSystem.gamepad.buttons[buttonNumber]
        if (button == null) return false

        return button.pressed
    }
}

export default InputSystem
export { Input }
export { ButtonInput }
export { AxisInput }
