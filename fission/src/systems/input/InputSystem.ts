import { TouchControlsJoystick } from "@/ui/components/TouchControls"
import Joystick from "../scene/Joystick"
import WorldSystem from "../WorldSystem"
import { InputScheme } from "./InputSchemeManager"

export type ModifierState = {
    alt: boolean
    ctrl: boolean
    shift: boolean
    meta: boolean
}
export const EmptyModifierState: ModifierState = { ctrl: false, alt: false, shift: false, meta: false }

/** Represents any user input */
abstract class Input {
    public inputName: string

    /** @param {string} inputName - The name given to this input to identify it's function. */
    constructor(inputName: string) {
        this.inputName = inputName
    }

    // Returns the current value of the input. Range depends on input type
    abstract getValue(useGamepad: boolean, useTouchControls: boolean): number
}

/** Represents any user input that is a single true/false button. */
class ButtonInput extends Input {
    public keyCode: string
    public keyModifiers: ModifierState

    public gamepadButton: number

    /**
     * All optional params will remain unassigned if not value is given. This can be assigned later by the user through the configuration panel.
     *
     * @param {string} inputName - The name given to this input to identify it's function.
     * @param {string} [keyCode] -  The keyboard button for this input if a gamepad is not used.
     * @param {number} [gamepadButton] -  The gamepad button for this input if a gamepad is used.
     * @param {ModifierState} [keyModifiers] -  The key modifier state for the keyboard input.
     */
    public constructor(inputName: string, keyCode?: string, gamepadButton?: number, keyModifiers?: ModifierState) {
        super(inputName)
        this.keyCode = keyCode ?? ""
        this.keyModifiers = keyModifiers ?? EmptyModifierState
        this.gamepadButton = gamepadButton ?? -1
    }

    /**
     * @param useGamepad Looks at the gamepad if true and the keyboard if false.
     * @returns 1 if pressed, 0 if not pressed or not found.
     */
    getValue(useGamepad: boolean): number {
        // Gamepad button input
        if (useGamepad) {
            return InputSystem.isGamepadButtonPressed(this.gamepadButton) ? 1 : 0
        }

        // Keyboard button input
        return InputSystem.isKeyPressed(this.keyCode, this.keyModifiers) ? 1 : 0
    }
}

/** Represents any user input that is an axis between -1 and 1. Can be a gamepad axis, two gamepad buttons, or two keyboard buttons. */
class AxisInput extends Input {
    public posKeyCode: string
    public posKeyModifiers: ModifierState
    public negKeyCode: string
    public negKeyModifiers: ModifierState

    public gamepadAxisNumber: number
    public touchControlAxis: TouchControlsJoystick
    public joystickInverted: boolean
    public useGamepadButtons: boolean
    public posGamepadButton: number
    public negGamepadButton: number

    /**
     * All optional params will remain unassigned if not value is given. This can be assigned later by the user through the configuration panel.
     *
     * @param {string} inputName - The name given to this input to identify it's function.
     * @param {string} [posKeyCode] - The keyboard input that corresponds to a positive input value (1).
     * @param {string} [negKeyCode] - The keyboard input that corresponds to a negative input value (-1).
     * @param {number} [gamepadAxisNumber] - The gamepad axis that this input looks at if the scheme is set to use a gamepad.
     * @param {boolean} [joystickInverted] - Inverts the input if a gamepad axis is used.
     * @param {boolean} [useGamepadButtons] - If this is true and the scheme is set to use a gamepad, this axis will be between two buttons on the controller.
     * @param {number} [posGamepadButton] - The gamepad button that corresponds to a positive input value (1).
     * @param {number} [negGamepadButton] - The gamepad button that corresponds to a negative input value (-1).
     * @param {ModifierState} [posKeyModifiers] - The key modifier state for the positive keyboard input.
     * @param {ModifierState} [negKeyModifiers] - The key modifier state for the negative keyboard input.
     */
    public constructor(
        inputName: string,
        posKeyCode?: string,
        negKeyCode?: string,
        gamepadAxisNumber?: number,
        joystickInverted?: boolean,
        useGamepadButtons?: boolean,
        posGamepadButton?: number,
        negGamepadButton?: number,
        touchControlAxis?: TouchControlsJoystick,
        posKeyModifiers?: ModifierState,
        negKeyModifiers?: ModifierState
    ) {
        super(inputName)

        this.posKeyCode = posKeyCode ?? ""
        this.posKeyModifiers = posKeyModifiers ?? EmptyModifierState
        this.negKeyCode = negKeyCode ?? ""
        this.negKeyModifiers = negKeyModifiers ?? EmptyModifierState

        this.gamepadAxisNumber = gamepadAxisNumber ?? -1
        this.touchControlAxis = touchControlAxis ?? TouchControlsJoystick.NONE
        this.joystickInverted = joystickInverted ?? false

        this.useGamepadButtons = useGamepadButtons ?? false
        this.posGamepadButton = posGamepadButton ?? -1
        this.negGamepadButton = negGamepadButton ?? -1
    }

