import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { BodyAssociate } from "@/systems/physics/BodyAssociate.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import type MirabufParser from "../../mirabuf/MirabufParser"
import { GAMEPIECE_SUFFIX, GROUNDED_JOINT_ID, type RigidNodeReadOnly } from "../../mirabuf/MirabufParser"
import { mirabuf } from "../../proto/mirabuf"
import {
    convertJoltRVec3ToJoltVec3,
    convertJoltVec3ToJoltRVec3,
    convertMirabufFloatToArrJoltFloat3,
    convertMirabufFloatToArrJoltVec3,
    convertMirabufVector3ToJoltRVec3,
    convertMirabufVector3ToJoltVec3,
    convertThreeMatrix4ToJoltMat44,
    convertThreeToJoltQuat,
    convertThreeVector3ToJoltRVec3,
    convertThreeVector3ToJoltVec3,
} from "../../util/TypeConversions"
import type { LocalSceneObjectId, Message } from "../multiplayer/types"
import PreferencesSystem from "../preferences/PreferencesSystem"
import World from "../World"
import WorldSystem from "../WorldSystem"
import {
    type CurrentContactData,
    OnContactAddedEvent,
    OnContactPersistedEvent,
    OnContactRemovedEvent,
    type OnContactValidateData,
    OnContactValidateEvent,
    type PhysicsEvent,
} from "./ContactEvents"
import Mechanism from "./Mechanism"
import type { JoltBodyIndexAndSequence } from "./PhysicsTypes"

/**
 * Layers used for determining enabled/disabled collisions.
 */
const LAYER_FIELD = 0 // Used for grounded rigid node of a field as well as any rigid nodes jointed to it.
const LAYER_GENERAL_DYNAMIC = 1 // Used for game pieces or any general dynamic objects that can collide with anything and everything.
const ROBOT_LAYERS: number[] = [
    // Reserved layers for robots. Robot layers have no collision with themselves but have collision with everything else.
    2,
    3, 4, 5, 6, 7, 8, 9,
]

// Layer for ghost objects used in constraint systems, interacts with nothing
const LAYER_GHOST = 10

// Please update this accordingly.
const COUNT_OBJECT_LAYERS = 11

export const STANDARD_SIMULATION_PERIOD = 1.0 / 60.0
const MIN_SIMULATION_PERIOD = 1.0 / 120.0
const MAX_SIMULATION_PERIOD = 1.0 / 10.0
const MIN_SUBSTEPS = 12
const MAX_SUBSTEPS = 20
const STANDARD_SUB_STEPS = 20
const TIMESTEP_ADJUSTMENT = 0.0001

const SIGNIFICANT_FRICTION_THRESHOLD = 0.05

const MAX_ROBOT_MASS = 250.0
const MAX_GP_MASS = 10.0

let lastDeltaT = STANDARD_SIMULATION_PERIOD
export function getLastDeltaT(): number {
    return lastDeltaT
}

// Friction constants
const FLOOR_FRICTION = 0.7
const DEFAULT_FRICTION = 0.7

// Transition GH-1152, AARD-1885:
// Temporary workaround to reduce visible levitation of robots by minimizing suspension.
// Setting these values to 0 causes physics issues (e.g., ground collisionn problems).
// Some robots still float slightly, assuming this is due to different export conditions.
const SUSPENSION_MIN_FACTOR = 0.0001
const SUSPENSION_MAX_FACTOR = 0.0001

const DEFAULT_PHYSICAL_MATERIAL_KEY = "default"

// Motor constant
const VELOCITY_DEFAULT = 30

/**
 * The PhysicsSystem handles all Jolt Physics interactions within Synthesis.
 * This system can create physical representations of objects such as Robots,
 * Fields, and Game pieces, and simulate them.
 */
class PhysicsSystem extends WorldSystem {
    private _joltInterface: Jolt.JoltInterface
    private _joltPhysSystem: Jolt.PhysicsSystem
    private _joltBodyInterface: Jolt.BodyInterface
    private _bodies: Array<Jolt.BodyID>
    private _constraints: Array<Jolt.Constraint>

    private _physicsEventQueue: PhysicsEvent[] = []

    private _pauseSet = new Set<string>()

    private _bodyAssociations: Map<JoltBodyIndexAndSequence, BodyAssociate>

    public get isPaused(): boolean {
        return this._pauseSet.size > 0
    }

    /**
     * Creates a PhysicsSystem object.
     */
    constructor() {
        super()

        this._bodies = []
        this._constraints = []

        const joltSettings = new JOLT.JoltSettings()
        setupCollisionFiltering(joltSettings)

        this._joltInterface = new JOLT.JoltInterface(joltSettings)
        JOLT.destroy(joltSettings)

        this._joltPhysSystem = this._joltInterface.GetPhysicsSystem()
        this._joltBodyInterface = this._joltPhysSystem.GetBodyInterface()
        this.setUpContactListener(this._joltPhysSystem)

        this._joltPhysSystem.SetGravity(new JOLT.Vec3(0, -9.8, 0))
        this._joltPhysSystem.GetPhysicsSettings().mDeterministicSimulation = false
        this._joltPhysSystem.GetPhysicsSettings().mSpeculativeContactDistance = 0.06
        this._joltPhysSystem.GetPhysicsSettings().mPenetrationSlop = 0.005

        const ground = this.createBox(
            new THREE.Vector3(7.5, 0.1, 7.5),
            undefined,
            new THREE.Vector3(0.0, -0.1, 0.0),
            undefined
        )
        ground.SetFriction(FLOOR_FRICTION)
        this._joltBodyInterface.AddBody(ground.GetID(), JOLT.EActivation_Activate)

        this._bodyAssociations = new Map()
    }

    /**
     * Get association to a given Jolt Body.
     *
     * @param bodyId BodyID to check for association
     * @returns Association for given Body
     */
    public getBodyAssociation(bodyId: Jolt.BodyID): BodyAssociate | undefined {
        return this._bodyAssociations.get(bodyId.GetIndexAndSequenceNumber())
    }

    /**
     * Sets assocation for a body
     *
     * @param assocation Assocation. See {@link BodyAssociate}
     */
    public setBodyAssociation<T extends BodyAssociate>(assocation: T) {
        this._bodyAssociations.set(assocation.associatedBody, assocation)
    }

    public removeBodyAssociation(bodyId: Jolt.BodyID) {
        this._bodyAssociations.delete(bodyId.GetIndexAndSequenceNumber())
    }

    /**
     * Holds a pause.
     *
     * @param ref String to reference your hold.
     */
    public holdPause(ref: string) {
        this._pauseSet.add(ref)
    }

    /**
     * Forces all holds on the pause to be released.
     */
    public forceUnpause() {
        this._pauseSet.clear()
    }

    /**
     * Releases a pause.
     *
     * @param ref String to reference your hold.
     *
     * @returns Whether or not your hold was successfully removed.
     */
    public releasePause(ref: string): boolean {
        return this._pauseSet.delete(ref)
    }

    /**
     * Disabling physics for a single body
     *
     * @param bodyId
     */
    public disablePhysicsForBody(bodyId: Jolt.BodyID) {
        if (!this.isBodyAdded(bodyId)) return

        this._joltBodyInterface.DeactivateBody(bodyId)

        this.getBody(bodyId).SetIsSensor(true)
    }

    /**
     * Enables physics for a single body
     *
     * @param bodyId
     */
    public enablePhysicsForBody(bodyId: Jolt.BodyID) {
        if (!this.isBodyAdded(bodyId)) return

        this._joltBodyInterface.ActivateBody(bodyId)
        this.getBody(bodyId).SetIsSensor(false)
    }

    public isBodyAdded(bodyId: Jolt.BodyID) {
        return this._joltBodyInterface.IsAdded(bodyId)
    }

    /**
     * TEMPORARY
     * Create a box.
     *
     * @param   halfExtents The half extents of the Box.
     * @param   mass        Mass of the Box. Leave undefined to make Box static.
     * @param   position    Position of the Box (default: 0, 0, 0)
     * @param   rotation    Rotation of the Box (default 0, 0, 0, 1)
     * @returns Reference to Jolt Body
     */
    public createBox(
        halfExtents: THREE.Vector3,
        mass: number | undefined,
        position: THREE.Vector3 | undefined,
        rotation: THREE.Euler | THREE.Quaternion | undefined
    ) {
        const size = convertThreeVector3ToJoltVec3(halfExtents)
        const shape = new JOLT.BoxShape(size, 0.1)
        JOLT.destroy(size)

        const pos = position ? convertThreeVector3ToJoltRVec3(position) : new JOLT.RVec3(0.0, 0.0, 0.0)
        const rot = convertThreeToJoltQuat(rotation)
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            pos,
            rot,
            mass ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
            mass ? LAYER_GENERAL_DYNAMIC : LAYER_FIELD
        )
        if (mass) {
            creationSettings.mOverrideMassProperties = JOLT.EOverrideMassProperties_CalculateInertia
            creationSettings.mMassPropertiesOverride.mMass = mass
        }

