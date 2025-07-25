import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import { TouchControlsAxes } from "@/ui/components/TouchControls"
import { InputScheme } from "./InputSchemeManager"
import { AxisInput, ButtonInput, ModifierState } from "./InputSystem"

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
            supportedDrivetrains: [DriveType.ARCADE, DriveType.SWERVE],
            inputs: [
                AxisInput.onKeyboard("arcadeDrive", "KeyW", "KeyS"),
                AxisInput.onKeyboard("arcadeTurn", "KeyD", "KeyA"),
                AxisInput.onKeyboard("swerveForward", "KeyW", "KeyS"),
                AxisInput.onKeyboard("swerveStrafe", "KeyA", "KeyD"),
                AxisInput.onKeyboard("swerveYaw", "ArrowRight", "ArrowLeft"),
                ButtonInput.onKeyboard("swerveResetFieldForward", "KeyR"),

                ButtonInput.onKeyboard("intake", "KeyE"),
                ButtonInput.onKeyboard("eject", "KeyQ"),

                AxisInput.onKeyboardSingleKey("joint 1", "Digit1", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Digit2", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Digit3", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "Digit4", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "Digit5", negativeModifierKeys),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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

                AxisInput.onKeyboardSingleKey("joint 1", "Digit1", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Digit2", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Digit3", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "Digit4", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "Digit5", negativeModifierKeys),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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

                AxisInput.onKeyboardSingleKey("joint 1", "Slash", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 2", "Period", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 3", "Comma", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 4", "KeyM", negativeModifierKeys),
                AxisInput.onKeyboardSingleKey("joint 5", "KeyN", negativeModifierKeys),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
            ],
        }
    }

    public static jax: InputSupplier = () => {
        return {
            schemeName: "Jax",
            descriptiveName: "Full Controller",
            customized: false,
            usesGamepad: true,
            supportedDrivetrains: [DriveType.ARCADE, DriveType.SWERVE, DriveType.TANK],
            usesTouchControls: false,
            inputs: [
                AxisInput.onGamepadJoystick("arcadeDrive", 1, true),
                AxisInput.onGamepadJoystick("arcadeTurn", 2, false),
                AxisInput.onGamepadJoystick("swerveForward", 1, true), // TODO: test inversion with actual controller
                AxisInput.onGamepadJoystick("swerveStrafe", 0, false),
                AxisInput.onGamepadJoystick("swerveYaw", 2, false),
                AxisInput.onGamepadJoystick("tankLeft", 1, true),
                AxisInput.onGamepadJoystick("tankRight", 3, true),

                ButtonInput.onGamepad("intake", 4),
                ButtonInput.onGamepad("eject", 5),

                AxisInput.onGamepadButtons("joint 1", 3, 0),
                AxisInput.onGamepadButtons("joint 2", 1, 2),
                AxisInput.onGamepadButtons("joint 3", 15, 14),
                AxisInput.onGamepadButtons("joint 4", 12, 13),
                AxisInput.unbound("joint 5"),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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

                AxisInput.onGamepadButtons("joint 1", 12, 13),
                AxisInput.onGamepadButtons("joint 2", 15, 14),
                AxisInput.unbound("joint 3"),
                AxisInput.unbound("joint 4"),
                AxisInput.unbound("joint 5"),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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

                AxisInput.onGamepadButtons("joint 1", 3, 0),
                AxisInput.onGamepadButtons("joint 2", 1, 2),
                AxisInput.unbound("joint 3"),
                AxisInput.unbound("joint 4"),
                AxisInput.unbound("joint 5"),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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
            supportedDrivetrains: [DriveType.ARCADE, DriveType.TANK, DriveType.SWERVE],
            inputs: [
                AxisInput.onTouchControl("arcadeDrive", TouchControlsAxes.LEFT_Y),
                AxisInput.onTouchControl("arcadeTurn", TouchControlsAxes.RIGHT_X),
                AxisInput.onTouchControl("tankLeft", TouchControlsAxes.LEFT_Y),
                AxisInput.onTouchControl("tankRight", TouchControlsAxes.RIGHT_Y),
                AxisInput.onTouchControl("swerveStrafe", TouchControlsAxes.LEFT_X),
                AxisInput.onTouchControl("swerveForward", TouchControlsAxes.LEFT_Y),
                AxisInput.onTouchControl("swerveYaw", TouchControlsAxes.RIGHT_Y),
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
            case DriveType.SWERVE:
                driveInputs = [
                    AxisInput.unbound("swerveStrafe"),
                    AxisInput.unbound("swerveForward"),
                    AxisInput.unbound("swerveYaw"),
                ]
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

                AxisInput.unbound("joint 1"),
                AxisInput.unbound("joint 2"),
                AxisInput.unbound("joint 3"),
                AxisInput.unbound("joint 4"),
                AxisInput.unbound("joint 5"),
                AxisInput.unbound("joint 6"),
                AxisInput.unbound("joint 7"),
                AxisInput.unbound("joint 8"),
                AxisInput.unbound("joint 9"),
                AxisInput.unbound("joint 10"),
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
    ]
}

export default DefaultInputs
