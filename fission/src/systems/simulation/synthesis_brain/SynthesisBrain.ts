import Brain from "../Brain"
import Behavior, { DriveType } from "../behavior/Behavior"
import World from "@/systems/World"
import WheelDriver from "../driver/WheelDriver"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import { SimulationLayer } from "../SimulationSystem"
import Jolt from "@barclah/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import HingeDriver from "../driver/HingeDriver"
import HingeStimulus from "../stimulus/HingeStimulus"
import GenericArmBehavior from "../behavior/synthesis/GenericArmBehavior"
import SliderDriver from "../driver/SliderDriver"
import SliderStimulus from "../stimulus/SliderStimulus"
import GenericElevatorBehavior from "../behavior/synthesis/GenericElevatorBehavior"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { DefaultSequentialConfig } from "@/systems/preferences/PreferenceTypes"
import InputSystem from "@/systems/input/InputSystem"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import IntakeDriver from "../driver/IntakeDriver"
import EjectorDriver from "../driver/EjectorDriver"
import GamepieceManipBehavior from "../behavior/synthesis/GamepieceManipBehavior"
import { JoltVec3_JoltRVec3 } from "@/util/TypeConversions"
import SkidSteerDriveBehavior from "@/systems/simulation/behavior/synthesis/drive/SkidSteerDriveBehavior.ts"
import { Global_AddToast } from "@/components/GlobalUIControls.ts"

class SynthesisBrain extends Brain {
    public static brainIndexMap = new Map<number, SynthesisBrain>()

    private _behaviors: Behavior[] = []
    private _simLayer: SimulationLayer
    private _assemblyName: string
    private _brainIndex: number
    private _assembly: MirabufSceneObject
    public driveType: DriveType = DriveType.ARCADE

    // Tracks how many joins have been made with unique controls
    private _currentJointIndex = 1

    public get assemblyName(): string {
        return this._assemblyName
    }

    public get behaviors(): Behavior[] {
        return this._behaviors
    }

    // Tracks the number of each specific mira file spawned
    public static numberRobotsSpawned: { [key: string]: number } = {}

    /** @returns {string} The name of the input scheme attached to this brain. */
    public get inputSchemeName(): string {
        const scheme = InputSystem.brainIndexSchemeMap.get(this._brainIndex)
        if (scheme == undefined) return "Not Configured"

        return scheme.schemeName
    }

    /** @returns {number} The unique index used to identify this brain. */
    public get brainIndex(): number {
        return this._brainIndex
    }

    public configure(driveType: DriveType): void {
        this.driveType = driveType
        this._behaviors = []
        // Only adds controls to mechanisms that are controllable (ignores fields)
        if (this._assembly.mechanism.controllable) {
            switch (driveType) {
                case DriveType.ARCADE:
                    this.configureSkidSteerDriveBehavior(true)
                    break
                case DriveType.TANK:
                    this.configureSkidSteerDriveBehavior(false)
                    break
                case DriveType.SWERVE:
                    this.configureSwerveDriveBehavior()
                    break
            }
            this.configureArmBehaviors()
            this.configureElevatorBehaviors()
            this.configureGamepieceManipBehavior()
        } else {
            this.configureField()
        }
    }

    /**
     * @param assembly
     * @param assemblyName The name of the assembly that corresponds to the mechanism used for identification.
     * @param driveType
     */
    public constructor(assembly: MirabufSceneObject, assemblyName: string, driveType: DriveType = DriveType.ARCADE) {
        super(assembly.mechanism, "synthesis")
        this._assembly = assembly
        this._simLayer = World.SimulationSystem.GetSimulationLayer(assembly.mechanism)!
        this._assemblyName = assemblyName

        // I'm not fixing this right now, but this is going to become an issue...
        this._brainIndex = SynthesisBrain.brainIndexMap.size
        SynthesisBrain.brainIndexMap.set(this._brainIndex, this)

        if (!this._simLayer) {
            console.error("SimulationLayer is undefined")
            return
        }

        this.configure(driveType)
    }

    public Enable(): void {}

    public Update(deltaT: number): void {
        this._behaviors.forEach(b => b.Update(deltaT))

        this._assembly.ejectorActive = InputSystem.getInput("eject", this._brainIndex) > 0.5
        this._assembly.intakeActive = InputSystem.getInput("intake", this._brainIndex) > 0.5
    }

    public Disable(): void {
        this.clearControls()
        this._behaviors = []
    }

    public clearControls(): void {
        InputSystem.brainIndexSchemeMap.delete(this._brainIndex)
    }

