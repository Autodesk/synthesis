import InputSystem from "@/systems/input/InputSystem"
import type { KeyCode } from "@/systems/input/KeyboardTypes"
import { SimInput } from "../SimInput"
import { SimType, worker } from "../WPILibTypes"

const GAMEPAD_AXIS = { LEFT_X: 0, LEFT_Y: 1, RIGHT_X: 2, RIGHT_Y: 3 }
const GAMEPAD_BUTTON = {
    A: 0,
    B: 1,
    X: 2,
    Y: 3,
    LEFT_BUMPER: 4,
    RIGHT_BUMPER: 5,
    LEFT_TRIGGER: 6,
    RIGHT_TRIGGER: 7,
    BACK: 8,
    START: 9,
    DPAD_UP: 12,
    DPAD_DOWN: 13,
    DPAD_LEFT: 14,
    DPAD_RIGHT: 15,
}

/** Pushes the local physical/keyboard gamepad state out over the WS worker every frame, for brains without their own driver-station-style input relay (e.g. FTC). */
export class SimGamepadInput extends SimInput {
    public update(_deltaT: number) {
        const data = InputSystem.gamepad ? this.readPhysicalGamepad() : this.readKeyboardGamepad()

        worker.getValue().postMessage({
            command: "update",
            data: { type: SimType.GAMEPAD, device: this._device, data },
        })
    }

    private readPhysicalGamepad() {
        const gamepad = InputSystem.gamepad!
        return {
            left_stick_x: InputSystem.getGamepadAxis(GAMEPAD_AXIS.LEFT_X),
            left_stick_y: InputSystem.getGamepadAxis(GAMEPAD_AXIS.LEFT_Y),
            right_stick_x: InputSystem.getGamepadAxis(GAMEPAD_AXIS.RIGHT_X),
            right_stick_y: InputSystem.getGamepadAxis(GAMEPAD_AXIS.RIGHT_Y),
            left_trigger: gamepad.buttons[GAMEPAD_BUTTON.LEFT_TRIGGER]?.value ?? 0,
            right_trigger: gamepad.buttons[GAMEPAD_BUTTON.RIGHT_TRIGGER]?.value ?? 0,
            a: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.A),
            b: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.B),
            x: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.X),
            y: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.Y),
            left_bumper: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.LEFT_BUMPER),
            right_bumper: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.RIGHT_BUMPER),
            dpad_up: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.DPAD_UP),
            dpad_down: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.DPAD_DOWN),
            dpad_left: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.DPAD_LEFT),
            dpad_right: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.DPAD_RIGHT),
            start: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.START),
            back: InputSystem.isGamepadButtonPressed(GAMEPAD_BUTTON.BACK),
        }
    }

    private readKeyboardGamepad() {
        const axis = (positiveKey: KeyCode, negativeKey: KeyCode) =>
            (InputSystem.isKeyPressed(positiveKey) ? 1 : 0) - (InputSystem.isKeyPressed(negativeKey) ? 1 : 0)

        return {
            left_stick_x: 0,
            left_stick_y: axis("KeyS", "KeyW"),
            right_stick_x: axis("KeyD", "KeyA"),
            right_stick_y: 0,
            left_trigger: 0,
            right_trigger: 0,
            a: false,
            b: false,
            x: false,
            y: false,
            left_bumper: false,
            right_bumper: false,
            dpad_up: false,
            dpad_down: false,
            dpad_left: false,
            dpad_right: false,
            start: false,
            back: false,
        }
    }
}
