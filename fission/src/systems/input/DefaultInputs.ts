import { AxisInput, Input, EmptyModifierState } from "./InputSystem"

export type InputScheme = {
    schemeName: string,
    usesGamepad: boolean,
    inputs: Input[]
}

class DefaultInputs {
    private static wasd: InputScheme = {
        schemeName: "WASD",
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "KeyW", "KeyS"),
            new AxisInput("arcadeTurn", "KeyD", "KeyA"),

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
        schemeName: "Arrow Keys",
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "ArrowUp", "ArrowDown"),
            new AxisInput("arcadeTurn", "ArrowRight", "ArrowLeft"),

            new AxisInput("joint 1", "Slash", "Slash", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: true, alt: false, shift: false, meta: false }),
            new AxisInput("joint 2", "Period", "Period", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: true, alt: false, shift: false, meta: false }),
            new AxisInput("joint 3", "Comma", "Comma", -1, false, false, -1, -1, false,
                EmptyModifierState, { ctrl: true, alt: false, shift: false, meta: false }),
            new AxisInput("joint 4", "KeyM", "KeyM", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: true, alt: false, shift: false, meta: false }),
            new AxisInput("joint 5", "KeyN", "true", -1, false, false, -1, -1, false, 
                EmptyModifierState, { ctrl: false, alt: false, shift: false, meta: false })
        ]
    };

    private static fullController: InputScheme = {
        schemeName: "Full Controller",
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 1, 2),
            new AxisInput("joint 3", "", "", -1, false, true, 4, 5),
            new AxisInput("joint 3", "", "", -1, false, true, 15, 14),
            new AxisInput("joint 3", "", "", -1, false, true, 12, 13)
        ]
    }

    private static leftStick: InputScheme = {
        schemeName: "Left Stick",
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 0, false),
            
            new AxisInput("joint 2", "", "", -1, false, true, 15, 14),
            new AxisInput("joint 1", "", "", -1, false, true, 12, 13)
        ]
    }

    private static rightStick: InputScheme = {
        schemeName: "Right Stick",
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 3, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 1, 2)
        ]
    }

    public static ALL_INPUT_SCHEMES: InputScheme[] = [this.wasd, this.arrowKeys, this.fullController, this.leftStick, this.rightStick];
}

export default DefaultInputs;