        const body = this._joltBodyInterface.CreateBody(creationSettings)
        JOLT.destroy(pos)
        JOLT.destroy(rot)
        JOLT.destroy(creationSettings)

        this._bodies.push(body.GetID())
        return body
    }

    /**
     * This creates a body in Jolt. Mostly used for Unit test validation.
     *
     * @param   shape       Shape to impart on the body.
     * @param   mass        Mass of the body.
     * @param   position    Position of the body.
     * @param   rotation    Rotation of the body.
     * @returns Resulting Body object.
     */
    public createBody(
        shape: Jolt.Shape,
        mass: number | undefined,
        position: THREE.Vector3 | undefined,
        rotation: THREE.Euler | THREE.Quaternion | undefined
    ) {
        const pos = position ? convertThreeVector3ToJoltRVec3(position) : new JOLT.RVec3(0.0, 0.0, 0.0)
        const rot = convertThreeToJoltQuat(rotation)
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            pos,
            rot,
            mass ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
            mass ? LAYER_GENERAL_DYNAMIC : LAYER_FIELD
        )
        if (mass) {
            creationSettings.mOverrideMassProperties = JOLT.EOverrideMassProperties_CalculateInertia
            creationSettings.mMassPropertiesOverride.mMass = mass
        }

        const body = this._joltBodyInterface.CreateBody(creationSettings)
        JOLT.destroy(pos)
        JOLT.destroy(rot)
        JOLT.destroy(creationSettings)

        this._bodies.push(body.GetID())
        return body
    }

    public addBodyToSystem(bodyId: Jolt.BodyID, shouldActivate: boolean) {
        this._joltBodyInterface.AddBody(
            bodyId,
            shouldActivate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
    }

    /**
     * Utility function for creating convex hulls. Mostly used for Unit test validation.
     *
     * @param   points  Flat pack array of vector 3 components.
     * @param   density Density of the convex hull.
     * @returns Resulting shape.
     */
    public createConvexHull(points: Float32Array, density: number = 1.0): Jolt.ShapeResult {
        if (points.length % 3) throw new Error(`Invalid size of points: ${points.length}`)

        const settings = new JOLT.ConvexHullShapeSettings()
        settings.mPoints.clear()
        settings.mPoints.reserve(points.length / 3.0)
        for (let i = 0; i < points.length; i += 3) {
            settings.mPoints.push_back(new JOLT.Vec3(points[i], points[i + 1], points[i + 2]))
        }
        settings.mDensity = density
        return settings.Create()
    }

    public createMechanismFromParser(parser: MirabufParser): Mechanism {
        const layer = parser.assembly.dynamic ? new LayerReserve() : undefined
        const bodyMap = this.createBodiesFromParser(parser, layer)
        const rootBody = parser.rootNode
        const mechanism = new Mechanism(rootBody, bodyMap, parser.assembly.dynamic, layer)
        this.createJointsFromParser(parser, mechanism)
        return mechanism
    }

    /**
     * Creates all the joints for a mirabuf assembly given an already compiled mapping of rigid nodes to bodies.
     *
     * @param   parser      Mirabuf parser with complete set of rigid nodes and assembly data.
     * @param   mechanism   Mapping of the name of rigid groups to Jolt bodies. Retrieved from CreateBodiesFromParser.
     */
    public createJointsFromParser(parser: MirabufParser, mechanism: Mechanism) {
        const jointData = parser.assembly.data!.joints!
        const joints = Object.entries(jointData.jointInstances!) as [string, mirabuf.joint.JointInstance][]
        joints.forEach(([jointGuid, jointInst]) => {
            if (jointGuid == GROUNDED_JOINT_ID) return

            const rnA = parser.partToNodeMap.get(jointInst.parentPart!)
            const rnB = parser.partToNodeMap.get(jointInst.childPart!)

            if (!rnA || !rnB) {
                console.warn(`Skipping joint '${jointInst.info!.name!}'. Couldn't find associated rigid nodes.`)
                return
            } else if (rnA.id == rnB.id) {
                console.warn(
                    `Skipping joint '${jointInst.info!.name!}'. Jointing the same parts. Likely in issue with Fusion Design structure.`
                )
                return
            }

            const jDef = parser.assembly.data!.joints!.jointDefinitions![
                jointInst.jointReference!
            ]! as mirabuf.joint.Joint
            const bodyIdA = mechanism.getBodyByNodeId(rnA.id)
            const bodyIdB = mechanism.getBodyByNodeId(rnB.id)
            if (!bodyIdA || !bodyIdB) {
                console.warn(
                    `Skipping joint '${jointInst.info!.name!}'. Failed to find rigid nodes' associated bodies.`
                )
                return
            }
            const bodyA = this.getBody(bodyIdA)
            const bodyB = this.getBody(bodyIdB)

            // Motor velocity and acceleration. Prioritizes preferences then mirabuf.
            const prefMotors = PreferencesSystem.getRobotPreferences(parser.assembly.info?.name ?? "").motors
            const prefMotor = prefMotors ? prefMotors.filter(x => x.name == jointInst.info?.name) : undefined
            const miraMotor = jointData.motorDefinitions![jDef.motorReference]

            let maxVel = VELOCITY_DEFAULT
            let maxForce
            if (prefMotor && prefMotor[0]) {
                maxVel = prefMotor[0].maxVelocity
                maxForce = prefMotor[0].maxForce
            } else if (miraMotor && miraMotor.simpleMotor) {
                maxVel = miraMotor.simpleMotor.maxVelocity ?? VELOCITY_DEFAULT
                maxForce = miraMotor.simpleMotor.stallTorque
            }

            let listener: Jolt.PhysicsStepListener | null = null

            const addConstraint = (c: Jolt.Constraint): void => {
                mechanism.addConstraint({
                    parentBody: bodyIdA,
                    childBody: bodyIdB,
                    primaryConstraint: c,
                    maxVelocity: maxVel ?? VELOCITY_DEFAULT,
                    info: jointInst.info ?? undefined, // remove possibility for null
                    extraConstraints: [],
                    extraBodies: [],
                })
            }

            switch (jDef.jointMotionType!) {
                case mirabuf.joint.JointMotion.REVOLUTE:
                    if (this.isWheel(jDef)) {
                        const preferences = PreferencesSystem.getRobotPreferences(parser.assembly.info?.name ?? "")
                        if (preferences.driveVelocity > 0) maxVel = preferences.driveVelocity
                        if (preferences.driveAcceleration > 0) maxForce = preferences.driveAcceleration

                        const [bodyOne, bodyTwo] = parser.directedGraph.getAdjacencyList(rnA.id).length
                            ? [bodyA, bodyB]
                            : [bodyB, bodyA]

                        const res = this.createWheelConstraint(
                            jointInst,
                            jDef,
                            maxForce ?? 1.5,
                            bodyOne,
                            bodyTwo,
                            parser.assembly.info!.version!
                        )
                        addConstraint(res[0])
                        addConstraint(res[1])
                        listener = res[2]

                        break
                    }

                    addConstraint(
                        this.createHingeConstraint(
                            jointInst,
                            jDef,
                            maxForce ?? 50,
                            bodyA,
                            bodyB,
                            parser.assembly.info!.version!
                        )
                    )

                    break

                case mirabuf.joint.JointMotion.SLIDER:
                    addConstraint(this.createSliderConstraint(jointInst, jDef, maxForce ?? 200, bodyA, bodyB))
                    break
                case mirabuf.joint.JointMotion.BALL:
                    this.createBallConstraint(jointInst, jDef, bodyA, bodyB, mechanism)
                    break
                default:
                    console.debug("Unsupported joint detected. Skipping...")
                    break
            }
            if (listener) mechanism.addStepListener(listener)
        })
    }

    /**
     * Creates a Hinge constraint.
     *
     * @param   jointInstance   Joint instance.
     * @param   jointDefinition Joint definition.
     * @param   bodyA           Parent body to connect.
     * @param   bodyB           Child body to connect.
     * @param   versionNum      Version number of the export. Used for compatibility purposes.
     * @returns Resulting Jolt Hinge Constraint.
     */
    private createHingeConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        torque: number,
        bodyA: Jolt.Body,
        bodyB: Jolt.Body,
        versionNum: number
    ): Jolt.Constraint {
        // HINGE CONSTRAINT
        const hingeConstraintSettings = new JOLT.HingeConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? convertMirabufVector3ToJoltRVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)
        // TODO: Offset transformation for robot builder.
        const jointOriginOffset = jointInstance.offset
            ? convertMirabufVector3ToJoltRVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)

        const anchorPoint = jointOrigin.AddRVec3(jointOriginOffset)
        hingeConstraintSettings.mPoint1 = hingeConstraintSettings.mPoint2 = anchorPoint

        const rotationalFreedom = jointDefinition.rotational!.rotationalFreedom!

        const miraAxis = rotationalFreedom.axis! as mirabuf.Vector3
        // No scaling, these are unit vectors
        const miraAxisX = (versionNum < 5 ? -miraAxis.x : miraAxis.x) ?? 0
        const axis = new JOLT.Vec3(miraAxisX, miraAxis.y! ?? 0, miraAxis.z! ?? 0)

        hingeConstraintSettings.mHingeAxis1 = hingeConstraintSettings.mHingeAxis2 = axis.Normalized()
        hingeConstraintSettings.mNormalAxis1 = hingeConstraintSettings.mNormalAxis2 = getPerpendicular(
            hingeConstraintSettings.mHingeAxis1
        )

        // Some values that are meant to be exactly PI are perceived as being past it, causing unexpected behavior.
        // This safety check caps the values to be within [-PI, PI] with minimal difference in precision.
        const piSafetyCheck = (v: number) => Math.min(3.14158, Math.max(-3.14158, v))

        if (
            rotationalFreedom.limits &&
            Math.abs((rotationalFreedom.limits.upper ?? 0) - (rotationalFreedom.limits.lower ?? 0)) > 0.001
        ) {
            const currentPos = piSafetyCheck(rotationalFreedom.value ?? 0)
            const upper = piSafetyCheck(rotationalFreedom.limits.upper ?? 0) - currentPos
            const lower = piSafetyCheck(rotationalFreedom.limits.lower ?? 0) - currentPos

            hingeConstraintSettings.mLimitsMin = -upper
            hingeConstraintSettings.mLimitsMax = -lower
        }

        hingeConstraintSettings.mMotorSettings.mMaxTorqueLimit = torque
        hingeConstraintSettings.mMotorSettings.mMinTorqueLimit = -torque

        const constraint = hingeConstraintSettings.Create(bodyA, bodyB)
        this._constraints.push(constraint)
        this._joltPhysSystem.AddConstraint(constraint)

        return constraint
    }

    /**
     * Creates a new slider constraint.
     *
     * @param   jointInstance   Joint instance.
     * @param   jointDefinition Joint definition.
     * @param   bodyA           Parent body to connect.
     * @param   bodyB           Child body to connect.
     *
     * @returns Resulting Jolt constraint.
     */
    private createSliderConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        maxForce: number,
        bodyA: Jolt.Body,
        bodyB: Jolt.Body
    ): Jolt.Constraint {
        const sliderConstraintSettings = new JOLT.SliderConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? convertMirabufVector3ToJoltRVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)
        // TODO: Offset transformation for robot builder.
        const jointOriginOffset = jointInstance.offset
            ? convertMirabufVector3ToJoltRVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)

        const anchorPoint = jointOrigin.AddRVec3(jointOriginOffset)
        sliderConstraintSettings.mPoint1 = sliderConstraintSettings.mPoint2 = anchorPoint

        const prismaticFreedom = jointDefinition.prismatic!.prismaticFreedom!

        const miraAxis = prismaticFreedom.axis! as mirabuf.Vector3
        const axis = new JOLT.Vec3(miraAxis.x! ?? 0, miraAxis.y! ?? 0, miraAxis.z! ?? 0)

        sliderConstraintSettings.mSliderAxis1 = sliderConstraintSettings.mSliderAxis2 = axis.Normalized()
        sliderConstraintSettings.mNormalAxis1 = sliderConstraintSettings.mNormalAxis2 = getPerpendicular(
            sliderConstraintSettings.mSliderAxis1
        )

        if (
            prismaticFreedom.limits &&
            Math.abs((prismaticFreedom.limits.upper ?? 0) - (prismaticFreedom.limits.lower ?? 0)) > 0.001
        ) {
            const currentPos = (prismaticFreedom.value ?? 0) * 0.01
            const upper = (prismaticFreedom.limits.upper ?? 0) * 0.01 - currentPos
            const lower = (prismaticFreedom.limits.lower ?? 0) * 0.01 - currentPos

            // Calculate mid point
            const midPoint = (upper + lower) / 2.0
            const halfRange = Math.abs((upper - lower) / 2.0)

            // Move the anchor points
            sliderConstraintSettings.mPoint2 = anchorPoint.Add(axis.Normalized().Mul(midPoint))

            sliderConstraintSettings.mLimitsMax = halfRange
            sliderConstraintSettings.mLimitsMin = -halfRange
        }

        sliderConstraintSettings.mMotorSettings.mMaxForceLimit = maxForce
        sliderConstraintSettings.mMotorSettings.mMinForceLimit = -maxForce

        const constraint = sliderConstraintSettings.Create(bodyA, bodyB)

        this._constraints.push(constraint)
        this._joltPhysSystem.AddConstraint(constraint)

        return constraint
    }

    public createWheelConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        maxAcc: number,
        bodyMain: Jolt.Body,
        bodyWheel: Jolt.Body,
        versionNum: number
    ): [Jolt.Constraint, Jolt.VehicleConstraint, Jolt.PhysicsStepListener] {
        // HINGE CONSTRAINT
        const fixedSettings = new JOLT.FixedConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? convertMirabufVector3ToJoltRVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)
        const jointOriginOffset = jointInstance.offset
            ? convertMirabufVector3ToJoltRVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.RVec3(0, 0, 0)

        const anchorPoint = jointOrigin.AddRVec3(jointOriginOffset)
        fixedSettings.mPoint1 = fixedSettings.mPoint2 = anchorPoint

        const rotationalFreedom = jointDefinition.rotational!.rotationalFreedom!

        // No scaling, these are unit vectors
        const miraAxis = rotationalFreedom.axis! as mirabuf.Vector3
        const miraAxisX: number = (versionNum < 5 ? -miraAxis.x : miraAxis.x) ?? 0
        const axis: Jolt.RVec3 = new JOLT.RVec3(miraAxisX, miraAxis.y ?? 0, miraAxis.z ?? 0)

        const bounds = bodyWheel.GetShape().GetLocalBounds()
        const radius = (bounds.mMax.GetY() - bounds.mMin.GetY()) / 2.0

        const wheelSettings = new JOLT.WheelSettingsWV()
        wheelSettings.mPosition = convertJoltRVec3ToJoltVec3(anchorPoint.AddRVec3(axis.Mul(0.1)))
        wheelSettings.mMaxSteerAngle = 0.0
        wheelSettings.mMaxHandBrakeTorque = 0.0
        wheelSettings.mRadius = radius * 1.05
        wheelSettings.mWidth = 0.1
        wheelSettings.mSuspensionMinLength = radius * SUSPENSION_MIN_FACTOR
        wheelSettings.mSuspensionMaxLength = radius * SUSPENSION_MAX_FACTOR
        wheelSettings.mInertia = 1

        const vehicleSettings = new JOLT.VehicleConstraintSettings()

        vehicleSettings.mWheels.clear()
        vehicleSettings.mWheels.push_back(wheelSettings)

        // Other than maxTorque, these controller settings are not being used as of now
        // because ArcadeDriveBehavior goes directly to the WheelDrivers.
        // maxTorque is only used as communication for WheelDriver to get maxAcceleration
        const controllerSettings = new JOLT.WheeledVehicleControllerSettings()
        controllerSettings.mEngine.mMaxTorque = maxAcc
        controllerSettings.mTransmission.mClutchStrength = 10.0
        controllerSettings.mTransmission.mGearRatios.clear()
        controllerSettings.mTransmission.mGearRatios.push_back(2)
        controllerSettings.mTransmission.mMode = JOLT.ETransmissionMode_Auto
        vehicleSettings.mController = controllerSettings

        vehicleSettings.mAntiRollBars.clear()

        const vehicleConstraint = new JOLT.VehicleConstraint(bodyMain, vehicleSettings)
        const fixedConstraint = JOLT.castObject(fixedSettings.Create(bodyMain, bodyWheel), JOLT.TwoBodyConstraint)

        // Wheel Collision Tester
        const tester = new JOLT.VehicleCollisionTesterCastCylinder(bodyWheel.GetObjectLayer(), 0.05)
        vehicleConstraint.SetVehicleCollisionTester(tester)
        const listener = new JOLT.VehicleConstraintStepListener(vehicleConstraint)
        this._joltPhysSystem.AddStepListener(listener)

        // const callbacks = new JOLT.VehicleConstraintCallbacksJS()
        // callbacks.GetCombinedFriction = (_wheelIndex, _tireFrictionDirection, tireFriction, _body2Ptr, _subShapeID2) => {
        //     return tireFriction
        // }
        // callbacks.OnPreStepCallback = (_vehicle, _stepContext) => { };
        // callbacks.OnPostCollideCallback = (_vehicle, _stepContext) => { };
        // callbacks.OnPostStepCallback = (_vehicle, _stepContext) => { };
        // callbacks.SetVehicleConstraint(vehicleConstraint)

        this._joltPhysSystem.AddConstraint(vehicleConstraint)
        this._joltPhysSystem.AddConstraint(fixedConstraint)

        this._constraints.push(fixedConstraint, vehicleConstraint)
        return [fixedConstraint, vehicleConstraint, listener]
    }

    private createBallConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        bodyA: Jolt.Body,
        bodyB: Jolt.Body,
        mechanism: Mechanism
    ): void {
        const jointOrigin = jointDefinition.origin
            ? convertMirabufVector3ToJoltVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)
        // TODO: Offset transformation for robot builder.
        const jointOriginOffset = jointInstance.offset
            ? convertMirabufVector3ToJoltVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)

        const anchorPoint = jointOrigin.Add(jointOriginOffset)

        const pitchDof = jointDefinition.custom!.dofs!.at(0)
        const yawDof = jointDefinition.custom!.dofs!.at(1)
        const rollDof = jointDefinition.custom!.dofs!.at(2)
        const pitchAxis = new JOLT.Vec3(pitchDof?.axis?.x ?? 0, pitchDof?.axis?.y ?? 0, pitchDof?.axis?.z ?? 0)
        const yawAxis = new JOLT.Vec3(yawDof?.axis?.x ?? 0, yawDof?.axis?.y ?? 0, yawDof?.axis?.z ?? 0)
        const rollAxis = new JOLT.Vec3(rollDof?.axis?.x ?? 0, rollDof?.axis?.y ?? 0, rollDof?.axis?.z ?? 0)

        const constraints: {
            axis: Jolt.Vec3
            friction: number
            value: number
            upper?: number
            lower?: number
        }[] = []

        if (!pitchDof?.limits || (pitchDof.limits.upper ?? 0) - (pitchDof.limits.lower ?? 0) > 0.001) {
            constraints.push({
                axis: pitchAxis,
                friction: 0.0,
                value: pitchDof?.value ?? 0,
                upper: pitchDof?.limits ? (pitchDof.limits.upper ?? 0) : undefined,
                lower: pitchDof?.limits ? (pitchDof.limits.lower ?? 0) : undefined,
            })
        }

        if (!yawDof?.limits || (yawDof.limits.upper ?? 0) - (yawDof.limits.lower ?? 0) > 0.001) {
            constraints.push({
                axis: yawAxis,
                friction: 0.0,
                value: yawDof?.value ?? 0,
                upper: yawDof?.limits ? (yawDof.limits.upper ?? 0) : undefined,
                lower: yawDof?.limits ? (yawDof.limits.lower ?? 0) : undefined,
            })
        }

        if (!rollDof?.limits || (rollDof.limits.upper ?? 0) - (rollDof.limits.lower ?? 0) > 0.001) {
            constraints.push({
                axis: rollAxis,
                friction: 0.0,
                value: rollDof?.value ?? 0,
                upper: rollDof?.limits ? (rollDof.limits.upper ?? 0) : undefined,
                lower: rollDof?.limits ? (rollDof.limits.lower ?? 0) : undefined,
            })
        }

        let bodyStart = bodyB
        let bodyNext = bodyA
        if (constraints.length > 1) {
            bodyNext = this.createGhostBody(anchorPoint)
            this._joltBodyInterface.AddBody(bodyNext.GetID(), JOLT.EActivation_Activate)
            mechanism.ghostBodies.push(bodyNext.GetID())
        }
        for (let i = 0; i < constraints.length; ++i) {
            const c = constraints[i]
            const hingeSettings = new JOLT.HingeConstraintSettings()
            hingeSettings.mMaxFrictionTorque = c.friction
            hingeSettings.mPoint1 = hingeSettings.mPoint2 = convertJoltVec3ToJoltRVec3(anchorPoint)
            hingeSettings.mHingeAxis1 = hingeSettings.mHingeAxis2 = c.axis.Normalized()
            hingeSettings.mNormalAxis1 = hingeSettings.mNormalAxis2 = getPerpendicular(hingeSettings.mHingeAxis1)
            if (c.upper && c.lower) {
                // Some values that are meant to be exactly PI are perceived as being past it, causing unexpected behavior.
                // This safety check caps the values to be within [-PI, PI] wth minimal difference in precision.
                const piSafetyCheck = (v: number) => Math.min(3.14158, Math.max(-3.14158, v))

                const currentPos = piSafetyCheck(c.value)
                const upper = piSafetyCheck(c.upper) - currentPos
                const lower = piSafetyCheck(c.lower) - currentPos

                hingeSettings.mLimitsMin = -upper
                hingeSettings.mLimitsMax = -lower
            }

            const hingeConstraint = hingeSettings.Create(bodyStart, bodyNext)
            this._joltPhysSystem.AddConstraint(hingeConstraint)
            this._constraints.push(hingeConstraint)
            bodyStart = bodyNext
            if (i == constraints.length - 2) {
                bodyNext = bodyA
            } else {
                bodyNext = this.createGhostBody(anchorPoint)
                this._joltBodyInterface.AddBody(bodyNext.GetID(), JOLT.EActivation_Activate)
                mechanism.ghostBodies.push(bodyNext.GetID())
            }
        }
    }

    // TODO: Ball socket joints should try to be reduced to the shoulder joint equivalent for Jolt (SwingTwistConstraint)
    // private CreateBallBadAgainConstraint(
    //     jointInstance: mirabuf.joint.JointInstance,
    //     jointDefinition: mirabuf.joint.Joint,
    //     bodyA: Jolt.Body,
    //     bodyB: Jolt.Body,
    //     mechanism: Mechanism,
    // ): void {

    //     const jointOrigin = jointDefinition.origin
    //         ? MirabufVector3_JoltVec3(jointDefinition.origin as mirabuf.Vector3)
    //         : new JOLT.Vec3(0, 0, 0)
    //     // TODO: Offset transformation for robot builder.
    //     const jointOriginOffset = jointInstance.offset
    //         ? MirabufVector3_JoltVec3(jointInstance.offset as mirabuf.Vector3)
    //         : new JOLT.Vec3(0, 0, 0)

    //     const anchorPoint = jointOrigin.Add(jointOriginOffset)

    //     const pitchDof = jointDefinition.custom!.dofs!.at(0)
    //     const yawDof = jointDefinition.custom!.dofs!.at(1)
    //     const rollDof = jointDefinition.custom!.dofs!.at(2)
    //     const pitchAxis = new JOLT.Vec3(pitchDof?.axis?.x ?? 0, pitchDof?.axis?.y ?? 0, pitchDof?.axis?.z ?? 0)
    //     const yawAxis = new JOLT.Vec3(yawDof?.axis?.x ?? 0, yawDof?.axis?.y ?? 0, yawDof?.axis?.z ?? 0)
    //     const rollAxis = new JOLT.Vec3(rollDof?.axis?.x ?? 0, rollDof?.axis?.y ?? 0, rollDof?.axis?.z ?? 0)

    //     console.debug(`Anchor Point: ${joltVec3ToString(anchorPoint)}`)
    //     console.debug(`Pitch Axis: ${joltVec3ToString(pitchAxis)} ${pitchDof?.limits ? `[${pitchDof.limits.lower!.toFixed(3)}, ${pitchDof.limits.upper!.toFixed(3)}]` : ''}`)
    //     console.debug(`Yaw Axis: ${joltVec3ToString(yawAxis)} ${yawDof?.limits ? `[${yawDof.limits.lower!.toFixed(3)}, ${yawDof.limits.upper!.toFixed(3)}]` : ''}`)
    //     console.debug(`Roll Axis: ${joltVec3ToString(rollAxis)} ${rollDof?.limits ? `[${rollDof.limits.lower!.toFixed(3)}, ${rollDof.limits.upper!.toFixed(3)}]` : ''}`)

    //     const constraints: { axis: Jolt.Vec3, friction: number, value: number, upper?: number, lower?: number }[] = []

    //     if (pitchDof?.limits && (pitchDof.limits.upper ?? 0) - (pitchDof.limits.lower ?? 0) < 0.001) {
    //         console.debug('Pitch Fixed')
    //     } else {
    //         constraints.push({
    //             axis: pitchAxis,
    //             friction: 0.0,
    //             value: pitchDof?.value ?? 0,
    //             upper: pitchDof?.limits ? pitchDof.limits.upper ?? 0 : undefined,
    //             lower: pitchDof?.limits ? pitchDof.limits.lower ?? 0 : undefined
    //         })
    //     }

    //     if (yawDof?.limits && (yawDof.limits.upper ?? 0) - (yawDof.limits.lower ?? 0) < 0.001) {
    //         console.debug('Yaw Fixed')
    //     } else {
    //         constraints.push({
    //             axis: yawAxis,
    //             friction: 0.0,
    //             value: yawDof?.value ?? 0,
    //             upper: yawDof?.limits ? yawDof.limits.upper ?? 0 : undefined,
    //             lower: yawDof?.limits ? yawDof.limits.lower ?? 0 : undefined
    //         })
    //     }

    //     if (rollDof?.limits && (rollDof.limits.upper ?? 0) - (rollDof.limits.lower ?? 0) < 0.001) {
    //         console.debug('Roll Fixed')
    //     } else {
    //         constraints.push({
    //             axis: rollAxis,
    //             friction: 0.0,
    //             value: rollDof?.value ?? 0,
    //             upper: rollDof?.limits ? rollDof.limits.upper ?? 0 : undefined,
    //             lower: rollDof?.limits ? rollDof.limits.lower ?? 0 : undefined
    //         })
    //     }

    //     let bodyStart = bodyB
    //     let bodyNext = bodyA
    //     if (constraints.length > 1) {
    //         console.debug('Starting with Ghost Body')
    //         bodyNext = this.CreateGhostBody(anchorPoint)
    //         this._joltBodyInterface.AddBody(bodyNext.GetID(), JOLT.EActivation_Activate)
    //         mechanism.ghostBodies.push(bodyNext.GetID())
    //     }
    //     for (let i = 0; i < constraints.length; ++i) {
    //         console.debug(`Constraint ${i}`)
    //         const c = constraints[i]
    //         const hingeSettings = new JOLT.HingeConstraintSettings()
    //         hingeSettings.mMaxFrictionTorque = c.friction;
    //         hingeSettings.mPoint1 = hingeSettings.mPoint2 = anchorPoint
    //         hingeSettings.mHingeAxis1 = hingeSettings.mHingeAxis2 = c.axis.Normalized()
    //         hingeSettings.mNormalAxis1 = hingeSettings.mNormalAxis2 = getPerpendicular(
    //             hingeSettings.mHingeAxis1
    //         )
    //         if (c.upper && c.lower) {
    //             // Some values that are meant to be exactly PI are perceived as being past it, causing unexpected behavior.
    //             // This safety check caps the values to be within [-PI, PI] wth minimal difference in precision.
    //             const piSafetyCheck = (v: number) => Math.min(3.14158, Math.max(-3.14158, v))

    //             const currentPos = piSafetyCheck(c.value)
    //             const upper = piSafetyCheck(c.upper) - currentPos
    //             const lower = piSafetyCheck(c.lower) - currentPos

    //             hingeSettings.mLimitsMin = -upper
    //             hingeSettings.mLimitsMax = -lower
    //         }

    //         const hingeConstraint = hingeSettings.Create(bodyStart, bodyNext)
    //         this._joltPhysSystem.AddConstraint(hingeConstraint)
    //         this._constraints.push(hingeConstraint)
    //         bodyStart = bodyNext
    //         if (i == constraints.length - 2) {
    //             bodyNext = bodyA
    //             console.debug('Finishing with Body A')
    //         } else {
    //             console.debug('New Ghost Body')
    //             bodyNext = this.CreateGhostBody(anchorPoint)
    //             this._joltBodyInterface.AddBody(bodyNext.GetID(), JOLT.EActivation_Activate)
    //             mechanism.ghostBodies.push(bodyNext.GetID())
    //         }
    //     }
    // }

    private isWheel(jDef: mirabuf.joint.Joint): boolean {
        return (jDef.info?.name !== "grounded" && (jDef.userData?.data?.wheel ?? "false") === "true") ?? false
    }

    /**
     * Creates a map, mapping the name of RigidNodes to Jolt BodyIDs
     *
     * @param   parser  MirabufParser containing properly parsed RigidNodes
     * @returns Mapping of Jolt BodyIDs
     */
    public createBodiesFromParser(parser: MirabufParser, layerReserve?: LayerReserve): Map<string, Jolt.BodyID> {
        const rnToBodies = new Map<string, Jolt.BodyID>()

        if ((parser.assembly.dynamic && !layerReserve) || layerReserve?.isReleased) {
            throw new Error("No layer reserve for dynamic assembly")
        }

        const reservedLayer: number | undefined = layerReserve?.layer

        const nonPhysicsNodes = filterNonPhysicsNodes([...parser.rigidNodes.values()], parser.assembly)

        const massMod = (() => {
            let assemblyMass = 0
            nonPhysicsNodes.forEach(x => {
                assemblyMass += x.mass
            })

            return parser.assembly.dynamic && assemblyMass > MAX_ROBOT_MASS ? MAX_ROBOT_MASS / assemblyMass : 1
        })()

        nonPhysicsNodes.forEach(rn => {
            const compoundShapeSettings = new JOLT.StaticCompoundShapeSettings()
            let shapesAdded = 0

            let totalMass = 0

            type FrictionPairing = {
                dynamic: number
                static: number
                weight: number
            }
            const frictionAccum: FrictionPairing[] = []

            const comAccum = new mirabuf.Vector3()

            const minBounds = new JOLT.Vec3(1000000.0, 1000000.0, 1000000.0)
            const maxBounds = new JOLT.Vec3(-1000000.0, -1000000.0, -1000000.0)

            const rnLayer: number = reservedLayer
                ? reservedLayer
                : rn.id.endsWith(GAMEPIECE_SUFFIX)
                  ? LAYER_GENERAL_DYNAMIC
                  : LAYER_FIELD

            rn.parts.forEach(partId => {
                const partInstance = parser.assembly.data!.parts!.partInstances![partId]!
                if (partInstance.skipCollider) return

                const partDefinition =
                    parser.assembly.data!.parts!.partDefinitions![partInstance.partDefinitionReference!]!

                const debugLabel = {
                    rn: rn.id,
                    partId,
                    defRef: partInstance.partDefinitionReference,
                    name: partDefinition.info?.name ?? partInstance.info?.name ?? "(unnamed)",
                }

                const partShapeResult = rn.isDynamic
                    ? this.createConvexShapeSettingsFromPart(partDefinition)
                    : this.createConcaveShapeSettingsFromPart(partDefinition, debugLabel)
                // const partShapeResult = this.CreateConvexShapeSettingsFromPart(partDefinition)

                if (!partShapeResult) {
                    console.warn("Skipping collider (no valid shape settings)", debugLabel)
                    return
                }

                const [shapeSettings, partMin, partMax] = partShapeResult

                const transform = convertThreeMatrix4ToJoltMat44(parser.globalTransforms.get(partId)!)
                const translation = transform.GetTranslation()
                const rotation = transform.GetQuaternion()
                compoundShapeSettings.AddShape(translation, rotation, shapeSettings, 0)
                shapesAdded++

                this.updateMinMaxBounds(transform.Multiply3x3(partMin), minBounds, maxBounds)
                this.updateMinMaxBounds(transform.Multiply3x3(partMax), minBounds, maxBounds)

                JOLT.destroy(partMin)
                JOLT.destroy(partMax)
                JOLT.destroy(transform)

                const physicalMaterial =
                    parser.assembly.data!.materials!.physicalMaterials![
                        partInstance.physicalMaterial ?? DEFAULT_PHYSICAL_MATERIAL_KEY
                    ]

                if (physicalMaterial) {
                    let frictionOverride: number | undefined =
                        partDefinition?.frictionOverride == null ? undefined : partDefinition?.frictionOverride
                    if ((partDefinition?.frictionOverride ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD) {
                        frictionOverride = undefined
                    }

                    if (
                        (physicalMaterial.dynamicFriction ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD ||
                        (physicalMaterial.staticFriction ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD
                    ) {
                        physicalMaterial.dynamicFriction = DEFAULT_FRICTION
                        physicalMaterial.staticFriction = DEFAULT_FRICTION
                    }

                    // TODO: Consider using roughness as dynamic friction.
                    const frictionPairing: FrictionPairing = {
                        dynamic: frictionOverride ?? physicalMaterial.dynamicFriction!,
                        static: frictionOverride ?? physicalMaterial.staticFriction!,
                        weight: partDefinition.physicalData?.area ?? 1.0,
                    }
                    frictionAccum.push(frictionPairing)
                } else {
                    const frictionPairing: FrictionPairing = {
                        dynamic: DEFAULT_FRICTION,
                        static: DEFAULT_FRICTION,
                        weight: partDefinition.physicalData?.area ?? 1.0,
                    }
                    frictionAccum.push(frictionPairing)
                }

                if (!partDefinition.physicalData?.com || !partDefinition.physicalData.mass) return

                const mass = partDefinition.massOverride
                    ? partDefinition.massOverride!
                    : partDefinition.physicalData.mass!

                totalMass += mass

                comAccum.x += (partDefinition.physicalData.com.x! * mass) / 100.0
                comAccum.y += (partDefinition.physicalData.com.y! * mass) / 100.0
                comAccum.z += (partDefinition.physicalData.com.z! * mass) / 100.0
            })

            if (shapesAdded > 0) {
                const shapeResult = compoundShapeSettings.Create()

                if (!shapeResult.IsValid || shapeResult.HasError()) {
                    // May want to consider crashing here.
                    // Unclear if the whole import is impossible if we reach this control step.
                    console.error(`Failed to create shape for RigidNode ${rn.id}\n${shapeResult.GetError().c_str()}`)
                    JOLT.destroy(compoundShapeSettings)
                    return
                }

                const shape = shapeResult.Get()

                if (rn.isDynamic) {
                    if (rn.isGamePiece) {
                        const mass = totalMass == 0.0 ? 1 : Math.min(totalMass, MAX_GP_MASS)
                        shape.GetMassProperties().mMass = mass
                    } else {
                        shape.GetMassProperties().mMass = totalMass == 0.0 ? 1 : totalMass * massMod
                    }
                }

                const bodySettings = new JOLT.BodyCreationSettings(
                    shape,
                    new JOLT.RVec3(0.0, 0.0, 0.0),
                    new JOLT.Quat(0, 0, 0, 1),
                    rn.isDynamic ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
                    rnLayer
                )
                const body = this._joltBodyInterface.CreateBody(bodySettings)
                this._joltBodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)
                body.SetAllowSleeping(false)
                rnToBodies.set(rn.id, body.GetID())

                // Set Friction Here
                let staticFriction = 0.0
                let dynamicFriction = 0.0
                let weightSum = 0.0
                frictionAccum.forEach(pairing => {
                    staticFriction += pairing.static * pairing.weight
                    dynamicFriction += pairing.dynamic * pairing.weight
                    weightSum += pairing.weight
                })
                staticFriction /= weightSum == 0.0 ? 1.0 : weightSum
                dynamicFriction /= weightSum == 0.0 ? 1.0 : weightSum

                // I guess this is an okay substitute.
                const friction = (staticFriction + dynamicFriction) / 2.0
                body.SetFriction(friction)

                // Little testing components
                this._bodies.push(body.GetID())
                body.SetRestitution(0.4)
            }
            // Cleanup
            JOLT.destroy(compoundShapeSettings)
        })

        return rnToBodies
    }

    /**
     * Creates the Jolt ShapeSettings for a given part using the Part Definition of said part.
     *
     * @param   partDefinition  Definition of the part to create.
     * @returns If successful, the created convex hull shape settings from the given Part Definition.
     */
    private createConvexShapeSettingsFromPart(
        partDefinition: mirabuf.IPartDefinition
    ): [Jolt.ShapeSettings, Jolt.Vec3, Jolt.Vec3] | undefined {
        const settings = new JOLT.ConvexHullShapeSettings()

        const min = new JOLT.Vec3(1000000.0, 1000000.0, 1000000.0)
        const max = new JOLT.Vec3(-1000000.0, -1000000.0, -1000000.0)

        const points = settings.mPoints
        partDefinition.bodies!.forEach(body => {
            const verts = body.triangleMesh?.mesh?.verts
            if (!verts) return

            for (let i = 0; i < verts.length; i += 3) {
                const vert = convertMirabufFloatToArrJoltVec3(verts, i)
                points.push_back(vert)
                this.updateMinMaxBounds(vert, min, max)
                JOLT.destroy(vert)
            }
        })

        if (points.size() < 4) {
            JOLT.destroy(settings)
            JOLT.destroy(min)
            JOLT.destroy(max)
            return
        }

        return [settings, min, max]
    }

    /**
     * Creates the Jolt ShapeSettings for a given part using the Part Definition of said part.
     *
     * @param   partDefinition  Definition of the part to create.
     * @returns If successful, the created convex hull shape settings from the given Part Definition.
     */
    private createConcaveShapeSettingsFromPart(
        partDefinition: mirabuf.IPartDefinition,
        debugLabel?: Record<string, unknown>
    ): [Jolt.ShapeSettings, Jolt.Vec3, Jolt.Vec3] | undefined {
        const settings = new JOLT.MeshShapeSettings()

        settings.mMaxTrianglesPerLeaf = 4

        settings.mTriangleVertices = new JOLT.VertexList()
        settings.mIndexedTriangles = new JOLT.IndexedTriangleList()
        settings.mMaterials = new JOLT.PhysicsMaterialList()

        settings.mMaterials.push_back(new JOLT.PhysicsMaterial())

        const min = new JOLT.Vec3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY)
        const max = new JOLT.Vec3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY)

        let maxIndex = -1
        partDefinition.bodies!.forEach(body => {
            const vertArr = body.triangleMesh?.mesh?.verts
            const indexArr = body.triangleMesh?.mesh?.indices
            if (!vertArr || !indexArr) return
            if (indexArr.length < 3 || indexArr.length % 3 !== 0) return

            for (let i = 0; i < vertArr.length; i += 3) {
                const vert = convertMirabufFloatToArrJoltFloat3(vertArr, i)
                settings.mTriangleVertices.push_back(vert)
                this.updateMinMaxBounds(new JOLT.Vec3(vert), min, max)
                JOLT.destroy(vert)
            }

            for (let i = 0; i < indexArr.length; i += 3) {
                const a = indexArr.at(i)!
                const b = indexArr.at(i + 1)!
                const c = indexArr.at(i + 2)!
                if (a > maxIndex) maxIndex = a
                if (b > maxIndex) maxIndex = b
                if (c > maxIndex) maxIndex = c
                settings.mIndexedTriangles.push_back(new JOLT.IndexedTriangle(a, b, c, 0))
            }
        })

        const vertCount = settings.mTriangleVertices.size()
        const triCountBeforeSanitize = settings.mIndexedTriangles.size()

        if (vertCount < 3 || triCountBeforeSanitize === 0 || maxIndex >= vertCount) {
            if (debugLabel) {
                console.warn("Concave collider invalid (no triangles or bad indices)", {
                    ...debugLabel,
                    vertCount,
                    triCount: triCountBeforeSanitize,
                    maxIndex,
                })
            }

            JOLT.destroy(settings)
            JOLT.destroy(min)
            JOLT.destroy(max)
            return
        }

        settings.Sanitize()
        const triCount = settings.mIndexedTriangles.size()
        if (triCount === 0) {
            if (debugLabel) {
                console.warn("Concave collider sanitized to zero triangles (degenerate)", {
                    ...debugLabel,
                    vertCount,
                    triCountBeforeSanitize,
                })
            }

            JOLT.destroy(settings)
            JOLT.destroy(min)
            JOLT.destroy(max)
            return
        }

        return [settings, min, max]
    }

    /**
     * Raycast a ray into the physics scene.
     *
     * @param from Originating point of the ray
     * @param dir Direction of the ray. Note: Length of dir specifies the maximum length it will check.
     * @returns Either the hit results of the closest object in the ray's path, or undefined if nothing was hit.
     */
    public rayCast(from: Jolt.Vec3, dir: Jolt.Vec3, ...ignoreBodies: Jolt.BodyID[]): RayCastHit | undefined {
        const ray = new JOLT.RRayCast(convertJoltVec3ToJoltRVec3(from), dir)

        const raySettings = new JOLT.RayCastSettings()
        raySettings.mTreatConvexAsSolid = false
        const collector = new JOLT.CastRayClosestHitCollisionCollector()
        const bpFilter = new JOLT.BroadPhaseLayerFilter()
        const objectFilter = new JOLT.ObjectLayerFilter()
        const bodyFilter = new JOLT.IgnoreMultipleBodiesFilter()
        const shapeFilter = new JOLT.ShapeFilter() // We don't want to filter out any shapes

        ignoreBodies.forEach(x => bodyFilter.IgnoreBody(x))

        this._joltPhysSystem
            .GetNarrowPhaseQuery()
            .CastRay(ray, raySettings, collector, bpFilter, objectFilter, bodyFilter, shapeFilter)

        if (!collector.HadHit()) return undefined

        const hitPoint = ray.GetPointOnRay(collector.mHit.mFraction)
        return { data: collector.mHit, point: convertJoltRVec3ToJoltVec3(hitPoint), ray: ray }
    }

    /**
     * Helper function to update min and max vector bounds.
     *
     * @param   v   Vector to add to min, max, bounds.
     * @param   min Minimum vector of the bounds.
     * @param   max Maximum vector of the bounds.
     */
    private updateMinMaxBounds(v: Jolt.Vec3, min: Jolt.Vec3, max: Jolt.Vec3) {
        if (v.GetX() < min.GetX()) min.SetX(v.GetX())
        if (v.GetY() < min.GetY()) min.SetY(v.GetY())
        if (v.GetZ() < min.GetZ()) min.SetZ(v.GetZ())

        if (v.GetX() > max.GetX()) max.SetX(v.GetX())
        if (v.GetY() > max.GetY()) max.SetY(v.GetY())
        if (v.GetZ() > max.GetZ()) max.SetZ(v.GetZ())
    }

    /**
     * Destroys bodies.
     *
     * @param bodies  Bodies to destroy.
     */
    public destroyBodies(...bodies: Jolt.Body[]) {
        bodies.forEach(x => {
            this._joltBodyInterface.RemoveBody(x.GetID())
            this._joltBodyInterface.DestroyBody(x.GetID())
        })
    }

    public destroyBodyIds(...bodies: Jolt.BodyID[]) {
        bodies.forEach(x => {
            if (this.isBodyAdded(x)) {
                this._joltBodyInterface.RemoveBody(x)
                this._joltBodyInterface.DestroyBody(x)
            }
        })
    }

    public destroyMechanism(mech: Mechanism) {
        mech.stepListeners.forEach(x => {
            this._joltPhysSystem.RemoveStepListener(x)
        })
        mech.constraints.forEach(x => {
            this._joltPhysSystem.RemoveConstraint(x.primaryConstraint)
        })
        mech.nodeToBody.forEach(x => {
            this._joltBodyInterface.RemoveBody(x)
            this._joltBodyInterface.DestroyBody(x)
        })
        mech.ghostBodies.forEach(x => {
            this._joltBodyInterface.RemoveBody(x)
            this._joltBodyInterface.DestroyBody(x)
        })
    }

    public getBody(bodyId: Jolt.BodyID): Jolt.Body {
        return this._joltPhysSystem.GetBodyLockInterface().TryGetBody(bodyId)
    }

    public update(deltaT: number): void {
        if (this._pauseSet.size > 0) {
            return
        }

        const diffDeltaT = deltaT - lastDeltaT

        lastDeltaT += Math.min(TIMESTEP_ADJUSTMENT, Math.max(-TIMESTEP_ADJUSTMENT, diffDeltaT))
        lastDeltaT = Math.min(MAX_SIMULATION_PERIOD, Math.max(MIN_SIMULATION_PERIOD, lastDeltaT))

        let substeps = Math.max(1, Math.floor((lastDeltaT / STANDARD_SIMULATION_PERIOD) * STANDARD_SUB_STEPS))
        substeps = Math.min(MAX_SUBSTEPS, Math.max(MIN_SUBSTEPS, substeps))

        this._joltInterface.Step(lastDeltaT, substeps)

        if (World.multiplayerSystem != null) {
            const interObjectCollisions = this._physicsEventQueue.filter(
                x => x instanceof OnContactAddedEvent && this.onSameLayer(x.message.body1, x.message.body2)
            )

            World.multiplayerSystem.getOwnSceneObjectIDs().forEach(clientSceneObjectId => {
                const clientSceneObject = World.sceneRenderer.sceneObjects.get(
                    clientSceneObjectId
                ) as MirabufSceneObject

                if (clientSceneObject == null) {
                    console.warn("Could not find multiplayer robot") // happens when you delete
                    World.multiplayerSystem?.unregisterOwnSceneObject(clientSceneObjectId)
                    return
                }
                const touchedBodies = clientSceneObject.mechanism.touchedObjects

                const message: Message =
                    interObjectCollisions.length > 0
                        ? {
                              type: "collision",
                              data: World.sceneRenderer.mirabufSceneObjects
                                  .getAll()
                                  .map(object => object.getUpdateData())
                                  .filter(n => n != null),
                          }
                        : {
                              type: "update",
                              data: [clientSceneObject, ...touchedBodies]
                                  .map(object => object.getUpdateData())
                                  .filter(n => n != null),
                          }
                World.multiplayerSystem?.broadcast(message)

                if (clientSceneObjectId != null) {
                    clientSceneObject.mechanism.touchedObjects = []
                }
            })
        }

        this._physicsEventQueue.forEach(x => x.dispatch())
        this._physicsEventQueue = []
    }

    private onSameLayer(body1: Jolt.BodyID, body2: Jolt.BodyID): boolean {
        return this.getBody(body1).GetObjectLayer() === this.getBody(body2).GetObjectLayer()
    }

    /*
     * Destroys PhysicsSystem and frees all objects
     */
    public destroy() {
        this._constraints.forEach(x => {
            this._joltPhysSystem.RemoveConstraint(x)
            // JOLT.destroy(x);
        })
        this._constraints = []

        // Destroy Jolt Bodies.
        this.destroyBodyIds(...this._bodies)
        this._bodies = []

        JOLT.destroy(this._joltBodyInterface)
        JOLT.destroy(this._joltInterface)
        JOLT.destroy(this._joltPhysSystem.GetContactListener())
    }

    private createGhostBody(position: Jolt.Vec3) {
        const size = new JOLT.Vec3(0.05, 0.05, 0.05)
        const shape = new JOLT.BoxShape(size)
        JOLT.destroy(size)

        const rot = new JOLT.Quat(0, 0, 0, 1)
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            convertJoltVec3ToJoltRVec3(position),
            rot,
            JOLT.EMotionType_Dynamic,
            LAYER_GHOST
        )
        creationSettings.mOverrideMassProperties = JOLT.EOverrideMassProperties_CalculateInertia
        creationSettings.mMassPropertiesOverride.mMass = 0.01

        const body = this._joltBodyInterface.CreateBody(creationSettings)
        JOLT.destroy(rot)
        JOLT.destroy(creationSettings)

        this._bodies.push(body.GetID())
        return body
    }

    public createSensor(shapeSettings: Jolt.ShapeSettings): Jolt.BodyID | undefined {
        const shape = shapeSettings.Create()
        if (shape.HasError()) {
            console.error(`Failed to create sensor body\n${shape.GetError().c_str}`)
            return undefined
        }
        const body = this.createBody(shape.Get(), undefined, undefined, undefined)
        this._bodies.push(body.GetID())
        body.SetIsSensor(true)
        this._joltBodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)
        return body.GetID()
    }

    /**
     * Exposes the SetPosition method on the _joltBodyInterface
     * Sets the position of the body
     *
     * @param id The id of the body
     * @param position The new position of the body
     */
    public setBodyPosition(id: Jolt.BodyID, position: Jolt.RVec3, activate: boolean = true): void {
        if (!this.isBodyAdded(id)) {
            return
        }

        this._joltBodyInterface.SetPosition(
            id,
            position,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
    }

    public setBodyRotation(id: Jolt.BodyID, rotation: Jolt.Quat, activate: boolean = true): void {
        if (!this.isBodyAdded(id)) return

        this._joltBodyInterface.SetRotation(
            id,
            rotation,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
    }

    public setBodyPositionAndRotation(
        id: Jolt.BodyID,
        position: Jolt.RVec3,
        rotation: Jolt.Quat,
        activate: boolean = true
    ): void {
        if (!this.isBodyAdded(id)) {
            return
        }

        this._joltBodyInterface.SetPositionAndRotation(
            id,
            position,
            rotation,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
    }

    public setBodyPositionRotationAndVelocity(
        id: Jolt.BodyID,
        position: Jolt.RVec3,
        rotation: Jolt.Quat,
        linear: Jolt.Vec3,
        angular: Jolt.Vec3,
        activate: boolean = true
    ): void {
        if (!this.isBodyAdded(id)) {
            return
        }

        this._joltBodyInterface.SetPositionAndRotation(
            id,
            position,
            rotation,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )

        this._joltBodyInterface.SetLinearVelocity(id, linear)
        this._joltBodyInterface.SetAngularVelocity(id, angular)
    }

    /**
     * Exposes SetShape method on the _joltBodyInterface
     * Sets the shape of the body
     *
     * @param id The id of the body
     * @param shape The new shape of the body
     * @param massProperties The mass properties of the new body
     * @param activationMode The activation mode of the new body
     */
    public setShape(
        id: Jolt.BodyID,
        shape: Jolt.Shape,
        massProperties: boolean,
        activationMode: Jolt.EActivation
    ): void {
        if (!this.isBodyAdded(id)) return

        this._joltBodyInterface.SetShape(id, shape, massProperties, activationMode)
    }

    /**
     * Finds the MirabufSceneObject containing the mechanism containing the body referenced by the given id
     */
    private bodyToMiraSceneObject(body: Jolt.Body): MirabufSceneObject | null {
        const id = body.GetID()
        return (
            World.sceneRenderer.mirabufSceneObjects.findWhere(obj =>
                [...obj.mechanism.nodeToBody].some(n => n[1] == id)
            ) ?? null
        )
    }

    /**
     * In multiplayer, returns whether the given jolt body is on the client's robot
     *
     * In singleplayer returns false
     */
    private isClient(body: Jolt.Body): boolean {
        return (
            (ROBOT_LAYERS.includes(body.GetObjectLayer()) &&
                World.multiplayerSystem
                    ?.getOwnSceneObjectIDs()
                    .includes(this.bodyToMiraSceneObject(body)?.id as LocalSceneObjectId)) ??
            false
        )
    }

    /**
     * Records the robot body as having touched another body
     * This is used for tracking which bodies the client needs to send the state of to peers
     */
    private recordOtherBodyCollision(robot?: Jolt.Body, other?: Jolt.Body) {
        if (other == null || robot == null) return

        const robotSceneObject = this.bodyToMiraSceneObject(robot)
        const otherSceneObject = this.bodyToMiraSceneObject(other)
        if (robotSceneObject == null || otherSceneObject == null) return

        robotSceneObject.mechanism.touchedObjects.push(otherSceneObject)
    }

    /**
     * Creates and assigns Jolt contact listener that dispatches events.
     *
     * @param physSystem The physics system the contact listener will attach to
     */
    private setUpContactListener(physSystem: Jolt.PhysicsSystem) {
        const contactListener = new JOLT.ContactListenerJS()

        contactListener.OnContactAdded = (bodyPtr1, bodyPtr2, manifoldPtr, settingsPtr) => {
            const body1 = JOLT.wrapPointer(bodyPtr1, JOLT.Body) as Jolt.Body
            const body2 = JOLT.wrapPointer(bodyPtr2, JOLT.Body) as Jolt.Body

            const body1Id = new JOLT.BodyID(body1.GetID().GetIndexAndSequenceNumber())
            const body2Id = new JOLT.BodyID(body2.GetID().GetIndexAndSequenceNumber())

            const message: CurrentContactData = {
                body1: body1Id,
                body2: body2Id,
                manifold: JOLT.wrapPointer(manifoldPtr, JOLT.ContactManifold) as Jolt.ContactManifold,
                settings: JOLT.wrapPointer(settingsPtr, JOLT.ContactSettings) as Jolt.ContactSettings,
            }

            // Detect if a robot is touching a gp, then push to the robot's touched list
            const [clientBody, otherBody] = this.isClient(body1)
                ? [body1, body2]
                : this.isClient(body2)
                  ? [body2, body1]
                  : [undefined, undefined]
            this.recordOtherBodyCollision(clientBody, otherBody)

            this._physicsEventQueue.push(new OnContactAddedEvent(message))
        }

        contactListener.OnContactPersisted = (bodyPtr1, bodyPtr2, manifoldPtr, settingsPtr) => {
            const body1 = JOLT.wrapPointer(bodyPtr1, JOLT.Body) as Jolt.Body
            const body2 = JOLT.wrapPointer(bodyPtr2, JOLT.Body) as Jolt.Body

            const body1Id = body1.GetID()
            const body2Id = body2.GetID()

            const message: CurrentContactData = {
                body1: body1Id,
                body2: body2Id,
                manifold: JOLT.wrapPointer(manifoldPtr, JOLT.ContactManifold) as Jolt.ContactManifold,
                settings: JOLT.wrapPointer(settingsPtr, JOLT.ContactSettings) as Jolt.ContactSettings,
            }

            this._physicsEventQueue.push(new OnContactPersistedEvent(message))
        }

        contactListener.OnContactRemoved = subShapePairPtr => {
            const shapePair = JOLT.wrapPointer(subShapePairPtr, JOLT.SubShapeIDPair) as Jolt.SubShapeIDPair

            new OnContactRemovedEvent(shapePair)
        }

        contactListener.OnContactValidate = (bodyPtr1, bodyPtr2, inBaseOffsetPtr, inCollisionResultPtr) => {
            const message: OnContactValidateData = {
                body1: JOLT.wrapPointer(bodyPtr1, JOLT.Body) as Jolt.Body,
                body2: JOLT.wrapPointer(bodyPtr2, JOLT.Body) as Jolt.Body,
                baseOffset: JOLT.wrapPointer(inBaseOffsetPtr, JOLT.RVec3) as Jolt.RVec3,
                collisionResult: JOLT.wrapPointer(
                    inCollisionResultPtr,
                    JOLT.CollideShapeResult
                ) as Jolt.CollideShapeResult,
            }

            this._physicsEventQueue.push(new OnContactValidateEvent(message))

            return JOLT.ValidateResult_AcceptAllContactsForThisBodyPair
        }

        physSystem.SetContactListener(contactListener)
    }
}

export class LayerReserve {
    private _layer: number
    private _isReleased: boolean

    public get layer() {
        return this._layer
    }
    public get isReleased() {
        return this._isReleased
    }

    public constructor() {
        this._layer = ROBOT_LAYERS.shift()!
        this._isReleased = false
    }

    public release(): void {
        if (this._isReleased) return

        ROBOT_LAYERS.push(this._layer)
        this._isReleased = true
    }
}

/**
 * Initialize collision groups and filtering for Jolt.
 *
 * @param   settings    Jolt object used for applying filters.
 */
function setupCollisionFiltering(settings: Jolt.JoltSettings) {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS)

    // Enable Field layer collisions
    objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, LAYER_GENERAL_DYNAMIC)
    objectFilter.EnableCollision(LAYER_FIELD, LAYER_GENERAL_DYNAMIC)
    ROBOT_LAYERS.forEach(layer => {
        objectFilter.EnableCollision(LAYER_FIELD, layer)
        objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, layer)
    })

    // Enable Collisions between other robots

    for (let i = 0; i < ROBOT_LAYERS.length - 1; i++) {
        for (let j = i + 1; j < ROBOT_LAYERS.length; j++) {
            objectFilter.EnableCollision(ROBOT_LAYERS[i], ROBOT_LAYERS[j])
        }
    }

    const BP_LAYER_FIELD = new JOLT.BroadPhaseLayer(LAYER_FIELD)
    const BP_LAYER_GENERAL_DYNAMIC = new JOLT.BroadPhaseLayer(LAYER_GENERAL_DYNAMIC)

    const bpRobotLayers = ROBOT_LAYERS.map(layer => new JOLT.BroadPhaseLayer(layer))

    const COUNT_BROAD_PHASE_LAYERS = 2 + ROBOT_LAYERS.length

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS)

    bpInterface.MapObjectToBroadPhaseLayer(LAYER_FIELD, BP_LAYER_FIELD)
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_GENERAL_DYNAMIC, BP_LAYER_GENERAL_DYNAMIC)
    bpRobotLayers.forEach((bpRobot, i) => {
        bpInterface.MapObjectToBroadPhaseLayer(ROBOT_LAYERS[i], bpRobot)
    })

    settings.mObjectLayerPairFilter = objectFilter
    settings.mBroadPhaseLayerInterface = bpInterface
    settings.mObjectVsBroadPhaseLayerFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(
        settings.mBroadPhaseLayerInterface,
        COUNT_BROAD_PHASE_LAYERS,
        settings.mObjectLayerPairFilter,
        COUNT_OBJECT_LAYERS
    )
}

