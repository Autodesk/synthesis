import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"

import Lazy from "@/util/Lazy.ts"
import WPILibWSWorker from "./WPILibWSWorker?worker"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"

import { SimAnalogOutput, SimDigitalOutput, SimOutput } from "./SimOutput"
import { SimAccelInput, SimAnalogInput, SimDigitalInput, SimGyroInput, SimInput } from "./SimInput"

const worker: Lazy<Worker> = new Lazy<Worker>(() => new WPILibWSWorker())

const PWM_SPEED = "<speed"
const PWM_POSITION = "<position"

const CANMOTOR_PERCENT_OUTPUT = "<percentOutput"
const CANMOTOR_BRAKE_MODE = "<brakeMode"
const CANMOTOR_NEUTRAL_DEADBAND = "<neutralDeadband"

const CANMOTOR_SUPPLY_CURRENT = ">supplyCurrent"
const CANMOTOR_MOTOR_CURRENT = ">motorCurrent"
const CANMOTOR_BUS_VOLTAGE = ">busVoltage"

const CANENCODER_POSITION = ">position"
const CANENCODER_VELOCITY = ">velocity"

export enum SimType {
    PWM = "PWM",
    SimDevice = "SimDevice",
    CANMotor = "CANMotor",
    Solenoid = "Solenoid",
    CANEncoder = "CANEncoder",
    Gyro = "Gyro",
    Accel = "Accel",
    DIO = "DIO",
    AI = "AI",
    AO = "AO",
}

enum FieldType {
    Read = 0,
    Write = 1,
    Both = 2,
    Unknown = -1,
}

function GetFieldType(field: string): FieldType {
    if (field.length < 2) {
        return FieldType.Unknown
    }

    switch (field.charAt(0)) {
        case "<":
            return field.charAt(1) == ">" ? FieldType.Both : FieldType.Read
        case ">":
            return FieldType.Write
        default:
            return FieldType.Unknown
    }
}

type DeviceName = string
type DeviceData = Map<string, number | boolean | string>

export const simMap = new Map<SimType, Map<DeviceName, DeviceData>>()

export class SimGeneric {
    private constructor() {}

    public static Get<T>(simType: SimType, device: string, field: string): T | undefined
    public static Get<T>(simType: SimType, device: string, field: string, defaultValue: T): T
    public static Get<T>(simType: SimType, device: string, field: string, defaultValue?: T): T | undefined {
        const fieldType = GetFieldType(field)
        if (fieldType != FieldType.Read && fieldType != FieldType.Both) {
            console.warn(`Field '${field}' is not a read or both field type`)
            return undefined
        }

        const map = simMap.get(simType)
        if (!map) {
            console.warn(`No '${simType}' devices found`)
            return undefined
        }

        const data = map.get(device)
        if (!data) {
            console.warn(`No '${simType}' device '${device}' found`)
            return undefined
        }

        return (data.get(field) as T | undefined) ?? defaultValue
    }

    public static Set<T extends number | boolean>(simType: SimType, device: string, field: string, value: T): boolean {
        const fieldType = GetFieldType(field)
        if (fieldType != FieldType.Write && fieldType != FieldType.Both) {
            console.warn(`Field '${field}' is not a write or both field type`)
            return false
        }

        const map = simMap.get(simType)
        if (!map) {
            console.warn(`No '${simType}' devices found`)
            return false
        }

        const data = map.get(device)
        if (!data) {
            console.warn(`No '${simType}' device '${device}' found`)
            return false
        }

        const selectedData: { [key: string]: number | boolean } = {}
        selectedData[field] = value
        data.set(field, value)

        worker.getValue().postMessage({
            command: "update",
            data: {
                type: simType,
                device: device,
                data: selectedData,
            },
        })

        window.dispatchEvent(new SimMapUpdateEvent(true))
        return true
    }
}

export class SimPWM {
    private constructor() {}

    public static GetSpeed(device: string): number | undefined {
        return SimGeneric.Get(SimType.PWM, device, PWM_SPEED, 0.0)
    }

    public static GetPosition(device: string): number | undefined {
        return SimGeneric.Get(SimType.PWM, device, PWM_POSITION, 0.0)
    }
}

