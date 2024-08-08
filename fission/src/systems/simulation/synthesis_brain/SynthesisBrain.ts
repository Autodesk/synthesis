import Mechanism from "@/systems/physics/Mechanism"
import Brain from "../Brain"
import Behavior from "../behavior/Behavior"
import World from "@/systems/World"
import WheelDriver from "../driver/WheelDriver"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import ArcadeDriveBehavior from "../behavior/synthesis/ArcadeDriveBehavior"
import { SimulationLayer } from "../SimulationSystem"
import Jolt from "@barclah/jolt-physics"
import HingeDriver from "../driver/HingeDriver"
import HingeStimulus from "../stimulus/HingeStimulus"
import GenericArmBehavior from "../behavior/synthesis/GenericArmBehavior"
import SliderDriver from "../driver/SliderDriver"
import SliderStimulus from "../stimulus/SliderStimulus"
import GenericElevatorBehavior from "../behavior/synthesis/GenericElevatorBehavior"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import InputSystem from "@/systems/input/InputSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { distance } from "framer-motion"
import SwerveDriveBehavior from "../behavior/synthesis/SwerveDriveBehavior"
import Synthesis from "@/Synthesis"
import { MaskPass } from "postprocessing"

class SynthesisBrain extends Brain {
    public static brainIndexMap = new Map<number, SynthesisBrain>()

    private _behaviors: Behavior[] = []
    private _simLayer: SimulationLayer
    private _assemblyName: string
    private _brainIndex: number

    // Tracks how many joins have been made with unique controls
    private _currentJointIndex = 1

    public get inputSchemeName(): string {
        const scheme = InputSystem.brainIndexSchemeMap.get(this._brainIndex)
        if (scheme == undefined) return "Not Configured"

        return scheme.schemeName
    }

    public get brainIndex(): number {
        return this._brainIndex
    }

    public constructor(mechanism: Mechanism, assemblyName: string) {
        super(mechanism)

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!
        this._assemblyName = assemblyName

        this._brainIndex = SynthesisBrain.brainIndexMap.size
        SynthesisBrain.brainIndexMap.set(this._brainIndex, this)

        if (!this._simLayer) {
            console.error("SimulationLayer is undefined")
            return
        }

        // Only adds controls to mechanisms that are controllable (ignores fields)
        if (mechanism.controllable) {
            const swerveData = this.isSwerve()

            if (swerveData.inSwerve) this.configureSwerveDrivetrain(swerveData.hinges)
            else this.configureArcadeDriveBehavior()

            this.configureArmBehaviors()
            this.configureElevatorBehaviors()
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
        InputSystem.brainIndexSchemeMap.delete(this._brainIndex)
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

    private static distance = (a: Jolt.Vec3, b: Jolt.Vec3) => {
        const dx = a.GetX() - b.GetX()
        const dy = a.GetY() - b.GetY()
        const dz = a.GetZ() - b.GetZ()

        return Math.sqrt(dx * dx + dy * dy + dz * dz)
    }

    private static distanceR = (a: Jolt.RVec3, b: Jolt.RVec3) => {
        const dx = a.GetX() - b.GetX()
        const dy = a.GetY() - b.GetY()
        const dz = a.GetZ() - b.GetZ()

        return Math.sqrt(dx * dx + dy * dy + dz * dz)
    }

    private configureSwerveDrivetrain(hingeDrivers: HingeDriver[]) {
        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]
        const wheelStimuli: WheelRotationStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof WheelRotationStimulus
        ) as WheelRotationStimulus[]

        const hingeStimuli: HingeStimulus[] = this._simLayer.stimuli.filter(
            stimulus => stimulus instanceof HingeStimulus
        ) as HingeStimulus[]

        const hingePositionMap: Map<HingeDriver, Jolt.Vec3> = new Map()
        const wheelPositionMap: Map<WheelDriver, Jolt.Vec3> = new Map()

        const pairedDrivers: Map<WheelDriver, HingeDriver> = new Map()

        hingeDrivers.forEach(h => {
            // TODO: is body 2 always the correct choice? May have to check to see which one is giving different values for each hinge
            const hingePos = h.constraint.GetConstraintToBody2Matrix().GetTranslation()
            const hingePosCopy = new JOLT.Vec3(hingePos.GetX(), hingePos.GetY(), hingePos.GetZ())

            hingePositionMap.set(h, hingePosCopy)
        })

        wheelDrivers.forEach(w => {
            const wheelPos = w.constraint
                .GetWheelWorldTransform(0, new JOLT.Vec3(1, 0, 0), new JOLT.Vec3(0, 1, 0))
                .GetTranslation()

            const wheelPosCopy = new JOLT.Vec3(wheelPos.GetX(), wheelPos.GetY(), wheelPos.GetZ())
            wheelPositionMap.set(w, wheelPosCopy)
        })

        wheelDrivers.forEach(w => {
            let minDist: number = Infinity
            let closestHinge: HingeDriver

            hingeDrivers.forEach(h => {
                const a = wheelPositionMap.get(w)!
                const b = hingePositionMap.get(h)!

                const dist = SynthesisBrain.distance(b, a)
                if (dist < minDist) {
                    minDist = dist
                    closestHinge = h
                }
            })
            pairedDrivers.set(w, closestHinge!)
        })

        const sortedWheels: WheelDriver[] = []
        const sortedHinges: HingeDriver[] = []

        for (const [key, value] of pairedDrivers) {
            sortedWheels.push(key)
            sortedHinges.push(value)
        }
        this._behaviors.push(
            new SwerveDriveBehavior(
                sortedWheels,
                sortedHinges,
                wheelStimuli,
                hingeStimuli,
                this._brainIndex,
                this._assemblyName
            )
        )
    }

    private isSwerve: () => { inSwerve: boolean; hinges: HingeDriver[] } = () => {
        const hingeDrivers: HingeDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof HingeDriver
        ) as HingeDriver[]

        const wheelDrivers: WheelDriver[] = this._simLayer.drivers.filter(
            driver => driver instanceof WheelDriver
        ) as WheelDriver[]

        const dotProducts: { hinge: HingeDriver; dot: number }[] = []

        hingeDrivers.forEach(h => {
            const t1 = h.constraint.GetConstraintToBody1Matrix().GetTranslation()
            const t1Copy = new JOLT.Vec3(t1.GetX(), t1.GetY(), t1.GetZ())

            const t2 = h.constraint.GetConstraintToBody2Matrix().GetTranslation()
            const t2Copy = new JOLT.Vec3(t2.GetX(), t2.GetY(), t2.GetZ())

            const normal = t1Copy.Sub(t2Copy).Normalized()
            dotProducts.push({ hinge: h, dot: normal.Dot(new JOLT.Vec3(0, 1, 0)) })
        })

        const swerveHinges: HingeDriver[] = dotProducts.filter(entry => entry.dot < 0.35).map(val => val.hinge)
        return { inSwerve: swerveHinges.length == wheelDrivers.length, hinges: swerveHinges }
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

    private configureField() {
        PreferencesSystem.getFieldPreferences(this._assemblyName)

        /** Put any field configuration here */
    }
}

export default SynthesisBrain
