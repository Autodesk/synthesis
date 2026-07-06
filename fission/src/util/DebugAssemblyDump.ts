import { mirabuf } from "@/proto/mirabuf"

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
