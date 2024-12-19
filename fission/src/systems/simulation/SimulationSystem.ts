import JOLT from "@/util/loading/JoltSyncLoader"
import Mechanism from "../physics/Mechanism"
import WorldSystem from "../WorldSystem"
import Brain from "./Brain"
import Driver, { DriverType, makeDriverID } from "./driver/Driver"
import Stimulus, { makeStimulusID, StimulusType } from "./stimulus/Stimulus"
import HingeDriver from "./driver/HingeDriver"
import WheelDriver from "./driver/WheelDriver"
import SliderDriver from "./driver/SliderDriver"
import HingeStimulus from "./stimulus/HingeStimulus"
import WheelRotationStimulus from "./stimulus/WheelStimulus"
import SliderStimulus from "./stimulus/SliderStimulus"
import ChassisStimulus from "./stimulus/ChassisStimulus"
import IntakeDriver from "./driver/IntakeDriver"
import World from "../World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EjectorDriver from "./driver/EjectorDriver"

class SimulationSystem extends WorldSystem {
    private _simMechanisms: Map<Mechanism, SimulationLayer>

    public static redScore = 0
    public static blueScore = 0

    constructor() {
        super()

        this._simMechanisms = new Map()
    }

    public RegisterMechanism(mechanism: Mechanism) {
        if (this._simMechanisms.has(mechanism)) return

        this._simMechanisms.set(mechanism, new SimulationLayer(mechanism))
    }

    public GetSimulationLayer(mechanism: Mechanism): SimulationLayer | undefined {
        return this._simMechanisms.get(mechanism)
    }

    public Update(deltaT: number): void {
        this._simMechanisms.forEach(simLayer => simLayer.Update(deltaT))
    }

    public Destroy(): void {
        this._simMechanisms.forEach(simLayer => simLayer.SetBrain(undefined))
        this._simMechanisms.clear()
    }

    public UnregisterMechanism(mech: Mechanism): boolean {
        const layer = this._simMechanisms.get(mech)
        if (this._simMechanisms.delete(mech)) {
            layer?.SetBrain(undefined)
            return true
        } else {
            return false
        }
    }

    public static ResetScores(): void {
        SimulationSystem.redScore = 0
        SimulationSystem.blueScore = 0
    }
}

class SimulationLayer {
    private _mechanism: Mechanism
    private _brain?: Brain

    private _drivers: Map<string, Driver>
    private _stimuli: Map<string, Stimulus>

    public get brain() {
        return this._brain
    }
    public get drivers() {
        return [...this._drivers.values()]
    }
    public get stimuli() {
        return [...this._stimuli.values()]
    }

    constructor(mechanism: Mechanism) {
        this._mechanism = mechanism

        const assembly = [...World.SceneRenderer.sceneObjects.values()].find(
            x => (x as MirabufSceneObject).mechanism == mechanism
        ) as MirabufSceneObject

        // Generate standard drivers and stimuli
        this._drivers = new Map()
        this._stimuli = new Map()
        this._mechanism.constraints.forEach(x => {
            if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Hinge) {
                const hinge = JOLT.castObject(x.constraint, JOLT.HingeConstraint)
                const driver = new HingeDriver(makeDriverID(x), hinge, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new HingeStimulus(makeStimulusID(x), hinge, x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            } else if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Vehicle) {
                const vehicle = JOLT.castObject(x.constraint, JOLT.VehicleConstraint)
                const driver = new WheelDriver(makeDriverID(x), vehicle, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new WheelRotationStimulus(makeStimulusID(x), vehicle.GetWheel(0), x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            } else if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Slider) {
                const slider = JOLT.castObject(x.constraint, JOLT.SliderConstraint)
                const driver = new SliderDriver(makeDriverID(x), slider, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new SliderStimulus(makeStimulusID(x), slider, x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            }
        })

        const chassisStim = new ChassisStimulus(
            { type: StimulusType.Stim_ChassisAccel, guid: "CHASSIS_GUID" },
            mechanism.nodeToBody.get(mechanism.rootBody)!,
            { GUID: "CHASSIS_GUID", name: "Chassis" }
        )
        this._stimuli.set(JSON.stringify(chassisStim.id), chassisStim)

        if (assembly) {
            const intakeDriv = new IntakeDriver({ type: DriverType.Driv_Intake, guid: "INTAKE_GUID" }, assembly, {
                GUID: "INTAKE_GUID",
                name: "Intake",
            })
            const ejectorDriv = new EjectorDriver({ type: DriverType.Driv_Ejector, guid: "EJECTOR_GUID" }, assembly, {
                GUID: "EJECTOR_GUID",
                name: "Ejector",
            })
            this._drivers.set(JSON.stringify(ejectorDriv.id), ejectorDriv)
            this._drivers.set(JSON.stringify(intakeDriv.id), intakeDriv)
        } else {
            console.debug("No Assembly found with given mechanism, skipping intake and ejector...")
        }
    }

    public Update(deltaT: number) {
        this._brain?.Update(deltaT)
        this._drivers.forEach(x => x.Update(deltaT))
        this._stimuli.forEach(x => x.Update(deltaT))
    }

    public SetBrain<T extends Brain>(brain: T | undefined) {
        if (this._brain) this._brain.Disable()

        this._brain = brain

        if (this._brain) this._brain.Enable()
    }

    public GetStimuli(id: string) {
        return this._stimuli.get(id)
    }

    public GetDriver(id: string) {
        return this._drivers.get(id)
    }
}

export default SimulationSystem
export { SimulationLayer }
