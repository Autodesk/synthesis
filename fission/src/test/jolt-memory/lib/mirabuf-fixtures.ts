// In-memory `mirabuf.Assembly` fixtures for the memory-audit suite. jsdom has no network and no
// `.mira` file is checked in, so exercising `PhysicsSystem`'s load/unload path
// (createBodiesFromParser/createJointsFromParser/createWheelConstraint/destroyMechanism) requires
// building a valid `mirabuf.Assembly` directly. Like factories.ts, every field exists because some
// code on that call chain dereferences it (traced through MirabufParser.ts,
// ConstraintSettingsUtilities.ts, PhysicsSystem.ts).
import { mirabuf } from "@/proto/mirabuf"

// Divided by 100 by convertMirabufFloatToArrJoltVec3/convertMirabufVector3ToJoltVec3 (cm -> m).
function boxVerts(halfExtentCm: number): number[] {
    const h = halfExtentCm
    const corners: number[] = []
    for (const x of [-h, h]) {
        for (const y of [-h, h]) {
            for (const z of [-h, h]) {
                corners.push(x, y, z)
            }
        }
    }
    return corners
}

function identityTransformAt(xCm: number, yCm: number, zCm: number): mirabuf.ITransform {
    // Row-major 4x4, see convertMirabufTransformToThreeMatrix: arr[3]/[7]/[11] are the translation.
    return {
        spatialMatrix: [1, 0, 0, xCm, 0, 1, 0, yCm, 0, 0, 1, zCm, 0, 0, 0, 1],
    }
}

// MirabufInstance.createBatchedMeshes skips any body missing normals/uv/indices, which would
// leave batches empty, computeBoundingBox() empty, and MirabufSceneObject.setObjectPosition's
// Jolt-heavy branch (`Number.isFinite(bounds.min.y)`) permanently short-circuited. These are
// throwaway values (not checked for visual correctness), just present with the right lengths.
function boxRenderData(): { normals: number[]; uv: number[]; indices: number[] } {
    const vertexCount = 8
    const normals: number[] = []
    const uv: number[] = []
    for (let i = 0; i < vertexCount; i++) {
        normals.push(0, 0, 1)
        uv.push(0, 0)
    }
    // Valid vertex indices (0-7), not required to form a geometrically closed cube for this suite.
    const indices = [0, 1, 2, 1, 2, 3, 2, 3, 4, 3, 4, 5, 4, 5, 6, 5, 6, 7, 0, 2, 4, 1, 3, 5, 2, 4, 6, 3, 5, 7]
    return { normals, uv, indices }
}

function boxPartDefinition(guid: string, halfExtentCm: number, massGrams: number): mirabuf.IPartDefinition {
    const { normals, uv, indices } = boxRenderData()
    return {
        info: { GUID: guid, name: guid },
        physicalData: {
            mass: massGrams,
            volume: (2 * halfExtentCm) ** 3,
            area: 6 * (2 * halfExtentCm) ** 2,
            com: { x: 0, y: 0, z: 0 },
        },
        bodies: [
            {
                info: { GUID: `${guid}-body` },
                triangleMesh: { mesh: { verts: boxVerts(halfExtentCm), normals, uv, indices } },
            },
        ],
    }
}

export type WheeledRobotAssembly = {
    assembly: mirabuf.Assembly
    chassisPartId: string
    wheelPartId: string
    wheelJointId: string
}

/**
 * Minimal dynamic (robot) assembly: one chassis body and one wheel body connected by a single
 * REVOLUTE + `userData.wheel="true"` joint, enough to drive `createBodiesFromParser` (2 bodies),
 * `createJointsFromParser` -> `isWheel` -> `createWheelConstraint` (fixed + vehicle constraint,
 * `VehicleCollisionTesterCastCylinder`, `VehicleConstraintStepListener`), and `destroyMechanism`.
 *
 * `variant`, when given, makes every GUID/part-def id and body dimension depend on it, so cycling
 * tests build a genuinely distinct assembly each time instead of re-parsing the same object graph,
 * closer to real "load robot, delete it, load a different one" usage.
 */
