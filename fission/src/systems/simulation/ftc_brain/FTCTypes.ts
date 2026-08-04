import Lazy from "@/util/Lazy.ts"
import WSWorker from "../shared/WSWorker?worker"

export const DEFAULT_WS_URL = "ws://localhost:3301/ftcsimws"

export type DeviceName = string
export type DeviceData = Map<string, number | boolean | string>

export type SimMap = Map<SimType, Map<DeviceName, DeviceData>>

/**
 * DcMotorSimple power out, Gamepad axes/buttons in. Widen alongside the
 * FTC shim (Servo, CRServo, IMU, ...), not ahead of it.
 */
export enum SimType {
    DC_MOTOR = "DcMotor",
    GAMEPAD = "Gamepad",
}

export type WSMessage = {
    type: string // might be a SimType
    device: string // device name
    data: Map<string, number>
}

// FTCWsBridge.sendMotorPower's payload key (com.autodesk.synthesis.ftc.FTCWsBridge)
export const DCMOTOR_POWER = "power"

export const worker: Lazy<Worker> = new Lazy<Worker>(() => new WSWorker())
