import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import { TouchControlsAxes } from "@/ui/components/TouchControls"
import type { InputScheme, ModifierState } from "./InputTypes"
import AxisInput from "./inputs/AxisInput"
import ButtonInput from "./inputs/ButtonInput"

type InputSupplier = () => InputScheme
/** The purpose of this class is to store any defaults related to the input system. */
class DefaultInputs {
    static ernie: InputSupplier = () => {
        const negativeModifierKeys: ModifierState = {
            ctrl: false,
            alt: false,
            shift: true,
            meta: false,
        }
        return {
            schemeName: "Ernie",
            descriptiveName: "WASD",
            customized: false,
            usesGamepad: false,
            usesTouchControls: false,
            supportedDrivetrains: [DriveType.ARCADE],
            inputs: [
                AxisInput.onKeyboard("arcadeDrive", "KeyW", "KeyS"),
                AxisInput.onKeyboard("arcadeTurn", "KeyD", "KeyA"),

                ButtonInput.onKeyboard("intake", "KeyE"),
                ButtonInput.onKeyboard("eject", "KeyQ"),
                ButtonInput.onKeyboard("unstick", "Space"),

                AxisInput.onKeyboardSingleKey("joint 1", "Digit1", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Digit2", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Digit3", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "Digit4", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "Digit5", negativeModifierKeys),
            ],
        }
    }

    static bert: InputSupplier = () => {
        const negativeModifierKeys: ModifierState = {
            ctrl: false,
            alt: false,
            shift: true,
            meta: false,
        }
        return {
            schemeName: "Bert",
            descriptiveName: "WSIK",
            customized: false,
            usesGamepad: false,
            usesTouchControls: false,
            supportedDrivetrains: [DriveType.TANK],
            inputs: [
                AxisInput.onKeyboard("tankLeft", "KeyW", "KeyS"),
                AxisInput.onKeyboard("tankRight", "KeyI", "KeyK"),

                ButtonInput.onKeyboard("intake", "KeyE"),
                ButtonInput.onKeyboard("eject", "KeyQ"),
                ButtonInput.onKeyboard("unstick", "Space"),

                AxisInput.onKeyboardSingleKey("joint 1", "Digit1", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Digit2", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Digit3", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "Digit4", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "Digit5", negativeModifierKeys),
            ],
        }
    }

    public static luna: InputSupplier = () => {
        const negativeModifierKeys: ModifierState = {
            ctrl: true,
            alt: false,
            shift: false,
            meta: false,
        }
        return {
            schemeName: "Luna",
            descriptiveName: "Arrow Keys",
            customized: false,
            usesGamepad: false,
            usesTouchControls: false,
            supportedDrivetrains: [DriveType.ARCADE],
            inputs: [
                AxisInput.onKeyboard("arcadeDrive", "ArrowUp", "ArrowDown"),
                AxisInput.onKeyboard("arcadeTurn", "ArrowRight", "ArrowLeft"),

                ButtonInput.onKeyboard("intake", "Semicolon"),
                ButtonInput.onKeyboard("eject", "KeyL"),
                ButtonInput.onKeyboard("unstick", "KeyK"),

                AxisInput.onKeyboardSingleKey("joint 1", "Quote", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Period", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Comma", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "KeyM", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "KeyN", negativeModifierKeys),
            ],
        }
    }

    public static jax: InputSupplier = () => {
        return {
            schemeName: "Jax",
            descriptiveName: "Full Controller",
            customized: false,
            usesGamepad: true,
            supportedDrivetrains: [DriveType.ARCADE, DriveType.TANK],
            usesTouchControls: false,
            inputs: [
                AxisInput.onGamepadJoystick("arcadeDrive", 1, true),
                AxisInput.onGamepadJoystick("arcadeTurn", 2, false),
                AxisInput.onGamepadJoystick("tankLeft", 1, true),
                AxisInput.onGamepadJoystick("tankRight", 3, true),

                ButtonInput.onGamepad("intake", 4),
                ButtonInput.onGamepad("eject", 5),
                ButtonInput.onGamepad("unstick", 6),

                AxisInput.onGamepadButtons("joint 1", 3, 0),
                AxisInput.onGamepadButtons("joint 2", 1, 2),
                AxisInput.onGamepadButtons("joint 3", 15, 14),
                AxisInput.onGamepadButtons("joint 4", 12, 13),
            ],
        }
    }

    /** We like this guy */
    public static hunter: InputSupplier = () => {
        return {
            schemeName: "Hunter",
            descriptiveName: "Left Stick",
            customized: false,
            usesGamepad: true,
            usesTouchControls: false,
            supportedDrivetrains: [DriveType.ARCADE],
            inputs: [
                AxisInput.onGamepadJoystick("arcadeDrive", 1, true),
                AxisInput.onGamepadJoystick("arcadeTurn", 0, false),

                ButtonInput.onGamepad("intake", 4),
                ButtonInput.onGamepad("eject", 5),
                ButtonInput.onGamepad("unstick", 6),

                AxisInput.onGamepadButtons("joint 1", 12, 13),
                AxisInput.onGamepadButtons("joint 2", 15, 14),
            ],
        }
    }