function filterNonPhysicsNodes(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly): RigidNodeReadOnly[] {
    return nodes.filter(x => {
        for (const part of x.parts) {
            const inst = mira.data!.parts!.partInstances![part]!
            const def = mira.data!.parts!.partDefinitions![inst.partDefinitionReference!]!
            if (def.bodies && def.bodies.length > 0) {
                return true
            }
        }
        return false
    })
}

function getPerpendicular(vec: Jolt.Vec3): Jolt.Vec3 {
    return tryGetPerpendicular(vec, new JOLT.Vec3(0, 1, 0)) ?? tryGetPerpendicular(vec, new JOLT.Vec3(0, 0, 1))!
}

function tryGetPerpendicular(vec: Jolt.Vec3, toCheck: Jolt.Vec3): Jolt.Vec3 | undefined {
    if (Math.abs(Math.abs(vec.Dot(toCheck)) - 1.0) < 0.0001) return undefined

    const a = vec.Dot(toCheck)
    return new JOLT.Vec3(
        toCheck.GetX() - vec.GetX() * a,
        toCheck.GetY() - vec.GetY() * a,
        toCheck.GetZ() - vec.GetZ() * a
    ).Normalized()
}

export type RayCastHit = {
    data: Jolt.RayCastResult
    point: Jolt.Vec3
    ray: Jolt.RRayCast
}

export default PhysicsSystem
