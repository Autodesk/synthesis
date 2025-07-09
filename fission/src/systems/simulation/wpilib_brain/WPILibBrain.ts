import Brain from "../Brain"

import Lazy from "@/util/Lazy.ts"
import WPILibWSWorker from "./WPILibWSWorker?worker"
import { SimulationLayer } from "../SimulationSystem"
import World from "@/systems/World"

import { SimAnalogOutput, SimDigitalOutput, SimOutput } from "./SimOutput"
import { SimAccelInput, SimAnalogInput, SimDigitalInput, SimGyroInput, SimInput } from "./SimInput"
import { random } from "@/util/Random"
import { NoraNumber, NoraNumber2, NoraNumber3, NoraTypes } from "../Nora"
import { SimFlow, SimReceiver, SimSupplier, validate } from "./SimDataFlow"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { SimConfig } from "@/ui/panels/simulation/SimConfigShared"
import SynthesisBrain from "../synthesis_brain/SynthesisBrain"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

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

export let isConnected: boolean = false

export enum SimType {
    PWM = "PWM",
    SIM_DEVICE = "SimDevice",
    CAN_MOTOR = "CANMotor",
    SOLENOID = "Solenoid",
    CAN_ENCODER = "CANEncoder",
    GYRO = "Gyro",
    ACCEL = "Accel",
    DIO = "DIO",
    AI = "AI",
    AO = "AO",
    DRIVERS_STATION = "DriverStation",
}

enum FieldType {
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

export const supplierTypeMap: { [k in SimType]: NoraTypes | undefined } = {
    [SimType.PWM]: NoraTypes.NUMBER,
    [SimType.SIM_DEVICE]: undefined,
    [SimType.CAN_MOTOR]: NoraTypes.NUMBER,
    [SimType.SOLENOID]: NoraTypes.NUMBER,
    [SimType.CAN_ENCODER]: undefined,
    [SimType.GYRO]: undefined,
    [SimType.ACCEL]: undefined,
    [SimType.DIO]: NoraTypes.NUMBER, // ?
    [SimType.AI]: undefined,
    [SimType.AO]: NoraTypes.NUMBER,
    [SimType.DRIVERS_STATION]: undefined,
}

export const receiverTypeMap: { [k in SimType]: NoraTypes | undefined } = {
    [SimType.PWM]: undefined,
    [SimType.SIM_DEVICE]: undefined,
    [SimType.CAN_MOTOR]: undefined,
    [SimType.SOLENOID]: undefined,
    [SimType.CAN_ENCODER]: NoraTypes.NUMBER2,
    [SimType.GYRO]: NoraTypes.NUMBER3, // Wrong but its fine
    [SimType.ACCEL]: NoraTypes.NUMBER3,
    [SimType.DIO]: NoraTypes.NUMBER, // ?
    [SimType.AI]: NoraTypes.NUMBER,
    [SimType.AO]: undefined,
    [SimType.DRIVERS_STATION]: undefined,
}

function getFieldType(field: string): FieldType {
    if (field.length < 2) {
        return FieldType.UNKNOWN
    }

    switch (field.charAt(0)) {
        case "<":
            return field.charAt(1) == ">" ? FieldType.BOTH : FieldType.READ
        case ">":
            return FieldType.WRITE
        default:
            return FieldType.UNKNOWN
    }
}

type DeviceName = string
type DeviceData = Map<string, number | boolean | string>

type SimMap = Map<SimType, Map<DeviceName, DeviceData>>
export const simMaps = new Map<string, SimMap>()

let simBrain: WPILibBrain | undefined
export function setSimBrain(brain: WPILibBrain | undefined) {
    if (brain && !simMaps.has(brain.assemblyName)) {
        simMaps.set(brain.assemblyName, new Map())
    }
    if (simBrain) worker.getValue().postMessage({ command: "disable" })
    simBrain = brain
    if (simBrain)
        worker.getValue().postMessage({
            command: "enable",
            reconnect: PreferencesSystem.getGlobalPreference("SimAutoReconnect"),
        })
}

export function hasSimBrain() {
    return simBrain != undefined
}

export function getSimMap(): SimMap | undefined {
    if (!simBrain) return undefined
    return simMaps.get(simBrain.assemblyName)
}

export class SimGeneric {
    private constructor() {}

