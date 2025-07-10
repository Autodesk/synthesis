import { InputScheme } from "./InputSchemeManager"
import { AxisInput, ButtonInput, EmptyModifierState } from "./InputSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"

type InputSupplier = () => InputScheme
/** The purpose of this class is to store any defaults related to the input system. */
class DefaultInputs {
    static ernie: InputSupplier = () => {
        return {
            schemeName: "Ernie",
            descriptiveName: "WASD",
            customized: false,
            usesGamepad: false,
            driveType: DriveType.ARCADE,
            inputs: [
                new AxisInput("arcadeDrive", "KeyW", "KeyS"),
                new AxisInput("arcadeTurn", "KeyD", "KeyA"),

                new ButtonInput("intake", "KeyE"),
                new ButtonInput("eject", "KeyQ"),

                new AxisInput("joint 1", "Digit1", "Digit1", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 2", "Digit2", "Digit2", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 3", "Digit3", "Digit3", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 4", "Digit4", "Digit4", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 5", "Digit5", "Digit5", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    static ernietank: InputSupplier = () => {
        return {
            schemeName: "Bert",
            descriptiveName: "WSIK",
            customized: false,
            usesGamepad: false,
            driveType: DriveType.TANK,
            inputs: [
                new AxisInput("tankLeft", "KeyW", "KeyS"),
                new AxisInput("tankRight", "KeyI", "KeyK"),

                new ButtonInput("intake", "KeyE"),
                new ButtonInput("eject", "KeyQ"),

                new AxisInput("joint 1", "Digit1", "Digit1", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 2", "Digit2", "Digit2", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 3", "Digit3", "Digit3", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 4", "Digit4", "Digit4", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 5", "Digit5", "Digit5", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: false,
                    alt: false,
                    shift: true,
                    meta: false,
                }),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    public static luna: InputSupplier = () => {
        return {
            schemeName: "Luna",
            descriptiveName: "Arrow Keys",
            customized: false,
            usesGamepad: false,
            driveType: DriveType.ARCADE,
            inputs: [
                new AxisInput("arcadeDrive", "ArrowUp", "ArrowDown"),
                new AxisInput("arcadeTurn", "ArrowRight", "ArrowLeft"),

                new ButtonInput("intake", "Semicolon"),
                new ButtonInput("eject", "KeyL"),

                new AxisInput("joint 1", "Slash", "Slash", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: true,
                    alt: false,
                    shift: false,
                    meta: false,
                }),
                new AxisInput("joint 2", "Period", "Period", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: true,
                    alt: false,
                    shift: false,
                    meta: false,
                }),
                new AxisInput("joint 3", "Comma", "Comma", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: true,
                    alt: false,
                    shift: false,
                    meta: false,
                }),
                new AxisInput("joint 4", "KeyM", "KeyM", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: true,
                    alt: false,
                    shift: false,
                    meta: false,
                }),
                new AxisInput("joint 5", "KeyN", "KeyN", -1, false, false, -1, -1, EmptyModifierState, {
                    ctrl: true,
                    alt: false,
                    shift: false,
                    meta: false,
                }),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    public static jax: InputSupplier = () => {
        return {
            schemeName: "Jax",
            descriptiveName: "Full Controller",
            customized: false,
            usesGamepad: true,
            driveType: DriveType.ARCADE,
            inputs: [
                new AxisInput("arcadeDrive", "", "", 1, true),
                new AxisInput("arcadeTurn", "", "", 2, false),

                new ButtonInput("intake", "", 4),
                new ButtonInput("eject", "", 5),

                new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
                new AxisInput("joint 2", "", "", -1, false, true, 1, 2),
                new AxisInput("joint 3", "", "", -1, false, true, 15, 14),
                new AxisInput("joint 4", "", "", -1, false, true, 12, 13),
                new AxisInput("joint 5"),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
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
            driveType: DriveType.ARCADE,
            inputs: [
                new AxisInput("arcadeDrive", "", "", 1, true),
                new AxisInput("arcadeTurn", "", "", 0, false),

                new ButtonInput("intake", "", 4),
                new ButtonInput("eject", "", 5),

                new AxisInput("joint 2", "", "", -1, false, true, 15, 14),
                new AxisInput("joint 1", "", "", -1, false, true, 12, 13),
                new AxisInput("joint 3"),
                new AxisInput("joint 4"),
                new AxisInput("joint 5"),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    public static carmela: InputSupplier = () => {
        return {
            schemeName: "Carmela",
            descriptiveName: "Right Stick",
            customized: false,
            usesGamepad: true,
            driveType: DriveType.ARCADE,
            inputs: [
                new AxisInput("arcadeDrive", "", "", 3, true),
                new AxisInput("arcadeTurn", "", "", 2, false),

                new ButtonInput("intake", "", 4),
                new ButtonInput("eject", "", 5),

                new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
                new AxisInput("joint 2", "", "", -1, false, true, 1, 2),
                new AxisInput("joint 3"),
                new AxisInput("joint 4"),
                new AxisInput("joint 5"),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    /** @returns {InputScheme[]} New copies of the default input schemes without reference to any others. */
    public static get defaultInputCopies(): InputScheme[] {
        return [
            DefaultInputs.ernie(),
            DefaultInputs.ernietank(),
            DefaultInputs.luna(),
            DefaultInputs.jax(),
            DefaultInputs.hunter(),
            DefaultInputs.carmela(),
        ]
    }

    /** @returns {InputScheme} A new blank input scheme with no control bound. */
    public static newBlankScheme(drivetype: DriveType): InputScheme {
        let driveInputs: AxisInput[]
        switch (drivetype) {
            case DriveType.ARCADE:
                driveInputs = [new AxisInput("arcadeDrive"), new AxisInput("arcadeTurn")]
                break
            case DriveType.TANK:
                driveInputs = [new AxisInput("tankLeft"), new AxisInput("tankRight")]
                break
            case DriveType.SWERVE:
                driveInputs = [new AxisInput("swerveX"), new AxisInput("swerveZ"), new AxisInput("swerveYaw")]
                break
        }
        return {
            schemeName: "",
            descriptiveName: "",
            customized: true,
            usesGamepad: false,
            driveType: drivetype,
            inputs: [
                ...driveInputs,

                new ButtonInput("intake"),
                new ButtonInput("eject"),

                new AxisInput("joint 1"),
                new AxisInput("joint 2"),
                new AxisInput("joint 3"),
                new AxisInput("joint 4"),
                new AxisInput("joint 5"),
                new AxisInput("joint 6"),
                new AxisInput("joint 7"),
                new AxisInput("joint 8"),
                new AxisInput("joint 9"),
                new AxisInput("joint 10"),
            ],
        }
    }

    public static NAMES: string[] = [
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
