import Lazy from "@/util/Lazy.ts"
import WSWorker from "../shared/WSWorker?worker"

export const DEFAULT_WS_URL = "ws://localhost:3300/wpilibws"

export type DeviceName = string
export type DeviceData = Map<string, number | boolean | string>

export type SimMap = Map<SimType, Map<DeviceName, DeviceData>>

export enum SimType {
    PWM = "PWM",
    SIM_DEVICE = "SimDevice",
    CAN_MOTOR = "CANMotor",
    SOLENOID = "Solenoid",
    CAN_ENCODER = "CANEncoder",
    GYRO = "Gyro",
    ACCELEROMETER = "Accel",
    DIO = "DIO",
    AI = "AI",
    AO = "AO",
    DRIVERS_STATION = "DriverStation",
}

export enum FieldType {
    READ = 0,
    WRITE = 1,
    BOTH = 2,
    UNKNOWN = -1,
}

export enum RobotSimMode {
    DISABLED = 0,
    TELEOP = 1,
    AUTO = 2,
}

export type AllianceStation = "red1" | "red2" | "red3" | "blue1" | "blue2" | "blue3"

export type WSMessage = {
    type: string // might be a SimType
    device: string // device name
    data: Map<string, number>
}

export const PWM_SPEED = "<speed"
export const PWM_POSITION = "<position"

export const CANMOTOR_PERCENT_OUTPUT = "<percentOutput"
export const CANMOTOR_BRAKE_MODE = "<brakeMode"
export const CANMOTOR_NEUTRAL_DEADBAND = "<neutralDeadband"

export const CANMOTOR_SUPPLY_CURRENT = ">supplyCurrent"
export const CANMOTOR_MOTOR_CURRENT = ">motorCurrent"
export const CANMOTOR_BUS_VOLTAGE = ">busVoltage"

export const CANENCODER_POSITION = ">position"
export const CANENCODER_VELOCITY = ">velocity"

export const worker: Lazy<Worker> = new Lazy<Worker>(() => new WSWorker())
