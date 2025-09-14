import type Jolt from "@azaleacolburn/jolt-physics"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { defaultSequentialConfig } from "@/systems/preferences/PreferenceTypes"
import SkidSteerDriveBehavior from "@/systems/simulation/behavior/synthesis/drive/SkidSteerDriveBehavior.ts"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltVec3ToJoltRVec3 } from "@/util/TypeConversions"
import Brain from "../Brain"
import type Behavior from "../behavior/Behavior"
import { DriveType } from "../behavior/Behavior"
import GamepieceManipBehavior from "../behavior/synthesis/GamepieceManipBehavior"
import GenericArmBehavior from "../behavior/synthesis/GenericArmBehavior"
import GenericElevatorBehavior from "../behavior/synthesis/GenericElevatorBehavior"
import EjectorDriver from "../driver/EjectorDriver"
import HingeDriver from "../driver/HingeDriver"
import IntakeDriver from "../driver/IntakeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
import type { SimulationLayer } from "../SimulationSystem"
import HingeStimulus from "../stimulus/HingeStimulus"
import SliderStimulus from "../stimulus/SliderStimulus"
import WheelRotationStimulus from "../stimulus/WheelStimulus"

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

    // Track previous unstick button state to detect button press (not hold)
    private _prevUnstickPressed = false

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

    public configureDriveBehavior(driveType: DriveType) {
        this.driveType = driveType
        const existing = this._behaviors.find((behavior: Behavior) => behavior instanceof SkidSteerDriveBehavior)
        if (existing == null) {
            console.error("Can't find drive behavior!")
            return
        }
        existing.setIsArcade(driveType == DriveType.ARCADE)
    }

    public configure(): void {
        this._behaviors = []
        // Only adds controls to mechanisms that are controllable (ignores fields)
        if (this._assembly.mechanism.controllable) {
            this.configureSkidSteerDriveBehavior(this.driveType == DriveType.ARCADE)
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
    public constructor(assembly: MirabufSceneObject, assemblyName: string) {
        super(assembly.mechanism, "synthesis")
        this._assembly = assembly
        this._simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)!
        this._assemblyName = assemblyName

        // I'm not fixing this right now, but this is going to become an issue...
        this._brainIndex = SynthesisBrain.brainIndexMap.size
        SynthesisBrain.brainIndexMap.set(this._brainIndex, this)

        if (!this._simLayer) {
            console.error("SimulationLayer is undefined")
            return
        }

        this.configure()
    }

    public enable(): void {}

    public update(deltaT: number): void {
        this._behaviors.forEach(b => b.update(deltaT))

        this._assembly.ejectorActive = InputSystem.getInput("eject", this._brainIndex) > 0.5
        this._assembly.intakeActive = InputSystem.getInput("intake", this._brainIndex) > 0.5

        // Handle unstick
        const unstickPressed = InputSystem.getInput("unstick", this._brainIndex) === 1
        if (unstickPressed && !this._prevUnstickPressed) {
            this.applyUnstickForce()
        }

        this._prevUnstickPressed = unstickPressed
    }

    /**
     * Applies a small upward force to the robot's main body to help unstick it
     */
    private applyUnstickForce(): void {
        const rootBodyId = this._mechanism.getBodyByNodeId(this._mechanism.rootBody)
        if (!rootBodyId) {
            console.warn("Could not find root body for unstick")
            return
        }

        const body = World.physicsSystem.getBody(rootBodyId)
        if (!body) {
            console.warn("Could not get body for unstick")
            return
        }

        const unstickForce = new JOLT.Vec3(0, PreferencesSystem.getRobotPreferences(this._assemblyName).unstickForce, 0)
        body.AddForce(unstickForce)
    }

    public disable(): void {
        this.clearControls()
        this._behaviors = []
    }

    public clearControls(): void {
        InputSystem.brainIndexSchemeMap.delete(this._brainIndex)
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
            const wheelPos = convertJoltVec3ToJoltRVec3(
                fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation()
            )

            const robotCOM = World.physicsSystem
                .getBody(this._mechanism.constraints[0].childBody)
                .GetCenterOfMassPosition()
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
                sequentialConfig = defaultSequentialConfig(this._currentJointIndex, "Arm")

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
                sequentialConfig = defaultSequentialConfig(this._currentJointIndex, "Elevator")

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

    public static getBrainIndex(assembly: MirabufSceneObject | undefined): number | undefined {
        return (assembly?.brain as SynthesisBrain)?.brainIndex
    }
}

export default SynthesisBrain
