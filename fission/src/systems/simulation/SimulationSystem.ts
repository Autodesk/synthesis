import World from "@/systems/World.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import type Mechanism from "../physics/Mechanism"
import WorldSystem from "../WorldSystem"
import type Brain from "./Brain"
import type Driver from "./driver/Driver"
import { DriverType, makeDriverID } from "./driver/Driver"
import EjectorDriver from "./driver/EjectorDriver"
import HingeDriver from "./driver/HingeDriver"
import IntakeDriver from "./driver/IntakeDriver"
import SliderDriver from "./driver/SliderDriver"
import WheelDriver from "./driver/WheelDriver"
import ChassisStimulus from "./stimulus/ChassisStimulus"
import HingeStimulus from "./stimulus/HingeStimulus"
import SliderStimulus from "./stimulus/SliderStimulus"
import type Stimulus from "./stimulus/Stimulus"
import { makeStimulusID, StimulusType } from "./stimulus/Stimulus"
import WheelRotationStimulus from "./stimulus/WheelStimulus"

class SimulationSystem extends WorldSystem {
    private _simMechanisms: Map<Mechanism, SimulationLayer>

    constructor() {
        super()

        this._simMechanisms = new Map()
    }

    public registerMechanism(mechanism: Mechanism) {
        if (this._simMechanisms.has(mechanism)) return

        this._simMechanisms.set(mechanism, new SimulationLayer(mechanism))
    }

    public getSimulationLayer(mechanism: Mechanism): SimulationLayer | undefined {
        return this._simMechanisms.get(mechanism)
    }

    public update(deltaT: number): void {
        this._simMechanisms.forEach(simLayer => simLayer.update(deltaT))
    }

    public destroy(): void {
        this._simMechanisms.forEach(simLayer => simLayer.setBrain(undefined))
        this._simMechanisms.clear()
    }

    public unregisterMechanism(mech: Mechanism): boolean {
        const layer = this._simMechanisms.get(mech)
        if (this._simMechanisms.delete(mech)) {
            layer?.setBrain(undefined)
            return true
        } else {
            return false
        }
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

        const assembly = World.sceneRenderer.mirabufSceneObjects.findWhere(obj => obj.mechanism == mechanism)

        // Generate standard drivers and stimuli
        this._drivers = new Map()
        this._stimuli = new Map()
        this._mechanism.constraints.forEach(x => {
            if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Hinge) {
                const hinge = JOLT.castObject(x.primaryConstraint, JOLT.HingeConstraint)
                const driver = new HingeDriver(makeDriverID(x), hinge, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new HingeStimulus(makeStimulusID(x), hinge, x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            } else if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Vehicle) {
                const vehicle = JOLT.castObject(x.primaryConstraint, JOLT.VehicleConstraint)
                const driver = new WheelDriver(makeDriverID(x), vehicle, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new WheelRotationStimulus(makeStimulusID(x), vehicle.GetWheel(0), x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            } else if (x.primaryConstraint.GetSubType() == JOLT.EConstraintSubType_Slider) {
                const slider = JOLT.castObject(x.primaryConstraint, JOLT.SliderConstraint)
                const driver = new SliderDriver(makeDriverID(x), slider, x.maxVelocity, x.info)
                this._drivers.set(JSON.stringify(driver.id), driver)
                const stim = new SliderStimulus(makeStimulusID(x), slider, x.info)
                this._stimuli.set(JSON.stringify(stim.id), stim)
            }
        })

        const chassisStim = new ChassisStimulus(
            { type: StimulusType.STIM_CHASSIS_ACCEL, guid: "CHASSIS_GUID" },
            mechanism.nodeToBody.get(mechanism.rootBody)!,
            { GUID: "CHASSIS_GUID", name: "Chassis" }
        )
        this._stimuli.set(JSON.stringify(chassisStim.id), chassisStim)

        if (assembly) {
            const intakeDriv = new IntakeDriver({ type: DriverType.INTAKE, guid: "INTAKE_GUID" }, assembly, {
                GUID: "INTAKE_GUID",
                name: "Intake",
            })
            const ejectorDriv = new EjectorDriver({ type: DriverType.EJECTOR, guid: "EJECTOR_GUID" }, assembly, {
                GUID: "EJECTOR_GUID",
                name: "Ejector",
            })
            this._drivers.set(JSON.stringify(ejectorDriv.id), ejectorDriv)
            this._drivers.set(JSON.stringify(intakeDriv.id), intakeDriv)
        } else {
            console.debug("No Assembly found with given mechanism, skipping intake and ejector...")
        }
    }

    public update(deltaT: number) {
        this._brain?.update(deltaT)
        this._drivers.forEach(x => x.update(deltaT))
        this._stimuli.forEach(x => x.update(deltaT))
    }

    public setBrain<T extends Brain>(brain: T | undefined) {
        if (this._brain) this._brain.disable()

        this._brain = brain

        if (this._brain) this._brain.enable()
    }

    public getStimuli(id: string) {
        return this._stimuli.get(id)
    }

    public getDriver(id: string) {
        return this._drivers.get(id)
    }
}

export default SimulationSystem
export { SimulationLayer }
