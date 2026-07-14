import type { mirabuf } from "@/proto/mirabuf"

export const URDF_IMPORT_TAG = "urdfImport"

export function isURDFImport(assembly: mirabuf.Assembly): boolean {
    return assembly.data?.parts?.userData?.data?.[URDF_IMPORT_TAG] === "true"
}
