import WorldSystem from "../WorldSystem";

export type ModifierState = {
    alt: boolean
    ctrl: boolean
    shift: boolean
    meta: boolean
}

abstract class Input {
    public inputName: string;
    public isGlobal: boolean;
    public usesGamepad: boolean;

    constructor(inputName: string, isGlobal: boolean, usesGamepad: boolean) {
        this.inputName = inputName;
        this.isGlobal = isGlobal;
        this.usesGamepad = usesGamepad;
    }    

    abstract getValue(): number;
}

// A single button
class ButtonInput extends Input {    
    public keyCode: string;
    public modifiers: ModifierState;

    public constructor(inputName: string, keyCode: string, isGlobal?: boolean, usesGamepad?: boolean, modifiers?: ModifierState) {
        super(inputName, isGlobal ?? false, usesGamepad ?? false);
        this.keyCode = keyCode;
        this.modifiers = modifiers ?? emptyModifierState;
    }    

    // Returns 1 if pressed and 0 if not pressed
    getValue(): number {
        throw new Error("Not Fully Implemented");
        // (pretty sure this is fixed, but double check) TODO: will not work because this will check for the key in allInputs[]
        return InputSystem.getInput(this.keyCode);
    }
}

// An axis between two buttons (-1 to 1)
class ButtonAxisInput extends Input {    
    public posKeyCode: string;
    public posModifiers: ModifierState;
    public negKeyCode: string;
    public negModifiers: ModifierState;

    public constructor(inputName: string, posKeyCode: string, negKeyCode: string, isGlobal?: boolean, usesGamepad?: boolean, posModifiers?: ModifierState, negModifiers?: ModifierState) {
        super(inputName, isGlobal ?? false, usesGamepad ?? false);
        
        this.posKeyCode = posKeyCode;
        this.posModifiers = posModifiers ?? emptyModifierState;
        this.negKeyCode = negKeyCode;
        this.negModifiers = negModifiers ?? emptyModifierState;
    }    

    // Returns 1 if positive pressed, -1 if negative pressed, or 0 if none or both are pressed
    getValue(): number {
        throw new Error("Not Fully Implemented");
        // TODO: will not work because this will check for the key in allInputs[]
        return (InputSystem.getInput(this.posKeyCode)) - (InputSystem.getInput(this.negKeyCode));
    }
}

// An axis using a joystick (-1 to 1)
class GamepadAxisInput extends Input {    
    public axisNumber: number;
    public inverted: boolean;

    public constructor(inputName: string, axisNumber: number, inverted?: boolean, isGlobal?: boolean,) {
        super(inputName, isGlobal ?? false, true);
        
        this.axisNumber = axisNumber;
        this.inverted = inverted ?? false;
    }    

    // A value between -1 and 1 based on the joysticks position with a deadband
    getValue(): number {
        return InputSystem.getGamepadAxis(this.axisNumber) * (this.inverted ? -1 : 1);
    }
}

export const emptyModifierState: ModifierState = { ctrl: false, alt: false, shift: false, meta: false };

// When a robot is loaded, default inputs replace any unassigned inputs


const defaultInputs: Input[] = [
    new ButtonInput("intakeGamepiece", "KeyE", true),
    new ButtonInput("shootGamepiece", "KeyQ", true),
    new ButtonInput("enableGodMode", "KeyG", true),

    new GamepadAxisInput("arcadeDrive", 1, true),
    new GamepadAxisInput("arcadeTurn", 0, false)
]

class InputSystem extends WorldSystem {
    public static allInputs: Input[] = [];
    private static _currentModifierState: ModifierState;

    // Inputs global to all of synthesis like camera controls
    public static get globalInputs(): Input[] {
        return this.allInputs.filter(input => input.isGlobal);
    }

    // Robot specific controls like driving
    public static get robotInputs(): Input[] {
        return this.allInputs.filter(input => !input.isGlobal);
    }

    // A list of keys currently being pressed
    private static _keysPressed: { [key: string]: boolean } = {};

    // TODO: make private
    public static _gpIndex: number | null;
    private static _gp: Gamepad | null;


