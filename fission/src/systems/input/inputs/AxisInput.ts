import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import { TouchControlsAxes } from "@/ui/components/TouchControls"
import InputSystem from "../InputSystem"
import { EMPTY_MODIFIER_STATE, type InputName, type KeyDescriptor, type ModifierState } from "../InputTypes"
import type { KeyCode } from "../KeyboardTypes"
import Input from "./Input"

/** Represents any user input that is an axis between -1 and 1. Can be a gamepad axis, two gamepad buttons, or two keyboard buttons. */
export default class AxisInput extends Input {
    public posKeyCode: KeyCode
    public posKeyModifiers: ModifierState
    public negKeyCode: KeyCode
    public negKeyModifiers: ModifierState

    public gamepadAxisNumber: number
    public touchControlAxis: TouchControlsAxes
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
        inputName: InputName,
        posKeyCode?: KeyCode,
        negKeyCode?: KeyCode,
        gamepadAxisNumber?: number,
        joystickInverted?: boolean,
        useGamepadButtons?: boolean,
        posGamepadButton?: number,
        negGamepadButton?: number,
        touchControlAxis?: TouchControlsAxes,
        posKeyModifiers?: ModifierState,
        negKeyModifiers?: ModifierState
    ) {
        super(inputName)

        this.posKeyCode = posKeyCode ?? ""
        this.posKeyModifiers = posKeyModifiers ?? EMPTY_MODIFIER_STATE
        this.negKeyCode = negKeyCode ?? ""
        this.negKeyModifiers = negKeyModifiers ?? EMPTY_MODIFIER_STATE

        this.gamepadAxisNumber = gamepadAxisNumber ?? -1
        this.touchControlAxis = touchControlAxis ?? TouchControlsAxes.NONE
        this.joystickInverted = joystickInverted ?? false

        this.useGamepadButtons = useGamepadButtons ?? false
        this.posGamepadButton = posGamepadButton ?? -1
        this.negGamepadButton = negGamepadButton ?? -1
    }
    public static unbound(inputName: InputName) {
        return new AxisInput(inputName)
    }
    public static onGamepadJoystick(inputName: InputName, gamepadAxisNumber: number, joystickInverted: boolean) {
        return new AxisInput(
            inputName,
            undefined,
            undefined,
            gamepadAxisNumber,
            joystickInverted,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined
        )
    }
    public static onGamepadButtons(inputName: InputName, posGamepadButton: number, negGamepadButton: number) {
        return new AxisInput(
            inputName,
            undefined,
            undefined,
            undefined,
            undefined,
            true,
            posGamepadButton,
            negGamepadButton,
            undefined,
            undefined,
            undefined
        )
    }
    public static onKeyboard(
        inputName: InputName,
        posKeyCode: KeyCode,
        negKeyCode: KeyCode,
        posKeyModifiers?: ModifierState,
        negKeyModifiers?: ModifierState
    ) {
        return new AxisInput(
            inputName,
            posKeyCode,
            negKeyCode,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            posKeyModifiers,
            negKeyModifiers
        )
    }
    public static onKeyboardSingleKey(inputName: InputName, key: KeyCode, negKeyModifiers?: ModifierState) {
        return new AxisInput(
            inputName,
            key,
            key,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            negKeyModifiers
        )
    }
    public static onTouchControl(inputName: InputName, touchControlAxis: TouchControlsAxes) {
        return new AxisInput(
            inputName,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            touchControlAxis,
            undefined,
            undefined
        )
    }

    /**
     * @param useGamepad Looks at the gamepad if true and the keyboard if false.
     * @returns {number} KEYBOARD: 1 if positive pressed, -1 if negative pressed, or 0 if none or both are pressed.
     * @returns {number} GAMEPAD: a number between -1 and 1 with a deadband in the middle.
     */
    getValue(useGamepad: boolean, useTouchControls: boolean): number {
        const matchModeType = MatchMode.getInstance().getMatchModeType()
        if (matchModeType === MatchModeType.MATCH_ENDED || matchModeType === MatchModeType.AUTONOMOUS) {
            return 0
        }

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

    get keysUsed(): KeyDescriptor[] {
        return [
            this.describeKey(this.posKeyCode, this.posKeyModifiers),
            this.describeKey(this.negKeyCode, this.negKeyModifiers),
            this.describeGamepadBtn(this.posGamepadButton),
            this.describeGamepadBtn(this.negGamepadButton),
            this.describeGamepadAxis(this.gamepadAxisNumber),
            this.describeTouchAxis(this.touchControlAxis),
        ]
    }
}
