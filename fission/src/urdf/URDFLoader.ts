import JSZip from "jszip"
import { mirabuf } from "@/proto/mirabuf"
import { convertURDF } from "./URDFConverter"
import { detectAndTagWheels } from "./WheelDetector"

const MESH_EXTENSIONS = new Set(["stl", "obj", "dae"])

async function buildMeshMap(zip: JSZip, urdfPath: string): Promise<Map<string, Uint8Array>> {
    const meshFiles = new Map<string, Uint8Array>()

    // The URDF file's containing directory is the package root for relative path resolution
    const packageRoot = urdfPath.includes("/") ? urdfPath.substring(0, urdfPath.lastIndexOf("/") + 1) : ""

    const entries = Object.entries(zip.files).filter(([, f]) => !f.dir)
    const meshEntries = entries.filter(([path]) => {
        const ext = path.split(".").pop()?.toLowerCase()
        return ext && MESH_EXTENSIONS.has(ext)
    })

    await Promise.all(
        meshEntries.map(async ([path, file]) => {
            const bytes = await file.async("uint8array")
            // Store under three keys so resolveMeshBytes in URDFConverter can find it:
            // 1. Full zip path (e.g. "robot_pkg/meshes/part.stl")
            meshFiles.set(path, bytes)
            // 2. Package-relative path (strip leading package root directory)
            if (packageRoot && path.startsWith(packageRoot)) {
                meshFiles.set(path.slice(packageRoot.length), bytes)
            }
            // 3. Basename only (e.g. "part.stl") — last-resort fallback
            const basename = path.split("/").pop()!
            if (!meshFiles.has(basename)) meshFiles.set(basename, bytes)
        })
    )

    return meshFiles
}

export async function loadURDF(buffer: ArrayBuffer, filename: string): Promise<mirabuf.Assembly> {
    const ext = filename.split(".").pop()?.toLowerCase()

    if (ext === "urdf") {
        // Bare URDF — no zip to extract. Mesh files unavailable; links without geometry still import.
        const text = new TextDecoder().decode(buffer)
        const assembly = convertURDF(text, new Map())
        detectAndTagWheels(assembly)
        return assembly
    }

    if (ext === "zip") {
        const zip = await JSZip.loadAsync(buffer)

        const urdfEntry = Object.values(zip.files).find(f => !f.dir && f.name.endsWith(".urdf"))
        if (!urdfEntry) throw new Error("No .urdf file found in the zip archive")

        const [urdfText, meshFiles] = await Promise.all([
            urdfEntry.async("text"),
            buildMeshMap(zip, urdfEntry.name),
        ])

        const assembly = convertURDF(urdfText, meshFiles)
        detectAndTagWheels(assembly)
        return assembly
    }

    throw new Error(`Unsupported file extension: .${ext ?? "unknown"}`)
}