export function createWheeledRobotAssembly(variant = 0): WheeledRobotAssembly {
    const chassisPartId = `chassis-${variant}`
    const wheelPartId = `wheel-${variant}`
    const chassisDefId = `chassisDef-${variant}`
    const wheelDefId = `wheelDef-${variant}`
    const wheelJointDefId = `wheelJointDef-${variant}`
    const wheelJointId = `wheelJoint-${variant}`
    const chassisHalfExtentCm = 15 + variant
    const wheelHalfExtentCm = 5 + (variant % 3)
    const chassisMassGrams = 4000 + variant * 100
    const wheelMassGrams = 500 + variant * 10

    const assembly = new mirabuf.Assembly({
        info: { GUID: `synthetic-wheeled-robot-${variant}`, name: `Synthetic Wheeled Robot ${variant}`, version: 6 },
        dynamic: true,
        designHierarchy: {
            nodes: [
                { value: chassisPartId, children: [] },
                { value: wheelPartId, children: [] },
            ],
        },
        data: {
            parts: {
                partDefinitions: {
                    [chassisDefId]: boxPartDefinition(chassisDefId, chassisHalfExtentCm, chassisMassGrams),
                    [wheelDefId]: boxPartDefinition(wheelDefId, wheelHalfExtentCm, wheelMassGrams),
                },
                partInstances: {
                    [chassisPartId]: {
                        info: { GUID: chassisPartId, name: chassisPartId },
                        partDefinitionReference: chassisDefId,
                        transform: identityTransformAt(0, 0, 0),
                        physicalMaterial: "default",
                    },
                    [wheelPartId]: {
                        info: { GUID: wheelPartId, name: wheelPartId },
                        partDefinitionReference: wheelDefId,
                        transform: identityTransformAt(30, -10, 0),
                        physicalMaterial: "default",
                    },
                },
            },
            joints: {
                jointDefinitions: {
                    [wheelJointDefId]: {
                        info: { name: "WheelJoint" },
                        jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
                        origin: { x: 30, y: -10, z: 0 },
                        rotational: {
                            rotationalFreedom: {
                                axis: { x: 0, y: 0, z: 1 },
                            },
                        },
                        userData: { data: { wheel: "true" } },
                    },
                },
                jointInstances: {
                    // MirabufParser special-cases this key: identifies the assembly's grounded part.
                    grounded: {
                        parts: { nodes: [{ value: chassisPartId, children: [] }] },
                    },
                    [wheelJointId]: {
                        info: { name: "WheelJoint" },
                        parentPart: chassisPartId,
                        childPart: wheelPartId,
                        jointReference: wheelJointDefId,
                        offset: { x: 0, y: 0, z: 0 },
                    },
                },
                rigidGroups: [],
                motorDefinitions: {},
            },
            materials: {
                physicalMaterials: {
                    default: { dynamicFriction: 0.6, staticFriction: 0.6 },
                },
                appearances: {},
            },
        },
    })

    return { assembly, chassisPartId, wheelPartId, wheelJointId }
}

export type DrivetrainAssembly = {
    assembly: mirabuf.Assembly
    chassisPartId: string
    wheelPartId: string
    hingePartId: string
    sliderPartId: string
    wheelJointId: string
    hingeJointId: string
    sliderJointId: string
}

/**
 * Chassis (grounded) + one REVOLUTE `userData.wheel="true"` joint (-> `VehicleConstraint`, see
 * {@link createWheeledRobotAssembly}), one plain REVOLUTE joint with limits (-> `HingeConstraint`),
 * and one SLIDER joint with limits (-> `SliderConstraint`): the three `MechanismConstraint`
 * subtypes `SimulationSystem.ts`'s driver-construction switch
 * (`EConstraintSubType_Hinge`/`_Vehicle`/`_Slider`) dispatches on, all off one mechanism, so
 * HingeDriver/SliderDriver/WheelDriver construct the same way real code does.
 */
