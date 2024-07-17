import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"
import Behavior from "../behavior/Behavior"
import World from "@/systems/World"
import WheelDriver from "../driver/WheelDriver"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import ArcadeDriveBehavior from "../behavior/ArcadeDriveBehavior"
import { SimulationLayer } from "../SimulationSystem"
import Jolt from "@barclah/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import HingeDriver from "../driver/HingeDriver"
import HingeStimulus from "../stimulus/HingeStimulus"
import GenericArmBehavior from "../behavior/GenericArmBehavior"
import SliderDriver from "../driver/SliderDriver"
import SliderStimulus from "../stimulus/SliderStimulus"
import GenericElevatorBehavior from "../behavior/GenericElevatorBehavior"
import { AxisInput, ButtonInput, Input } from "@/systems/input/InputSystem"
import { InputScheme } from "@/systems/input/InputSchemeManager"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

class SynthesisBrain extends Brain {
    private _behaviors: Behavior[] = []
    private _simLayer: SimulationLayer

    // Tracks how many joins have been made with unique controls
    private _currentJointIndex = 1

    private _assemblyName: string
    private _brainIndex: number

    // Tracks the number of each specific mira file spawned
    // public static numberRobotsSpawned: { [key: string]: number } = {}

    // A list of all the robots spawned including their assembly index
    // public static robotsSpawned: string[] = []

    // The total number of robots spawned
    // public static currentBrainIndex: number = 0

    public static brainIndexMap = new Map<number, SynthesisBrain>()

    public constructor(mechanism: Mechanism, assemblyName: string) {
        super(mechanism)

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!
        this._assemblyName = assemblyName

        this._brainIndex = SynthesisBrain.brainIndexMap.size
        SynthesisBrain.brainIndexMap.set(this._brainIndex, this)

        console.log(SynthesisBrain.brainIndexMap)

        if (!this._simLayer) {
            console.log("SimulationLayer is undefined")
            return
        }

        // Only adds controls to mechanisms that are controllable (ignores fields)
        if (mechanism.controllable) {
            /* if (SynthesisBrain.numberRobotsSpawned[assemblyName] == undefined)
                SynthesisBrain.numberRobotsSpawned[assemblyName] = 0
            else SynthesisBrain.numberRobotsSpawned[assemblyName]++

            this._assemblyIndex = SynthesisBrain.numberRobotsSpawned[assemblyName]
            SynthesisBrain.robotsSpawned.push(this.getNumberedAssemblyName()) */
            this.configureArcadeDriveBehavior()
            this.configureArmBehaviors()
            this.configureElevatorBehaviors()

            window.addEventListener("brainConfigured", this.configureInputs as EventListener)
            // console.log(this._assemblyIndex)
        } else {
            this.configureField()
        }
    }

    public Enable(): void {}

    public Update(deltaT: number): void {
        this._behaviors.forEach(b => b.Update(deltaT))
    }

    public Disable(): void {
        this._behaviors = []
    }

    public clearControls(): void {
        // TODO
        throw new Error("Not Implemented")
        // const index = SynthesisBrain.robotsSpawned.indexOf(this.getNumberedAssemblyName())
        // SynthesisBrain.robotsSpawned.splice(index, 1)
    }

