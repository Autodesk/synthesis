import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Brain from "../Brain"
import { compile } from "../wiring/Compile"
import type { SimulationLayer } from "../SimulationSystem"
import SynthesisBrain from "../synthesis_brain/SynthesisBrain"
import { type SimFlow, validate } from "./SimDataFlow"
import type { SimInput } from "./SimInput"
import type { SimOutput } from "./SimOutput"
import { getSimBrain, getSimMap, setConnected, setSimBrain } from "./WPILibState"
import { type DeviceData, type SimType, type WSMessage, worker } from "./WPILibTypes"
import SimDriverStation from "./sim/SimDriverStation"

worker.getValue().addEventListener("message", (eventData: MessageEvent) => {
    let data: WSMessage | undefined

    if (eventData.data.status) {
        switch (eventData.data.status) {
            case "open":
                setConnected(true)
                SimDriverStation.setDsAttached(true)
                break
            case "close":
                setConnected(false)
                SimDriverStation.setDsAttached(false)
                break
            case "error":
                setConnected(false)
                SimDriverStation.setDsAttached(false)
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

    Object.entries(updateData).forEach(([key, value]) => currentData.set(key, value))
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

    public get assemblyId() {
        return this._assembly.assemblyId
    }

    private _brainType: "wpilib" | "ftc"

    public override get brainType() {
        return this._brainType
    }

    constructor(assembly: MirabufSceneObject, brainType: "wpilib" | "ftc" = "wpilib") {
        super(assembly.mechanism)

        this._assembly = assembly
        this._brainType = brainType

        this._simLayer = World.simulationSystem.getSimulationLayer(this._mechanism)!

        if (!this._simLayer) {
            return
        }

        // TODO: support these
        // this.addSimInput(new SimDigitalInput("SYN DI[0]", () => random() > 0.5))
        // this.addSimOutput(new SimDigitalOutput("SYN DO[1]"))
        // this.addSimInput(new SimAnalogInput("SYN AI[0]", () => random() * 12))
        // this.addSimOutput(new SimAnalogOutput("SYN AO[1]"))

        this.loadSimConfig()

        World.sceneRenderer.mirabufSceneObjects.getRobots().forEach(v => {
            if (v.brain?.isWPILib() || v.brain?.isFTC()) {
                v.brain = new SynthesisBrain(v)
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

        const { flows, error } = compile(configData, this._assembly)
        if (!flows) {
            console.error(`Failed to compile saved simulation configuration data for '${this.assemblyName}': ${error}`)
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
        worker.getValue().postMessage({ command: "enable", reconnect: true })
    }

    public disable(): void {
        if (getSimBrain() == this) {
            setSimBrain(undefined)
        }
        worker.getValue().postMessage({ command: "disable" })
    }
}

export default WPILibBrain