    public static getUnsafe<T>(simType: SimType, device: string, field: string): T | undefined
    public static getUnsafe<T>(simType: SimType, device: string, field: string, defaultValue: T): T
    public static getUnsafe<T>(simType: SimType, device: string, field: string, defaultValue?: T): T | undefined {
        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return undefined
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return undefined
        }

        return (data.get(field) as T | undefined) ?? defaultValue
    }

    public static get<T>(simType: SimType, device: string, field: string): T | undefined
    public static get<T>(simType: SimType, device: string, field: string, defaultValue: T): T
    public static get<T>(simType: SimType, device: string, field: string, defaultValue?: T): T | undefined {
        const fieldType = getFieldType(field)
        if (fieldType != FieldType.READ && fieldType != FieldType.BOTH) {
            console.warn(`Field '${field}' is not a read or both field type`)
            return undefined
        }

        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return undefined
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return undefined
        }

        return (data.get(field) as T | undefined) ?? defaultValue
    }

    public static set<T extends number | boolean | string>(
        simType: SimType,
        device: string,
        field: string,
        value: T
    ): boolean {
        const fieldType = getFieldType(field)
        if (fieldType != FieldType.WRITE && fieldType != FieldType.BOTH) {
            console.warn(`Field '${field}' is not a write or both field type`)
            return false
        }

        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return false
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return false
        }

        const selectedData: { [key: string]: number | boolean | string } = {}
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

export class SimDriverStation {
    private constructor() {}

    public static setMatchTime(time: number) {
        SimGeneric.set<number>(SimType.DRIVERS_STATION, "", ">match_time", time)
    }

    public static setGameData(gameData: string) {
        SimGeneric.set<string>(SimType.DRIVERS_STATION, "", ">match_time", gameData)
    }

    public static isEnabled(): boolean {
        return SimGeneric.getUnsafe<boolean>(SimType.DRIVERS_STATION, "", ">enabled", false)
    }

    public static setMode(mode: RobotSimMode) {
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">enabled", mode != RobotSimMode.DISABLED)
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">autonomous", mode == RobotSimMode.AUTO)
    }

    public static setStation(station: AllianceStation) {
        SimGeneric.set<string>(SimType.DRIVERS_STATION, "", ">station", station)
    }
}

export class SimPWM {
    private constructor() {}

    public static getSpeed(device: string): number | undefined {
        return SimDriverStation.isEnabled() ? SimGeneric.get(SimType.PWM, device, PWM_SPEED, 0.0) : 0.0
    }

    public static getPosition(device: string): number | undefined {
        return SimGeneric.get(SimType.PWM, device, PWM_POSITION, 0.0)
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => supplierTypeMap[SimType.PWM]!,
            getSupplierValue: () => SimPWM.getSpeed(device) ?? 0,
        }
    }
}

export class SimCAN {
    private constructor() {}

    public static getDeviceWithID(id: number, type: SimType): DeviceData | undefined {
        const idExp = /SYN.*\[(\d+)\]/g
        const map = getSimMap()
        if (!map) return undefined
        const entries = [...map.entries()].filter(([simType, _data]) => simType == type)
        for (const [_simType, data] of entries) {
            for (const key of data.keys()) {
                const result = [...key.matchAll(idExp)]
                if (result?.length <= 0 || result[0].length <= 1) continue
                const parsedId = parseInt(result[0][1])
                if (parsedId != id) continue
                return data.get(key)
            }
        }
        return undefined
    }
}

export class SimCANMotor {
    private constructor() {}

