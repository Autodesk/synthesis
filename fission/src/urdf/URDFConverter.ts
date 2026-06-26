import { mirabuf } from "@/proto/mirabuf"
import { parseOBJ } from "./OBJParser"
import { parseSTL, type ParsedMesh } from "./STLParser"

// URDF uses Z-up (ROS convention). Synthesis/Three.js uses Y-up.
// Frame change matrix: Rx(-90°) = [[1,0,0],[0,0,1],[0,-1,0]]
// Point (x,y,z)_urdf -> (x, z, -y)_yup

interface URDFLink {
    name: string
    visualMeshPath: string | null
    visualMeshScale: [number, number, number]
    // <visual><origin> positions the mesh frame relative to the link frame.
    // Many Onshape-exported URDFs define mesh vertices in the assembly global frame
    // and use visual origin to correct back to link-relative space.
    visualOriginXYZ: [number, number, number]
    visualOriginRPY: [number, number, number]
    materialName: string | null
    materialRGBA: [number, number, number, number] | null
    mass: number
    comXYZ: [number, number, number]
}

interface URDFJoint {
    name: string
    type: "fixed" | "revolute" | "continuous" | "prismatic" | "floating" | "planar"
    parent: string
    child: string
    originXYZ: [number, number, number]
    originRPY: [number, number, number]
    axisXYZ: [number, number, number]
    limitLower: number
    limitUpper: number
}

type Mat3 = number[][]

function mat3Mul(a: Mat3, b: Mat3): Mat3 {
    return [0, 1, 2].map(i => [0, 1, 2].map(j => [0, 1, 2].reduce((s, k) => s + a[i][k] * b[k][j], 0)))
}
function transpose3(m: Mat3): Mat3 {
    return [0, 1, 2].map(i => [0, 1, 2].map(j => m[j][i]))
}

// RPY -> rotation matrix (ZYX Euler, URDF convention: R = Rz(yaw)*Ry(pitch)*Rx(roll))
function rpyToMatrix(roll: number, pitch: number, yaw: number): Mat3 {
    const [cr, sr] = [Math.cos(roll), Math.sin(roll)]
    const [cp, sp] = [Math.cos(pitch), Math.sin(pitch)]
    const [cy, sy] = [Math.cos(yaw), Math.sin(yaw)]
    return [
        [cy * cp, cy * sp * sr - sy * cr, cy * sp * cr + sy * sr],
        [sy * cp, sy * sp * sr + cy * cr, sy * sp * cr - cy * sr],
        [-sp, cp * sr, cp * cr],
    ]
}

// Rx(-90°): converts URDF Z-up frame to Y-up
const RZy: Mat3 = [[1, 0, 0], [0, 0, 1], [0, -1, 0]]

// Build mirabuf spatialMatrix (16 floats, row-major) from a URDF joint origin.
// Converts to Y-up space; mesh vertices are also converted to Y-up (see toYupMesh),
// so body-local frames are Y-up throughout. This keeps Jolt physics constraints correct.
function originToSpatialMatrix(xyz: [number, number, number], rpy: [number, number, number]): number[] {
    const RU = rpyToMatrix(rpy[0], rpy[1], rpy[2])
    const RY = mat3Mul(RZy, mat3Mul(RU, transpose3(RZy)))
    const [px, py, pz] = xyz
    const [tx, ty, tz] = [px * 100, pz * 100, -py * 100] // metres -> cm, Z-up -> Y-up
    return [
        RY[0][0], RY[0][1], RY[0][2], tx,
        RY[1][0], RY[1][1], RY[1][2], ty,
        RY[2][0], RY[2][1], RY[2][2], tz,
        0, 0, 0, 1,
    ]
}

// Root link transform. No rotation needed because mesh vertices are already
// stored in Y-up space. This ensures the physics chassis body has no rotation, so
// body-local = world (Y-up), which Jolt's VehicleConstraint requires.
const ROOT_SPATIAL_MATRIX = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

