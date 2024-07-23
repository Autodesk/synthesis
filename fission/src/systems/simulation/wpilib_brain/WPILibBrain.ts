/* eslint-disable @typescript-eslint/no-explicit-any */
import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"

import WPILibWSWorker from "./WPILibWSWorker?worker"
import Behavior from "../behavior/Behavior"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"
import Driver from "../driver/Driver"
import WheelDriver from "../driver/WheelDriver"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import Jolt from "@barclah/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import ArcadeDriveBehavior from "../behavior/synthesis/ArcadeDriveBehavior"
import WPILibArcadeDriveBehavior from "../behavior/wpilib/WPILibArcadeDriveBehavior"
import { mirabuf } from "@/proto/mirabuf"

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

const defaultSimMap: Map<SimType, Map<string, any>> = new Map(
    Object.entries({
        "CANMotor": {
            "CANSparkMax[0]": {
                "<percentOutput": 0.75
            },
            "CANSparkMax[1]": {
                "<percentOutput": 0.75
            },
            "CANSparkMax[2]": {
                "<percentOutput": 0.75
            },
            "CANSparkMax[3]": {
                "<percentOutput": -0.75
            },
            "CANSparkMax[4]": {
                "<percentOutput": -0.75
            },
            "CANSparkMax[5]": {
                "<percentOutput": -0.75
            },
        }
    })
        .map(([key, value]) => [key as SimType, new Map(Object.entries(value))])
)

export const simMap = new Map<SimType, Map<string, any>>()

export class SimGeneric {
    private constructor() { }

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
    private constructor() { }

    public static GetSpeed(device: string): number | undefined {
        return SimGeneric.Get("PWM", device, PWM_SPEED, 0.0)
    }

    public static GetPosition(device: string): number | undefined {
        return SimGeneric.Get("PWM", device, PWM_POSITION, 0.0)
    }
}

export class SimCAN {
    private constructor() { }

    public static GetDeviceWithID(id: number): any {
        const id_exp = /.*\[(\d+)\]/g;
        const entries = [...simMap.entries()].filter(([simType, _data]) => simType.startsWith("CAN"))
        entries.forEach(([_simType, data]) => {
            [...data.keys()].forEach(key => {
                let result;
                if ((result = [...key.matchAll(id_exp)]) != undefined) {
                    if (result.length > 0 && result[0].length > 1) {
                        const parsed_id = parseInt(result[0][1]);
                        if (parsed_id == id)
                            return data.get(key)
                    }
                }
            })
        })
        return undefined
    }
}

export class SimCANMotor {
    private constructor() { }

    public static GetDutyCycle(device: string): number | undefined {
        return SimGeneric.Get("CANMotor", device, CANMOTOR_DUTY_CYCLE, 0.0)
    }

    public static SetSupplyVoltage(device: string, voltage: number): boolean {
        return SimGeneric.Set("CANMotor", device, CANMOTOR_SUPPLY_VOLTAGE, voltage)
    }
}

export class SimCANEncoder {
    private constructor() { }

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
    private _behaviors: Behavior[] = []
    private _simLayer: SimulationLayer

    private _simDevices: SimDevice[] = []

    // private _driverDevices: Map<SimType, Map<string, Driver>> = new Map()

    public static robotsSpawned: string[] = []

    private static _currentRobotIndex: number = 0

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
    
    // public configureArcadeDriveBehavior() {
    //     const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
    //         driver => driver instanceof WheelDriver
    //     ) as WheelDriver[]
    //     wheelDrivers.forEach((wheel, idx) => {
    //         wheel.deviceType = 'SimDevice'
    //         wheel.device = `SYN CANSparkMax[${idx}]`
    //     })
    //     const wheelStimuli: WheelRotationStimulus[] = this._simLayer.stimuli.filter(
    //         stimulus => stimulus instanceof WheelRotationStimulus
    //     ) as WheelRotationStimulus[]

    //     // Two body constraints are part of wheels and are used to determine which way a wheel is facing
    //     const fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints
    //         .filter(mechConstraint => mechConstraint.constraint instanceof JOLT.TwoBodyConstraint)
    //         .map(mechConstraint => mechConstraint.constraint as Jolt.TwoBodyConstraint)

    //     const leftWheels: WheelDriver[] = []
    //     const leftStimuli: WheelRotationStimulus[] = []

    //     const rightWheels: WheelDriver[] = []
    //     const rightStimuli: WheelRotationStimulus[] = []

    //     // Determines which wheels and stimuli belong to which side of the robot
    //     for (let i = 0; i < wheelDrivers.length; i++) {
    //         const wheelPos = fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation()

    //         const robotCOM = World.PhysicsSystem.GetBody(
    //             this._mechanism.constraints[0].childBody
    //         ).GetCenterOfMassPosition() as Jolt.Vec3
    //         const rightVector = new JOLT.Vec3(1, 0, 0)

    //         const dotProduct = rightVector.Dot(wheelPos.Sub(robotCOM))

    //         if (dotProduct < 0) {
    //             rightWheels.push(wheelDrivers[i])
    //             rightStimuli.push(wheelStimuli[i])
    //         } else {
    //             leftWheels.push(wheelDrivers[i])
    //             leftStimuli.push(wheelStimuli[i])
    //         }
    //     }

    //     // TODO: all this is very temporary
    //     this._driverDevices.set("CANMotor", new Map<string, Driver>());
    //     leftWheels.forEach(wheel => this._driverDevices.get(wheel.deviceType!)!.set(wheel.device!, wheel))
    //     rightWheels.forEach(wheel => this._driverDevices.get(wheel.deviceType!)!.set(wheel.device!, wheel))

    //     this._behaviors.push(
    //         new WPILibArcadeDriveBehavior(
    //             leftWheels,
    //             rightWheels,
    //             leftStimuli,
    //             rightStimuli
    //         )
    //     )
    // }
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

export class SimDevice {
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

    public Update(deltaT: number) {
        console.log('SimDevice update...')
    }
}