    private configureSwerveDriveBehavior(): void {
        Global_AddToast?.("error", "Swerve not supported", "check back soon")
    }
    /** Creates an instance of ArcadeDriveBehavior and automatically configures it. */
    private configureSkidSteerDriveBehavior(isArcade: boolean) {
        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]
        const wheelStimuli: WheelRotationStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof WheelRotationStimulus
        ) as WheelRotationStimulus[]

        // Two body constraints are part of wheels and are used to determine which way a wheel is facing
        const fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints
            .filter(mechConstraint => mechConstraint.primaryConstraint instanceof JOLT.TwoBodyConstraint)
            .map(mechConstraint => mechConstraint.primaryConstraint as Jolt.TwoBodyConstraint)

        const leftWheels: WheelDriver[] = []
        const leftStimuli: WheelRotationStimulus[] = []

        const rightWheels: WheelDriver[] = []
        const rightStimuli: WheelRotationStimulus[] = []

        // Determines which wheels and stimuli belong to which side of the robot
        for (let i = 0; i < wheelDrivers.length; i++) {
            const wheelPos = JoltVec3_JoltRVec3(fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation())

            const robotCOM = World.PhysicsSystem.GetBody(
                this._mechanism.constraints[0].childBody
            ).GetCenterOfMassPosition()
            const rightVector = new JOLT.RVec3(1, 0, 0)

            const dotProduct = rightVector.Dot(wheelPos.SubRVec3(robotCOM))

            if (dotProduct < 0) {
                rightWheels.push(wheelDrivers[i])
                rightStimuli.push(wheelStimuli[i])
            } else {
                leftWheels.push(wheelDrivers[i])
                leftStimuli.push(wheelStimuli[i])
            }
        }

        this._behaviors.push(
            new SkidSteerDriveBehavior(leftWheels, rightWheels, leftStimuli, rightStimuli, this._brainIndex, isArcade)
        )
    }

    /** Creates instances of ArmBehavior and automatically configures them. */
    private configureArmBehaviors() {
        const hingeDrivers: HingeDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof HingeDriver
        ) as HingeDriver[]
        const hingeStimuli: HingeStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof HingeStimulus
        ) as HingeStimulus[]

        for (let i = 0; i < hingeDrivers.length; i++) {
            let sequentialConfig = PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig?.find(
                sc => sc.jointIndex == this._currentJointIndex
            )

            if (sequentialConfig == undefined) {
                sequentialConfig = DefaultSequentialConfig(this._currentJointIndex, "Arm")

                if (PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig == undefined)
                    PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig = []

                PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig?.push(sequentialConfig)
                PreferencesSystem.savePreferences()
            }

            this._behaviors.push(
                new GenericArmBehavior(
                    hingeDrivers[i],
                    hingeStimuli[i],
                    this._currentJointIndex,
                    this._brainIndex,
                    sequentialConfig
                )
            )
            this._currentJointIndex++
        }
    }

    /** Creates instances of ElevatorBehavior and automatically configures them. */
    private configureElevatorBehaviors() {
        const sliderDrivers: SliderDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof SliderDriver
        ) as SliderDriver[]
        const sliderStimuli: SliderStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof SliderStimulus
        ) as SliderStimulus[]

        for (let i = 0; i < sliderDrivers.length; i++) {
            let sequentialConfig = PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig?.find(
                sc => sc.jointIndex == this._currentJointIndex
            )

            if (sequentialConfig == undefined) {
                sequentialConfig = DefaultSequentialConfig(this._currentJointIndex, "Elevator")

                if (PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig == undefined)
                    PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig = []

                PreferencesSystem.getRobotPreferences(this._assemblyName).sequentialConfig?.push(sequentialConfig)
                PreferencesSystem.savePreferences()
            }

            this._behaviors.push(
                new GenericElevatorBehavior(
                    sliderDrivers[i],
                    sliderStimuli[i],
                    this._currentJointIndex,
                    this._brainIndex,
                    sequentialConfig
                )
            )
            this._currentJointIndex++
        }
    }

    private configureGamepieceManipBehavior() {
        let intake: IntakeDriver | undefined = undefined
        let ejector: EjectorDriver | undefined = undefined
        this._simLayer.drivers.forEach(x => {
            if (x instanceof IntakeDriver) {
                intake = x
            } else if (x instanceof EjectorDriver) {
                ejector = x
            }
        })

        if (!intake || !ejector) return

        this._behaviors.push(new GamepieceManipBehavior(ejector, intake, this._brainIndex))
    }

    /** Gets field preferences and handles any field specific configuration. */
    private configureField() {
        PreferencesSystem.getFieldPreferences(this._assemblyName)

        /** Put any field configuration here */
    }

    public static GetBrainIndex(assembly: MirabufSceneObject | undefined): number | undefined {
        return (assembly?.brain as SynthesisBrain)?.brainIndex
    }
}

export default SynthesisBrain
