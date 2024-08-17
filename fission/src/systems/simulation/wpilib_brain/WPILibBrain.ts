import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"

import WPILibWSWorker from "./WPILibWSWorker?worker"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"

import { SimOutputGroup } from "./SimOutput"
import { SimInput } from "./SimInput"

const worker = new WPILibWSWorker()
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
    CANMotor = "CANMotor",
    Solenoid = "Solenoid",
    CANEncoder = "CANEncoder",
    Gyro = "Gyro",
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
type DeviceData = Map<string, number>

export const simMap = new Map<SimType, Map<DeviceName, DeviceData>>()

export class SimGeneric {
    private constructor() {}

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

    public static Set<T extends number>(simType: SimType, device: string, field: string, value: T): boolean {
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

        const selectedData: { [key: string]: number } = {}
        selectedData[field] = value
        data.set(field, value)

        console.log(`encoder update: ${JSON.stringify(selectedData)}`)

        worker.postMessage({
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
        const id_exp = /.*\[(\d+)\]/g
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
}

type WSMessage = {
    type: string // might be a SimType
    device: string // device name
    data: Map<string, number>
}

worker.addEventListener("message", (eventData: MessageEvent) => {
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

    if (!data?.type || !(Object.values(SimType) as string[]).includes(data.type) || data.device.split(" ")[0] != "SYN")
        return

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

    private _simOutputs: SimOutputGroup[] = []
    private _simInputs: SimInput[] = []

    constructor(mechanism: Mechanism) {
        super(mechanism)

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!

        if (!this._simLayer) {
            console.warn("SimulationLayer is undefined")
            return
        }
    }

    public addSimOutputGroup(device: SimOutputGroup) {
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
        worker.postMessage({ command: "connect" })
    }

    public Disable(): void {
        worker.postMessage({ command: "disconnect" })
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