    // Creates an instance of ArcadeDriveBehavior and automatically configures it
    private configureArcadeDriveBehavior() {
        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]
        const wheelStimuli: WheelRotationStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof WheelRotationStimulus
        ) as WheelRotationStimulus[]

        // Two body constraints are part of wheels and are used to determine which way a wheel is facing
        const fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints
            .filter(mechConstraint => mechConstraint.constraint instanceof JOLT.TwoBodyConstraint)
            .map(mechConstraint => mechConstraint.constraint as Jolt.TwoBodyConstraint)

        const leftWheels: WheelDriver[] = []
        const leftStimuli: WheelRotationStimulus[] = []

        const rightWheels: WheelDriver[] = []
        const rightStimuli: WheelRotationStimulus[] = []

        // Determines which wheels and stimuli belong to which side of the robot
        for (let i = 0; i < wheelDrivers.length; i++) {
            const wheelPos = fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation()

            const robotCOM = World.PhysicsSystem.GetBody(
                this._mechanism.constraints[0].childBody
            ).GetCenterOfMassPosition() as Jolt.Vec3
            const rightVector = new JOLT.Vec3(1, 0, 0)

            const dotProduct = rightVector.Dot(wheelPos.Sub(robotCOM))

            if (dotProduct < 0) {
                rightWheels.push(wheelDrivers[i])
                rightStimuli.push(wheelStimuli[i])
            } else {
                leftWheels.push(wheelDrivers[i])
                leftStimuli.push(wheelStimuli[i])
            }
        }

        this._behaviors.push(
            new ArcadeDriveBehavior(leftWheels, rightWheels, leftStimuli, rightStimuli, this._brainIndex)
        )
    }

    // Creates instances of ArmBehavior and automatically configures them
    private configureArmBehaviors() {
        const hingeDrivers: HingeDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof HingeDriver
        ) as HingeDriver[]
        const hingeStimuli: HingeStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof HingeStimulus
        ) as HingeStimulus[]

        for (let i = 0; i < hingeDrivers.length; i++) {
            this._behaviors.push(
                new GenericArmBehavior(hingeDrivers[i], hingeStimuli[i], this._currentJointIndex, this._brainIndex)
            )
            this._currentJointIndex++
        }
    }

    // Creates instances of ElevatorBehavior and automatically configures them
    private configureElevatorBehaviors() {
        const sliderDrivers: SliderDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof SliderDriver
        ) as SliderDriver[]
        const sliderStimuli: SliderStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof SliderStimulus
        ) as SliderStimulus[]

        for (let i = 0; i < sliderDrivers.length; i++) {
            this._behaviors.push(
                new GenericElevatorBehavior(
                    sliderDrivers[i],
                    sliderStimuli[i],
                    this._currentJointIndex,
                    this._brainIndex
                )
            )
            this._currentJointIndex++
        }
    }

    private configureInputs(/* e: BrainConfiguredEvent */) {
        // window.removeEventListener("brainConfigured", this.configureInputs as EventListener)
        // console.log(this._assemblyIndex + " " + this._assemblyName + " configured with " + e.schemeName)
        // // Check for existing inputs
        // const robotConfig = PreferencesSystem.getRobotPreferences(this._assemblyName)
        // if (robotConfig.inputsSchemes[this._assemblyIndex] != undefined) {
        //     SynthesisBrain.parseInputs(robotConfig.inputsSchemes[this._assemblyIndex])
        //     return
        // }
        // // Configure with default inputs
        // const scheme = DefaultInputs.AVAILABLE_INPUT_SCHEMES[SynthesisBrain._currentBrainIndex]
        // robotConfig.inputsSchemes[this._assemblyIndex] = {
        //     schemeName: this._assemblyName,
        //     descriptiveName: "",
        //     customized: false,
        //     usesGamepad: scheme?.usesGamepad ?? false,
        //     inputs: [],
        // }
        // const inputList = robotConfig.inputsSchemes[this._assemblyIndex].inputs
        // if (scheme) {
        //     const arcadeDrive = scheme.inputs.find(i => i.inputName === "arcadeDrive")
        //     if (arcadeDrive) inputList.push(arcadeDrive.getCopy())
        //     else inputList.push(new AxisInput("arcadeDrive"))
        //     const arcadeTurn = scheme.inputs.find(i => i.inputName === "arcadeTurn")
        //     if (arcadeTurn) inputList.push(arcadeTurn.getCopy())
        //     else inputList.push(new AxisInput("arcadeTurn"))
        //     for (let i = 1; i < this._currentJointIndex; i++) {
        //         const controlPreset = scheme.inputs.find(input => input.inputName == "joint " + i)
        //         if (controlPreset) inputList.push(controlPreset.getCopy())
        //         else inputList.push(new AxisInput("joint " + i))
        //     }
        // }
    }

    private configureField() {
        const fieldPrefs = PreferencesSystem.getFieldPreferences(this._assemblyName)
        console.log("Loaded field prefs " + fieldPrefs)

        /** Put any scoring zone or other field configuration here */
    }

    /* private getNumberedAssemblyName(): string {
        return `[${this._assemblyIndex}] ${this._assemblyName}`
    } */
}

export default SynthesisBrain
