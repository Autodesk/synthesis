/* eslint-disable @typescript-eslint/no-explicit-any */
import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"

import WPILibWSWorker from "./WPILibWSWorker?worker"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"

import { SimOutputGroup } from "./SimOutput"
import { SimGyroInput, SimInput } from "./SimInput"

const worker = new WPILibWSWorker()

const PWM_SPEED = "<speed"
const PWM_POSITION = "<position"
const CANMOTOR_DUTY_CYCLE = "<dutyCycle"
const CANMOTOR_SUPPLY_VOLTAGE = ">supplyVoltage"
const CANENCODER_RAW_INPUT_POSITION = ">rawPositionInput"

export type SimType = "PWM" | "CANMotor" | "Solenoid" | "SimDevice" | "CANEncoder" | "Gyro"

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

export const simMap = new Map<SimType, Map<string, any>>()

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

        return (data[field] as T | undefined) ?? defaultValue
    }

    public static Set<T>(simType: SimType, device: string, field: string, value: T): boolean {
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

        const selectedData: any = {}
        selectedData[field] = value

        data[field] = value
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
        return SimGeneric.Get("PWM", device, PWM_SPEED, 0.0)
    }

    public static GetPosition(device: string): number | undefined {
        return SimGeneric.Get("PWM", device, PWM_POSITION, 0.0)
    }
}

export class SimCAN {
    private constructor() {}

    public static GetDeviceWithID(id: number, type: SimType): any {
        const id_exp = /.*\[(\d+)\]/g
        const entries = [...simMap.entries()].filter(([simType, _data]) => simType == type || simType == "SimDevice")
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

    public static GetDutyCycle(device: string): number | undefined {
        return SimGeneric.Get("CANMotor", device, CANMOTOR_DUTY_CYCLE, 0.0)
    }

    public static SetSupplyVoltage(device: string, voltage: number): boolean {
        return SimGeneric.Set("CANMotor", device, CANMOTOR_SUPPLY_VOLTAGE, voltage)
    }
}

export class SimCANEncoder {
    private constructor() {}

    public static SetRawInputPosition(device: string, rawInputPosition: number): boolean {
        return SimGeneric.Set("CANEncoder", device, CANENCODER_RAW_INPUT_POSITION, rawInputPosition)
    }
}

export class SimGyro {
    private constructor() {}

    public static SetAngleX(device: string, angle: number): boolean {
        return SimGeneric.Set("Gyro", device, ">angle_x", angle)
    }

    public static SetAngleY(device: string, angle: number): boolean {
        return SimGeneric.Set("Gyro", device, ">angle_y", angle)
    }

    public static SetAngleZ(device: string, angle: number): boolean {
        return SimGeneric.Set("Gyro", device, ">angle_z", angle)
    }

    public static SetRateX(device: string, rate: number): boolean {
        return SimGeneric.Set("Gyro", device, ">rate_x", rate)
    }

    public static SetRateY(device: string, rate: number): boolean {
        return SimGeneric.Set("Gyro", device, ">rate_y", rate)
    }

    public static SetRateZ(device: string, rate: number): boolean {
        return SimGeneric.Set("Gyro", device, ">rate_z", rate)
    }
}

worker.addEventListener("message", (eventData: MessageEvent) => {
    let data: any | undefined
    try {
        if (typeof eventData.data == "object") {
            data = eventData.data
        } else {
            data = JSON.parse(eventData.data)
        }
    } catch (e) {
        console.warn(`Failed to parse data:\n${JSON.stringify(eventData.data)}`)
    }

    if (!data || !data.type) {
        console.log("No data, bailing out")
        return
    }

    const device = data.device
    const updateData = data.data

    switch (data.type) {
        case "PWM":
            UpdateSimMap("PWM", device, updateData)
            break
        case "Solenoid":
            UpdateSimMap("Solenoid", device, updateData)
            break
        case "SimDevice":
            UpdateSimMap("SimDevice", device, updateData)
            break
        case "CANMotor":
            UpdateSimMap("CANMotor", device, updateData)
            break
        case "CANEncoder":
            UpdateSimMap("CANEncoder", device, updateData)
            break
        case "Gyro":
            UpdateSimMap("Gyro", device, updateData)
            break
        default:
            break
    }
})

function UpdateSimMap(type: SimType, device: string, updateData: any) {
    let typeMap = simMap.get(type)
    if (!typeMap) {
        typeMap = new Map<string, any>()
        simMap.set(type, typeMap)
    }

    let currentData = typeMap.get(device)
    if (!currentData) {
        currentData = {}
        typeMap.set(device, currentData)
    }
    Object.entries(updateData).forEach(([key, value]) => (currentData[key] = value))

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

        this.addSimInput(new SimGyroInput("Test Gyro[1]", mechanism))
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
        // console.log(simMap)
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
