import Jolt from "@azaleacolburn/jolt-physics"
import PhysicsSystem, {
    DEFAULT_FRICTION,
    DEFAULT_PHYSICAL_MATERIAL_KEY,
    LAYER_FIELD,
    LAYER_GENERAL_DYNAMIC,
    LayerReserve,
} from "./PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { mirabuf } from "@/proto/mirabuf"
import MirabufParser, { GAMEPIECE_SUFFIX, RigidNodeId, RigidNodeReadOnly } from "@/mirabuf/MirabufParser"
import {
    convertMirabufFloatToArrJoltFloat3,
    convertMirabufFloatToArrJoltVec3,
    convertThreeMatrix4ToJoltMat44,
} from "@/util/TypeConversions"
import { isDefined } from "@/util/Utility"

const SIGNIFICANT_FRICTION_THRESHOLD = 0.05

const MAX_ROBOT_MASS = 250.0
const MAX_GP_MASS = 10.0

// Minimum threshold for Wadell sphericity to consider a convex hull a sphere.
// 2025 & 2026 spheres have a sphericity of 0.9999
// 2023 cube has a value of 0.9532
const MIN_SPHERICITY = 0.99

const SPHERE_GP_ANGULAR_DAMPING = 0.5
const SPHERE_GP_LINEAR_DAMPING = 0.1

type Part = [mirabuf.IPartDefinition, mirabuf.IPartInstance]

type FrictionPairing = {
    dynamic: number
    static: number
    weight: number
}

type PhysicalMaterialsMap = {
    [k: string]: mirabuf.material.IPhysicalMaterial
}

