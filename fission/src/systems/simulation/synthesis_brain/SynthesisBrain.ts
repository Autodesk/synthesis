import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { defaultSequentialConfig } from "@/systems/preferences/PreferenceTypes"
import type { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import SkidSteerDriveBehavior from "@/systems/simulation/behavior/synthesis/drive/SkidSteerDriveBehavior.ts"
import SwerveDriveBehavior from "@/systems/simulation/behavior/synthesis/drive/SwerveDriveBehavior.ts"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltVec3ToJoltRVec3 } from "@/util/TypeConversions"
import Brain from "../Brain"
import type Behavior from "../behavior/Behavior"
import { DriveType } from "../behavior/Behavior"
import GamepieceManipBehavior from "../behavior/synthesis/GamepieceManipBehavior"
import GenericArmBehavior from "../behavior/synthesis/GenericArmBehavior"
import GenericElevatorBehavior from "../behavior/synthesis/GenericElevatorBehavior"
import { DriverControlMode } from "../driver/Driver"
import EjectorDriver from "../driver/EjectorDriver"
import HingeDriver from "../driver/HingeDriver"
import IntakeDriver from "../driver/IntakeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
import type { SimulationLayer } from "../SimulationSystem"
import HingeStimulus from "../stimulus/HingeStimulus"
import SliderStimulus from "../stimulus/SliderStimulus"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import { pairNearestHinges } from "./SwervePairing"

class SynthesisBrain extends Brain {
    public static brainIndexMap = new Map<number, SynthesisBrain>()

    /**
     * Max allowed perpendicular-to-up component of a hinge axis for it to count as a swerve
     * azimuth hinge. Ported verbatim from the original Unity swerve detection (0.05).
     */
    private static readonly SWERVE_AXIS_TOLERANCE = 0.05

    private _behaviors: Behavior[] = []
    private _simLayer: SimulationLayer
    private _brainIndex: number
    private _assembly: MirabufSceneObject
    public driveType: DriveType = DriveType.ARCADE

    // Tracks how many joins have been made with unique controls
    private _currentJointIndex = 1

    // Track previous unstick button state to detect button press (not hold)
    private _prevUnstickPressed = false

    public get assemblyName(): string {
        return this._assembly.assemblyName
    }

    public get behaviors(): Behavior[] {
        return this._behaviors
    }

    // Tracks the number of each specific mirabuf file spawned
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
        const wasSwerve = this.driveType === DriveType.SWERVE
        this.driveType = driveType

        // Transitioning into or out of swerve requires a full rebuild so that the
        // azimuth (steering) hinges are correctly excluded from / restored to arm control.
        if (driveType === DriveType.SWERVE || wasSwerve) {
            this.configure()
            return
        }

