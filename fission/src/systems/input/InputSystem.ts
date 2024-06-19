import WorldSystem from "../WorldSystem";

declare global {
    type ModifierState = {
        alt: boolean
        ctrl: boolean
        shift: boolean
        meta: boolean
    }

    type Input = {
        name: string
        keybind: string
        isGlobal: boolean
        modifiers: ModifierState
    }
}

export const emptyModifierState: ModifierState = { ctrl: false, alt: false, shift: false, meta: false };

// When a robot is loaded, default inputs replace any unassigned inputs
const defaultInputs: { [key: string]: Input } = {
    "intake": { name: "intake", keybind: "e", isGlobal: true, modifiers: emptyModifierState },
    "shootGamepiece": { name: "shootGamepiece", keybind: "q", isGlobal: true, modifiers: emptyModifierState },
    "enableGodMode": { name: "enableGodMode", keybind: "g", isGlobal: true, modifiers: emptyModifierState },

    "arcadeForward": { name: "arcadeForward", keybind: "w", isGlobal: false, modifiers: emptyModifierState },
    "arcadeBackward": { name: "arcadeBackward", keybind: "s", isGlobal: false, modifiers: emptyModifierState },
    "arcadeLeft": { name: "arcadeLeft", keybind: "a", isGlobal: false, modifiers: emptyModifierState },
    "arcadeRight": { name: "arcadeRight", keybind: "d", isGlobal: false, modifiers: emptyModifierState },
}

class InputSystem extends WorldSystem {
    public static allInputs: { [key: string]: Input } = { }
    private static _currentModifierState: ModifierState;

    // Inputs global to all of synthesis like camera controls
    public static get globalInputs(): { [key: string]: Input } {
        return Object.fromEntries(
            Object.entries(InputSystem.allInputs)
                .filter(([_, input]) => input.isGlobal));
    }

    // Robot specific controls like driving
    public static get robotInputs(): { [key: string]: Input } {
        return Object.fromEntries(
            Object.entries(InputSystem.allInputs)
                .filter(([_, input]) => !input.isGlobal));
    }

    // A list of keys currently being pressed
    private static _keysPressed: { [key: string]: boolean } = {};

    constructor() {
        super();

        this.HandleKeyDown = this.HandleKeyDown.bind(this);
        document.addEventListener('keydown', this.HandleKeyDown);

        this.HandleKeyUp = this.HandleKeyUp.bind(this);
        document.addEventListener('keyup', this.HandleKeyUp);
        
        // TODO: Load saved inputs from mira (robot specific) & global inputs

        for (const key in defaultInputs) {
            if (Object.prototype.hasOwnProperty.call(defaultInputs, key)) {
              InputSystem.allInputs[key] = defaultInputs[key];
            }
        }
    }

    public Update(_: number): void {InputSystem
        InputSystem._currentModifierState = { ctrl: InputSystem.isKeyPressed("Control"), alt: InputSystem.isKeyPressed("Alt"), shift: InputSystem.isKeyPressed("Shift"), meta: InputSystem.isKeyPressed("Meta") }
    }

    public Destroy(): void {    
        document.removeEventListener('keydown', this.HandleKeyDown);
        document.removeEventListener('keyup', this.HandleKeyUp);
    }   

    // Called when any key is pressed
    private HandleKeyDown(event: KeyboardEvent) {
        InputSystem._keysPressed[event.key] = true;
    }

    // Called when any key is released
    private HandleKeyUp(event: KeyboardEvent) {
        InputSystem._keysPressed[event.key] = false;
    }

    // Returns true if the given key is currently down
    private static isKeyPressed(key: string): boolean {
        return !!InputSystem._keysPressed[key];
    }

    // If an input exists, return true if it is pressed
    public static getInput(inputName: string) : boolean {
        // Checks if there is an input assigned to this action
        if (inputName in this.allInputs) {
            const targetInput = this.allInputs[inputName];

            // Check for input modifiers
            if (!this.CompareModifiers(InputSystem._currentModifierState, targetInput.modifiers)) 
                return false;

            return this.isKeyPressed(targetInput.keybind);
        }

        // If the input does not exist, returns false
        return false;
    }

    // Combines two inputs into a positive/negative axis
    public static GetAxis(positive: string, negative: string) {
        return (this.getInput(positive) ? 1 : 0) - (this.getInput(negative) ? 1 : 0);
    }

    // Converts camelCase to Title Case for the inputs modal
    public static ToTitleCase(camelCase: string) : string {
        const result = camelCase.replace(/([A-Z])/g, " $1");
        const finalResult = result.charAt(0).toUpperCase() + result.slice(1);
        return finalResult;
    }

    // Returns true if two modifier states are identical
    private static CompareModifiers(state1: ModifierState, state2: ModifierState) : boolean {
        return state1.alt == state2.alt && state1.ctrl == state2.ctrl && state1.meta == state2.meta && state1.shift == state2.shift;
    }
}

export default InputSystem;