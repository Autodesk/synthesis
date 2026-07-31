import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import type { KeyCode } from "@/systems/input/KeyboardTypes"
import World from "@/systems/World"
import Brain from "../Brain"
import type Driver from "../driver/Driver"
import type { SimulationLayer } from "../SimulationSystem"
import { getSimBrain, getSimMap, setConnected, setSimBrain } from "./FTCState"
import { DCMOTOR_POWER, type DeviceData, SimType, type WSMessage, worker } from "./FTCTypes"

worker.getValue().addEventListener("message", (eventData: MessageEvent) => {
    let data: WSMessage | undefined

    if (eventData.data.status) {
        switch (eventData.data.status) {
            case "open":
                setConnected(true)
                break
            case "close":
            case "error":
                setConnected(false)
                break
            default:
                return
        }
        return
    }

    if (typeof eventData.data == "object") {
        data = eventData.data
    } else {
        try {
            data = JSON.parse(eventData.data)
        } catch (_e) {
            return
        }
    }

    if (!data?.type) return

    updateSimMap(data.type as SimType, data.device, data.data)
})

function updateSimMap(type: SimType, device: string, updateData: DeviceData) {
    const simMap = getSimMap()
    if (!simMap) return
    let typeMap = simMap.get(type)
    if (!typeMap) {
        typeMap = new Map<string, DeviceData>()
        simMap.set(type, typeMap)
    }

    let currentData = typeMap.get(device)
    if (!currentData) {
        currentData = new Map<string, number>()
        typeMap.set(device, currentData)
    }

    Object.entries(updateData).forEach(([key, value]) => currentData!.set(key, value))
}

// Standard W3C Gamepad indices (Xbox-style), matching what InputSystem.getGamepadAxis/
// isGamepadButtonPressed already assume elsewhere in the codebase.
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

/**
 * FTC counterpart to WPILibBrain. Only handles what SyntheSimFTC's shim
 * implements today: DcMotorSimple power (harness -> mechanism, one-directional,
 * no encoder feedback yet) and gamepad1 axes/buttons (Fission -> harness).
 *
 * Wiring (FTC device name -> Driver) is set via addMotorWiring, called from
 * FTCCreateDeviceModal. It is session-only, not persisted through
 * assembly.simConfigData the way WPILibBrain's saved config is -- see
 * project_ftc_codesim_scope_decisions memory for why (there's no on-disk
 * robot config to round-trip against in the first place, real FTC hardware
 * config never lives in the team's source tree either).
 */
class FTCBrain extends Brain {
    private _simLayer: SimulationLayer
    private _assembly: MirabufSceneObject
    private _motorWiring = new Map<string, Driver[]>()

    public get assemblyName() {
        return this._assembly.assemblyName
    }

    public get assemblyId() {
        return this._assembly.assemblyId
    }

    public get motorWiring(): ReadonlyMap<string, Driver[]> {
        return this._motorWiring
    }

    public override get brainType() {
        return "ftc" as const
    }

    constructor(assembly: MirabufSceneObject) {
        super(assembly.mechanism)

        this._assembly = assembly
        this._simLayer = World.simulationSystem.getSimulationLayer(this._mechanism)!
    }

    public addMotorWiring(deviceName: string, driver: Driver) {
        const drivers = this._motorWiring.get(deviceName) ?? []
        if (!drivers.includes(driver)) {
            drivers.push(driver)
        }
        this._motorWiring.set(deviceName, drivers)
    }

    public removeMotorWiring(deviceName: string) {
        this._motorWiring.delete(deviceName)
    }

    public update(deltaT: number): void {
        this.pushGamepadState()

        const motorData = getSimMap()?.get(SimType.DC_MOTOR)
        this._motorWiring.forEach((drivers, deviceName) => {
            const power = motorData?.get(deviceName)?.get(DCMOTOR_POWER)
            if (typeof power !== "number") return
            drivers.forEach(driver => driver.setReceiverValue(power))
        })
    }

    private pushGamepadState() {
        const data = InputSystem.gamepad ? this.readPhysicalGamepad() : this.readKeyboardGamepad()

        worker.getValue().postMessage({
            command: "update",
            data: { type: SimType.GAMEPAD, device: "1", data },
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

    // No physical gamepad connected -- emulate gamepad1's sticks off the keyboard so
    // OnBotJava-style OpModes are still drivable. WASD matches the codebase's existing
    // arcade-drive keyboard convention (see DefaultInputs.ts): W/S drive the left stick's
    // Y axis, A/D drive the right stick's X axis. Up = -1/down = +1 mirrors the W3C
    // Gamepad axis convention real sticks report, so OpMode code doesn't need to special-case
    // keyboard vs. physical input.
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

    public enable(): void {
        setSimBrain(this)
    }

    public disable(): void {
        if (getSimBrain() == this) {
            setSimBrain(undefined)
        }
    }
}

export default FTCBrain
