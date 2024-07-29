import JOLT from "@/util/loading/JoltSyncLoader"
import Mechanism from "../physics/Mechanism"
import WorldSystem from "../WorldSystem"
import Brain from "./Brain"
import Driver from "./driver/Driver"
import Stimulus from "./stimulus/Stimulus"
import HingeDriver from "./driver/HingeDriver"
import WheelDriver from "./driver/WheelDriver"
import SliderDriver from "./driver/SliderDriver"
import HingeStimulus from "./stimulus/HingeStimulus"
import WheelRotationStimulus from "./stimulus/WheelStimulus"
import SliderStimulus from "./stimulus/SliderStimulus"
import ChassisStimulus from "./stimulus/ChassisStimulus"

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

    private _drivers: Driver[]
    private _stimuli: Stimulus[]

    public get brain() {
        return this._brain
    }
    public get drivers() {
        return this._drivers
    }
    public get stimuli() {
        return this._stimuli
    }

    constructor(mechanism: Mechanism) {
        this._mechanism = mechanism

        // Generate standard drivers and stimuli
        this._drivers = []
        this._stimuli = []
        this._mechanism.constraints.forEach(x => {
            if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Hinge) {
                const hinge = JOLT.castObject(x.constraint, JOLT.HingeConstraint)
                const driver = new HingeDriver(hinge, x.info)
                this._drivers.push(driver)
                const stim = new HingeStimulus(hinge, x.info)
                this._stimuli.push(stim)
            } else if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Vehicle) {
                const vehicle = JOLT.castObject(x.constraint, JOLT.VehicleConstraint)
                const driver = new WheelDriver(vehicle, x.info)
                this._drivers.push(driver)
                const stim = new WheelRotationStimulus(vehicle.GetWheel(0), x.info)
                this._stimuli.push(stim)
            } else if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Slider) {
                const slider = JOLT.castObject(x.constraint, JOLT.SliderConstraint)
                const driver = new SliderDriver(slider, x.info)
                this._drivers.push(driver)
                const stim = new SliderStimulus(slider, x.info)
                this._stimuli.push(stim)
            }
        })
        this._stimuli.push(new ChassisStimulus(mechanism.nodeToBody.get(mechanism.rootBody)!))
    }

    public Update(deltaT: number) {
        this._brain?.Update(deltaT)
        this._drivers.forEach(x => x.Update(deltaT))
        this._stimuli.forEach(x => x.Update(deltaT))
    }

    public SetBrain<T extends Brain>(brain: T | undefined) {
        if (this._brain) this._brain.Disable()

        this._brain = brain

        if (this._brain) {
            this._brain.Enable()
        }
    }
}

export default SimulationSystem
export { SimulationLayer }