export function createFullDrivetrainAssembly(variant = 0): DrivetrainAssembly {
    const chassisPartId = `dt-chassis-${variant}`
    const wheelPartId = `dt-wheel-${variant}`
    const hingePartId = `dt-hinge-${variant}`
    const sliderPartId = `dt-slider-${variant}`
    const chassisDefId = `dt-chassisDef-${variant}`
    const wheelDefId = `dt-wheelDef-${variant}`
    const hingeDefId = `dt-hingeDef-${variant}`
    const sliderDefId = `dt-sliderDef-${variant}`
    const wheelJointDefId = `dt-wheelJointDef-${variant}`
    const hingeJointDefId = `dt-hingeJointDef-${variant}`
    const sliderJointDefId = `dt-sliderJointDef-${variant}`
    const wheelJointId = `dt-wheelJoint-${variant}`
    const hingeJointId = `dt-hingeJoint-${variant}`
    const sliderJointId = `dt-sliderJoint-${variant}`

    const chassisHalfExtentCm = 15 + variant
    const wheelHalfExtentCm = 5 + (variant % 3)
    const hingeHalfExtentCm = 6 + (variant % 3)
    const sliderHalfExtentCm = 4 + (variant % 3)
    const chassisMassGrams = 4000 + variant * 100
    const wheelMassGrams = 500 + variant * 10
    const hingeMassGrams = 300 + variant * 10
    const sliderMassGrams = 200 + variant * 10

    const assembly = new mirabuf.Assembly({
        info: { GUID: `synthetic-drivetrain-${variant}`, name: `Synthetic Drivetrain ${variant}`, version: 6 },
        dynamic: true,
        designHierarchy: {
            nodes: [
                { value: chassisPartId, children: [] },
                { value: wheelPartId, children: [] },
                { value: hingePartId, children: [] },
                { value: sliderPartId, children: [] },
            ],
        },
        data: {
            parts: {
                partDefinitions: {
                    [chassisDefId]: boxPartDefinition(chassisDefId, chassisHalfExtentCm, chassisMassGrams),
                    [wheelDefId]: boxPartDefinition(wheelDefId, wheelHalfExtentCm, wheelMassGrams),
                    [hingeDefId]: boxPartDefinition(hingeDefId, hingeHalfExtentCm, hingeMassGrams),
                    [sliderDefId]: boxPartDefinition(sliderDefId, sliderHalfExtentCm, sliderMassGrams),
                },
                partInstances: {
                    [chassisPartId]: {
                        info: { GUID: chassisPartId, name: chassisPartId },
                        partDefinitionReference: chassisDefId,
                        transform: identityTransformAt(0, 0, 0),
                        physicalMaterial: "default",
                    },
                    [wheelPartId]: {
                        info: { GUID: wheelPartId, name: wheelPartId },
                        partDefinitionReference: wheelDefId,
                        transform: identityTransformAt(30, -10, 0),
                        physicalMaterial: "default",
                    },
                    [hingePartId]: {
                        info: { GUID: hingePartId, name: hingePartId },
                        partDefinitionReference: hingeDefId,
                        transform: identityTransformAt(-30, 10, 0),
                        physicalMaterial: "default",
                    },
                    [sliderPartId]: {
                        info: { GUID: sliderPartId, name: sliderPartId },
                        partDefinitionReference: sliderDefId,
                        transform: identityTransformAt(0, 30, 0),
                        physicalMaterial: "default",
                    },
                },
            },
            joints: {
                jointDefinitions: {
                    [wheelJointDefId]: {
                        info: { name: "WheelJoint" },
                        jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
                        origin: { x: 30, y: -10, z: 0 },
                        rotational: {
                            rotationalFreedom: {
                                axis: { x: 0, y: 0, z: 1 },
                            },
                        },
                        userData: { data: { wheel: "true" } },
                    },
                    [hingeJointDefId]: {
                        info: { name: "HingeJoint" },
                        jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
                        origin: { x: -30, y: 10, z: 0 },
                        rotational: {
                            rotationalFreedom: {
                                axis: { x: 0, y: 0, z: 1 },
                                limits: { lower: -1.2, upper: 1.2 },
                                value: 0,
                            },
                        },
                    },
                    [sliderJointDefId]: {
                        info: { name: "SliderJoint" },
                        jointMotionType: mirabuf.joint.JointMotion.SLIDER,
                        origin: { x: 0, y: 30, z: 0 },
                        prismatic: {
                            prismaticFreedom: {
                                axis: { x: 0, y: 1, z: 0 },
                                limits: { lower: -10, upper: 10 },
                                value: 0,
                            },
                        },
                    },
                },
                jointInstances: {
                    // MirabufParser special-cases this key: identifies the assembly's grounded part.
                    grounded: {
                        parts: { nodes: [{ value: chassisPartId, children: [] }] },
                    },
                    [wheelJointId]: {
                        info: { name: "WheelJoint" },
                        parentPart: chassisPartId,
                        childPart: wheelPartId,
                        jointReference: wheelJointDefId,
                        offset: { x: 0, y: 0, z: 0 },
                    },
                    [hingeJointId]: {
                        info: { name: "HingeJoint" },
                        parentPart: chassisPartId,
                        childPart: hingePartId,
                        jointReference: hingeJointDefId,
                        offset: { x: 0, y: 0, z: 0 },
                    },
                    [sliderJointId]: {
                        info: { name: "SliderJoint" },
                        parentPart: chassisPartId,
                        childPart: sliderPartId,
                        jointReference: sliderJointDefId,
                        offset: { x: 0, y: 0, z: 0 },
                    },
                },
                rigidGroups: [],
                motorDefinitions: {},
            },
            materials: {
                physicalMaterials: {
                    default: { dynamicFriction: 0.6, staticFriction: 0.6 },
                },
                appearances: {},
            },
        },
    })

    return {
        assembly,
        chassisPartId,
        wheelPartId,
        hingePartId,
        sliderPartId,
        wheelJointId,
        hingeJointId,
        sliderJointId,
    }
}