    /**
     * @param useGamepad Looks at the gamepad if true and the keyboard if false.
     * @returns {number} KEYBOARD: 1 if positive pressed, -1 if negative pressed, or 0 if none or both are pressed.
     * @returns {number} GAMEPAD: a number between -1 and 1 with a deadband in the middle.
     */
    getValue(useGamepad: boolean, useTouchControls: boolean): number {
        if (useGamepad) {
            // Gamepad joystick axis
            if (!this.useGamepadButtons)
                return InputSystem.getGamepadAxis(this.gamepadAxisNumber) * (this.joystickInverted ? -1 : 1)

            // Gamepad button axis
            return (
                (InputSystem.isGamepadButtonPressed(this.posGamepadButton) ? 1 : 0) -
                (InputSystem.isGamepadButtonPressed(this.negGamepadButton) ? 1 : 0)
            )
        }

        if (useTouchControls) {
            return InputSystem.getTouchControlsAxis(this.touchControlAxis) * (this.joystickInverted ? -1 : 1)
        }

        // Keyboard button axis
        return (
            (InputSystem.isKeyPressed(this.posKeyCode, this.posKeyModifiers) ? 1 : 0) -
            (InputSystem.isKeyPressed(this.negKeyCode, this.negKeyModifiers) ? 1 : 0)
        )
    }
}

/**
 *  The input system listens for and records key presses and joystick positions to be used by robots.
 *  It also maps robot behaviors (such as an arcade drivetrain or an arm) to specific keys through customizable input schemes.
 */
class InputSystem extends WorldSystem {
    public static currentModifierState: ModifierState

    /** The keys currently being pressed. */
    private static _keysPressed: { [key: string]: boolean } = {}

    private static _gpIndex: number | null
    public static gamepad: Gamepad | null

    private static leftJoystick: Joystick
    private static rightJoystick: Joystick

    /** Maps a brain index to an input scheme. */
    public static brainIndexSchemeMap: Map<number, InputScheme> = new Map()

    constructor() {
        super()

        // Initialize input events
        this.handleKeyDown = this.handleKeyDown.bind(this)
        document.addEventListener("keydown", this.handleKeyDown)

        this.handleKeyUp = this.handleKeyUp.bind(this)
        document.addEventListener("keyup", this.handleKeyUp)

        this.gamepadConnected = this.gamepadConnected.bind(this)
        window.addEventListener("gamepadconnected", this.gamepadConnected)

        this.gamepadDisconnected = this.gamepadDisconnected.bind(this)
        window.addEventListener("gamepaddisconnected", this.gamepadDisconnected)

        InputSystem.leftJoystick = new Joystick(
            document.getElementById("joystick-base-left")!,
            document.getElementById("joystick-stick-left")!
        )
        InputSystem.rightJoystick = new Joystick(
            document.getElementById("joystick-base-right")!,
            document.getElementById("joystick-stick-right")!
        )

        // Initialize an event that's triggered when the user exits/enters the page
        document.addEventListener("visibilitychange", () => {
            if (document.hidden) this.clearKeyData()
        })

        // Disable gesture inputs on track pad to zoom into UI
        window.addEventListener(
            "wheel",
            function (e) {
                if (e.ctrlKey) {
                    e.preventDefault() // Prevent the zoom
                }
            },
            { passive: false }
        )
    }

