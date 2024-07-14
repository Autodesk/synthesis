import {
    JoltVec3_ThreeVector3,
    MirabufFloatArr_JoltFloat3,
    MirabufFloatArr_JoltVec3,
    MirabufVector3_JoltVec3,
    ThreeMatrix4_JoltMat44,
    ThreeVector3_JoltVec3,
    _JoltQuat,
} from "../../util/TypeConversions"
import JOLT from "../../util/loading/JoltSyncLoader"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import { mirabuf } from "../../proto/mirabuf"
import MirabufParser, { GAMEPIECE_SUFFIX, GROUNDED_JOINT_ID, RigidNodeReadOnly } from "../../mirabuf/MirabufParser"
import WorldSystem from "../WorldSystem"
import Mechanism from "./Mechanism"

export type JoltBodyIndexAndSequence = number

/**
 * Layers used for determining enabled/disabled collisions.
 */
const LAYER_FIELD = 0 // Used for grounded rigid node of a field as well as any rigid nodes jointed to it.
const LAYER_GENERAL_DYNAMIC = 1 // Used for game pieces or any general dynamic objects that can collide with anything and everything.
const RobotLayers: number[] = [
    // Reserved layers for robots. Robot layers have no collision with themselves but have collision with everything else.
    2,
    3, 4, 5, 6, 7, 8, 9,
]

// Layer for ghost object in god mode, interacts with nothing
const LAYER_GHOST = 10

// Please update this accordingly.
const COUNT_OBJECT_LAYERS = 11

export const STANDARD_SIMULATION_PERIOD = 1.0 / 60.0
const MIN_SIMULATION_PERIOD = 1.0 / 120.0
const MAX_SIMULATION_PERIOD = 1.0 / 10.0
const MIN_SUBSTEPS = 2
const MAX_SUBSTEPS = 6
const STANDARD_SUB_STEPS = 4
const TIMESTEP_ADJUSTMENT = 0.0001

let lastDeltaT = STANDARD_SIMULATION_PERIOD
export function GetLastDeltaT(): number {
    return lastDeltaT
}

