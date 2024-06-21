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

    constructor(inputName: string, isGlobal: boolean) {
        this.inputName = inputName;
        this.isGlobal = isGlobal;
    }    

    abstract getValue(): number;
}

// A single button
class ButtonInput extends Input {    
    public keyCode: string;
    public keyModifiers: ModifierState;

    public gamepadButton: number;

    public constructor(inputName: string, keyCode: string, gamepadButton: number, isGlobal?: boolean, keyModifiers?: ModifierState) {
        super(inputName, isGlobal ?? false);
        this.keyCode = keyCode;
        this.keyModifiers = keyModifiers ?? emptyModifierState;
        this.gamepadButton = gamepadButton;
    }    

    // Returns 1 if pressed and 0 if not pressed
    getValue(): number {
        // Gamepad button input
        if (InputSystem.useGamepad) {
            return InputSystem.isGamepadButtonPressed(this.gamepadButton) ? 1 : 0;
        }
        
        // Keyboard button input
        return InputSystem.isKeyPressed(this.keyCode, this.keyModifiers) ? 1 : 0;
    }
}

// An axis between two buttons (-1 to 1)
class AxisInput extends Input {    
    public posKeyCode: string;
    public posKeyModifiers: ModifierState;
    public negKeyCode: string;
    public negKeyModifiers: ModifierState;

    public gamepadAxisNumber: number;
    public joystickInverted: boolean;

    public constructor(inputName: string, posKeyCode: string, negKeyCode: string, gamepadAxisNumber: number, joystickInverted?: boolean, isGlobal?: boolean, posKeyModifiers?: ModifierState, negKeyModifiers?: ModifierState) {
        super(inputName, isGlobal ?? false);
        
        this.posKeyCode = posKeyCode;
        this.posKeyModifiers = posKeyModifiers ?? emptyModifierState;
        this.negKeyCode = negKeyCode;
        this.negKeyModifiers = negKeyModifiers ?? emptyModifierState;

        this.gamepadAxisNumber = gamepadAxisNumber;
        this.joystickInverted = joystickInverted ?? false;
    }    

    // Returns 1 if positive pressed, -1 if negative pressed, or 0 if none or both are pressed
    getValue(): number {
        // Gamepad axis input
        if (InputSystem.useGamepad) {
            return InputSystem.getGamepadAxis(this.gamepadAxisNumber) * (this.joystickInverted ? -1 : 1);
        }

        // Button axis input
        return (InputSystem.isKeyPressed(this.posKeyCode, this.posKeyModifiers) ? 1 : 0) - (InputSystem.isKeyPressed(this.negKeyCode, this.negKeyModifiers) ? 1 : 0);
    }
}

export const emptyModifierState: ModifierState = { ctrl: false, alt: false, shift: false, meta: false };

// When a robot is loaded, default inputs replace any unassigned inputs
const defaultInputs: Input[] = [
    new ButtonInput("intakeGamepiece", "KeyE", 0, true),
    new ButtonInput("shootGamepiece", "KeyQ", 2, true),
    new ButtonInput("enableGodMode", "KeyG", 3, true),

    new AxisInput("arcadeDrive", "KeyW", "KeyS", 1, true),
    new AxisInput("arcadeTurn", "KeyD", "KeyA", 0, false)
]

class InputSystem extends WorldSystem {
    public static allInputs: Input[] = [];
    public static useGamepad: boolean = false;

    public static currentModifierState: ModifierState;

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
        InputSystem.currentModifierState = { ctrl: InputSystem.isKeyPressed("ControlLeft") || InputSystem.isKeyPressed("ControlRight"), alt: InputSystem.isKeyPressed("AltLeft") || InputSystem.isKeyPressed("AltRight"), shift: InputSystem.isKeyPressed("ShiftLeft") || InputSystem.isKeyPressed("ShiftRight"), meta: InputSystem.isKeyPressed("MetaLeft") || InputSystem.isKeyPressed("MetaRight") }
        
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
    public static isKeyPressed(key: string, modifiers?: ModifierState): boolean {
        if (modifiers != null && !InputSystem.compareModifiers(InputSystem.currentModifierState, modifiers)) 
            return false;

        return !!InputSystem._keysPressed[key];
    }

    // If an input exists, return true if it is pressed
    public static getInput(inputName: string) : number {
        // Looks for an input assigned to this action
        const targetInput = this.allInputs.find(input => input.inputName == inputName);

        if (targetInput == null)
            return 0;

        return targetInput.getValue();
    }

    // Combines two inputs into a positive/negative axis
    public static getButtonAxis(positive: string, negative: string) {
        return (this.getInput(positive)) - (this.getInput(negative));
    }

    // Returns true if two modifier states are identical
    public static compareModifiers(state1: ModifierState, state2: ModifierState) : boolean {
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

        const gp = navigator.getGamepads()[InputSystem._gpIndex]!;

        if (buttonNumber < 0 || buttonNumber >= gp.buttons.length)
            return false;

        return gp.buttons[buttonNumber].pressed;
    }
}

export default InputSystem;
export {Input};
export {ButtonInput};
export {AxisInput};