    public static carmela: InputSupplier = () => {
        return {
            schemeName: "Carmela",
            descriptiveName: "Right Stick",
            customized: false,
            usesGamepad: true,
            supportedDrivetrains: [DriveType.ARCADE],
            usesTouchControls: false,
            inputs: [
                AxisInput.onGamepadJoystick("arcadeDrive", 3, true),
                AxisInput.onGamepadJoystick("arcadeTurn", 2, false),

                ButtonInput.onGamepad("intake", 4),
                ButtonInput.onGamepad("eject", 5),
                ButtonInput.onGamepad("unstick", 6),

                AxisInput.onGamepadButtons("joint 1", 3, 0),
                AxisInput.onGamepadButtons("joint 2", 1, 2),
            ],
        }
    }

    public static brandon: InputSupplier = () => {
        return {
            schemeName: "Brandon",
            descriptiveName: "Touch Controls",
            customized: false,
            usesGamepad: false,
            usesTouchControls: true,
            supportedDrivetrains: [DriveType.ARCADE],
            inputs: [
                AxisInput.onTouchControl("arcadeDrive", TouchControlsAxes.LEFT_Y),
                AxisInput.onTouchControl("arcadeTurn", TouchControlsAxes.RIGHT_X),
            ],
        }
    }
    public static julian: InputSupplier = () => {
        return {
            schemeName: "Julian",
            descriptiveName: "Touch Controls",
            customized: false,
            usesGamepad: false,
            usesTouchControls: true,
            supportedDrivetrains: [DriveType.TANK],
            inputs: [
                AxisInput.onTouchControl("tankLeft", TouchControlsAxes.LEFT_Y),
                AxisInput.onTouchControl("tankRight", TouchControlsAxes.RIGHT_Y),
            ],
        }
    }

    /** @returns {InputScheme[]} New copies of the default input schemes without reference to any others. */
    public static get defaultInputCopies(): InputScheme[] {
        return [
            DefaultInputs.ernie(),
            DefaultInputs.bert(),
            DefaultInputs.luna(),
            DefaultInputs.jax(),
            DefaultInputs.hunter(),
            DefaultInputs.carmela(),
            DefaultInputs.brandon(),
            DefaultInputs.julian(),
        ]
    }

    /** @returns {InputScheme} A new blank input scheme with no control bound. */
    public static newBlankScheme(drivetype: DriveType): InputScheme {
        let driveInputs: AxisInput[]
        switch (drivetype) {
            case DriveType.ARCADE:
                driveInputs = [AxisInput.unbound("arcadeDrive"), AxisInput.unbound("arcadeTurn")]
                break
            case DriveType.TANK:
                driveInputs = [AxisInput.unbound("tankLeft"), AxisInput.unbound("tankRight")]
                break
        }
        return {
            schemeName: "",
            descriptiveName: "",
            customized: true,
            usesGamepad: false,
            usesTouchControls: false,
            supportedDrivetrains: [drivetype],
            inputs: [
                ...driveInputs,

                ButtonInput.unbound("intake"),
                ButtonInput.unbound("eject"),
                ButtonInput.unbound("unstick"),
            ],
        }
    }

    public static readonly NAMES: string[] = [
        "Kennedy",
        "Duke",
        "Bria",
        "Creed",
        "Angie",
        "Moises",
        "Hattie",
        "Quinton",
        "Luisa",
        "Ocean",
        "Marlowe",
        "Jimmy",
        "Brielle",
        "Forest",
        "Katherine",
        "Cade",
        "Kori",
        "Myles",
        "Valeria",
        "Braylon",
        "Gracelyn",
        "Killian",
        "Holland",
        "Jake",
        "Jovie",
        "William",
        "Makenzie",
        "Eden",
        "Mabel",
        "Ian",
        "Leilany",
        "Jayson",
        "Kylie",
        "Cal",
        "Juliet",
        "Emory",
        "Eden",
        "Nathanael",
        "Eloise",
        "Darian",
        "Shelby",
        "Neil",
        "Scarlett",
        "Ace",
        "Florence",
        "Alessandro",
        "Sariyah",
        "Joey",
        "Aubrie",
        "Edward",
        "Octavia",
        "Bode",
        "Aaliyah",
        "Francis",
        "Camilla",
        "Wilson",
        "Elaina",
        "Kayson",
        "Kara",
        "Rey",
        "Madison",
        "Emir",
        "Alaya",
        "Finley",
        "Jayleen",
        "Joseph",
        "Arianna",
        "Samson",
        "Ezra",
        "Amias",
        "Ellen",
        "Zion",
        "Harley",
        "Abraham",
        "Elaine",
        "Conner",
        "Jolene",
        "Kylan",
        "Aislinn",
        "Omar",
        "Skyla",
        "Shepard",
        "Jaylin",
        "Osiris",
        "Lilyana",
        "Noe",
        "Crystal",
        "Jeffrey",
        "Emily",
        "Rayan",
        "Elise",
        "Forrest",
        "Aarya",
        "Beckett",
        "Jacqueline",
        "Kyle",
        "Kailey",
        "Hank",
        "Alanna",
        "Marco",
        "Drake",
    ]
}

export default DefaultInputs