type PhysicalProperties = {
    totalMass: number
    totalArea: number
    totalVolume: number
    frictionAccumulation: FrictionPairing[]
    centerOfMass: mirabuf.Vector3
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

/**
 * Wadell sphericity of a solid from its volume and surface area. Returns a value in (0, 1],
 * approaching 1 as the solid approaches a perfect sphere, or 0 when the area is non-positive.
 * https://en.wikipedia.org/wiki/Sphericity
 *
 * @param volume    Solid volume (any unit).
 * @param area      Solid surface area (consistent unit. The measure is dimensionless).
 */
function computeSphericity(volume: number, area: number): number {
    if (volume <= 0 || area <= 0) return 0
    const volumeEquivalentSphereArea = Math.cbrt(Math.PI) * Math.pow(6 * volume, 2 / 3)
    return volumeEquivalentSphereArea / area
}

/**
 * Checks if the gamepiece is a spheroid. If it is, this function applies a sphere collider to it, to optimize collision handling.
 *
 * @returns Whether the sphere collider was applied
 */
function tryOptimizeSpheroid(centerOfMass: Jolt.Vec3, totalVolume: number, totalArea: number): Jolt.Shape | undefined {
    if (computeSphericity(totalVolume, totalArea) < MIN_SPHERICITY) {
        return undefined
    }

    const volumeMeters3 = totalVolume * 1e-6 // Convert cm^3 to m^3
    const radius = Math.max(Math.cbrt((3 * volumeMeters3) / (4 * Math.PI)), 0.01)

    const sphereSettings = new JOLT.SphereShapeSettings(radius)
    const identityRotation = new JOLT.Quat(0, 0, 0, 1)

    const offsetSettings = new JOLT.RotatedTranslatedShapeSettings(centerOfMass, identityRotation, sphereSettings)

    const shape = offsetSettings.Create().Get()

    JOLT.destroy(identityRotation)
    JOLT.destroy(sphereSettings)

    return shape
}

/**
 * Helper function to update min and max vector bounds.
 *
 * @param   v   Vector to add to min, max, bounds.
 * @param   min Minimum vector of the bounds.
 * @param   max Maximum vector of the bounds.
 */
function updateMinMaxBounds(v: Jolt.Vec3, min: Jolt.Vec3, max: Jolt.Vec3) {
    if (v.GetX() < min.GetX()) min.SetX(v.GetX())
    if (v.GetY() < min.GetY()) min.SetY(v.GetY())
    if (v.GetZ() < min.GetZ()) min.SetZ(v.GetZ())

    if (v.GetX() > max.GetX()) max.SetX(v.GetX())
    if (v.GetY() > max.GetY()) max.SetY(v.GetY())
    if (v.GetZ() > max.GetZ()) max.SetZ(v.GetZ())
}

function calculateAndSetBodyFriction(body: Jolt.Body, frictionAccum: FrictionPairing[]) {
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
}

function calculatePartFriction(
    partDefinition: mirabuf.IPartDefinition,
    physicalMaterial: mirabuf.material.IPhysicalMaterial | undefined
) {
    const friction = {
        dynamic: DEFAULT_FRICTION,
        static: DEFAULT_FRICTION,
        weight: partDefinition.physicalData?.area ?? 1.0,
    } satisfies FrictionPairing

    if (physicalMaterial) {
        let frictionOverride: number | undefined =
            partDefinition?.frictionOverride == null ? undefined : partDefinition?.frictionOverride

        const frictionOverrideIsInsignificant =
            (partDefinition?.frictionOverride ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD

        if (frictionOverrideIsInsignificant) frictionOverride = undefined

        const physicalFrictionIsInsignificant =
            (physicalMaterial.dynamicFriction ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD ||
            (physicalMaterial.staticFriction ?? 0.0) < SIGNIFICANT_FRICTION_THRESHOLD

        if (physicalFrictionIsInsignificant) {
            physicalMaterial.dynamicFriction = DEFAULT_FRICTION
            physicalMaterial.staticFriction = DEFAULT_FRICTION
        }

        // TODO: Consider using roughness as dynamic friction.
        friction.dynamic = frictionOverride ?? physicalMaterial.dynamicFriction!
        friction.static = frictionOverride ?? physicalMaterial.staticFriction!
    }

    return friction
}

function calculatePhysicalProperties(parts: Part[], physicalMaterials: PhysicalMaterialsMap): PhysicalProperties {
    // Accumulated geometry used to decide whether a game piece is sphere-like.
    let totalVolume = 0
    let totalArea = 0

    let totalMass = 0
    const frictionAccumulation: FrictionPairing[] = []

    // NOTE
    // `centerOfMass` if never used, but I'm leaving it in because it might be helpful later
    const centerOfMass = new mirabuf.Vector3()

    parts.forEach(([partDefinition, partInstance]) => {
        const physicalData = partDefinition.physicalData

        totalVolume += physicalData?.volume ?? 0
        totalArea += physicalData?.area ?? 0

        const physicalMaterial = physicalMaterials![partInstance.physicalMaterial ?? DEFAULT_PHYSICAL_MATERIAL_KEY]
        frictionAccumulation.push(calculatePartFriction(partDefinition, physicalMaterial))

        if (!physicalData?.com || !physicalData.mass) return

        const mass = partDefinition.massOverride ? partDefinition.massOverride : physicalData.mass
        totalMass += mass

        centerOfMass.x += (physicalData.com.x! * mass) / 100.0
        centerOfMass.y += (physicalData.com.y! * mass) / 100.0
        centerOfMass.z += (physicalData.com.z! * mass) / 100.0
    })

    return {
        totalMass,
        totalVolume,
        totalArea,
        frictionAccumulation,
        centerOfMass,
    } satisfies PhysicalProperties
}

function constructPartDefinition(
    partId: string,
    parser: MirabufParser,
    rn: RigidNodeReadOnly,
    compoundShapeSettings: Jolt.CompoundShapeSettings,
    minBounds: Jolt.Vec3,
    maxBounds: Jolt.Vec3
): [mirabuf.IPartDefinition, mirabuf.IPartInstance] | undefined {
    const parts = parser.assembly.data?.parts!

    const partInstance = parts.partInstances![partId]!
    if (partInstance.skipCollider) return undefined

    const partDefinition = parts.partDefinitions![partInstance.partDefinitionReference!]!

    const debugLabel = {
        rn: rn.id,
        partId,
        defRef: partInstance.partDefinitionReference,
        name: partDefinition.info?.name ?? partInstance.info?.name ?? "(unnamed)",
    }

    const partShapeResult = rn.isDynamic
        ? createConvexShapeSettingsFromPart(partDefinition)
        : createConcaveShapeSettingsFromPart(partDefinition, debugLabel)

    if (!partShapeResult) {
        console.warn("Skipping collider (no valid shape settings)", debugLabel)
        return undefined
    }

    const [shapeSettings, partMin, partMax] = partShapeResult

    const transform = convertThreeMatrix4ToJoltMat44(parser.globalTransforms.get(partId)!)
    const translation = transform.GetTranslation()
    const rotation = transform.GetQuaternion()

    // NOTE
    // `AddShape` consumes `translation` and `rotation`
    compoundShapeSettings.AddShape(translation, rotation, shapeSettings, 0)

    updateMinMaxBounds(transform.Multiply3x3(partMin), minBounds, maxBounds)
    updateMinMaxBounds(transform.Multiply3x3(partMax), minBounds, maxBounds)

    JOLT.destroy(partMin)
    JOLT.destroy(partMax)
    JOLT.destroy(transform)

    return [partDefinition, partInstance]
}

function constructBodyFromRigidNode(
    physicsSystem: PhysicsSystem,
    rn: RigidNodeReadOnly,
    parser: MirabufParser,
    massMod: number,
    minBounds: Jolt.Vec3,
    maxBounds: Jolt.Vec3,
    reservedLayer: number | undefined
): [RigidNodeId, Jolt.BodyID] | undefined {
    const compoundShapeSettings = new JOLT.StaticCompoundShapeSettings()

    const constructPartDefinitionOnShape = (partId: string) =>
        constructPartDefinition(partId, parser, rn, compoundShapeSettings, minBounds, maxBounds)

    // NOTE for reviewers
    // We clone the set here, so it's slightly slower than before, but I think that's worth it for the readability and concision
    const parts = [...rn.parts].map(constructPartDefinitionOnShape).filter(isDefined)
    if (parts.length === 0) {
        JOLT.destroy(compoundShapeSettings)
        return
    }

    const { totalMass, totalVolume, totalArea, frictionAccumulation } = calculatePhysicalProperties(
        parts,
        parser.assembly.data?.materials?.physicalMaterials!
    )

    const shapeResult = compoundShapeSettings.Create()
    if (!shapeResult.IsValid || shapeResult.HasError()) {
        // May want to consider crashing here.
        // Unclear if the whole import is impossible if we reach this control step.
        console.error(`Failed to create shape for RigidNode ${rn.id}\n${shapeResult.GetError().c_str()}`)
        JOLT.destroy(compoundShapeSettings)
        return
    }

    let shape = shapeResult.Get()
    let appliedSphereCollider = false

    if (rn.isDynamic) {
        if (rn.isGamePiece) {
            const newShape = tryOptimizeSpheroid(shape.GetCenterOfMass(), totalVolume, totalArea)
            if (newShape) {
                appliedSphereCollider = true
                shape = newShape
            }

            const mass = totalMass == 0.0 ? 1 : Math.min(totalMass, MAX_GP_MASS)
            shape.GetMassProperties().mMass = mass
        } else {
            shape.GetMassProperties().mMass = totalMass == 0.0 ? 1 : totalMass * massMod
        }
    }

    const rnLayer: number = reservedLayer
        ? reservedLayer
        : rn.id.endsWith(GAMEPIECE_SUFFIX)
          ? LAYER_GENERAL_DYNAMIC
          : LAYER_FIELD

    const body = physicsSystem.createBody(shape, undefined, undefined, undefined, rnLayer)
    body.SetAllowSleeping(false)

    calculateAndSetBodyFriction(body, frictionAccumulation)
    body.SetRestitution(0.4)

    physicsSystem.addBodyToSystem(body.GetID(), true)

    if (appliedSphereCollider) {
        body.GetMotionProperties().SetAngularDamping(SPHERE_GP_ANGULAR_DAMPING)
        body.GetMotionProperties().SetLinearDamping(SPHERE_GP_LINEAR_DAMPING)
        physicsSystem.newSphereBody(body.GetID())
    }

    JOLT.destroy(compoundShapeSettings)

    return [rn.id, body.GetID()]
}

function calculateMassModifier(nodes: RigidNodeReadOnly[], dynamic: boolean) {
    const assemblyMass = nodes.map(x => x.mass).reduce((acc, n) => acc + n)

    return dynamic && assemblyMass > MAX_ROBOT_MASS ? MAX_ROBOT_MASS / assemblyMass : 1
}
/**
 * Creates the Jolt ShapeSettings for a given part using the Part Definition of said part.
 *
 * @param   partDefinition  Definition of the part to create.
 * @returns If successful, the created convex hull shape settings from the given Part Definition.
 */
function createConvexShapeSettingsFromPart(
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
            updateMinMaxBounds(vert, min, max)
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
function createConcaveShapeSettingsFromPart(
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

            const vertVec = new JOLT.Vec3(vert)
            updateMinMaxBounds(vertVec, min, max)

            JOLT.destroy(vertVec)
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
 * Creates a jolt body for each rigid node in the assembly
 *
 * @param   parser  MirabufParser containing properly parsed RigidNodes
 * @returns A map from the ids of the rigid nodes to those of the jolt bodies
 */
export default function createBodiesFromParser(
    this: PhysicsSystem,
    parser: MirabufParser,
    layerReserve?: LayerReserve
): Map<string, Jolt.BodyID> {
    const dynamic = parser.assembly.dynamic
    if ((dynamic && !layerReserve) || layerReserve?.isReleased) {
        throw new Error("No layer reserve for dynamic assembly")
    }

    const reservedLayer: number | undefined = layerReserve?.layer

    const nonPhysicsNodes = filterNonPhysicsNodes([...parser.rigidNodes.values()], parser.assembly)

    const massMod = calculateMassModifier(nonPhysicsNodes, dynamic)

    const minBounds = new JOLT.Vec3(1000000.0, 1000000.0, 1000000.0)
    const maxBounds = new JOLT.Vec3(-1000000.0, -1000000.0, -1000000.0)

    const createBodyInBounds = (rn: RigidNodeReadOnly) =>
        constructBodyFromRigidNode(this, rn, parser, massMod, minBounds, maxBounds, reservedLayer)

    const rnToBodyPairs = nonPhysicsNodes.map(createBodyInBounds).filter(isDefined)

    return new Map<RigidNodeId, Jolt.BodyID>(rnToBodyPairs)
}
