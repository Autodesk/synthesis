import type { KeyCode } from "@/systems/input/KeyboardTypes.ts"
import { TouchControlsAxes } from "@/ui/components/TouchControls"
import World from "../World"
import WorldSystem from "../WorldSystem"
import type { InputName, InputScheme, ModifierState } from "./InputTypes"
import type Input from "./inputs/Input"

const LOG_GAMEPAD_EVENTS = false

// returns true if 'esc' was consumed
type EscapeHandler = () => boolean

export const ESCAPE_PRIORITY = { COMMAND_PALETTE: 30, TOUR: 20, MODAL: 10, PANEL: 0 } as const

/**
 *  The input system listens for and records key presses and joystick positions to be used by robots.
 *  It also maps robot behaviors (such as an arcade drivetrain or an arm) to specific keys through customizable input schemes.
 */
class InputSystem extends WorldSystem {
    public static currentModifierState: ModifierState

    /** The keys currently being pressed. */
    private static _keysPressed: Partial<Record<KeyCode, boolean>> = {}

    /** Whether the command palette is currently open, which blocks robot input */
    private static _isCommandPaletteOpen: boolean = false

    private static _gpIndex: number | null
    public static gamepad: Gamepad | null

    /** Normalized joystick positions (-1 to 1) set by TouchControls component via react-joystick-component */
    private static _leftJoystickPos: { x: number; y: number } = { x: 0, y: 0 }
    private static _rightJoystickPos: { x: number; y: number } = { x: 0, y: 0 }

    /** Maps a brain index to an input scheme. */
    private static _brainIndexSchemeMap: Map<number, InputScheme> = new Map()

    public static get brainIndexSchemeMap() {
        return this._brainIndexSchemeMap
    }

    public static setBrainIndexSchemeMapping(index: number, scheme: InputScheme) {
        this.brainIndexSchemeMap.set(index, scheme)
        World.analyticsSystem?.event("Scheme Applied", {
            isCustomized: scheme.customized,
            schemeName: scheme.schemeName,
        })
    }
    public static getBrainIndexSchemeMapping(index: number): InputScheme | undefined {
        return this.brainIndexSchemeMap.get(index)
    }

    private static _escapeHandlers: { priority: number; handler: EscapeHandler }[] = []

    /** highest priority is asked first. call the result to unregister. */
    public static addEscapeHandler(handler: EscapeHandler, priority = 0): () => void {
        const entry = { priority, handler }
        InputSystem._escapeHandlers.push(entry)
        InputSystem._escapeHandlers.sort((a, b) => b.priority - a.priority)

        return () => {
            InputSystem._escapeHandlers = InputSystem._escapeHandlers.filter(e => e !== entry)
        }
    }

    /**
     * Sets whether the command palette is open, which blocks all robot inputs
     */
    public static setCommandPaletteOpen(isOpen: boolean) {
        InputSystem._isCommandPaletteOpen = isOpen
    }

    /** Called by TouchControls component to update the left joystick position. Values are normalized (-1 to 1) */
    public static setLeftJoystick(x: number, y: number) {
        InputSystem._leftJoystickPos = { x, y }
    }

    /** Called by TouchControls component to update the right joystick position. Values are normalized (-1 to 1) */
    public static setRightJoystick(x: number, y: number) {
        InputSystem._rightJoystickPos = { x, y }
    }

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

    public update(_: number): void {
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

    public destroy(): void {
        document.removeEventListener("keydown", this.handleKeyDown)
        document.removeEventListener("keyup", this.handleKeyUp)
        window.removeEventListener("gamepadconnected", this.gamepadConnected)
        window.removeEventListener("gamepaddisconnected", this.gamepadDisconnected)
    }

    /** Called when any key is first pressed */
    private handleKeyDown(event: KeyboardEvent) {
        InputSystem._keysPressed[event.code as KeyCode] = true
        this.checkEscapeKey(event)
    }

    /* Called when any key is released */
    private handleKeyUp(event: KeyboardEvent) {
        if (InputSystem._keysPressed["Escape"] == false) {
            // Sometimes focus issues prevent the keydown from being fired, but the keyup is still sent.
            this.checkEscapeKey(event)
        }

        InputSystem._keysPressed[event.code as KeyCode] = false
    }

    private checkEscapeKey(event: KeyboardEvent) {
        if (event.key == "Escape") {
            if (InputSystem._escapeHandlers.some(entry => entry.handler())) {
                event.preventDefault()
            }
        }
    }

    /** Clears all stored key data when the user leaves the page. */
    private clearKeyData() {
        for (const keyCode in InputSystem._keysPressed) delete InputSystem._keysPressed[keyCode as KeyCode]
    }

    /* Called once when a gamepad is first connected */
    private gamepadConnected(event: GamepadEvent) {
        if (LOG_GAMEPAD_EVENTS) {
            console.log(
                "Gamepad connected at index %d: %s. %d buttons, %d axes.",
                event.gamepad.index,
                event.gamepad.id,
                event.gamepad.buttons.length,
                event.gamepad.axes.length
            )
        }

        InputSystem._gpIndex = event.gamepad.index
    }

    /* Called once when a gamepad is first disconnected */
    private gamepadDisconnected(event: GamepadEvent) {
        if (LOG_GAMEPAD_EVENTS) {
            console.log("Gamepad disconnected from index %d: %s", event.gamepad.index, event.gamepad.id)
        }

        InputSystem._gpIndex = null
    }

    /**
     * @param {string} key - The keycode of the target key.
     * @param {ModifierState} modifiers - The target modifier state. Assumed to be no modifiers if undefined.
     * @returns {boolean} True if the key is pressed or false otherwise.
     */
    public static isKeyPressed(key: KeyCode, modifiers?: ModifierState): boolean {
        if (modifiers != null && !InputSystem.compareModifiers(InputSystem.currentModifierState, modifiers))
            return false

        return Boolean(InputSystem._keysPressed[key])
    }

    /**
     * @param {string} inputName The name of the function of the input.
     * @param {number} brainIndex The robot brain index for this input. Used to map to a control scheme.
     * @returns {number} A number between -1 and 1 based on the current state of the input.
     */
    public static getInput(inputName: InputName, brainIndex: number): number {
        // Block all robot inputs when command palette is open
        if (InputSystem._isCommandPaletteOpen) {
            return 0
        }

        const targetScheme = InputSystem.getBrainIndexSchemeMapping(brainIndex)

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

    /** Returns a number between -1 and 1 from the touch controls */
    public static getTouchControlsAxis(axisType: TouchControlsAxes): number {
        if (axisType === TouchControlsAxes.LEFT_X) return InputSystem._leftJoystickPos.x
        if (axisType === TouchControlsAxes.LEFT_Y) return InputSystem._leftJoystickPos.y
        if (axisType === TouchControlsAxes.RIGHT_X) return InputSystem._rightJoystickPos.x
        if (axisType === TouchControlsAxes.RIGHT_Y) return InputSystem._rightJoystickPos.y
        return 0
    }
}

export default InputSystem