    public static getPercentOutput(device: string): number | undefined {
        return SimDriverStation.isEnabled()
            ? SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_PERCENT_OUTPUT, 0.0)
            : 0.0
    }

    public static getBrakeMode(device: string): number | undefined {
        return SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_BRAKE_MODE, 0.0)
    }

    public static getNeutralDeadband(device: string): number | undefined {
        return SimGeneric.get(SimType.CAN_MOTOR, device, CANMOTOR_NEUTRAL_DEADBAND, 0.0)
    }

    public static setSupplyCurrent(device: string, current: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_SUPPLY_CURRENT, current)
    }

    public static setMotorCurrent(device: string, current: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_MOTOR_CURRENT, current)
    }

    public static setBusVoltage(device: string, voltage: number): boolean {
        return SimGeneric.set(SimType.CAN_MOTOR, device, CANMOTOR_BUS_VOLTAGE, voltage)
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => supplierTypeMap[SimType.CAN_MOTOR]!,
            getSupplierValue: () => SimCANMotor.getPercentOutput(device) ?? 0,
        }
    }
}
export class SimCANEncoder {
    private constructor() {}

    public static setVelocity(device: string, velocity: number): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_VELOCITY, velocity)
    }

    public static setPosition(device: string, position: number): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_POSITION, position)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.CAN_ENCODER]!,
            setReceiverValue: ([count, rate]: NoraNumber2) => {
                SimCANEncoder.setPosition(device, count)
                SimCANEncoder.setVelocity(device, rate)
            },
        }
    }
}

export class SimGyro {
    private constructor() {}

    public static setAngleX(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_x", angle)
    }

    public static setAngleY(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_y", angle)
    }

    public static setAngleZ(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_z", angle)
    }

    public static setRateX(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_x", rate)
    }

    public static setRateY(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_y", rate)
    }

    public static setRateZ(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_z", rate)
    }
}

export class SimAccel {
    private constructor() {}

    public static setX(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCEL, device, ">x", accel)
    }

    public static setY(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCEL, device, ">y", accel)
    }

    public static setZ(device: string, accel: number): boolean {
        return SimGeneric.set(SimType.ACCEL, device, ">z", accel)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.ACCEL]!,
            setReceiverValue: ([x, y, z]: NoraNumber3) => {
                SimAccel.setX(device, x)
                SimAccel.setY(device, y)
                SimAccel.setZ(device, z)
            },
        }
    }
}

export class SimDIO {
    private constructor() {}

    public static setValue(device: string, value: boolean): boolean {
        return SimGeneric.set(SimType.DIO, device, "<>value", value)
    }

    public static getValue(device: string): boolean {
        return SimGeneric.get(SimType.DIO, device, "<>value", false)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.DIO]!,
            setReceiverValue: (a: NoraNumber) => {
                SimDIO.setValue(device, a > 0.5)
            },
        }
    }

    public static genSupplier(device: string): SimSupplier {
        return {
            getSupplierType: () => receiverTypeMap[SimType.DIO]!,
            getSupplierValue: () => (SimDIO.getValue(device) ? 1 : 0),
        }
    }
}

export class SimAI {
    constructor() {}

    public static setValue(device: string, value: number): boolean {
        return SimGeneric.set(SimType.AI, device, ">voltage", value)
    }

    /**
     * The number of averaging bits
     */
    public static getAvgBits(device: string) {
        return SimGeneric.get(SimType.AI, device, "<avg_bits")
    }
    /**
     * The number of oversampling bits
     */
    public static getOversampleBits(device: string) {
        return SimGeneric.get(SimType.AI, device, "<oversample_bits")
    }
    /**
     * Input voltage, in volts
     */
    public static setVoltage(device: string, voltage: number) {
        return SimGeneric.set(SimType.AI, device, ">voltage", voltage)
    }
    /**
     * If the accumulator is initialized in the robot program
     */
    public static getAccumInit(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_init")
    }
    /**
     * The accumulated value
     */
    public static setAccumValue(device: string, accumValue: number) {
        return SimGeneric.set(SimType.AI, device, ">accum_value", accumValue)
    }
    /**
     * The number of accumulated values
     */
    public static setAccumCount(device: string, accumCount: number) {
        return SimGeneric.set(SimType.AI, device, ">accum_count", accumCount)
    }
    /**
     * The center value of the accumulator
     */
    public static getAccumCenter(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_center")
    }
    /**
     * The accumulator's deadband
     */
    public static getAccumDeadband(device: string) {
        return SimGeneric.get(SimType.AI, device, "<accum_deadband")
    }
}

