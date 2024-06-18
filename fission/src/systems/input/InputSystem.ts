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

// When a robot is loaded, default inputs replace any unassigned inputs
const defaultInputs: { [key: string]: Input } = {
    "intake": { name: "intake", keybind: "e", isGlobal: true, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "shootGamepiece": { name: "shootGamepiece", keybind: "q", isGlobal: true, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "enableGodMode": { name: "enableGodMode", keybind: "g", isGlobal: true, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },

    "arcadeForward": { name: "arcadeForward", keybind: "w", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "arcadeBackward": { name: "arcadeBackward", keybind: "s", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "arcadeLeft": { name: "arcadeLeft", keybind: "a", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "arcadeRight": { name: "arcadeRight", keybind: "d", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "armPositive": { name: "armPositive", keybind: "1", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "armNegative": { name: "armNegative", keybind: "2", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "elevatorNegative": { name: "elevatorNegative", keybind: "4", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
    "elevatorPositive": { name: "elevatorPositive", keybind: "3", isGlobal: false, modifiers: { ctrl: false, alt: false, shift: false, meta: false } },
}

class InputSystem extends WorldSystem {
    public static allInputs: { [key: string]: Input } = { }

    public static get globalInputs(): { [key: string]: Input } {
        return Object.fromEntries(
            Object.entries(InputSystem.allInputs)
                .filter(([_, input]) => input.isGlobal));
    }

    public static get robotInputs(): { [key: string]: Input } {
        return Object.fromEntries(
            Object.entries(InputSystem.allInputs)
                .filter(([_, input]) => !input.isGlobal));
    }

    // A list of keys currently being pressed
    private static _keysPressed: { [key: string]: boolean } = {};

    constructor() {
        super();

        this.handleKeyDown = this.handleKeyDown.bind(this);
        document.addEventListener('keydown', this.handleKeyDown);

        this.handleKeyUp = this.handleKeyUp.bind(this);
        document.addEventListener('keyup', this.handleKeyUp);

        // TODO: Load saved global controls
        
        // TODO: Load saved robot specific controls

        for (let key in defaultInputs) {
            if (defaultInputs.hasOwnProperty(key)) {
              InputSystem.allInputs[key] = defaultInputs[key];
            }
        }
    }

    // #region WorldSystem Functions
    static _currentModifierState: ModifierState;
    public Update(_: number): void {InputSystem
        InputSystem._currentModifierState = { ctrl: InputSystem.isKeyPressed("Control"), alt: InputSystem.isKeyPressed("Alt"), shift: InputSystem.isKeyPressed("Shift"), meta: InputSystem.isKeyPressed("Meta") }
     }

    public Destroy(): void {    
        document.removeEventListener('keydown', this.handleKeyDown);
        document.removeEventListener('keyup', this.handleKeyUp);
    }   

    // #endregion
    // #region Input Events

    handleKeyDown(event: KeyboardEvent) {
        InputSystem._keysPressed[event.key] = true;
    }

    handleKeyUp(event: KeyboardEvent) {
        InputSystem._keysPressed[event.key] = false;
    }

    // #endregion
    // #region Get Inputs

    private static isKeyPressed(key: string): boolean {
        return !!InputSystem._keysPressed[key];
    }

    public static getInput(inputName: string) : boolean {
        // Checks if there is a global control for this action
        if (inputName in this.allInputs) {
            let targetInput = this.allInputs[inputName];

            // Check for input modifiers

            if (!this.compareModifiers(InputSystem._currentModifierState, targetInput.modifiers)) 
                return false;

            return this.isKeyPressed(targetInput.keybind);
        }

        // If the input does not exist, returns false
        return false;
    }

    public static getAxis(positive: string, negative: string) {
        return (this.getInput(positive) ? 1 : 0) - (this.getInput(negative) ? 1 : 0);
    }

    public static toTitleCase(camelCase: string) : string {
        const result = camelCase.replace(/([A-Z])/g, " $1");
        const finalResult = result.charAt(0).toUpperCase() + result.slice(1);
        return finalResult;
    }

    // #endregion

    private static compareModifiers(state1: ModifierState, state2: ModifierState) : boolean {
        return state1.alt == state2.alt && state1.ctrl == state2.ctrl && state1.meta == state2.meta && state1.shift == state2.shift;
    }
}

export default InputSystem;