import { v4 as uuidv4 } from "uuid"
import { mirabuf } from "@/proto/mirabuf"
import { parseOBJ } from "./OBJParser"
import { parseSTL, type ParsedMesh } from "./STLParser"

// URDF uses Z-up (ROS convention). Synthesis/Three.js uses Y-up.
// Frame change matrix: Rx(-90°) = [[1,0,0],[0,0,1],[0,-1,0]]
// Point (x,y,z)_urdf -> (x, z, -y)_yup

interface URDFVisual {
    visualMeshPath: string | null
    visualMeshScale: [number, number, number]
    // <visual><origin> positions the mesh frame relative to the link frame.
    // Many Onshape-exported URDFs define mesh vertices in the assembly global frame
    // and use visual origin to correct back to link-relative space.
    visualOriginXYZ: [number, number, number]
    visualOriginRPY: [number, number, number]
    materialName: string | null
    materialRGBA: [number, number, number, number] | null
}

interface URDFLink {
    name: string
    visuals: URDFVisual[]
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

function mat3VecMul(m: Mat3, v: [number, number, number]): [number, number, number] {
    return [0, 1, 2].map(i => m[i][0] * v[0] + m[i][1] * v[1] + m[i][2] * v[2]) as [number, number, number]
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
// biome-ignore format: matrix layout
const RZy: Mat3 = [[1, 0, 0], [0, 0, 1], [0, -1, 0]]

// Build mirabuf spatialMatrix (16 floats, row-major) from a URDF joint origin.
// Converts to Y-up space; mesh vertices are also converted to Y-up (see toYupMesh),
// so body-local frames are Y-up throughout. This keeps Jolt physics constraints correct.
function originToSpatialMatrix(xyz: [number, number, number], rpy: [number, number, number]): number[] {
    const RU = rpyToMatrix(rpy[0], rpy[1], rpy[2])
    const RY = mat3Mul(RZy, mat3Mul(RU, transpose3(RZy)))
    const [px, py, pz] = xyz
    const [tx, ty, tz] = [px * 100, pz * 100, -py * 100] // metres -> cm, Z-up -> Y-up
    // biome-ignore format: spatial matrix row layout
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

interface URDFTransform {
    rotation: Mat3
    position: [number, number, number]
}

interface JointFrame {
    originXYZ: [number, number, number]
    axisXYZ: [number, number, number]
}

const IDENTITY_ROTATION: Mat3 = [
    [1, 0, 0],
    [0, 1, 0],
    [0, 0, 1],
]

function addVec3(a: [number, number, number], b: [number, number, number]): [number, number, number] {
    return [a[0] + b[0], a[1] + b[1], a[2] + b[2]]
}

function transformPoint(t: URDFTransform, p: [number, number, number]): [number, number, number] {
    return addVec3(t.position, mat3VecMul(t.rotation, p))
}

function transformDirection(t: URDFTransform, d: [number, number, number]): [number, number, number] {
    return mat3VecMul(t.rotation, d)
}

function inverseTransformPoint(t: URDFTransform, p: [number, number, number]): [number, number, number] {
    return mat3VecMul(transpose3(t.rotation), [p[0] - t.position[0], p[1] - t.position[1], p[2] - t.position[2]])
}

function buildGlobalJointFrames(joints: URDFJoint[], rootName: string): Map<string, JointFrame> {
    const childrenOf = new Map<string, URDFJoint[]>()
    for (const joint of joints) {
        if (!childrenOf.has(joint.parent)) childrenOf.set(joint.parent, [])
        childrenOf.get(joint.parent)!.push(joint)
    }

    const frames = new Map<string, JointFrame>()
    const linkTransforms = new Map<string, URDFTransform>([
        [rootName, { rotation: IDENTITY_ROTATION, position: [0, 0, 0] }],
    ])

    function visit(parent: string) {
        const parentTransform = linkTransforms.get(parent)
        if (!parentTransform) return

        for (const joint of childrenOf.get(parent) ?? []) {
            const jointOrigin = transformPoint(parentTransform, joint.originXYZ)
            frames.set(joint.name, {
                originXYZ: jointOrigin,
                axisXYZ: transformDirection(parentTransform, joint.axisXYZ),
            })

            if (linkTransforms.has(joint.child)) continue
            linkTransforms.set(joint.child, {
                rotation: mat3Mul(parentTransform.rotation, rpyToMatrix(...joint.originRPY)),
                position: jointOrigin,
            })
            visit(joint.child)
        }
    }

    visit(rootName)
    return frames
}

// Accumulate each link's global transform (URDF Z-up metres) by walking the joint tree.
function buildGlobalLinkTransforms(joints: URDFJoint[], rootName: string): Map<string, URDFTransform> {
    const childrenOf = new Map<string, URDFJoint[]>()
    for (const joint of joints) {
        if (!childrenOf.has(joint.parent)) childrenOf.set(joint.parent, [])
        childrenOf.get(joint.parent)!.push(joint)
    }

    const transforms = new Map<string, URDFTransform>([
        [rootName, { rotation: IDENTITY_ROTATION, position: [0, 0, 0] }],
    ])

    function visit(parent: string) {
        const pt = transforms.get(parent)
        if (!pt) return
        for (const joint of childrenOf.get(parent) ?? []) {
            if (transforms.has(joint.child)) continue
            transforms.set(joint.child, {
                rotation: mat3Mul(pt.rotation, rpyToMatrix(...joint.originRPY)),
                position: transformPoint(pt, joint.originXYZ),
            })
            visit(joint.child)
        }
    }

    visit(rootName)
    return transforms
}

// Rotation magnitude (radians) of a 3x3 rotation matrix — used to test whether a transform is "trivial".
function rotationAngle(m: Mat3): number {
    const trace = m[0][0] + m[1][1] + m[2][2]
    return Math.acos(Math.min(1, Math.max(-1, (trace - 1) / 2)))
}

function attr(el: Element | null | undefined, name: string, fallback = ""): string {
    return el?.getAttribute(name) ?? fallback
}

function parseVec3(el: Element | null | undefined, attrName = "xyz"): [number, number, number] {
    const p = (el?.getAttribute(attrName) ?? "0 0 0").trim().split(/\s+/)
    return [parseFloat(p[0] ?? "0") || 0, parseFloat(p[1] ?? "0") || 0, parseFloat(p[2] ?? "0") || 0]
}

function parseVec4(s: string): [number, number, number, number] {
    const p = s.trim().split(/\s+/)
    return [parseFloat(p[0] ?? "1"), parseFloat(p[1] ?? "1"), parseFloat(p[2] ?? "1"), parseFloat(p[3] ?? "1")]
}

function extractLinks(doc: Document): URDFLink[] {
    return Array.from(doc.querySelectorAll("link")).map(link => {
        const name = attr(link, "name")
        const inertialEl = link.querySelector("inertial")
        const massEl = inertialEl?.querySelector("mass") ?? null
        const visuals: URDFVisual[] = Array.from(link.querySelectorAll("visual")).map(visual => {
            const meshEl = visual.querySelector("mesh")
            const matEl = visual.querySelector("material")
            const sp = attr(meshEl, "scale", "1 1 1").trim().split(/\s+/)
            const colorRgba = matEl?.querySelector("color")?.getAttribute("rgba")
            const visualOriginEl = visual.querySelector("origin")

            return {
                visualMeshPath: meshEl ? attr(meshEl, "filename") || null : null,
                visualMeshScale: [
                    parseFloat(sp[0] ?? "1") || 1,
                    parseFloat(sp[1] ?? "1") || 1,
                    parseFloat(sp[2] ?? "1") || 1,
                ],
                visualOriginXYZ: parseVec3(visualOriginEl),
                visualOriginRPY: parseVec3(visualOriginEl, "rpy"),
                materialName: matEl ? attr(matEl, "name") || null : null,
                materialRGBA: colorRgba ? parseVec4(colorRgba) : null,
            }
        })

        return {
            name,
            visuals,
            mass: parseFloat(massEl?.getAttribute("value") ?? "0") || 0,
            comXYZ: parseVec3(inertialEl?.querySelector("origin")),
        } satisfies URDFLink
    })
}

const DEFAULT_STEEL_MATERIAL_NAME = "__urdf_default_steel__"
const DEFAULT_STEEL_RGBA: [number, number, number, number] = [0.647059, 0.647059, 0.647059, 1]

// If no material try find another instance of same mesh to match with.
// Otherwise fallback to default steel material.
function fillMissingMaterials(links: URDFLink[]): void {
    const meshMaterials = new Map<string, { name: string; rgba: [number, number, number, number] }>()
    for (const link of links) {
        for (const visual of link.visuals) {
            if (
                visual.visualMeshPath &&
                visual.materialName &&
                visual.materialRGBA &&
                !meshMaterials.has(visual.visualMeshPath)
            ) {
                meshMaterials.set(visual.visualMeshPath, { name: visual.materialName, rgba: visual.materialRGBA })
            }
        }
    }

    for (const link of links) {
        for (const visual of link.visuals) {
            if (visual.materialName && visual.materialRGBA) continue
            const fallback = (visual.visualMeshPath && meshMaterials.get(visual.visualMeshPath)) || {
                name: DEFAULT_STEEL_MATERIAL_NAME,
                rgba: DEFAULT_STEEL_RGBA,
            }

            visual.materialName = fallback.name
            visual.materialRGBA = fallback.rgba
        }
    }
}

function extractJoints(doc: Document): URDFJoint[] {
    const validTypes = new Set(["fixed", "revolute", "continuous", "prismatic", "floating", "planar"])
    return Array.from(doc.querySelectorAll("joint")).map(joint => {
        const typeStr = attr(joint, "type", "fixed")
        const limitEl = joint.querySelector("limit")
        const originEl = joint.querySelector("origin")
        return {
            name: attr(joint, "name"),
            type: (validTypes.has(typeStr) ? typeStr : "fixed") as URDFJoint["type"],
            parent: joint.querySelector("parent")?.getAttribute("link") ?? "",
            child: joint.querySelector("child")?.getAttribute("link") ?? "",
            originXYZ: parseVec3(originEl),
            originRPY: parseVec3(originEl, "rpy"),
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

function applyMat3(src: ArrayLike<number>, r: Mat3, tx = 0, ty = 0, tz = 0): Float32Array {
    const out = new Float32Array(src.length)
    for (let i = 0; i < src.length; i += 3) {
        const x = src[i]
        const y = src[i + 1]
        const z = src[i + 2]
        out[i] = r[0][0] * x + r[0][1] * y + r[0][2] * z + tx
        out[i + 1] = r[1][0] * x + r[1][1] * y + r[1][2] * z + ty
        out[i + 2] = r[2][0] * x + r[2][1] * y + r[2][2] * z + tz
    }

    return out
}

function applyVisualTransform(mesh: ParsedMesh, rotation: Mat3, xyz: [number, number, number]): ParsedMesh {
    const hasOffset = xyz[0] !== 0 || xyz[1] !== 0 || xyz[2] !== 0
    const hasRotation = rotationAngle(rotation) > 1e-9
    if (!hasOffset && !hasRotation) return mesh

    const [ox, oy, oz] = xyz
    return {
        verts: applyMat3(mesh.verts, rotation, ox, oy, oz),
        normals: applyMat3(mesh.normals, rotation),
        indices: mesh.indices,
        uv: mesh.uv,
    }
}

// Convert a flat float array of 3D vectors from URDF Z-up to Y-up by applying Rx(-90°):
// new_x = x, new_y = z, new_z = -y. Works for both positions and direction vectors (normals).
function toYup(arr: ArrayLike<number>): Float32Array {
    const out = new Float32Array(arr.length)
    for (let i = 0; i < arr.length; i += 3) {
        out[i] = arr[i]
        out[i + 1] = arr[i + 2]
        out[i + 2] = -arr[i + 1]
    }

    return out
}

// Centroid of a flat [x,y,z,...] vertex array, in the mesh's own (untransformed) coordinate space.
// A centroid magnitude near zero means the mesh is authored in link-LOCAL space.
function meshCentroid(verts: ArrayLike<number>): { c: [number, number, number]; mag: number } {
    const n = verts.length / 3
    if (n === 0) return { c: [0, 0, 0], mag: 0 }
    let sx = 0
    let sy = 0
    let sz = 0
    for (let i = 0; i < verts.length; i += 3) {
        sx += verts[i]
        sy += verts[i + 1]
        sz += verts[i + 2]
    }

    const c: [number, number, number] = [sx / n, sy / n, sz / n]
    return { c, mag: Math.hypot(c[0], c[1], c[2]) }
}

function visualCentroidInLinkFrame(
    visual: URDFVisual,
    meshFiles: Map<string, Uint8Array>,
    meshCache: Map<string, ParsedMesh | null>
) {
    if (!visual.visualMeshPath) return null

    const parsed = loadMesh(visual.visualMeshPath, meshFiles, meshCache)
    if (!parsed) return null

    const raw = meshCentroid(parsed.verts)
    const rotated = mat3VecMul(rpyToMatrix(...visual.visualOriginRPY), raw.c)
    const inLink: [number, number, number] = [
        rotated[0] + visual.visualOriginXYZ[0],
        rotated[1] + visual.visualOriginXYZ[1],
        rotated[2] + visual.visualOriginXYZ[2],
    ]

    return {
        rawCentroid: raw.c,
        rawMagnitude: raw.mag,
        linkCentroid: inLink,
        vertexCount: parsed.verts.length / 3,
    }
}

function visualTransformInLinkFrame(
    visual: URDFVisual,
    robotSpaceVisuals: boolean,
    linkGlobalTransform?: URDFTransform
): { rotation: Mat3; translation: [number, number, number]; frame: "robotSpace" | "linkLocal" } {
    const visualRotation = rpyToMatrix(...visual.visualOriginRPY)
    if (!robotSpaceVisuals || !linkGlobalTransform) {
        return { rotation: visualRotation, translation: visual.visualOriginXYZ, frame: "linkLocal" }
    }

    // Robot-space: pre-multiply by T_link^-1 so the later T_link application cancels and the mesh lands
    // at V·meshVerts = the part's true Onshape global transform. See shouldTreatVisualOriginsAsRobotSpace.
    return {
        rotation: mat3Mul(transpose3(linkGlobalTransform.rotation), visualRotation),
        translation: inverseTransformPoint(linkGlobalTransform, visual.visualOriginXYZ),
        frame: "robotSpace",
    }
}

/**
 * Detects Onshape's "robot-space visual" encoding for collapsed assembly links.
 *
 * In standard URDF, a mesh is placed as T_link * V * meshVerts, where T_link comes from the joint tree
 * and V is the <visual><origin> transform from mesh frame to link frame. Some Onshape exports instead
 * put each visual's assembly-global occurrence transform in V while also giving the collapsed link a
 * non-identity T_link. Applying both transforms displaces the whole collapsed assembly.
 *
 * When this returns true, visualTransformInLinkFrame() rewrites V as T_link^-1 * V so the normal
 * T_link application cancels and the mesh lands at the exported occurrence transform.
 *
 * The heuristic intentionally uses only visual data. For the affected links, T_link is the unreliable
 * graft transform introduced while flattening an assembly graph into a URDF tree, so comparing visuals
 * to the FK link origin can reject the links that need correction most. Robot-space visuals show up as
 * multi-visual links whose transformed geometry and visual-origin translations are both far from the
 * link-local origin; ordinary link-local visual offsets stay near zero.
 */
function shouldTreatVisualOriginsAsRobotSpace(
    link: URDFLink,
    globalTransform: URDFTransform | undefined,
    meshFiles: Map<string, Uint8Array>,
    meshCache: Map<string, ParsedMesh | null>
): boolean {
    if (!globalTransform || link.visuals.length < 2) return false

    const summaries = link.visuals
        .map(visual => visualCentroidInLinkFrame(visual, meshFiles, meshCache))
        .filter(summary => summary !== null)
    if (summaries.length < 2) return false

    let totalVertices = 0
    const centroid: [number, number, number] = [0, 0, 0]
    for (const summary of summaries) {
        totalVertices += summary.vertexCount
        for (let i = 0; i < 3; i++) centroid[i] += summary.linkCentroid[i] * summary.vertexCount
    }
    if (totalVertices === 0) return false
    for (let i = 0; i < 3; i++) centroid[i] /= totalVertices

    // Mean magnitude of the visual <origin> translations, global-scale (~decimetres) for robot-space
    // baked-in occurrence transforms, ~mm for real link-local mounting offsets.
    const meshVisuals = link.visuals.filter(visual => visual.visualMeshPath !== null)
    const meanVisualOrigin =
        meshVisuals.reduce((sum, visual) => sum + Math.hypot(...visual.visualOriginXYZ), 0) /
        Math.max(1, meshVisuals.length)

    // Robot-space needs BOTH the aggregate geometry far from the link-local origin AND global-scale
    // visual origins. Both come from the visuals alone (never the joint-FK origin, see header).
    const visualMagnitude = Math.hypot(centroid[0], centroid[1], centroid[2])
    return visualMagnitude > 0.15 && meanVisualOrigin > 0.15
}

function loadMesh(
    meshPath: string,
    meshFiles: Map<string, Uint8Array>,
    cache: Map<string, ParsedMesh | null>
): ParsedMesh | null {
    const cached = cache.get(meshPath)
    if (cached !== undefined) return cached

    const parsed = parseMesh(meshPath, meshFiles)
    cache.set(meshPath, parsed)
    return parsed
}

function parseMesh(meshPath: string, meshFiles: Map<string, Uint8Array>): ParsedMesh | null {
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

function buildRigidGroups(
    links: URDFLink[],
    joints: URDFJoint[]
): {
    rigidGroups: mirabuf.joint.IRigidGroup[]
    linkToGroup: Map<string, string>
} {
    const fixedTypes = new Set<string>(["fixed", "floating", "planar"])
    const parent = new Map<string, string>(links.map(l => [l.name, l.name]))

    function find(x: string): string {
        if (parent.get(x) !== x) parent.set(x, find(parent.get(x)!))
        return parent.get(x)!
    }

    function union(a: string, b: string) {
        const ra = find(a)
        const rb = find(b)
        if (ra !== rb) parent.set(ra, rb)
    }

    for (const j of joints) {
        if (fixedTypes.has(j.type) || isZeroTravelPrismatic(j)) union(j.parent, j.child)
    }

    // Onshape emits "_loop_closure" joints to close kinematic loops in gear trains and belt
    // drives. They carry no real DOF — merge them as fixed so the synthetic loop closure link
    // attaches to its body rather than spawning as a free-floating orphan in the simulation.
    // union(child, parent) keeps the real part as the union-find representative.
    const loopClosureChildren = new Set<string>()
    for (const j of joints) {
        if (j.name.includes("_loop_closure")) {
            union(j.child, j.parent)
            loopClosureChildren.add(j.child)
        }
    }

    const childJointsByParent = new Map<string, URDFJoint[]>()
    for (const j of joints) {
        if (!childJointsByParent.has(j.parent)) childJointsByParent.set(j.parent, [])
        childJointsByParent.get(j.parent)!.push(j)
    }

    // Phantom links: zero-mass, no-geometry intermediaries Onshape emits for multi-DOF joints
    // (cylindrical, planar). Merge each one into its parent to eliminate the massless body that
    // would otherwise destabilise Jolt's constraint solver.
    // union(phantom, parent) keeps the real part as the union-find representative.
    const parentJointOf = new Map<string, URDFJoint>(joints.map(j => [j.child, j]))
    const isPhantomLink = (link: URDFLink) =>
        link.visuals.every(visual => visual.visualMeshPath === null) && link.mass === 0

    const phantomWithParentJoint = links
        .filter(isPhantomLink)
        .map(link => ({ link, pj: parentJointOf.get(link.name) }))
        .filter((x): x is { link: URDFLink; pj: URDFJoint } => x.pj !== undefined)

    phantomWithParentJoint.forEach(({ link, pj }) => union(link.name, pj.parent))

    // Onshape exports cylindrical mates as prismatic -> massless phantom -> continuous.
    // The generated translation range is often enormous, and the final continuous joint
    // makes ordinary bolted hardware like motor housings free to spin or orbit in physics.
    // Collapse that synthetic chain into a rigid group; true drivetrain wheel/steer joints
    // are separate non-cylindrical joints and remain physical.
    phantomWithParentJoint
        .filter(({ link, pj }) => isCylindricalPhantom(link, pj))
        .flatMap(({ link, pj }) => (childJointsByParent.get(link.name) ?? []).map(childJoint => ({ childJoint, pj })))
        .filter(({ childJoint }) => childJoint.type === "continuous" || childJoint.type === "revolute")
        .forEach(({ childJoint, pj }) => union(childJoint.child, pj.parent))

    // Loop-closure links are synthetic bookkeeping links. Phantom links, however, must remain
    // in emitted rigid-group occurrences so MirabufParser maps joints that reference them onto
    // a real rigid node instead of leaving separate zero-geometry bodies.
    const excludeFromOccurrences = loopClosureChildren

    const groups = new Map<string, string[]>()
    for (const l of links) {
        const root = find(l.name)
        if (!groups.has(root)) groups.set(root, [])
        groups.get(root)!.push(l.name)
    }

    const linkToGroup = new Map<string, string>()
    const rigidGroups: mirabuf.joint.IRigidGroup[] = []
    for (const group of groups.values()) {
        if (group.length <= 1) continue

        const name = group.join("_rigid")
        for (const linkName of group) linkToGroup.set(linkName, name)

        const occurrences = group.filter(n => !excludeFromOccurrences.has(n))
        if (occurrences.length > 1) rigidGroups.push({ name, occurrences })
    }

    return { rigidGroups, linkToGroup }
}

function mapJointMotion(type: URDFJoint["type"]): mirabuf.joint.JointMotion {
    if (type === "revolute" || type === "continuous") return mirabuf.joint.JointMotion.REVOLUTE
    if (type === "prismatic") return mirabuf.joint.JointMotion.SLIDER
    return mirabuf.joint.JointMotion.RIGID
}

const ZERO_TRAVEL_EPSILON = 1e-6 // metres, prismatic joints with a range below this are treated as fixed

function isZeroTravelPrismatic(joint: URDFJoint): boolean {
    return joint.type === "prismatic" && Math.abs(joint.limitUpper - joint.limitLower) <= ZERO_TRAVEL_EPSILON
}

function isCylindricalPhantom(link: URDFLink, parentJoint: URDFJoint): boolean {
    return (
        parentJoint.type === "prismatic" &&
        (link.name.startsWith("cylindrical") || parentJoint.name.startsWith("cylindrical"))
    )
}

function buildLinkBody(
    link: URDFLink,
    visual: URDFVisual,
    index: number,
    meshFiles: Map<string, Uint8Array>,
    robotSpaceVisuals: boolean,
    meshCache: Map<string, ParsedMesh | null>,
    linkGlobalTransform?: URDFTransform
): mirabuf.IBody | null {
    if (!visual.visualMeshPath) return null

    const parsed = loadMesh(visual.visualMeshPath, meshFiles, meshCache)
    if (!parsed) return null

    const visualTransform = visualTransformInLinkFrame(visual, robotSpaceVisuals, linkGlobalTransform)
    const inLinkFrame = applyVisualTransform(parsed, visualTransform.rotation, visualTransform.translation)

    // Convert Z-up→Y-up and scale (metres→cm) in one pass per array.
    // Jolt VehicleConstraint requires mPosition in body-local Y-up space.
    const [sx, sy, sz] = visual.visualMeshScale.map(s => s * 100)
    const rv = inLinkFrame.verts
    const scaled = new Float32Array(rv.length)
    for (let i = 0; i < rv.length; i += 3) {
        scaled[i] = rv[i] * sx
        scaled[i + 1] = rv[i + 2] * sy // Z-up→Y-up swap
        scaled[i + 2] = -rv[i + 1] * sz
    }
    const yupNormals = toYup(inLinkFrame.normals)
    const uv = inLinkFrame.uv.length > 0 ? inLinkFrame.uv : new Float32Array((scaled.length / 3) * 2)

    // mirabuf.IMesh (protobuf-generated) requires plain number[]
    return {
        info: { GUID: `${link.name}_body_${index}`, name: `${link.name}_body_${index}` },
        triangleMesh: {
            mesh: {
                verts: Array.from(scaled),
                normals: Array.from(yupNormals),
                uv: Array.from(uv),
                indices: Array.from(inLinkFrame.indices),
            },
        },
        appearanceOverride: visual.materialName ?? undefined,
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
    const globalTransforms = buildGlobalLinkTransforms(joints, rootLink.name)
    const meshCache = new Map<string, ParsedMesh | null>()

    for (const link of links) {
        const globalTransform = globalTransforms.get(link.name)
        const robotSpaceVisuals = shouldTreatVisualOriginsAsRobotSpace(link, globalTransform, meshFiles, meshCache)

        const bodies = link.visuals
            .map((visual, index) =>
                buildLinkBody(link, visual, index, meshFiles, robotSpaceVisuals, meshCache, globalTransform)
            )
            .filter((body): body is mirabuf.IBody => body !== null)

        partDefinitions[link.name] = {
            info: { GUID: link.name, name: link.name, version: 1 },
            physicalData: {
                mass: link.mass,
                com: positionToYup(link.comXYZ[0], link.comXYZ[1], link.comXYZ[2]),
            },
            baseTransform: { spatialMatrix: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1] },
            bodies,
        }

        const pj = parentJoint.get(link.name)
        const spatialMatrix =
            link === rootLink
                ? ROOT_SPATIAL_MATRIX
                : pj
                  ? originToSpatialMatrix(pj.originXYZ, pj.originRPY)
                  : ROOT_SPATIAL_MATRIX

        partInstances[link.name] = {
            info: { GUID: link.name, name: link.name, version: 1 },
            partDefinitionReference: link.name,
            transform: { spatialMatrix },
            appearance: link.visuals[0]?.materialName ?? undefined,
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
        for (const visual of link.visuals) {
            if (visual.materialName && visual.materialRGBA) add(visual.materialName, visual.materialRGBA)
        }
    }

    // Collect top-level robot materials (may define colors that links reference by name only)
    for (const matEl of Array.from(doc.querySelectorAll("robot > material"))) {
        const name = attr(matEl, "name")
        if (!name) continue
        const colorRgba = matEl.querySelector("color")?.getAttribute("rgba")
        if (!colorRgba) continue
        add(name, parseVec4(colorRgba))
    }

    return appearances
}

function buildJointDefinition(joint: URDFJoint, frame?: JointFrame): mirabuf.joint.IJoint {
    const motionType = mapJointMotion(joint.type)
    const originXYZ = frame?.originXYZ ?? joint.originXYZ
    const jDef: mirabuf.joint.IJoint = {
        info: { GUID: joint.name, name: joint.name, version: 1 },
        origin: positionToYup(originXYZ[0], originXYZ[1], originXYZ[2]),
        jointMotionType: motionType,
    }

    if (motionType === mirabuf.joint.JointMotion.REVOLUTE) {
        const axis = axisToYup(...(frame?.axisXYZ ?? joint.axisXYZ))
        const isContinuous = joint.type === "continuous"
        jDef.rotational = {
            rotationalFreedom: {
                axis,
                // Omitting limits (rather than a huge fake range) is how ConstraintSettingsUtilities
                // recognizes an unbounded hinge - applyHingeLimits clamps any explicit range to ±π.
                limits: isContinuous ? undefined : { lower: joint.limitLower, upper: joint.limitUpper },
                value: 0,
            },
        }
    } else if (motionType === mirabuf.joint.JointMotion.SLIDER) {
        const axis = axisToYup(...(frame?.axisXYZ ?? joint.axisXYZ))
        jDef.prismatic = {
            prismaticFreedom: {
                axis,
                // Prismatic limits are stored in cm; PhysicsSystem multiplies them by 0.01 (cm->m).
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
    rootLink: URDFLink,
    jointFrames?: Map<string, JointFrame>
): {
    jointDefinitions: Record<string, mirabuf.joint.IJoint>
    jointInstances: Record<string, mirabuf.joint.IJointInstance>
} {
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
        jointDefinitions[joint.name] = buildJointDefinition(joint, jointFrames?.get(joint.name))
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

    fillMissingMaterials(links)

    const childSet = new Set(joints.map(j => j.child))
    const rootLink = links.find(l => !childSet.has(l.name))
    if (!rootLink) throw new Error("URDF has no root link - every link is listed as a child joint")

    // rigidGroups must be computed before physicsJoints — filtering depends on group membership.
    // Must be an array (not undefined): bandageRigidNodes calls .forEach on it directly.
    const { rigidGroups, linkToGroup } = buildRigidGroups(links, joints)

    // Map each ungrouped link to itself so we can identify within-group joints.
    for (const link of links) {
        if (!linkToGroup.has(link.name)) linkToGroup.set(link.name, link.name)
    }

    // Physics joints: exclude loop closure joints (no real DOF, now merged into rigid groups)
    // and joints whose both endpoints are in the same rigid group (within-body constraints
    // that became no-ops after phantom link and loop closure merging).
    const physicsJoints = joints.filter(
        j => !j.name.includes("_loop_closure") && linkToGroup.get(j.parent) !== linkToGroup.get(j.child)
    )

    // buildParts uses original joints for transform computation — phantom links still need
    // their correct spatial matrices derived from their original parent joints.
    const { partDefinitions, partInstances } = buildParts(links, rootLink, joints, meshFiles)

    const appearances = buildAppearances(links, doc)
    const jointFrames = buildGlobalJointFrames(joints, rootLink.name)
    const { jointDefinitions, jointInstances } = buildJoints(physicsJoints, rootLink, jointFrames)

    // The design hierarchy must stay complete even when physics joints are filtered out.
    // MirabufParser builds _partToNodeMap by walking this tree, and rigidGroups may still
    // reference links connected by filtered fixed/loop-closure joints.
    const hierarchy = buildDesignHierarchy(joints, rootLink.name)

    return mirabuf.Assembly.create({
        info: { GUID: uuidv4(), name: robotName, version: 5 },
        dynamic: true,
        designHierarchy: hierarchy,
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