        // Tank <-> Arcade is a lightweight toggle on the existing skid-steer behavior.
        const existing = this._behaviors.find((behavior: Behavior) => behavior instanceof SkidSteerDriveBehavior)
        if (existing == null) {
            console.error("Can't find drive behavior!")
            return
        }
        existing.isArcade = driveType == DriveType.ARCADE
    }

    public resetSwerveOrientation(): void {
        const swerve = this._behaviors.find(b => b instanceof SwerveDriveBehavior) as SwerveDriveBehavior | undefined
        if (!swerve) return
        swerve.resetFieldForward()
    }

    public configure(): void {
        this._behaviors = []
        this._currentJointIndex = 1
        // Only adds controls to mechanisms that are controllable (ignores fields)
        if (this._assembly.mechanism.controllable) {
            // In swerve mode, detect the azimuth hinges up front so they can drive the modules and
            // be excluded from arm behaviors. Fall back to arcade if detection fails.
            const swerveInfo =
                this.driveType === DriveType.SWERVE
                    ? this.detectSwerve()
                    : { inSwerve: false, hinges: [] as HingeDriver[] }

            const useSwerve = this.driveType === DriveType.SWERVE && swerveInfo.inSwerve
            if (this.driveType === DriveType.SWERVE && !swerveInfo.inSwerve) {
                console.warn("[Swerve] swerve detection failed for this robot; falling back to arcade drive.")
            }

            this._behaviors.push(
                useSwerve
                    ? this.createSwerveDriveBehavior(swerveInfo.hinges)
                    : this.createSkidSteerDriveBehavior(this.driveType === DriveType.ARCADE)
            )

            this.configureArmBehaviors(useSwerve ? swerveInfo.hinges : [])
            this.configureElevatorBehaviors()
            this.configureGamepieceManipBehavior()
        } else {
            this.configureField()
        }
    }

    /**
     * @param assembly
     * @param driveType
     */
    public constructor(assembly: MirabufSceneObject) {
        super(assembly.mechanism, "synthesis")
        this._assembly = assembly
        this._simLayer = World.simulationSystem.getSimulationLayer(assembly.mechanism)!

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

        const unstickForce = new JOLT.Vec3(0, this._assembly.robotPreferences.unstickForce, 0)
        body.AddForce(unstickForce) // CLONE
        JOLT.destroy(unstickForce)
    }

    public disable(): void {
        this.clearControls()
        this._behaviors = []
    }

    public clearControls(): void {
        InputSystem.brainIndexSchemeMap.delete(this._brainIndex)
    }

    /** Creates and returns a configured skid-steer (tank/arcade) drive behavior. */
    private createSkidSteerDriveBehavior(isArcade: boolean): DriveBehavior {
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

        // Use the chassis (root) body's CoM as the reference, not a wheel body.
        const rootBodyId = this._mechanism.getBodyByNodeId(this._mechanism.rootBody)
        const chassisBody = rootBodyId ? World.physicsSystem.getBody(rootBodyId) : undefined
        const robotCOM = chassisBody
            ? chassisBody.GetCenterOfMassPosition()
            : World.physicsSystem.getBody(this._mechanism.constraints[0].childBody)!.GetCenterOfMassPosition()

        // Collect constraint positions to determine the correct lateral axis.
        // For skid-steer robots the lateral axis (left vs right) is the one that splits
        // wheels into two equal groups. Try X and Z; pick the more balanced split.
        const constraintPositions: { x: number; z: number }[] = []
        for (let i = 0; i < wheelDrivers.length; i++) {
            const m = fixedConstraints[i].GetConstraintToBody1Matrix() // STATIC_ALIAS
            const t = m.GetTranslation()
            constraintPositions.push({ x: t.GetX() - robotCOM.GetX(), z: t.GetZ() - robotCOM.GetZ() })
        }

        const xImbalance = Math.abs(
            constraintPositions.filter(p => p.x >= 0).length - constraintPositions.filter(p => p.x < 0).length
        )
        const zImbalance = Math.abs(
            constraintPositions.filter(p => p.z >= 0).length - constraintPositions.filter(p => p.z < 0).length
        )

        // Use Z axis when it gives a more balanced split (URDF robots); fall back to X (Fusion 360 robots).
        // URDF's usual +Y-left convention converts to -Z-left in Synthesis, so +Z is the right side.
        const useLateralZ = zImbalance < xImbalance
        const rightVector = useLateralZ ? new JOLT.RVec3(0, 0, -1) : new JOLT.RVec3(1, 0, 0)

        for (let i = 0; i < wheelDrivers.length; i++) {
            // Jolt value returns (GetConstraintToBody1Matrix, GetTranslation, SubRVec3,
            // GetCenterOfMassPosition) point to reused static temporaries, not heap
            // allocations. Don't destroy them; freeing a non-heap address corrupts the heap.
            const constraintMatrix = fixedConstraints[i].GetConstraintToBody1Matrix()
            const translation = constraintMatrix.GetTranslation()
            const wheelPos = convertJoltVec3ToJoltRVec3(translation, false)

            const dotProduct = rightVector.Dot(wheelPos.SubRVec3(robotCOM))
            const [wheels, stimuli] = dotProduct < 0 ? [rightWheels, rightStimuli] : [leftWheels, leftStimuli]

            wheels.push(wheelDrivers[i])
            stimuli.push(wheelStimuli[i])

            // wheelPos is the only heap allocation in this loop.
            JOLT.destroy(wheelPos)
        }
        JOLT.destroy(rightVector)

        return new SkidSteerDriveBehavior(
            leftWheels,
            rightWheels,
            leftStimuli,
            rightStimuli,
            this._brainIndex,
            isArcade
        )
    }

    /**
     * Detects whether this robot is a swerve drivetrain and returns its azimuth (steering) hinges.
     * A hinge is an azimuth hinge when its rotation axis is essentially vertical (perpendicular-to-up
     * magnitude below {@link SynthesisBrain.SWERVE_AXIS_TOLERANCE}). The robot is swerve when the
     * azimuth-hinge count is at least the wheel count.
     */
    private detectSwerve(): { inSwerve: boolean; hinges: HingeDriver[] } {
        const hingeDrivers: HingeDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof HingeDriver
        ) as HingeDriver[]

        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]

        // World-up; robots spawn upright so this matches the original's grounded-node up vector.
        const up = new THREE.Vector3(0, 1, 0)

        const swerveHinges: HingeDriver[] = []
        hingeDrivers.forEach(h => {
            const a = h.worldAxis
            const axis = new THREE.Vector3(a.GetX(), a.GetY(), a.GetZ()).normalize()
            // Magnitude of the axis component perpendicular to up; near zero means the axis is vertical.
            const perpMag = axis
                .clone()
                .sub(up.clone().multiplyScalar(up.dot(axis)))
                .length()
            if (perpMag < SynthesisBrain.SWERVE_AXIS_TOLERANCE) swerveHinges.push(h)
        })

        const inSwerve = wheelDrivers.length > 0 && swerveHinges.length >= wheelDrivers.length
        return { inSwerve, hinges: swerveHinges }
    }

    /**
     * Creates and returns a configured swerve drive behavior, pairing each drive wheel
     * with its nearest azimuth hinge. Falls back to arcade if the robot lacks the wheels
     * or hinges needed for swerve.
     */
    private createSwerveDriveBehavior(hingeDrivers: HingeDriver[]): DriveBehavior {
        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]
        const wheelStimuli: WheelRotationStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof WheelRotationStimulus
        ) as WheelRotationStimulus[]
        const hingeStimuli: HingeStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof HingeStimulus
        ) as HingeStimulus[]

        if (wheelDrivers.length === 0 || hingeDrivers.length === 0) {
            console.error(
                `Cannot configure swerve drivetrain (${wheelDrivers.length} wheels, ` +
                    `${hingeDrivers.length} azimuth hinges). Falling back to arcade.`
            )
            return this.createSkidSteerDriveBehavior(true)
        }

        // Pair each wheel with its nearest azimuth hinge so paired drivers share an index.
        // Both positions are taken as world-space anchors, matching the original which paired
        // on WheelDriver.Anchor / RotationalDriver.Anchor.
        const wheelPositions = wheelDrivers.map(w => {
            const forward = new JOLT.Vec3(1, 0, 0)
            const up = new JOLT.Vec3(0, 1, 0)
            const transform = w.constraint.GetWheelWorldTransform(0, forward, up)
            const pos = {
                x: transform.GetTranslation().GetX(),
                y: transform.GetTranslation().GetY(),
                z: transform.GetTranslation().GetZ(),
            }
            JOLT.destroy(forward)
            JOLT.destroy(up)
            return pos
        })
        const hingePositions = hingeDrivers.map(h => {
            const t = h.worldAnchor
            return { x: t.GetX(), y: t.GetY(), z: t.GetZ() }
        })

        const pairing = pairNearestHinges(wheelPositions, hingePositions)
        const sortedHinges = pairing.map(hingeIndex => hingeDrivers[hingeIndex])

        return new SwerveDriveBehavior(
            wheelDrivers,
            sortedHinges,
            wheelStimuli,
            hingeStimuli,
            this._brainIndex,
            this._assembly.assemblyId
        )
    }

    /** Creates instances of ArmBehavior and automatically configures them. */
    private configureArmBehaviors(excludeHinges: HingeDriver[] = []) {
        const excludeGuids = new Set(excludeHinges.map(h => h.id.guid))
        const hingeDrivers: HingeDriver[] = (
            this._simLayer.drivers.filter(driver => driver instanceof HingeDriver) as HingeDriver[]
        ).filter(h => !excludeGuids.has(h.id.guid))
        const hingeStimuli: HingeStimulus[] = (
            this._simLayer.stimuli.filter(stimulus => stimulus instanceof HingeStimulus) as HingeStimulus[]
        ).filter(s => !excludeGuids.has(s.id.guid))

        for (let i = 0; i < hingeDrivers.length; i++) {
            // An arm joint is always velocity-controlled. Reset the control mode in case this
            // hinge was previously left in POSITION mode by a swerve configuration.
            hingeDrivers[i].controlMode = DriverControlMode.VELOCITY

            let sequentialConfig = PreferencesSystem.getRobotPreferences(
                this._assembly.assemblyId
            ).sequentialConfig?.find(sc => sc.jointIndex == this._currentJointIndex)

            if (sequentialConfig == undefined) {
                sequentialConfig = defaultSequentialConfig(this._currentJointIndex, "Arm")
                if (this._assembly.robotPreferences.sequentialConfig == undefined)
                    this._assembly.robotPreferences.sequentialConfig = []

                this._assembly.robotPreferences.sequentialConfig?.push(sequentialConfig)
                this._assembly.savePreferences()
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

    /** Creates instances of `ElevatorBehavior` and automatically configures them. */
    private configureElevatorBehaviors() {
        const sliderDrivers: SliderDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof SliderDriver
        ) as SliderDriver[]
        const sliderStimuli: SliderStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof SliderStimulus
        ) as SliderStimulus[]

        for (let i = 0; i < sliderDrivers.length; i++) {
            let sequentialConfig = PreferencesSystem.getRobotPreferences(
                this._assembly.assemblyId
            ).sequentialConfig?.find(sc => sc.jointIndex == this._currentJointIndex)

            if (sequentialConfig == undefined) {
                sequentialConfig = defaultSequentialConfig(this._currentJointIndex, "Elevator")

                if (this._assembly.robotPreferences.sequentialConfig == undefined)
                    this._assembly.robotPreferences.sequentialConfig = []

                this._assembly.robotPreferences.sequentialConfig?.push(sequentialConfig)
                this._assembly.savePreferences()
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
        /** Put any field configuration here */
    }

    public static getBrainIndex(assembly: MirabufSceneObject | undefined): number | undefined {
        return (assembly?.brain as SynthesisBrain)?.brainIndex
    }
}

export default SynthesisBrain