export type ArmAssembly = {
    assembly: mirabuf.Assembly
    chassisPartId: string
    armPartId: string
    hingeJointId: string
}

/**
 * Chassis (grounded) + one plain REVOLUTE joint with limits (-> `HingeConstraint`), no wheel
 * joint: a wheel-less robot, so `SynthesisBrain.configure()`'s `createSkidSteerDriveBehavior` runs
 * with zero `WheelDriver`s. Used by synthesis-brain-lifecycle.test.ts to exercise
 * `applyUnstickForce` without going through the wheel-detection loop, which throws in this suite's
 * plain-Node Jolt build regardless of any fix here since `mechanism.constraints[i].primaryConstraint`
 * is always base-`Constraint`-typed (never pre-cast to a subtype before storage), so `instanceof
 * JOLT.TwoBodyConstraint` never holds without a `JOLT.castObject` this suite's build doesn't
 * perform automatically, unlike a real browser WASM build's embind RTTI (suspected). A separate,
 * pre-existing correctness question, out of scope for this memory audit.
 */
export function createArmAssembly(variant = 0): ArmAssembly {
    const chassisPartId = `arm-chassis-${variant}`
    const armPartId = `arm-arm-${variant}`
    const chassisDefId = `arm-chassisDef-${variant}`
    const armDefId = `arm-armDef-${variant}`
    const hingeJointDefId = `arm-hingeJointDef-${variant}`
    const hingeJointId = `arm-hingeJoint-${variant}`

    const chassisHalfExtentCm = 15 + variant
    const armHalfExtentCm = 6 + (variant % 3)
    const chassisMassGrams = 4000 + variant * 100
    const armMassGrams = 300 + variant * 10

    const assembly = new mirabuf.Assembly({
        info: { GUID: `synthetic-arm-${variant}`, name: `Synthetic Arm ${variant}`, version: 6 },
        dynamic: true,
        designHierarchy: {
            nodes: [
                { value: chassisPartId, children: [] },
                { value: armPartId, children: [] },
            ],
        },
        data: {
            parts: {
                partDefinitions: {
                    [chassisDefId]: boxPartDefinition(chassisDefId, chassisHalfExtentCm, chassisMassGrams),
                    [armDefId]: boxPartDefinition(armDefId, armHalfExtentCm, armMassGrams),
                },
                partInstances: {
                    [chassisPartId]: {
                        info: { GUID: chassisPartId, name: chassisPartId },
                        partDefinitionReference: chassisDefId,
                        transform: identityTransformAt(0, 0, 0),
                        physicalMaterial: "default",
                    },
                    [armPartId]: {
                        info: { GUID: armPartId, name: armPartId },
                        partDefinitionReference: armDefId,
                        transform: identityTransformAt(-30, 10, 0),
                        physicalMaterial: "default",
                    },
                },
            },
            joints: {
                jointDefinitions: {
                    [hingeJointDefId]: {
                        info: { name: "HingeJoint" },
                        jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
                        origin: { x: -30, y: 10, z: 0 },
                        rotational: {
                            rotationalFreedom: {
                                axis: { x: 0, y: 0, z: 1 },
                                limits: { lower: -1.2, upper: 1.2 },
                                value: 0,
                            },
                        },
                    },
                },
                jointInstances: {
                    grounded: {
                        parts: { nodes: [{ value: chassisPartId, children: [] }] },
                    },
                    [hingeJointId]: {
                        info: { name: "HingeJoint" },
                        parentPart: chassisPartId,
                        childPart: armPartId,
                        jointReference: hingeJointDefId,
                        offset: { x: 0, y: 0, z: 0 },
                    },
                },
                rigidGroups: [],
                motorDefinitions: {},
            },
            materials: {
                physicalMaterials: {
                    default: { dynamicFriction: 0.6, staticFriction: 0.6 },
                },
                appearances: {},
            },
        },
    })

    return { assembly, chassisPartId, armPartId, hingeJointId }
}