    constructor() {
        super();

        this.handleKeyDown = this.handleKeyDown.bind(this);
        document.addEventListener('keydown', this.handleKeyDown);

        this.handleKeyUp = this.handleKeyUp.bind(this);
        document.addEventListener('keyup', this.handleKeyUp);

        this.gamepadConnected = this.gamepadConnected.bind(this);
        window.addEventListener('gamepadconnected', this.gamepadConnected);

        this.gamepadDisconnected = this.gamepadDisconnected.bind(this);
        window.addEventListener('gamepaddisconnected', this.gamepadDisconnected);
        
        // TODO: Load saved inputs from mira (robot specific) & global inputs

        for (const key in defaultInputs) {
            if (Object.prototype.hasOwnProperty.call(defaultInputs, key)) {
              InputSystem.allInputs[key] = defaultInputs[key];
            }
        }
    }

    public Update(_: number): void {InputSystem
        InputSystem._currentModifierState = { ctrl: InputSystem.isKeyPressed("ControlLeft") || InputSystem.isKeyPressed("ControlRight"), alt: InputSystem.isKeyPressed("AltLeft") || InputSystem.isKeyPressed("AltRight"), shift: InputSystem.isKeyPressed("ShiftLeft") || InputSystem.isKeyPressed("ShiftRight"), meta: InputSystem.isKeyPressed("MetaLeft") || InputSystem.isKeyPressed("MetaRight") }
        
        // Fetch current gamepad information
        if (InputSystem._gpIndex == null)
            InputSystem._gp = null;
        else InputSystem._gp = navigator.getGamepads()[InputSystem._gpIndex]
    }

    public Destroy(): void {    
        document.removeEventListener('keydown', this.handleKeyDown);
        document.removeEventListener('keyup', this.handleKeyUp);
    }   

    // Called when any key is pressed
    private handleKeyDown(event: KeyboardEvent) {
        InputSystem._keysPressed[event.code] = true;
    }

    // Called when any key is released
    private handleKeyUp(event: KeyboardEvent) {
        InputSystem._keysPressed[event.code] = false;
    }

    private gamepadConnected(event: GamepadEvent) {
        console.log(
            "Gamepad connected at index %d: %s. %d buttons, %d axes.",
            event.gamepad.index,
            event.gamepad.id,
            event.gamepad.buttons.length,
            event.gamepad.axes.length,
          );
            
        InputSystem._gpIndex = event.gamepad.index;
    }

    private gamepadDisconnected(event: GamepadEvent) {
        console.log(
            "Gamepad disconnected from index %d: %s",
            event.gamepad.index,
            event.gamepad.id,
          );

          InputSystem._gpIndex = null;
    }

    // Returns true if the given key is currently down
    private static isKeyPressed(key: string): boolean {
        return !!InputSystem._keysPressed[key];
    }

    // If an input exists, return true if it is pressed
    public static getInput(inputName: string) : number {
        // Checks if there is an input assigned to this action
        if (this.allInputs.some(input => input.inputName == inputName)) {

            // TODO: assumes all inputs are button inputs
            let targetInput = this.allInputs.find(input => input.inputName == inputName);

            // Button input
            if (targetInput instanceof ButtonInput) {
                // Check if the input is a gamepad button
                if (targetInput.keyCode.startsWith("Gamepad")) {
                    return this.isGamepadButtonPressed(parseInt(targetInput.keyCode.substring(8))) ? 1 : 0;
                }

                // Check for input modifiers
                if (!this.compareModifiers(InputSystem._currentModifierState, targetInput.modifiers)) 
                    return 0;

                return this.isKeyPressed(targetInput.keyCode) ? 1 : 0;
            }

            // Joystick Axis
            if (targetInput instanceof GamepadAxisInput) {
                return (targetInput).getValue();
            }

        }

        // If the input does not exist, returns false
        return 0;
    }

    // Combines two inputs into a positive/negative axis
    public static getButtonAxis(positive: string, negative: string) {
        return (this.getInput(positive)) - (this.getInput(negative));
    }

    // Returns true if two modifier states are identical
    private static compareModifiers(state1: ModifierState, state2: ModifierState) : boolean {
        return state1.alt == state2.alt && state1.ctrl == state2.ctrl && state1.meta == state2.meta && state1.shift == state2.shift;
    }

    public static getGamepadAxis(axisNumber: number): number {
        if (InputSystem._gp == null)
            return 0;

        const value = InputSystem._gp.axes[axisNumber];

        // Return value with a deadband
        return Math.abs(value) < 0.15 ? 0 : value;
    }

    public static isGamepadButtonPressed(buttonNumber: number): boolean {
        if (InputSystem._gpIndex == null)
            return false;

        return navigator.getGamepads()[InputSystem._gpIndex]!.buttons[buttonNumber].pressed;
    }
}

export default InputSystem;
export {Input};
export {ButtonInput};
export {ButtonAxisInput};
export {GamepadAxisInput};