// Friction constants
const FLOOR_FRICTION = 0.7
const SUSPENSION_MIN_FACTOR = 0.1
const SUSPENSION_MAX_FACTOR = 0.3

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

    private _pauseCounter = 0

    private _bodyAssociations: Map<JoltBodyIndexAndSequence, BodyAssociated>

    public get isPaused(): boolean {
        return this._pauseCounter > 0
    }

    /**
     * Creates a PhysicsSystem object.
     */
    constructor() {
        super()

        this._bodies = []
        this._constraints = []

        const joltSettings = new JOLT.JoltSettings()
        SetupCollisionFiltering(joltSettings)

        this._joltInterface = new JOLT.JoltInterface(joltSettings)
        JOLT.destroy(joltSettings)

        this._joltPhysSystem = this._joltInterface.GetPhysicsSystem()
        this._joltBodyInterface = this._joltPhysSystem.GetBodyInterface()

        this._joltPhysSystem.SetGravity(new JOLT.Vec3(0, -9.8, 0))

        const ground = this.CreateBox(
            new THREE.Vector3(5.0, 0.5, 5.0),
            undefined,
            new THREE.Vector3(0.0, -2.0, 0.0),
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
    public GetBodyAssociation<T extends object & BodyAssociated>(bodyId: Jolt.BodyID): T | undefined {
        const res = this._bodyAssociations.get(bodyId.GetIndexAndSequenceNumber())
        if (res) {
            // Avoids error, simply returns undefined if invalid
            return res as unknown as T
        } else {
            return res
        }
    }

    /**
     * Sets assocation for a body
     *
     * @param assocation Assocation. See {@link BodyAssociated}
     */
    public SetBodyAssociation<T extends BodyAssociated>(assocation: T) {
        this._bodyAssociations.set(assocation.associatedBody, assocation)
    }

    public RemoveBodyAssocation(bodyId: Jolt.BodyID) {
        this._bodyAssociations.delete(bodyId.GetIndexAndSequenceNumber())
    }

    /**
     * Holds a pause.
     *
     * The pause works off of a request counter.
     */
    public HoldPause() {
        this._pauseCounter++
    }

    /**
     * Forces all holds on the pause to be released.
     */
    public ForceUnpause() {
        this._pauseCounter = 0
    }

    /**
     * Releases a pause.
     *
     * The pause works off of a request counter.
     */
    public ReleasePause() {
        if (this._pauseCounter > 0) {
            this._pauseCounter--
        }
    }

    /**
     * Disabling physics for a single body
     *
     * @param bodyId
     */
    public DisablePhysicsForBody(bodyId: Jolt.BodyID) {
        if (!this.IsBodyAdded(bodyId)) {
            return
        }

        this._joltBodyInterface.DeactivateBody(bodyId)
        this.GetBody(bodyId).SetIsSensor(true)
    }

    /**
     * Enabing physics for a single body
     *
     * @param bodyId
     */
    public EnablePhysicsForBody(bodyId: Jolt.BodyID) {
        if (!this.IsBodyAdded(bodyId)) {
            return
        }

        this._joltBodyInterface.ActivateBody(bodyId)
        this.GetBody(bodyId).SetIsSensor(false)
    }

    public IsBodyAdded(bodyId: Jolt.BodyID) {
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
    public CreateBox(
        halfExtents: THREE.Vector3,
        mass: number | undefined,
        position: THREE.Vector3 | undefined,
        rotation: THREE.Euler | THREE.Quaternion | undefined
    ) {
        const size = ThreeVector3_JoltVec3(halfExtents)
        const shape = new JOLT.BoxShape(size, 0.1)
        JOLT.destroy(size)

        const pos = position ? ThreeVector3_JoltVec3(position) : new JOLT.Vec3(0.0, 0.0, 0.0)
        const rot = _JoltQuat(rotation)
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            pos,
            rot,
            mass ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
            mass ? LAYER_GENERAL_DYNAMIC : LAYER_FIELD
        )
        if (mass) {
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
    public CreateBody(
        shape: Jolt.Shape,
        mass: number | undefined,
        position: THREE.Vector3 | undefined,
        rotation: THREE.Euler | THREE.Quaternion | undefined
    ) {
        const pos = position ? ThreeVector3_JoltVec3(position) : new JOLT.Vec3(0.0, 0.0, 0.0)
        const rot = _JoltQuat(rotation)
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            pos,
            rot,
            mass ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
            mass ? LAYER_GENERAL_DYNAMIC : LAYER_FIELD
        )
        if (mass) {
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
     * Utility function for creating convex hulls. Mostly used for Unit test validation.
     *
     * @param   points  Flat pack array of vector 3 components.
     * @param   density Density of the convex hull.
     * @returns Resulting shape.
     */
    public CreateConvexHull(points: Float32Array, density: number = 1.0): Jolt.ShapeResult {
        if (points.length % 3) {
            throw new Error(`Invalid size of points: ${points.length}`)
        }
        const settings = new JOLT.ConvexHullShapeSettings()
        settings.mPoints.clear()
        settings.mPoints.reserve(points.length / 3.0)
        for (let i = 0; i < points.length; i += 3) {
            settings.mPoints.push_back(new JOLT.Vec3(points[i], points[i + 1], points[i + 2]))
        }
        settings.mDensity = density
        return settings.Create()
    }

    public CreateMechanismFromParser(parser: MirabufParser): Mechanism {
        const layer = parser.assembly.dynamic ? new LayerReserve() : undefined
        const bodyMap = this.CreateBodiesFromParser(parser, layer)
        const rootBody = parser.rootNode
        const mechanism = new Mechanism(rootBody, bodyMap, parser.assembly.dynamic, layer)
        this.CreateJointsFromParser(parser, mechanism)
        return mechanism
    }

    /**
     * Creates all the joints for a mirabuf assembly given an already compiled mapping of rigid nodes to bodies.
     *
     * @param   parser      Mirabuf parser with complete set of rigid nodes and assembly data.
     * @param   mechanism   Mapping of the name of rigid groups to Jolt bodies. Retrieved from CreateBodiesFromParser.
     */
    public CreateJointsFromParser(parser: MirabufParser, mechanism: Mechanism) {
        const jointData = parser.assembly.data!.joints!
        for (const [jGuid, jInst] of Object.entries(jointData.jointInstances!) as [
            string,
            mirabuf.joint.JointInstance,
        ][]) {
            if (jGuid == GROUNDED_JOINT_ID) continue

            const rnA = parser.partToNodeMap.get(jInst.parentPart!)
            const rnB = parser.partToNodeMap.get(jInst.childPart!)

            if (!rnA || !rnB) {
                console.warn(`Skipping joint '${jInst.info!.name!}'. Couldn't find associated rigid nodes.`)
                continue
            } else if (rnA.id == rnB.id) {
                console.warn(
                    `Skipping joint '${jInst.info!.name!}'. Jointing the same parts. Likely in issue with Fusion Design structure.`
                )
                continue
            }

            const jDef = parser.assembly.data!.joints!.jointDefinitions![jInst.jointReference!]! as mirabuf.joint.Joint
            const bodyIdA = mechanism.GetBodyByNodeId(rnA.id)
            const bodyIdB = mechanism.GetBodyByNodeId(rnB.id)
            if (!bodyIdA || !bodyIdB) {
                console.warn(`Skipping joint '${jInst.info!.name!}'. Failed to find rigid nodes' associated bodies.`)
                continue
            }
            const bodyA = this.GetBody(bodyIdA)
            const bodyB = this.GetBody(bodyIdB)

            const constraints: Jolt.Constraint[] = []
            let listener: Jolt.PhysicsStepListener | undefined = undefined

            switch (jDef.jointMotionType!) {
                case mirabuf.joint.JointMotion.REVOLUTE:
                    if (this.IsWheel(jDef)) {
                        if (parser.directedGraph.GetAdjacencyList(rnA.id).length > 0) {
                            const res = this.CreateWheelConstraint(
                                jInst,
                                jDef,
                                bodyA,
                                bodyB,
                                parser.assembly.info!.version!
                            )
                            constraints.push(res[0])
                            constraints.push(res[1])
                            listener = res[2]
                        } else {
                            const res = this.CreateWheelConstraint(
                                jInst,
                                jDef,
                                bodyB,
                                bodyA,
                                parser.assembly.info!.version!
                            )
                            constraints.push(res[0])
                            constraints.push(res[1])
                            listener = res[2]
                        }
                    } else {
                        constraints.push(
                            this.CreateHingeConstraint(jInst, jDef, bodyA, bodyB, parser.assembly.info!.version!)
                        )
                    }
                    break
                case mirabuf.joint.JointMotion.SLIDER:
                    constraints.push(this.CreateSliderConstraint(jInst, jDef, bodyA, bodyB))
                    break
                default:
                    console.debug("Unsupported joint detected. Skipping...")
                    break
            }

            if (constraints.length > 0) {
                constraints.forEach(x =>
                    mechanism.AddConstraint({
                        parentBody: bodyIdA,
                        childBody: bodyIdB,
                        constraint: x,
                    })
                )
            }
            if (listener) {
                mechanism.AddStepListener(listener)
            }
        }
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
    private CreateHingeConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        bodyA: Jolt.Body,
        bodyB: Jolt.Body,
        versionNum: number
    ): Jolt.Constraint {
        // HINGE CONSTRAINT
        const hingeConstraintSettings = new JOLT.HingeConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? MirabufVector3_JoltVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)
        // TODO: Offset transformation for robot builder.
        const jointOriginOffset = jointInstance.offset
            ? MirabufVector3_JoltVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)

        const anchorPoint = jointOrigin.Add(jointOriginOffset)
        hingeConstraintSettings.mPoint1 = hingeConstraintSettings.mPoint2 = anchorPoint

        const rotationalFreedom = jointDefinition.rotational!.rotationalFreedom!

        const miraAxis = rotationalFreedom.axis! as mirabuf.Vector3
        let axis: Jolt.Vec3
        // No scaling, these are unit vectors
        if (versionNum < 5) {
            axis = new JOLT.Vec3(-miraAxis.x ?? 0, miraAxis.y ?? 0, miraAxis.z! ?? 0)
        } else {
            axis = new JOLT.Vec3(miraAxis.x! ?? 0, miraAxis.y! ?? 0, miraAxis.z! ?? 0)
        }
        hingeConstraintSettings.mHingeAxis1 = hingeConstraintSettings.mHingeAxis2 = axis.Normalized()
        hingeConstraintSettings.mNormalAxis1 = hingeConstraintSettings.mNormalAxis2 = getPerpendicular(
            hingeConstraintSettings.mHingeAxis1
        )

        // Some values that are meant to be exactly PI are perceived as being past it, causing unexpected behavior.
        // This safety check caps the values to be within [-PI, PI] wth minimal difference in precision.
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

        const constraint = hingeConstraintSettings.Create(bodyA, bodyB)
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
    private CreateSliderConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        bodyA: Jolt.Body,
        bodyB: Jolt.Body
    ): Jolt.Constraint {
        const sliderConstraintSettings = new JOLT.SliderConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? MirabufVector3_JoltVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)
        // TODO: Offset transformation for robot builder.
        const jointOriginOffset = jointInstance.offset
            ? MirabufVector3_JoltVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)

        const anchorPoint = jointOrigin.Add(jointOriginOffset)
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

        const constraint = sliderConstraintSettings.Create(bodyA, bodyB)

        this._constraints.push(constraint)
        this._joltPhysSystem.AddConstraint(constraint)

        return constraint
    }

    public CreateWheelConstraint(
        jointInstance: mirabuf.joint.JointInstance,
        jointDefinition: mirabuf.joint.Joint,
        bodyMain: Jolt.Body,
        bodyWheel: Jolt.Body,
        versionNum: number
    ): [Jolt.Constraint, Jolt.VehicleConstraint, Jolt.PhysicsStepListener] {
        // HINGE CONSTRAINT
        const fixedSettings = new JOLT.FixedConstraintSettings()

        const jointOrigin = jointDefinition.origin
            ? MirabufVector3_JoltVec3(jointDefinition.origin as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)
        const jointOriginOffset = jointInstance.offset
            ? MirabufVector3_JoltVec3(jointInstance.offset as mirabuf.Vector3)
            : new JOLT.Vec3(0, 0, 0)

        const anchorPoint = jointOrigin.Add(jointOriginOffset)
        fixedSettings.mPoint1 = fixedSettings.mPoint2 = anchorPoint

        const rotationalFreedom = jointDefinition.rotational!.rotationalFreedom!

        const miraAxis = rotationalFreedom.axis! as mirabuf.Vector3
        let axis: Jolt.Vec3
        // No scaling, these are unit vectors
        if (versionNum < 5) {
            axis = new JOLT.Vec3(-miraAxis.x ?? 0, miraAxis.y ?? 0, miraAxis.z ?? 0)
        } else {
            axis = new JOLT.Vec3(miraAxis.x ?? 0, miraAxis.y ?? 0, miraAxis.z ?? 0)
        }

        const bounds = bodyWheel.GetShape().GetLocalBounds()
        const radius = (bounds.mMax.GetY() - bounds.mMin.GetY()) / 2.0

        const wheelSettings = new JOLT.WheelSettingsWV()
        wheelSettings.mPosition = anchorPoint.Add(axis.Mul(0.1))
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

        const controllerSettings = new JOLT.WheeledVehicleControllerSettings()
        controllerSettings.mEngine.mMaxTorque = 1500.0
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

        this._joltPhysSystem.AddConstraint(vehicleConstraint)
        this._joltPhysSystem.AddConstraint(fixedConstraint)

        this._constraints.push(fixedConstraint, vehicleConstraint)
        return [fixedConstraint, vehicleConstraint, listener]
    }

    private IsWheel(jDef: mirabuf.joint.Joint) {
        return (
            jDef.info!.name! != "grounded" &&
            jDef.userData &&
            (new Map(Object.entries(jDef.userData.data!)).get("wheel") ?? "false") == "true"
        )
    }

    /**
     * Creates a map, mapping the name of RigidNodes to Jolt BodyIDs
     *
     * @param   parser  MirabufParser containing properly parsed RigidNodes
     * @returns Mapping of Jolt BodyIDs
     */
    public CreateBodiesFromParser(parser: MirabufParser, layerReserve?: LayerReserve): Map<string, Jolt.BodyID> {
        const rnToBodies = new Map<string, Jolt.BodyID>()

        if ((parser.assembly.dynamic && !layerReserve) || layerReserve?.isReleased) {
            throw new Error("No layer reserve for dynamic assembly")
        }

        const reservedLayer: number | undefined = layerReserve?.layer

        filterNonPhysicsNodes([...parser.rigidNodes.values()], parser.assembly).forEach(rn => {
            const compoundShapeSettings = new JOLT.StaticCompoundShapeSettings()
            let shapesAdded = 0

            let totalMass = 0
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
                if (
                    partInstance.skipCollider == null ||
                    partInstance == undefined ||
                    partInstance.skipCollider == false
                ) {
                    const partDefinition =
                        parser.assembly.data!.parts!.partDefinitions![partInstance.partDefinitionReference!]!

                    const partShapeResult = rn.isDynamic
                        ? this.CreateConvexShapeSettingsFromPart(partDefinition)
                        : this.CreateConcaveShapeSettingsFromPart(partDefinition)

                    if (partShapeResult) {
                        const [shapeSettings, partMin, partMax] = partShapeResult

                        const transform = ThreeMatrix4_JoltMat44(parser.globalTransforms.get(partId)!)
                        const translation = transform.GetTranslation()
                        const rotation = transform.GetQuaternion()
                        compoundShapeSettings.AddShape(translation, rotation, shapeSettings, 0)
                        shapesAdded++

                        this.UpdateMinMaxBounds(transform.Multiply3x3(partMin), minBounds, maxBounds)
                        this.UpdateMinMaxBounds(transform.Multiply3x3(partMax), minBounds, maxBounds)

                        JOLT.destroy(partMin)
                        JOLT.destroy(partMax)
                        JOLT.destroy(transform)

                        if (
                            partDefinition.physicalData &&
                            partDefinition.physicalData.com &&
                            partDefinition.physicalData.mass
                        ) {
                            const mass = partDefinition.massOverride
                                ? partDefinition.massOverride!
                                : partDefinition.physicalData.mass!
                            totalMass += mass
                            comAccum.x += (partDefinition.physicalData.com.x! * mass) / 100.0
                            comAccum.y += (partDefinition.physicalData.com.y! * mass) / 100.0
                            comAccum.z += (partDefinition.physicalData.com.z! * mass) / 100.0
                        }
                    }
                }
            })

            if (shapesAdded > 0) {
                const shapeResult = compoundShapeSettings.Create()

                if (!shapeResult.IsValid || shapeResult.HasError()) {
                    console.error(`Failed to create shape for RigidNode ${rn.id}\n${shapeResult.GetError().c_str()}`)
                }

                const shape = shapeResult.Get()

                if (rn.isDynamic) {
                    shape.GetMassProperties().mMass = totalMass == 0.0 ? 1 : totalMass
                }

                const bodySettings = new JOLT.BodyCreationSettings(
                    shape,
                    new JOLT.Vec3(0.0, 0.0, 0.0),
                    new JOLT.Quat(0, 0, 0, 1),
                    rn.isDynamic ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
                    rnLayer
                )
                const body = this._joltBodyInterface.CreateBody(bodySettings)
                this._joltBodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)
                body.SetAllowSleeping(false)
                rnToBodies.set(rn.id, body.GetID())

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
    private CreateConvexShapeSettingsFromPart(
        partDefinition: mirabuf.IPartDefinition
    ): [Jolt.ShapeSettings, Jolt.Vec3, Jolt.Vec3] | undefined | null {
        const settings = new JOLT.ConvexHullShapeSettings()

        const min = new JOLT.Vec3(1000000.0, 1000000.0, 1000000.0)
        const max = new JOLT.Vec3(-1000000.0, -1000000.0, -1000000.0)

        const points = settings.mPoints
        partDefinition.bodies!.forEach(body => {
            if (body.triangleMesh && body.triangleMesh.mesh && body.triangleMesh.mesh.verts) {
                const vertArr = body.triangleMesh.mesh.verts
                for (let i = 0; i < body.triangleMesh.mesh.verts.length; i += 3) {
                    const vert = MirabufFloatArr_JoltVec3(vertArr, i)
                    points.push_back(vert)
                    this.UpdateMinMaxBounds(vert, min, max)
                    JOLT.destroy(vert)
                }
            }
        })

        if (points.size() < 4) {
            JOLT.destroy(settings)
            JOLT.destroy(min)
            JOLT.destroy(max)
            return
        } else {
            return [settings, min, max]
        }
    }

    /**
     * Creates the Jolt ShapeSettings for a given part using the Part Definition of said part.
     *
     * @param   partDefinition  Definition of the part to create.
     * @returns If successful, the created convex hull shape settings from the given Part Definition.
     */
    private CreateConcaveShapeSettingsFromPart(
        partDefinition: mirabuf.IPartDefinition
    ): [Jolt.ShapeSettings, Jolt.Vec3, Jolt.Vec3] | undefined | null {
        const settings = new JOLT.MeshShapeSettings()

        settings.mMaxTrianglesPerLeaf = 8

        settings.mTriangleVertices = new JOLT.VertexList()
        settings.mIndexedTriangles = new JOLT.IndexedTriangleList()
        settings.mMaterials = new JOLT.PhysicsMaterialList()

        settings.mMaterials.push_back(new JOLT.PhysicsMaterial())

        const min = new JOLT.Vec3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY)
        const max = new JOLT.Vec3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY)

        partDefinition.bodies!.forEach(body => {
            const vertArr = body.triangleMesh?.mesh?.verts
            const indexArr = body.triangleMesh?.mesh?.indices
            if (!vertArr || !indexArr) return
            for (let i = 0; i < vertArr.length; i += 3) {
                const vert = MirabufFloatArr_JoltFloat3(vertArr, i)
                settings.mTriangleVertices.push_back(vert)
                this.UpdateMinMaxBounds(new JOLT.Vec3(vert), min, max)
                JOLT.destroy(vert)
            }
            for (let i = 0; i < indexArr.length; i += 3) {
                settings.mIndexedTriangles.push_back(
                    new JOLT.IndexedTriangle(indexArr.at(i)!, indexArr.at(i + 1)!, indexArr.at(i + 2)!, 0)
                )
            }
        })

        if (settings.mTriangleVertices.size() < 4) {
            JOLT.destroy(settings)
            JOLT.destroy(min)
            JOLT.destroy(max)
            return
        } else {
            settings.Sanitize()
            return [settings, min, max]
        }
    }

    /**
     * Raycast a ray into the physics scene.
     *
     * @param from Originating point of the ray
     * @param dir Direction of the ray. Note: Length of dir specifies the maximum length it will check.
     * @returns Either the hit results of the closest object in the ray's path, or undefined if nothing was hit.
     */
    public RayCast(from: Jolt.Vec3, dir: Jolt.Vec3, ...ignoreBodies: Jolt.BodyID[]): RayCastHit | undefined {
        const ray = new JOLT.RayCast(from, dir)

        const raySettings = new JOLT.RayCastSettings()
        raySettings.mTreatConvexAsSolid = false
        const collector = new JOLT.CastRayClosestHitCollisionCollector()
        const bp_filter = new JOLT.BroadPhaseLayerFilter()
        const object_filter = new JOLT.ObjectLayerFilter()
        const body_filter = new JOLT.IgnoreMultipleBodiesFilter()
        const shape_filter = new JOLT.ShapeFilter() // We don't want to filter out any shapes

        ignoreBodies.forEach(x => body_filter.IgnoreBody(x))

        this._joltPhysSystem
            .GetNarrowPhaseQuery()
            .CastRay(ray, raySettings, collector, bp_filter, object_filter, body_filter, shape_filter)

        if (collector.HadHit()) {
            const hitPoint = ray.GetPointOnRay(collector.mHit.mFraction)
            return { data: collector.mHit, point: hitPoint, ray: ray }
        }

        return undefined
    }

    /**
     * Helper function to update min and max vector bounds.
     *
     * @param   v   Vector to add to min, max, bounds.
     * @param   min Minimum vector of the bounds.
     * @param   max Maximum vector of the bounds.
     */
    private UpdateMinMaxBounds(v: Jolt.Vec3, min: Jolt.Vec3, max: Jolt.Vec3) {
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
     * @param   bodies  Bodies to destroy.
     */
    public DestroyBodies(...bodies: Jolt.Body[]) {
        bodies.forEach(x => {
            this._joltBodyInterface.RemoveBody(x.GetID())
            this._joltBodyInterface.DestroyBody(x.GetID())
        })
    }

    public DestroyBodyIds(...bodies: Jolt.BodyID[]) {
        bodies.forEach(x => {
            this._joltBodyInterface.RemoveBody(x)
            this._joltBodyInterface.DestroyBody(x)
        })
    }

    public DestroyMechanism(mech: Mechanism) {
        mech.stepListeners.forEach(x => {
            this._joltPhysSystem.RemoveStepListener(x)
        })
        mech.constraints.forEach(x => {
            this._joltPhysSystem.RemoveConstraint(x.constraint)
        })
        mech.nodeToBody.forEach(x => {
            this._joltBodyInterface.RemoveBody(x)
            // this._joltBodyInterface.DestroyBody(x);
        })
    }

    public GetBody(bodyId: Jolt.BodyID) {
        return this._joltPhysSystem.GetBodyLockInterface().TryGetBody(bodyId)
    }

    public Update(deltaT: number): void {
        if (this._pauseCounter > 0) {
            return
        }

        const diffDeltaT = deltaT - lastDeltaT

        lastDeltaT = lastDeltaT + Math.min(TIMESTEP_ADJUSTMENT, Math.max(-TIMESTEP_ADJUSTMENT, diffDeltaT))
        lastDeltaT = Math.min(MAX_SIMULATION_PERIOD, Math.max(MIN_SIMULATION_PERIOD, lastDeltaT))

        let substeps = Math.max(1, Math.floor((lastDeltaT / STANDARD_SIMULATION_PERIOD) * STANDARD_SUB_STEPS))
        substeps = Math.min(MAX_SUBSTEPS, Math.max(MIN_SUBSTEPS, substeps))

        this._joltInterface.Step(lastDeltaT, substeps)
    }

    public Destroy(): void {
        this._constraints.forEach(x => {
            this._joltPhysSystem.RemoveConstraint(x)
            // JOLT.destroy(x);
        })
        this._constraints = []

        // Destroy Jolt Bodies.
        this.DestroyBodyIds(...this._bodies)
        this._bodies = []

        JOLT.destroy(this._joltBodyInterface)
        JOLT.destroy(this._joltInterface)
    }

    /**
     * Creates a ghost object and a distance constraint that connects it to the given body
     * The ghost body is part of the LAYER_GHOST which doesn't interact with any other layer
     * The caller is responsible for cleaning up the ghost body and the constraint
     *
     * @param id The id of the body to be attatched to and moved
     * @returns The ghost body and the constraint
     */

    public CreateGodModeBody(id: Jolt.BodyID, anchorPoint: Jolt.Vec3): [Jolt.Body, Jolt.Constraint] {
        const body = this.GetBody(id)
        const ghostBody = this.CreateBox(
            new THREE.Vector3(0.05, 0.05, 0.05),
            undefined,
            JoltVec3_ThreeVector3(anchorPoint),
            undefined
        )

        const ghostBodyId = ghostBody.GetID()
        this._joltBodyInterface.SetObjectLayer(ghostBodyId, LAYER_GHOST)
        this._joltBodyInterface.AddBody(ghostBodyId, JOLT.EActivation_Activate)
        this._bodies.push(ghostBodyId)

        const constraintSettings = new JOLT.PointConstraintSettings()
        constraintSettings.set_mPoint1(anchorPoint)
        constraintSettings.set_mPoint2(anchorPoint)
        const constraint = constraintSettings.Create(ghostBody, body)
        this._joltPhysSystem.AddConstraint(constraint)
        this._constraints.push(constraint)

        return [ghostBody, constraint]
    }

    public CreateSensor(shapeSettings: Jolt.ShapeSettings): Jolt.BodyID | undefined {
        const shape = shapeSettings.Create()
        if (shape.HasError()) {
            console.error(`Failed to create sensor body\n${shape.GetError().c_str}`)
            return undefined
        }
        const body = this.CreateBody(shape.Get(), undefined, undefined, undefined)
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
    public SetBodyPosition(id: Jolt.BodyID, position: Jolt.Vec3, activate: boolean = true): void {
        if (!this.IsBodyAdded(id)) {
            return
        }

        this._joltBodyInterface.SetPosition(
            id,
            position,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
    }

    public SetBodyRotation(id: Jolt.BodyID, rotation: Jolt.Quat, activate: boolean = true): void {
        if (!this.IsBodyAdded(id)) {
            return
        }

        this._joltBodyInterface.SetRotation(
            id,
            rotation,
            activate ? JOLT.EActivation_Activate : JOLT.EActivation_DontActivate
        )
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
        this._layer = RobotLayers.shift()!
        this._isReleased = false
    }

    public Release(): void {
        if (!this._isReleased) {
            RobotLayers.push(this._layer)
            this._isReleased = true
        }
    }
}

/**
 * Initialize collision groups and filtering for Jolt.
 *
 * @param   settings    Jolt object used for applying filters.
 */
function SetupCollisionFiltering(settings: Jolt.JoltSettings) {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS)

    // Enable Field layer collisions
    objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, LAYER_GENERAL_DYNAMIC)
    objectFilter.EnableCollision(LAYER_FIELD, LAYER_GENERAL_DYNAMIC)
    for (let i = 0; i < RobotLayers.length; i++) {
        objectFilter.EnableCollision(LAYER_FIELD, RobotLayers[i])
        objectFilter.EnableCollision(LAYER_GENERAL_DYNAMIC, RobotLayers[i])
    }

    // Enable Collisions between other robots
    for (let i = 0; i < RobotLayers.length - 1; i++) {
        for (let j = i + 1; j < RobotLayers.length; j++) {
            objectFilter.EnableCollision(RobotLayers[i], RobotLayers[j])
        }
    }

    const BP_LAYER_FIELD = new JOLT.BroadPhaseLayer(LAYER_FIELD)
    const BP_LAYER_GENERAL_DYNAMIC = new JOLT.BroadPhaseLayer(LAYER_GENERAL_DYNAMIC)

    const bpRobotLayers = new Array<Jolt.BroadPhaseLayer>(RobotLayers.length)
    for (let i = 0; i < bpRobotLayers.length; i++) {
        bpRobotLayers[i] = new JOLT.BroadPhaseLayer(RobotLayers[i])
    }

    const COUNT_BROAD_PHASE_LAYERS = 2 + RobotLayers.length

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS)

    bpInterface.MapObjectToBroadPhaseLayer(LAYER_FIELD, BP_LAYER_FIELD)
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_GENERAL_DYNAMIC, BP_LAYER_GENERAL_DYNAMIC)
    for (let i = 0; i < bpRobotLayers.length; i++) {
        bpInterface.MapObjectToBroadPhaseLayer(RobotLayers[i], bpRobotLayers[i])
    }

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
    if (Math.abs(Math.abs(vec.Dot(toCheck)) - 1.0) < 0.0001) {
        return undefined
    }

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
    ray: Jolt.RayCast
}

/**
 * An interface to create an association between a body and anything.
 */
export interface BodyAssociated {
    readonly associatedBody: JoltBodyIndexAndSequence
}

export default PhysicsSystem
