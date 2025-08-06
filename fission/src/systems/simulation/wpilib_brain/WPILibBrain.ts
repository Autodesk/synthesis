import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { random } from "@/util/Random"
import Brain from "../Brain"
import type { SimulationLayer } from "../SimulationSystem"
import SynthesisBrain from "../synthesis_brain/SynthesisBrain"
import { type SimFlow, validate } from "./SimDataFlow"
import type { SimInput } from "./SimInput"
import { SimAnalogOutput, SimDigitalOutput, type SimOutput } from "./SimOutput"
import { SimAccelInput } from "./sim/SimAccel"
import { SimAnalogInput } from "./sim/SimAI"
import { SimDigitalInput } from "./sim/SimDIO"
import { SimGyroInput } from "./sim/SimGyro"
import { getSimBrain, getSimMap, setConnected, setSimBrain } from "./WPILibState"
import { type DeviceData, SimMapUpdateEvent, SimType, type WSMessage, worker } from "./WPILibTypes"

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

        World.sceneRenderer.mirabufSceneObjects.getRobots().forEach(v => {
            if (v.brain?.brainType == "wpilib") {
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

        // const flows = SimConfig.Compile(configData, this._assembly)
        // if (!flows) {
        //     console.error(`Failed to compile saved simulation configuration data for '${this.assemblyName}'`)
        //     return false
        // }

        // let counter = 0
        // flows.forEach(x => {
        //     if (!this.addSimFlow(x)) {
        //         console.debug("Failed to validate flow, skipping...")
        //     } else {
        //         counter++
        //     }
        // })
        // console.debug(`${counter} Flows added!`)
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
        if (getSimBrain() == this) {
            setSimBrain(undefined)
        }
        // worker.getValue().postMessage({ command: "disable" })
    }
}

export default WPILibBrain
