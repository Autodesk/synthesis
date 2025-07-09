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
import { OnScoreChangedEvent } from "@/mirabuf/ScoringZoneSceneObject"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

class SimulationSystem extends WorldSystem {
    private _simMechanisms: Map<Mechanism, SimulationLayer>
    public static perRobotScore: Map<MirabufSceneObject, number> = new Map()

    public static redScore = 0
    public static blueScore = 0

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

    public static resetScores(): void {
        SimulationSystem.redScore = 0
        SimulationSystem.blueScore = 0
        this.perRobotScore = new Map()
        new OnScoreChangedEvent(SimulationSystem.redScore, SimulationSystem.blueScore).dispatch()
    }

    public static addPerRobotScore(robot: MirabufSceneObject, scoreToAdd: number): void {
        const currentRobotScore = this.perRobotScore.get(robot) ?? 0
        this.perRobotScore.set(robot, currentRobotScore + scoreToAdd)
    }

    public static robotPenalty(robot: MirabufSceneObject, penaltyPoints: number, penaltyInfo: string): void {
        // Display a toast showing that a penalty was committed
        globalAddToast?.(
            "warning",
            "PENALTY COMMITTED",
            `Robot ${robot.nameTag?.text()} (${robot.assemblyName}), Committed Penalty: ${penaltyInfo}`
        )
        // Update match score
        if (robot.alliance == "red") {
            SimulationSystem.blueScore += penaltyPoints
        } else {
            SimulationSystem.redScore += penaltyPoints
        }
        new OnScoreChangedEvent(SimulationSystem.redScore, SimulationSystem.blueScore).dispatch()
        // Update per robot score
        this.addPerRobotScore(robot, -penaltyPoints)
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

        const assembly = [...World.sceneRenderer.sceneObjects.values()].find(
            x => (x as MirabufSceneObject).mechanism == mechanism
        ) as MirabufSceneObject

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