// Convert URDF unit axis vector (Z-up) to Y-up IVector3
function axisToYup(ax: number, ay: number, az: number): mirabuf.IVector3 {
    const len = Math.sqrt(ax * ax + ay * ay + az * az) || 1
    return { x: ax / len, y: az / len, z: -ay / len }
}

// Convert URDF position (meters, Z-up) to Y-up IVector3 (cm)
function positionToYup(x: number, y: number, z: number): mirabuf.IVector3 {
    return { x: x * 100, y: z * 100, z: -y * 100 }
}

function attr(el: Element | null | undefined, name: string, fallback = ""): string {
    return el?.getAttribute(name) ?? fallback
}

function parseVec3(el: Element | null | undefined, attrName = "xyz"): [number, number, number] {
    const p = (el?.getAttribute(attrName) ?? "0 0 0").trim().split(/\s+/)
    return [parseFloat(p[0] ?? "0") || 0, parseFloat(p[1] ?? "0") || 0, parseFloat(p[2] ?? "0") || 0]
}

function extractLinks(doc: Document): URDFLink[] {
    return Array.from(doc.querySelectorAll("link")).map(link => {
        const name = attr(link, "name")
        const visual = link.querySelector("visual")
        const meshEl = visual?.querySelector("mesh") ?? null
        const matEl = visual?.querySelector("material") ?? null
        const inertialEl = link.querySelector("inertial")
        const massEl = inertialEl?.querySelector("mass") ?? null

        let visualMeshPath: string | null = null
        let visualMeshScale: [number, number, number] = [1, 1, 1]
        if (meshEl) {
            visualMeshPath = attr(meshEl, "filename") || null
            const sp = attr(meshEl, "scale", "1 1 1").trim().split(/\s+/)
            visualMeshScale = [parseFloat(sp[0] ?? "1") || 1, parseFloat(sp[1] ?? "1") || 1, parseFloat(sp[2] ?? "1") || 1]
        }

        let materialRGBA: [number, number, number, number] | null = null
        const colorRgba = matEl?.querySelector("color")?.getAttribute("rgba")
        if (colorRgba) {
            const p = colorRgba.trim().split(/\s+/)
            materialRGBA = [parseFloat(p[0] ?? "1"), parseFloat(p[1] ?? "1"), parseFloat(p[2] ?? "1"), parseFloat(p[3] ?? "1")]
        }

        const visualOriginEl = visual?.querySelector("origin") ?? null
        return {
            name,
            visualMeshPath,
            visualMeshScale,
            visualOriginXYZ: parseVec3(visualOriginEl),
            visualOriginRPY: parseVec3(visualOriginEl, "rpy"),
            materialName: matEl ? attr(matEl, "name") || null : null,
            materialRGBA,
            mass: parseFloat(massEl?.getAttribute("value") ?? "0") || 0,
            comXYZ: parseVec3(inertialEl?.querySelector("origin")),
        } satisfies URDFLink
    })
}

function extractJoints(doc: Document): URDFJoint[] {
    const validTypes = new Set(["fixed", "revolute", "continuous", "prismatic", "floating", "planar"])
    return Array.from(doc.querySelectorAll("joint")).map(joint => {
        const typeStr = attr(joint, "type", "fixed")
        const limitEl = joint.querySelector("limit")
        return {
            name: attr(joint, "name"),
            type: (validTypes.has(typeStr) ? typeStr : "fixed") as URDFJoint["type"],
            parent: joint.querySelector("parent")?.getAttribute("link") ?? "",
            child: joint.querySelector("child")?.getAttribute("link") ?? "",
            originXYZ: parseVec3(joint.querySelector("origin")),
            originRPY: parseVec3(joint.querySelector("origin"), "rpy"),
            axisXYZ: parseVec3(joint.querySelector("axis")) || ([0, 0, 1] as [number, number, number]),
            limitLower: parseFloat(limitEl?.getAttribute("lower") ?? "0") || 0,
            limitUpper: parseFloat(limitEl?.getAttribute("upper") ?? "0") || 0,
        } satisfies URDFJoint
    })
}

