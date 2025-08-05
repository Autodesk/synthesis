import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import InputSystem from "../InputSystem"
import { EMPTY_MODIFIER_STATE, type InputName, type KeyDescriptor, type ModifierState } from "../InputTypes"
import type { KeyCode } from "../KeyboardTypes"
import Input from "./Input"

/** Represents any user input that is a single true/false button. */
export default class ButtonInput extends Input {
    public keyCode: KeyCode
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
    public constructor(inputName: InputName, keyCode?: KeyCode, gamepadButton?: number, keyModifiers?: ModifierState) {
        super(inputName)
        this.keyCode = keyCode ?? ""
        this.keyModifiers = keyModifiers ?? EMPTY_MODIFIER_STATE
        this.gamepadButton = gamepadButton ?? -1
    }

    /**
     * @param useGamepad Looks at the gamepad if true and the keyboard if false.
     * @returns 1 if pressed, 0 if not pressed or not found.
     */
    getValue(useGamepad: boolean): number {
        const matchModeType = MatchMode.getInstance().getMatchModeType()
        if (matchModeType === MatchModeType.MATCH_ENDED || matchModeType === MatchModeType.AUTONOMOUS) {
            return 0
        }

        // Gamepad button input
        if (useGamepad) {
            return InputSystem.isGamepadButtonPressed(this.gamepadButton) ? 1 : 0
        }

        // Keyboard button input
        return InputSystem.isKeyPressed(this.keyCode, this.keyModifiers) ? 1 : 0
    }

    get keysUsed(): KeyDescriptor[] {
        return [this.describeKey(this.keyCode, this.keyModifiers), this.describeGamepadBtn(this.gamepadButton)]
    }

    static onGamepad(inputName: InputName, gamepadButton: number) {
        return new ButtonInput(inputName, undefined, gamepadButton, undefined)
    }
    static onKeyboard(inputName: InputName, keyCode: KeyCode, keyModifiers?: ModifierState) {
        return new ButtonInput(inputName, keyCode, undefined, keyModifiers)
    }
    static unbound(inputName: InputName) {
        return new ButtonInput(inputName, undefined, undefined, undefined)
    }
}
