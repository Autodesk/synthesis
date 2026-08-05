import { mirabuf } from "@/proto/mirabuf"
import { downloadBlob } from "@/util/Utility"

const TO_OBJECT_OPTIONS = { longs: String, enums: String, bytes: String }

/** Logs label + value as two separate console.log calls (value and its JSON form) to avoid truncation. */
function logJson(label: string, value: unknown): void {
    console.log(`${label}:`, value)
    console.log(`${label} (JSON):`, JSON.stringify(value))
}

/** Logs an assembly's joints, design/joint hierarchy, and a lightweight part-instance summary. */
// @ts-expect-error unused, kept for ad-hoc debugging
// biome-ignore lint/correctness/noUnusedVariables: kept for ad-hoc debugging
function dumpAssemblyStructure(assembly: mirabuf.Assembly, label: string): void {
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

/** Replaces each body's raw mesh vertex/normal/uv/index/color arrays with just their lengths, in place. */
function stripMeshGeometry(assemblyObj: Record<string, unknown>): void {
    const defs = (assemblyObj.data as Record<string, unknown> | undefined)?.parts as Record<string, unknown> | undefined
    const partDefinitions = defs?.partDefinitions as Record<string, Record<string, unknown>> | undefined
    if (!partDefinitions) return

    for (const def of Object.values(partDefinitions)) {
        const bodies = def.bodies as Record<string, unknown>[] | undefined
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

/** Serializes the whole assembly to a downloaded .json file, with mesh geometry stripped to array lengths. */
// @ts-expect-error unused, kept for ad-hoc debugging
// biome-ignore lint/correctness/noUnusedVariables: kept for ad-hoc debugging
function downloadFullAssemblyJson(assembly: mirabuf.Assembly, filename: string): void {
    const full = mirabuf.Assembly.toObject(assembly as mirabuf.Assembly, TO_OBJECT_OPTIONS) as Record<string, unknown>
    stripMeshGeometry(full)

    const resolvedFilename = filename.endsWith(".json") ? filename : `${filename}.json`
    try {
        downloadBlob(resolvedFilename, JSON.stringify(full, null, 2))
    } catch (error) {
        console.error(`[DebugAssemblyDump] Failed to stringify assembly even after stripping mesh geometry:`, error)
    }
}
