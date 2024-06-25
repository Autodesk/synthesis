import { AxisInput, Input, EmptyModifierState } from "./InputSystem"

export type InputScheme = {
    usesGamepad: boolean,
    inputs: Input[]
}

class DefaultInputs {
    private static wasd: InputScheme = {
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "KeyW", "KeyS", -1, true),
            new AxisInput("arcadeTurn", "KeyD", "KeyA", -1, false),

            new AxisInput("joint 1", "Digit1", "Digit1", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }),
            new AxisInput("joint 2", "Digit2", "Digit2", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }),
            new AxisInput("joint 3", "Digit3", "Digit3", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }),
            new AxisInput("joint 4", "Digit4", "Digit4", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }),
            new AxisInput("joint 5", "Digit5", "Digit5", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: true, meta: false })
        ]
    };

    private static arrowKeys: InputScheme = {
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "ArrowUp", "ArrowDown", 1, true),
            new AxisInput("arcadeTurn", "ArrowRight", "ArrowLeft", 0, false),

            new AxisInput("joint 1", "Slash", "Slash", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: true, shift: false, meta: false }),
            new AxisInput("joint 2", "Period", "Period", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: true, shift: false, meta: false }),
            new AxisInput("joint 3", "Comma", "Comma", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: true, shift: false, meta: false }),
            new AxisInput("joint 4", "KeyM", "KeyM", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: true, shift: false, meta: false }),
            new AxisInput("joint 5", "KeyN", "KeyN", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: true, shift: false, meta: false })
        ]
    };

    private static leftStick: InputScheme = {
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 0, false),

            new AxisInput("joint 1", "", "", -1, false, true, 12, 13),
            new AxisInput("joint 2", "", "", -1, false, true, 15, 14)
        ]
    }

    private static rightStick: InputScheme = {
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 3, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 2, 1)
        ]
    }

    private static fullController: InputScheme = {
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 2, 1),
            new AxisInput("joint 3", "", "", -1, false, true, 4, 5),
            new AxisInput("joint 3", "", "", -1, false, true, 12, 13),
            new AxisInput("joint 3", "", "", -1, false, true, 15, 14)
        ]
    }

    public static ALL_INPUT_SCHEMES: InputScheme[] = [this.wasd, this.arrowKeys, this.fullController, this.leftStick, this.rightStick];
}

export default DefaultInputs;