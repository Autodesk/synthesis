import WorldSystem from "../WorldSystem";

declare global {
    type ModifierState = {
        alt?: boolean
        ctrl?: boolean
        shift?: boolean
        meta?: boolean
    }

    type Input = {
        name: string
        keybind: string
        isGlobal: boolean
        modifiers?: ModifierState
    }
}

// When a robot is loaded, default inputs replace any unassigned inputs
const defaultInputs: { [key: string]: Input } = {
    "intake": { name: "intake", keybind: "e", isGlobal: true },
    "shootGamepiece": { name: "shootGamepiece", keybind: "q", isGlobal: true },
    "enableGodMode": { name: "enableGodMode", keybind: "g", isGlobal: true },

    "arcadeForward": { name: "arcadeForward", keybind: "w", isGlobal: false },
    "arcadeBackward": { name: "arcadeBackward", keybind: "s", isGlobal: false },
    "arcadeLeft": { name: "arcadeLeft", keybind: "a", isGlobal: false },
    "arcadeRight": { name: "arcadeRight", keybind: "d", isGlobal: false },
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
    private static keysPressed: { [key: string]: boolean } = {};

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

    public Update(_: number): void { }

    public Destroy(): void {    
        document.removeEventListener('keydown', this.handleKeyDown);
        document.removeEventListener('keyup', this.handleKeyUp);
    }   

    // #endregion
    // #region Input Events

    handleKeyDown(event: KeyboardEvent) {
        InputSystem.keysPressed[event.key] = true;
    }

    handleKeyUp(event: KeyboardEvent) {
        InputSystem.keysPressed[event.key] = false;
    }

    // #endregion
    // #region Get Inputs

    private static isKeyPressed(key: string): boolean {
        return !!InputSystem.keysPressed[key];
    }

    public static getInput(inputName: string) : boolean {
        // Checks if there is a global control for this action
        if (inputName in this.allInputs) {
            // TODO: support for control modifiers
            return this.isKeyPressed(this.allInputs[inputName].keybind);
        }

        // If the input does not exist, returns false
        return false;
    }

    public static getAxis(positive: string, negative: string) {
        return (this.getInput(positive) ? 1 : 0) - (this.getInput(negative) ? 1 : 0);
    }

    // #endregion
}

export default InputSystem;