export class SimAO {
    constructor() {}

    public static getVoltage(device: string): number {
        return SimGeneric.get(SimType.AI, device, ">voltage", 0.0)
    }
}

type WSMessage = {
    type: string // might be a SimType
    device: string // device name
    data: Map<string, number>
}

worker.getValue().addEventListener("message", (eventData: MessageEvent) => {
    let data: WSMessage | undefined

    if (eventData.data.status) {
        switch (eventData.data.status) {
            case "open":
                isConnected = true
                break
            case "close":
            case "error":
                isConnected = false
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
        } catch (e) {
            console.error(`Failed to parse data:\n${JSON.stringify(eventData.data)}`)
            return
        }
    }

    if (!data?.type || !(Object.values(SimType) as string[]).includes(data.type)) return

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

    Object.entries(updateData).forEach(([key, value]) => currentData.set(key, value))

    window.dispatchEvent(new SimMapUpdateEvent(false))
}

class WPILibBrain extends Brain {
    private _simLayer: SimulationLayer
    private _assembly: MirabufSceneObject

    private _simOutputs: SimOutput[] = []
    private _simInputs: SimInput[] = []
    private _simFlows: SimFlow[] = []

    public get assemblyName() {
        return this._assembly.assemblyName
    }

    constructor(assembly: MirabufSceneObject) {
        super(assembly.mechanism, "wpilib")

        this._assembly = assembly

        this._simLayer = World.simulationSystem.getSimulationLayer(this._mechanism)!

        if (!this._simLayer) {
            console.warn("SimulationLayer is undefined")
            return
        }

        this.addSimInput(new SimGyroInput("Test Gyro[1]", this._mechanism))
        this.addSimInput(new SimAccelInput("ADXL362[4]", this._mechanism))
        this.addSimInput(new SimDigitalInput("SYN DI[0]", () => random() > 0.5))
        this.addSimOutput(new SimDigitalOutput("SYN DO[1]"))
        this.addSimInput(new SimAnalogInput("SYN AI[0]", () => random() * 12))
        this.addSimOutput(new SimAnalogOutput("SYN AO[1]"))

        this.loadSimConfig()

        World.sceneRenderer.sceneObjects.forEach(v => {
            if (v instanceof MirabufSceneObject && v.brain?.brainType == "wpilib") {
                v.brain = new SynthesisBrain(v, v.assemblyName)
            }
        })
    }

    public addSimOutput(device: SimOutput) {
        this._simOutputs.push(device)
    }

    public addSimInput(input: SimInput) {
        this._simInputs.push(input)
    }

    public addSimFlow(flow: SimFlow): boolean {
        if (validate(flow.supplier, flow.receiver)) {
            this._simFlows.push(flow)
            return true
        }
        return false
    }

    public loadSimConfig(): boolean {
        this._simFlows = []
        const configData = this._assembly.simConfigData
        if (!configData) return false

        const flows = SimConfig.compile(configData, this._assembly)
        if (!flows) {
            console.error(`Failed to compile saved simulation configuration data for '${this.assemblyName}'`)
            return false
        }

        let counter = 0
        flows.forEach(x => {
            if (!this.addSimFlow(x)) {
                console.debug("Failed to validate flow, skipping...")
            } else {
                counter++
            }
        })
        console.debug(`${counter} Flows added!`)
        return true
    }

    public update(deltaT: number): void {
        this._simOutputs.forEach(d => d.update(deltaT))
        this._simInputs.forEach(i => i.update(deltaT))
        this._simFlows.forEach(({ supplier, receiver }) => {
            receiver.setReceiverValue(supplier.getSupplierValue())
        })
    }

    public enable(): void {
        setSimBrain(this)
        // worker.getValue().postMessage({ command: "enable", reconnect: RECONNECT })
    }

    public disable(): void {
        if (simBrain == this) {
            setSimBrain(undefined)
        }
        // worker.getValue().postMessage({ command: "disable" })
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