export class SimCAN {
    private constructor() {}

    public static GetDeviceWithID(id: number, type: SimType): DeviceData | undefined {
        const id_exp = /SYN.*\[(\d+)\]/g
        const entries = [...simMap.entries()].filter(([simType, _data]) => simType == type)
        for (const [_simType, data] of entries) {
            for (const key of data.keys()) {
                const result = [...key.matchAll(id_exp)]
                if (result?.length <= 0 || result[0].length <= 1) continue
                const parsed_id = parseInt(result[0][1])
                if (parsed_id != id) continue
                return data.get(key)
            }
        }
        return undefined
    }
}

export class SimCANMotor {
    private constructor() {}

    public static GetPercentOutput(device: string): number | undefined {
        return SimGeneric.Get(SimType.CANMotor, device, CANMOTOR_PERCENT_OUTPUT, 0.0)
    }

    public static GetBrakeMode(device: string): number | undefined {
        return SimGeneric.Get(SimType.CANMotor, device, CANMOTOR_BRAKE_MODE, 0.0)
    }

    public static GetNeutralDeadband(device: string): number | undefined {
        return SimGeneric.Get(SimType.CANMotor, device, CANMOTOR_NEUTRAL_DEADBAND, 0.0)
    }

    public static SetSupplyCurrent(device: string, current: number): boolean {
        return SimGeneric.Set(SimType.CANMotor, device, CANMOTOR_SUPPLY_CURRENT, current)
    }

    public static SetMotorCurrent(device: string, current: number): boolean {
        return SimGeneric.Set(SimType.CANMotor, device, CANMOTOR_MOTOR_CURRENT, current)
    }

    public static SetBusVoltage(device: string, voltage: number): boolean {
        return SimGeneric.Set(SimType.CANMotor, device, CANMOTOR_BUS_VOLTAGE, voltage)
    }
}
export class SimCANEncoder {
    private constructor() {}

    public static SetVelocity(device: string, velocity: number): boolean {
        return SimGeneric.Set(SimType.CANEncoder, device, CANENCODER_VELOCITY, velocity)
    }

    public static SetPosition(device: string, position: number): boolean {
        return SimGeneric.Set(SimType.CANEncoder, device, CANENCODER_POSITION, position)
    }
}

export class SimGyro {
    private constructor() {}

    public static SetAngleX(device: string, angle: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">angle_x", angle)
    }

    public static SetAngleY(device: string, angle: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">angle_y", angle)
    }

    public static SetAngleZ(device: string, angle: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">angle_z", angle)
    }

    public static SetRateX(device: string, rate: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">rate_x", rate)
    }

    public static SetRateY(device: string, rate: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">rate_y", rate)
    }

    public static SetRateZ(device: string, rate: number): boolean {
        return SimGeneric.Set(SimType.Gyro, device, ">rate_z", rate)
    }
}

export class SimAccel {
    private constructor() {}

    public static SetX(device: string, accel: number): boolean {
        return SimGeneric.Set(SimType.Accel, device, ">x", accel)
    }

    public static SetY(device: string, accel: number): boolean {
        return SimGeneric.Set(SimType.Accel, device, ">y", accel)
    }

    public static SetZ(device: string, accel: number): boolean {
        return SimGeneric.Set(SimType.Accel, device, ">z", accel)
    }
}

export class SimDIO {
    private constructor() {}

    public static SetValue(device: string, value: boolean): boolean {
        return SimGeneric.Set(SimType.DIO, device, "<>value", value)
    }

    public static GetValue(device: string): boolean {
        return SimGeneric.Get(SimType.DIO, device, "<>value", false)
    }
}

export class SimAI {
    constructor() {}

    public static SetValue(device: string, value: number): boolean {
        return SimGeneric.Set(SimType.AI, device, ">voltage", value)
    }

