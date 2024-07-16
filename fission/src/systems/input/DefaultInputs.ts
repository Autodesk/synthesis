import { AxisInput, Input, EmptyModifierState } from "./InputSystem"

export type InputScheme = {
    schemeName: string
    descriptiveName: string
    customized: boolean
    usesGamepad: boolean
    inputs: Input[]
}

class DefaultInputs {
    private static wasd: InputScheme = {
        schemeName: "Ernie",
        descriptiveName: "WASD",
        customized: false,
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "KeyW", "KeyS"),
            new AxisInput("arcadeTurn", "KeyD", "KeyA"),

            new AxisInput("joint 1", "Digit1", "Digit1", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: true,
                meta: false,
            }),
            new AxisInput("joint 2", "Digit2", "Digit2", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: true,
                meta: false,
            }),
            new AxisInput("joint 3", "Digit3", "Digit3", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: true,
                meta: false,
            }),
            new AxisInput("joint 4", "Digit4", "Digit4", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: true,
                meta: false,
            }),
            new AxisInput("joint 5", "Digit5", "Digit5", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: true,
                meta: false,
            }),
        ],
    }

    private static arrowKeys: InputScheme = {
        schemeName: "Luna",
        descriptiveName: "Arrow Keys",
        customized: false,
        usesGamepad: false,
        inputs: [
            new AxisInput("arcadeDrive", "ArrowUp", "ArrowDown"),
            new AxisInput("arcadeTurn", "ArrowRight", "ArrowLeft"),

            new AxisInput("joint 1", "Slash", "Slash", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: true,
                alt: false,
                shift: false,
                meta: false,
            }),
            new AxisInput("joint 2", "Period", "Period", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: true,
                alt: false,
                shift: false,
                meta: false,
            }),
            new AxisInput("joint 3", "Comma", "Comma", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: true,
                alt: false,
                shift: false,
                meta: false,
            }),
            new AxisInput("joint 4", "KeyM", "KeyM", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: true,
                alt: false,
                shift: false,
                meta: false,
            }),
            new AxisInput("joint 5", "KeyN", "true", -1, false, false, -1, -1, false, EmptyModifierState, {
                ctrl: false,
                alt: false,
                shift: false,
                meta: false,
            }),
        ],
    }

    private static fullController: InputScheme = {
        schemeName: "Jax",
        descriptiveName: "Full Controller",
        customized: false,
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 1, 2),
            new AxisInput("joint 3", "", "", -1, false, true, 4, 5),
            new AxisInput("joint 3", "", "", -1, false, true, 15, 14),
            new AxisInput("joint 3", "", "", -1, false, true, 12, 13),
        ],
    }

    private static leftStick: InputScheme = {
        schemeName: "Hunter",
        descriptiveName: "Left Stick",
        customized: false,
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 1, true),
            new AxisInput("arcadeTurn", "", "", 0, false),

            new AxisInput("joint 2", "", "", -1, false, true, 15, 14),
            new AxisInput("joint 1", "", "", -1, false, true, 12, 13),
        ],
    }

    private static rightStick: InputScheme = {
        schemeName: "Carmela",
        descriptiveName: "Right Stick",
        customized: false,
        usesGamepad: true,
        inputs: [
            new AxisInput("arcadeDrive", "", "", 3, true),
            new AxisInput("arcadeTurn", "", "", 2, false),

            new AxisInput("joint 1", "", "", -1, false, true, 3, 0),
            new AxisInput("joint 2", "", "", -1, false, true, 1, 2),
        ],
    }

    public static AVAILABLE_INPUT_SCHEMES: InputScheme[] = [
        this.wasd,
        this.arrowKeys,
        this.fullController,
        this.leftStick,
        this.rightStick,
    ]

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
