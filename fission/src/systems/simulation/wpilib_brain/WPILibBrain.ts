/* eslint-disable @typescript-eslint/no-explicit-any */
import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"

import WPILibWSWorker from "./WPILibWSWorker?worker"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"
import Driver from "../driver/Driver"

const worker = new WPILibWSWorker()

const PWM_SPEED = "<speed"
const PWM_POSITION = "<position"
const CANMOTOR_DUTY_CYCLE = "<dutyCycle"
const CANMOTOR_SUPPLY_VOLTAGE = ">supplyVoltage"
const CANENCODER_RAW_INPUT_POSITION = ">rawPositionInput"

export type SimType = "PWM" | "CANMotor" | "Solenoid" | "SimDevice" | "CANEncoder"

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
                let result = [...key.matchAll(id_exp)]
                if (result?.length <= 0 || result?[0].length <= 1) continue
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

    // console.debug(data)

    const device = data.device
    const updateData = data.data

    switch (data.type) {
        case "PWM":
            console.debug("pwm")
            UpdateSimMap("PWM", device, updateData)
            break
        case "Solenoid":
            console.debug("solenoid")
            UpdateSimMap("Solenoid", device, updateData)
            break
        case "SimDevice":
            console.debug("simdevice")
            UpdateSimMap("SimDevice", device, updateData)
            break
        case "CANMotor":
            console.debug("canmotor")
            UpdateSimMap("CANMotor", device, updateData)
            break
        case "CANEncoder":
            console.debug("canencoder")
            UpdateSimMap("CANEncoder", device, updateData)
            break
        default:
            // console.debug(`Unrecognized Message:\n${data}`)
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

    private _simDevices: SimOutputGroup[] = []

    public static robotsSpawned: string[] = []

    constructor(mechanism: Mechanism) {
        super(mechanism)

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!

        if (!this._simLayer) {
            console.warn("SimulationLayer is undefined")
            return
        }

        // if (mechanism.controllable) {
        //     WPILibBrain.robotsSpawned.push(this.getNumberedAssemblyName())
        // }

        // WPILibBrain._currentRobotIndex++
        // this.configureArcadeDriveBehavior()
    }

    public addSimOutputGroup(device: SimOutputGroup) {
        this._simDevices.push(device)
    }

    public Update(deltaT: number): void {
        // this._behaviors.forEach(b => b.Update(deltaT))
        this._simDevices.forEach(d => d.Update(deltaT))
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

abstract class SimOutputGroup {
    public name: string
    public ports: number[]
    public drivers: Driver[]
    public type: SimType

    public constructor(name: string, ports: number[], drivers: Driver[], type: SimType) {
        this.name = name
        this.ports = ports
        this.drivers = drivers
        this.type = type
    }

    public abstract Update(deltaT: number): void
}

export class PWMGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, "PWM")
    }

    public Update(_deltaT: number) {
        // let average = 0;
        for (const port of this.ports) {
            const speed = SimPWM.GetSpeed(`${port}`) ?? 0
            // average += speed;
            console.log(port, speed)
        }
        // average /= this.ports.length

        // this.drivers.forEach(d => {
        //     (d as WheelDriver).targetWheelSpeed = average * 40
        //     d.Update(_deltaT)
        // })
    }
}

export class CANGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, "CANMotor")
    }

    public Update(_deltaT: number) {
        for (const port of this.ports) {
            const device = SimCAN.GetDeviceWithID(port, this.type)
            console.log(port, device)
        }
    }
}