    public Update(_: number): void {
        // Fetch current gamepad information
        if (InputSystem._gpIndex == null) InputSystem.gamepad = null
        else InputSystem.gamepad = navigator.getGamepads()[InputSystem._gpIndex]

        if (!document.hasFocus()) this.clearKeyData()

        // Update the current modifier state to be checked against target stats when getting input values
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

    /** Called when any key is first pressed */
    private handleKeyDown(event: KeyboardEvent) {
        console.log(event.code)
        InputSystem._keysPressed[event.code] = true
    }

    /* Called when any key is released */
    private handleKeyUp(event: KeyboardEvent) {
        InputSystem._keysPressed[event.code] = false
    }

    /** Clears all stored key data when the user leaves the page. */
    private clearKeyData() {
        for (const keyCode in InputSystem._keysPressed) delete InputSystem._keysPressed[keyCode]
    }

    /* Called once when a gamepad is first connected */
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

    /* Called once when a gamepad is first disconnected */
    private gamepadDisconnected(event: GamepadEvent) {
        console.log("Gamepad disconnected from index %d: %s", event.gamepad.index, event.gamepad.id)

        InputSystem._gpIndex = null
    }

    /**
     * @param {string} key - The keycode of the target key.
     * @param {ModifierState} modifiers - The target modifier state. Assumed to be no modifiers if undefined.
     * @returns {boolean} True if the key is pressed or false otherwise.
     */
    public static isKeyPressed(key: string, modifiers?: ModifierState): boolean {
        if (modifiers != null && !InputSystem.compareModifiers(InputSystem.currentModifierState, modifiers))
            return false

        return !!InputSystem._keysPressed[key]
    }

    /**
     * @param {string} inputName The name of the function of the input.
     * @param {number} brainIndex The robot brain index for this input. Used to map to a control scheme.
     * @returns {number} A number between -1 and 1 based on the current state of the input.
     */
    public static getInput(inputName: string, brainIndex: number): number {
        const targetScheme = InputSystem.brainIndexSchemeMap.get(brainIndex)

        const targetInput = targetScheme?.inputs.find(input => input.inputName == inputName) as Input

        if (targetScheme == null || targetInput == null) return 0

        return targetInput.getValue(targetScheme.usesGamepad, targetScheme.usesTouchControls)
    }

    /**
     * @param {ModifierState} state1 Any key modifier state.
     * @param {ModifierState} state2 Any key modifier state.
     * @returns {boolean} True if the modifier states are identical and false otherwise.
     */
    public static compareModifiers(state1: ModifierState, state2: ModifierState): boolean {
        if (!state1 || !state2) return false

        return (
            state1.alt == state2.alt &&
            state1.ctrl == state2.ctrl &&
            state1.meta == state2.meta &&
            state1.shift == state2.shift
        )
    }

    /**
     * @param {number} axisNumber The joystick axis index. Must be an integer.
     * @returns {number} A number between -1 and 1 based on the position of this axis or 0 if no gamepad is connected or the axis is not found.
     */
    public static getGamepadAxis(axisNumber: number): number {
        if (InputSystem.gamepad == null) return 0

        if (axisNumber < 0 || axisNumber >= InputSystem.gamepad.axes.length) return 0

        const value = InputSystem.gamepad.axes[axisNumber]

        // Return value with a deadband
        return Math.abs(value) < 0.15 ? 0 : value
    }

    /**
     *
     * @param {number} buttonNumber - The gamepad button index. Must be an integer.
     * @returns {boolean} True if the button is pressed, false if not, a gamepad isn't connected, or the button can't be found.
     */
    public static isGamepadButtonPressed(buttonNumber: number): boolean {
        if (InputSystem.gamepad == null) return false

        if (buttonNumber < 0 || buttonNumber >= InputSystem.gamepad.buttons.length) return false

        const button = InputSystem.gamepad.buttons[buttonNumber]
        if (button == null) return false

        return button.pressed
    }

    // Returns a number between -1 and 1 from the touch controls
    public static getTouchControlsAxis(axisNumber: TouchControlsJoystick): number {
        let value: number
        if (axisNumber === TouchControlsJoystick.LEFT) value = -InputSystem.leftJoystick.y
        else value = InputSystem.rightJoystick.x

        return value
    }
}

export default InputSystem
export { Input }
export { ButtonInput }
export { AxisInput }
