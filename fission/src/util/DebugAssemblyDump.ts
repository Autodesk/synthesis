import { mirabuf } from "@/proto/mirabuf"
import { downloadBlob } from "@/util/Utility"

const TO_OBJECT_OPTIONS = { longs: String, enums: String, bytes: String }

/**
 * Logs `label` + a plain-object value and its JSON.stringify'd form as two SEPARATE console.log calls.
 * Splitting each section of the assembly into its own small call (rather than one giant combined dump)
 * matters: some log-capture/export tools truncate an individual console argument once it crosses some
 * threshold regardless of the assembly's actual size -- observed truncating a small test robot's dump at
 * the exact same byte count as a much larger robot's. `partDefinitions` (mass/appearance/body GUIDs/mesh
 * metadata per part) was the main contributor to that size and isn't needed to diff joint/wheel/hierarchy
 * structure, so it's dropped entirely rather than merely shrunk.
 */
function logJson(label: string, value: unknown): void {
    console.log(`${label}:`, value)
    console.log(`${label} (JSON):`, JSON.stringify(value))
}

/**
 * Logs an assembly's joints container, design/joint hierarchy, and a lightweight part-instance summary
 * -- everything relevant to diffing wheel/joint structure between two robots -- while omitting
 * `partDefinitions` (mesh geometry, appearance, body GUIDs) entirely.
 */
export function dumpAssemblyStructure(assembly: mirabuf.Assembly, label: string): void {
    if (assembly.data?.joints) {
        const joints = mirabuf.joint.Joints.toObject(assembly.data.joints as mirabuf.joint.Joints, TO_OBJECT_OPTIONS)
        logJson(`${label} -- assembly.data.joints`, joints)
    }

    if (assembly.designHierarchy) {
        const designHierarchy = mirabuf.GraphContainer.toObject(
            assembly.designHierarchy as mirabuf.GraphContainer,
            TO_OBJECT_OPTIONS
        )
        logJson(`${label} -- assembly.designHierarchy`, designHierarchy)
    }

    if (assembly.jointHierarchy) {
        const jointHierarchy = mirabuf.GraphContainer.toObject(
            assembly.jointHierarchy as mirabuf.GraphContainer,
            TO_OBJECT_OPTIONS
        )
        logJson(`${label} -- assembly.jointHierarchy`, jointHierarchy)
    }

    const partInstances = Object.values(assembly.data?.parts?.partInstances ?? {}).map(inst => ({
        GUID: inst.info?.GUID,
        name: inst.info?.name,
        partDefinitionReference: inst.partDefinitionReference,
        joints: inst.joints,
    }))
    logJson(`${label} -- part instances (GUID/name/partDefinitionReference/joints only)`, partInstances)
}

/**
 * Replaces each body's raw triangleMesh vertex/normal/uv/index/color float arrays with just their
 * lengths, in place. For a real multi-part robot these arrays are the overwhelming majority of the
 * assembly's serialized size (millions of floats) and aren't needed to debug joint/rigid-node structure --
 * keeping them is what made JSON.stringify throw "RangeError: Invalid string length" on a complex
 * assembly. Everything else on partDefinitions (mass, appearance, body GUIDs, joint refs) is preserved.
 */
function stripMeshGeometry(assemblyObj: Record<string, unknown>): void {
    const defs = (assemblyObj.data as Record<string, unknown> | undefined)?.parts as
        | Record<string, unknown>
        | undefined
    const partDefinitions = defs?.partDefinitions as Record<string, Record<string, unknown>> | undefined
    if (!partDefinitions) return

    for (const def of Object.values(partDefinitions)) {
        const bodies = def.bodies as Array<Record<string, unknown>> | undefined
        for (const body of bodies ?? []) {
            const mesh = (body.triangleMesh as Record<string, unknown> | undefined)?.mesh as
                | Record<string, unknown>
                | undefined
            if (!mesh) continue

            for (const key of ["verts", "normals", "uv", "indices", "colors"]) {
                const arr = mesh[key]
                if (Array.isArray(arr)) mesh[key] = `<omitted ${arr.length} values>`
            }
        }
    }
}

/**
 * Serializes the whole assembly -- joints, rigidGroups, design/joint hierarchy, and every partDefinition's
 * metadata (mass, appearance, body GUIDs, joint refs) -- to a single downloaded .json file, with raw mesh
 * geometry stripped down to just array lengths (see stripMeshGeometry). Use this when console dumps aren't
 * enough to see the whole picture at once, e.g. cross-referencing rigidGroups/jointInstances/hierarchy/
 * part instances all against each other for one robot.
 */
export function downloadFullAssemblyJson(assembly: mirabuf.Assembly, filename: string): void {
    const full = mirabuf.Assembly.toObject(assembly as mirabuf.Assembly, TO_OBJECT_OPTIONS) as Record<
        string,
        unknown
    >
    stripMeshGeometry(full)

    const resolvedFilename = filename.endsWith(".json") ? filename : `${filename}.json`
    try {
        downloadBlob(resolvedFilename, JSON.stringify(full, null, 2))
    } catch (error) {
        console.error(
            `[DebugAssemblyDump] Failed to stringify assembly even after stripping mesh geometry:`,
            error
        )
    }
}