function resolveMeshBytes(packagePath: string, meshFiles: Map<string, Uint8Array>): Uint8Array | null {
    // Strip package:// or model:// scheme prefix
    const stripped = packagePath.replace(/^(?:package|model):\/\/[^/]+\//, "").replace(/^\//, "")
    return (
        meshFiles.get(stripped) ??
        meshFiles.get(stripped.replace(/\\/g, "/")) ??
        meshFiles.get(stripped.split("/").pop()!) ??
        null
    )
}

// Apply visual origin transform (rotation + translation) to raw mesh vertices and normals.
// This maps mesh-local coords -> link-local coords, both in URDF Z-up metres.
// Must run before scale/unit conversion.
function applyVisualOrigin(mesh: ParsedMesh, xyz: [number, number, number], rpy: [number, number, number]): ParsedMesh {
    const hasOffset = xyz[0] !== 0 || xyz[1] !== 0 || xyz[2] !== 0
    const hasRotation = rpy[0] !== 0 || rpy[1] !== 0 || rpy[2] !== 0
    if (!hasOffset && !hasRotation) return mesh

    const R = rpyToMatrix(rpy[0], rpy[1], rpy[2])
    const [ox, oy, oz] = xyz

    const verts = new Array<number>(mesh.verts.length)
    for (let i = 0; i < mesh.verts.length; i += 3) {
        const x = mesh.verts[i], y = mesh.verts[i + 1], z = mesh.verts[i + 2]
        verts[i]     = R[0][0] * x + R[0][1] * y + R[0][2] * z + ox
        verts[i + 1] = R[1][0] * x + R[1][1] * y + R[1][2] * z + oy
        verts[i + 2] = R[2][0] * x + R[2][1] * y + R[2][2] * z + oz
    }

    const normals = new Array<number>(mesh.normals.length)
    for (let i = 0; i < mesh.normals.length; i += 3) {
        const x = mesh.normals[i], y = mesh.normals[i + 1], z = mesh.normals[i + 2]
        normals[i]     = R[0][0] * x + R[0][1] * y + R[0][2] * z
        normals[i + 1] = R[1][0] * x + R[1][1] * y + R[1][2] * z
        normals[i + 2] = R[2][0] * x + R[2][1] * y + R[2][2] * z
    }

    return { verts, normals, indices: mesh.indices, uv: mesh.uv }
}

// Convert a flat float array of 3D vectors from URDF Z-up to Y-up by applying Rx(-90°):
// new_x = x, new_y = z, new_z = -y. Works for both positions and direction vectors (normals).
function toYup(arr: number[]): number[] {
    const out = new Array<number>(arr.length)
    for (let i = 0; i < arr.length; i += 3) {
        out[i]     = arr[i]
        out[i + 1] = arr[i + 2]
        out[i + 2] = -arr[i + 1]
    }

    return out
}

function loadMesh(meshPath: string, meshFiles: Map<string, Uint8Array>): ParsedMesh | null {
    const data = resolveMeshBytes(meshPath, meshFiles)
    if (!data) {
        console.warn(`[URDF] Mesh not found: ${meshPath}`)
        return null
    }

    const ext = meshPath.split(".").pop()?.toLowerCase()
    if (ext === "stl") return parseSTL(data)
    if (ext === "obj") return parseOBJ(data)
    console.warn(`[URDF] Unsupported mesh format: .${ext} (${meshPath}) — link will have no geometry`)
    return null
}

function buildDesignHierarchy(joints: URDFJoint[], rootName: string): mirabuf.IGraphContainer {
    const childrenOf = new Map<string, string[]>()
    for (const j of joints) {
        if (!childrenOf.has(j.parent)) childrenOf.set(j.parent, [])
        childrenOf.get(j.parent)!.push(j.child)
    }

    function buildNode(name: string): mirabuf.INode {
        return { value: name, children: (childrenOf.get(name) ?? []).map(buildNode) }
    }

    return { nodes: [buildNode(rootName)] }
}

function buildRigidGroups(links: URDFLink[], joints: URDFJoint[]): mirabuf.joint.IRigidGroup[] {
    const fixedTypes = new Set<string>(["fixed", "floating", "planar"])
    const parent = new Map<string, string>(links.map(l => [l.name, l.name]))

    function find(x: string): string {
        if (parent.get(x) !== x) parent.set(x, find(parent.get(x)!))
        return parent.get(x)!
    }

    joints.filter(j => fixedTypes.has(j.type)).forEach(j => {
        const ra = find(j.parent),
            rb = find(j.child)
        if (ra !== rb) parent.set(ra, rb)
    })

    const groups = new Map<string, string[]>()
    for (const l of links) {
        const root = find(l.name)
        if (!groups.has(root)) groups.set(root, [])
        groups.get(root)!.push(l.name)
    }

    return Array.from(groups.values())
        .filter(g => g.length > 1)
        .map(g => ({ name: g.join("_rigid"), occurrences: g }))
}

function mapJointMotion(type: URDFJoint["type"]): mirabuf.joint.JointMotion {
    if (type === "revolute" || type === "continuous") return mirabuf.joint.JointMotion.REVOLUTE
    if (type === "prismatic") return mirabuf.joint.JointMotion.SLIDER
    return mirabuf.joint.JointMotion.RIGID
}

function buildLinkBody(link: URDFLink, meshFiles: Map<string, Uint8Array>): mirabuf.IBody | null {
    if (!link.visualMeshPath) return null

    const parsed = loadMesh(link.visualMeshPath, meshFiles)
    if (!parsed) return null

    // 1. Apply visual origin: maps mesh vertices from mesh-local frame -> link-local URDF Z-up frame.
    const inLinkFrame = applyVisualOrigin(parsed, link.visualOriginXYZ, link.visualOriginRPY)

    // 2. Convert from URDF Z-up to Y-up so body-local frames align with world (Y-up).
    //    This is required for Jolt physics: VehicleConstraint expects mPosition in body-local
    //    Y-up space, and the wheel radius is computed from the Y-extent of the local bounding box.
    const yupVerts = toYup(inLinkFrame.verts)
    const yupNormals = toYup(inLinkFrame.normals)

    // 3. Scale: mesh file units -> cm. URDF meshes are in metres; mirabuf stores cm.
    const [sx, sy, sz] = link.visualMeshScale.map(s => s * 100)
    const scaled = new Array<number>(yupVerts.length)
    for (let i = 0; i < yupVerts.length; i += 3) {
        scaled[i]     = yupVerts[i]     * sx
        scaled[i + 1] = yupVerts[i + 1] * sy
        scaled[i + 2] = yupVerts[i + 2] * sz
    }

    return {
        info: { GUID: `${link.name}_body`, name: `${link.name}_body` },
        triangleMesh: {
            mesh: {
                verts: scaled,
                normals: Array.from(yupNormals),
                // uv must be non-empty: MirabufInstance.ts:184 checks !mesh.uv
                uv: inLinkFrame.uv.length > 0 ? inLinkFrame.uv : new Array((yupVerts.length / 3) * 2).fill(0),
                indices: Array.from(inLinkFrame.indices),
            },
        },
        appearanceOverride: link.materialName ?? undefined,
    }
}

function buildParts(
    links: URDFLink[],
    rootLink: URDFLink,
    joints: URDFJoint[],
    meshFiles: Map<string, Uint8Array>
): { partDefinitions: Record<string, mirabuf.IPartDefinition>; partInstances: Record<string, mirabuf.IPartInstance> } {
    const partDefinitions: Record<string, mirabuf.IPartDefinition> = {}
    const partInstances: Record<string, mirabuf.IPartInstance> = {}
    const parentJoint = new Map<string, URDFJoint>(joints.map(j => [j.child, j]))

    for (const link of links) {
        const body = buildLinkBody(link, meshFiles)

        partDefinitions[link.name] = {
            info: { GUID: link.name, name: link.name, version: 1 },
            physicalData: {
                mass: link.mass,
                com: positionToYup(link.comXYZ[0], link.comXYZ[1], link.comXYZ[2]),
            },
            baseTransform: { spatialMatrix: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1] },
            bodies: body ? [body] : [],
        }

        const pj = parentJoint.get(link.name)
        const spatialMatrix =
            link === rootLink
                ? ROOT_SPATIAL_MATRIX
                : pj
                  ? originToSpatialMatrix(pj.originXYZ, pj.originRPY)
                  : [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

        partInstances[link.name] = {
            info: { GUID: link.name, name: link.name, version: 1 },
            partDefinitionReference: link.name,
            transform: { spatialMatrix },
            appearance: link.materialName ?? undefined,
        }
    }

    return { partDefinitions, partInstances }
}

function buildAppearances(links: URDFLink[], doc: Document): Record<string, mirabuf.material.IAppearance> {
    const appearances: Record<string, mirabuf.material.IAppearance> = {}

    function add(name: string, rgba: [number, number, number, number]) {
        if (appearances[name]) return
        appearances[name] = {
            info: { GUID: name, name },
            albedo: {
                // Math.max(1,...) avoids 0-value channels which short-circuit the existing
                // loadMaterials albedo check (A && B && G && R) causing incorrect fallback color
                R: Math.max(1, Math.round(rgba[0] * 255)),
                G: Math.max(1, Math.round(rgba[1] * 255)),
                B: Math.max(1, Math.round(rgba[2] * 255)),
                A: Math.max(1, Math.round(rgba[3] * 255)),
            },
            roughness: 0.5,
            metallic: 0.0,
        }
    }

    for (const link of links) {
        if (link.materialName && link.materialRGBA) add(link.materialName, link.materialRGBA)
    }

    // Collect top-level robot materials (may define colors that links reference by name only)
    for (const matEl of Array.from(doc.querySelectorAll("robot > material"))) {
        const name = attr(matEl, "name")
        if (!name) continue
        const colorRgba = matEl.querySelector("color")?.getAttribute("rgba")
        if (!colorRgba) continue
        const p = colorRgba.trim().split(/\s+/)
        add(name, [parseFloat(p[0] ?? "1"), parseFloat(p[1] ?? "1"), parseFloat(p[2] ?? "1"), parseFloat(p[3] ?? "1")])
    }

    return appearances
}

function buildJointDefinition(joint: URDFJoint): mirabuf.joint.IJoint {
    const motionType = mapJointMotion(joint.type)
    const jDef: mirabuf.joint.IJoint = {
        info: { GUID: joint.name, name: joint.name, version: 1 },
        origin: positionToYup(joint.originXYZ[0], joint.originXYZ[1], joint.originXYZ[2]),
        jointMotionType: motionType,
    }

    if (motionType === mirabuf.joint.JointMotion.REVOLUTE) {
        const axis = axisToYup(...(joint.axisXYZ as [number, number, number]))
        const isContiguous = joint.type === "continuous"
        jDef.rotational = {
            rotationalFreedom: {
                axis,
                limits: {
                    lower: isContiguous ? -Math.PI * 1e6 : joint.limitLower,
                    upper: isContiguous ? Math.PI * 1e6 : joint.limitUpper,
                },
                value: 0,
            },
        }
    } else if (motionType === mirabuf.joint.JointMotion.SLIDER) {
        const axis = axisToYup(...(joint.axisXYZ as [number, number, number]))
        jDef.prismatic = {
            prismaticFreedom: {
                axis,
                // PhysicsSystem.ts:574 multiplies limits by 0.01 (cm->m), so store in cm
                limits: { lower: joint.limitLower * 100, upper: joint.limitUpper * 100 },
                value: 0,
            },
        }
    }
    // RIGID joints: no oneof set. PhysicsSystem switch falls through to default (skip), which is correct.

    return jDef
}

function buildJoints(
    joints: URDFJoint[],
    rootLink: URDFLink
): { jointDefinitions: Record<string, mirabuf.joint.IJoint>; jointInstances: Record<string, mirabuf.joint.IJointInstance> } {
    const jointDefinitions: Record<string, mirabuf.joint.IJoint> = {}
    const jointInstances: Record<string, mirabuf.joint.IJointInstance> = {}

    // Synthetic grounded joint; MirabufParser requires this entry.
    // parts.nodes[0].value is used to identify the root rigid node (MirabufParser.ts:92,122).
    jointInstances["grounded"] = {
        info: { GUID: "grounded", name: "grounded" },
        parentPart: "",
        childPart: rootLink.name,
        parts: { nodes: [{ value: rootLink.name }] },
    }

    for (const joint of joints) {
        jointDefinitions[joint.name] = buildJointDefinition(joint)
        jointInstances[joint.name] = {
            info: { GUID: joint.name, name: joint.name, version: 1 },
            parentPart: joint.parent,
            childPart: joint.child,
            jointReference: joint.name,
            offset: { x: 0, y: 0, z: 0 },
            // parts.nodes must be EMPTY for non-grounded joints. MirabufParser.ts:162-163 moves
            // everything in parts.nodes into the PARENT rigid node, so [{ value: child }] would
            // collapse the child into the parent's rigid node, preventing joint creation.
            parts: { nodes: [] },
        }
    }

    return { jointDefinitions, jointInstances }
}

export function convertURDF(urdfText: string, meshFiles: Map<string, Uint8Array>): mirabuf.Assembly {
    const doc = new DOMParser().parseFromString(urdfText, "text/xml")

    const parseError = doc.querySelector("parsererror")
    if (parseError) throw new Error(`URDF XML parse error: ${parseError.textContent}`)

    const robotName = doc.querySelector("robot")?.getAttribute("name") ?? "robot"
    const links = extractLinks(doc)
    const joints = extractJoints(doc)

    if (links.length === 0) throw new Error("URDF contains no <link> elements")

    const childSet = new Set(joints.map(j => j.child))
    const rootLink = links.find(l => !childSet.has(l.name))
    if (!rootLink) throw new Error("URDF has no root link - every link is listed as a child joint")

    const { partDefinitions, partInstances } = buildParts(links, rootLink, joints, meshFiles)
    const appearances = buildAppearances(links, doc)
    const { jointDefinitions, jointInstances } = buildJoints(joints, rootLink)
    // rigidGroups must be an array (not undefined): bandageRigidNodes calls .forEach on it directly
    const rigidGroups = buildRigidGroups(links, joints)

    return mirabuf.Assembly.create({
        info: { GUID: robotName, name: robotName, version: 5 },
        dynamic: true,
        designHierarchy: buildDesignHierarchy(joints, rootLink.name),
        data: {
            parts: { partDefinitions, partInstances },
            // motorDefinitions must be an object: PhysicsSystem.ts:375 indexes it before any null-check
            joints: { jointDefinitions, jointInstances, rigidGroups, motorDefinitions: {} },
            // appearances must be an object (not undefined/null): loadMaterials calls Object.entries on it
            // physicalMaterials must be an object (not undefined): PhysicsSystem.ts:918 indexes it directly
            // before the null-check at line 922, so undefined throws, an empty map is fine
            materials: { appearances, physicalMaterials: {} },
        },
    })
}