    /**
     * The number of averaging bits
     */
    public static GetAvgBits(device: string) {
        return SimGeneric.Get(SimType.AI, device, "<avg_bits")
    }
    /**
     * The number of oversampling bits
     */
    public static GetOversampleBits(device: string) {
        return SimGeneric.Get(SimType.AI, device, "<oversample_bits")
    }
    /**
     * Input voltage, in volts
     */
    public static SetVoltage(device: string, voltage: number) {
        return SimGeneric.Set(SimType.AI, device, ">voltage", voltage)
    }
    /**
     * If the accumulator is initialized in the robot program
     */
    public static GetAccumInit(device: string) {
        return SimGeneric.Get(SimType.AI, device, "<accum_init")
    }
    /**
     * The accumulated value
     */
    public static SetAccumValue(device: string, accum_value: number) {
        return SimGeneric.Set(SimType.AI, device, ">accum_value", accum_value)
    }
    /**
     * The number of accumulated values
     */
    public static SetAccumCount(device: string, accum_count: number) {
        return SimGeneric.Set(SimType.AI, device, ">accum_count", accum_count)
    }
    /**
     * The center value of the accumulator
     */
    public static GetAccumCenter(device: string) {
        return SimGeneric.Get(SimType.AI, device, "<accum_center")
    }
    /**
     * The accumulator's deadband
     */
    public static GetAccumDeadband(device: string) {
        return SimGeneric.Get(SimType.AI, device, "<accum_deadband")
    }
}

export class SimAO {
    constructor() {}

    public static GetVoltage(device: string): number {
        return SimGeneric.Get(SimType.AI, device, ">voltage", 0.0)
    }
}

type WSMessage = {
    type: string // might be a SimType
    device: string // device name
    data: Map<string, number>
}

worker.getValue().addEventListener("message", (eventData: MessageEvent) => {
    let data: WSMessage | undefined

    if (typeof eventData.data == "object") {
        data = eventData.data
    } else {
        try {
            data = JSON.parse(eventData.data)
        } catch (e) {
            console.error(`Failed to parse data:\n${JSON.stringify(eventData.data)}`)
            return
        }
    }

    if (!data?.type || !(Object.values(SimType) as string[]).includes(data.type)) return

    UpdateSimMap(data.type as SimType, data.device, data.data)
})

function UpdateSimMap(type: SimType, device: string, updateData: DeviceData) {
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

    Object.entries(updateData).forEach(([key, value]) => currentData.set(key, value))

    window.dispatchEvent(new SimMapUpdateEvent(false))
}

class WPILibBrain extends Brain {
    private _simLayer: SimulationLayer

    private _simOutputs: SimOutput[] = []
    private _simInputs: SimInput[] = []

    constructor(mechanism: Mechanism) {
        super(mechanism)

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!

        if (!this._simLayer) {
            console.warn("SimulationLayer is undefined")
            return
        }

        // this.addSimInput(new SimGyroInput("Test Gyro[1]", mechanism))
        this.addSimInput(new SimAccelInput("ADXL362[4]", mechanism))
        this.addSimInput(new SimDigitalInput("SYN DI[0]", () => Math.random() > 0.5))
        this.addSimOutput(new SimDigitalOutput("SYN DO[1]"))
        this.addSimInput(new SimAnalogInput("SYN AI[0]", () => Math.random() * 12))
        this.addSimOutput(new SimAnalogOutput("SYN AO[1]"))
    }

    public addSimOutput(device: SimOutput) {
        this._simOutputs.push(device)
    }

    public addSimInput(input: SimInput) {
        this._simInputs.push(input)
    }

    public Update(deltaT: number): void {
        this._simOutputs.forEach(d => d.Update(deltaT))
        this._simInputs.forEach(i => i.Update(deltaT))
    }

    public Enable(): void {
        worker.getValue().postMessage({ command: "connect" })
    }

    public Disable(): void {
        worker.getValue().postMessage({ command: "disconnect" })
    }
}

export class SimMapUpdateEvent extends Event {
    public static readonly TYPE: string = "ws/sim-map-update"

    private _internalUpdate: boolean

    public get internalUpdate(): boolean {
        return this._internalUpdate
    }

    public constructor(internalUpdate: boolean) {
        super(SimMapUpdateEvent.TYPE)

        this._internalUpdate = internalUpdate
    }
}

export default WPILibBrain
