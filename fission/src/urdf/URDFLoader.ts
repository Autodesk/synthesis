import JSZip from "jszip"
import type { mirabuf } from "@/proto/mirabuf"
import { convertURDF } from "./URDFConverter"
import { detectAndTagWheels } from "@/systems/simulation/synthesis_brain/WheelDetector"
import {ProgressHandle} from "@/components/ProgressNotificationData.ts";

const MESH_EXTENSIONS = new Set(["stl", "obj", "gltf", "bin"])

export function applyConservativeURDFImport(assembly: mirabuf.Assembly): void {
    const jointData = assembly.data?.joints
    if (!jointData?.jointInstances || !jointData.jointDefinitions) return

    const keptJointInstances: Record<string, mirabuf.joint.IJointInstance> = {}
    const keptJointDefinitions: Record<string, mirabuf.joint.IJoint> = {}
    const rigidGroups = [...(jointData.rigidGroups ?? [])]

    for (const [name, jointInstance] of Object.entries(jointData.jointInstances)) {
        if (name === "grounded") {
            keptJointInstances[name] = jointInstance
            continue
        }

        const jointDefinition = jointInstance.jointReference
            ? jointData.jointDefinitions[jointInstance.jointReference]
            : undefined
        const isDetectedWheel = jointDefinition?.userData?.data?.["wheel"] === "true"

        if (isDetectedWheel && jointInstance.jointReference) {
            keptJointInstances[name] = jointInstance
            keptJointDefinitions[jointInstance.jointReference] = jointDefinition
            continue
        }

        if (jointInstance.parentPart && jointInstance.childPart) {
            rigidGroups.push({
                name: `${name}_conservative_rigid`,
                occurrences: [jointInstance.parentPart, jointInstance.childPart],
            })
        }
    }

    jointData.jointInstances = keptJointInstances
    jointData.jointDefinitions = keptJointDefinitions
    jointData.rigidGroups = rigidGroups
}

function validateURDFMeshFormats(urdfText: string): void {
    const doc = new DOMParser().parseFromString(urdfText, "text/xml")
    const meshFilenames = [...doc.querySelectorAll("mesh[filename]")].map(el => el.getAttribute("filename")!)
    const unsupported = meshFilenames.filter(f => {
        const ext = f.split(".").pop()?.toLowerCase()
        return ext !== "stl" && ext !== "obj" && ext !== "gltf"
    })

    if (unsupported.length > 0) {
        const formats = [...new Set(unsupported.map(f => `.${f.split(".").pop()?.toLowerCase() ?? "unknown"}`))]
        throw new Error(
            `Unsupported mesh format(s) in URDF: ${formats.join(", ")}. Only STL, OBJ, and glTF exports are supported.`
        )
    }
}

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

            // 3. Basename only (e.g. "part.stl")
            const basename = path.split("/").pop()!
            if (!meshFiles.has(basename)) meshFiles.set(basename, bytes)
        })
    )

    return meshFiles
}

export async function loadURDF(buffer: ArrayBuffer, filename: string, progressHandle?:ProgressHandle): Promise<mirabuf.Assembly> {
    const ext = filename.split(".").pop()?.toLowerCase()

    if (ext === "urdf") {
        throw new Error(
            "Plain URDF files are not supported. Please select a ZIP archive containing the URDF and its meshes."
        )
    }

    if (ext === "zip") {
        const zip = await JSZip.loadAsync(buffer)
        console.timeLog("URDF Import", "Unzipped")
        const urdfEntry = Object.values(zip.files).find(f => !f.dir && f.name.endsWith(".urdf"))
        if (!urdfEntry) throw new Error("No .urdf file found in the zip archive")
        const [urdfText, meshFiles] = await Promise.all([urdfEntry.async("text"), buildMeshMap(zip, urdfEntry.name)])
        progressHandle?.update("Loaded meshes", 0.3)
        console.timeLog("URDF Import", "Mesh Map Built")
        validateURDFMeshFormats(urdfText)
        const assembly = convertURDF(urdfText, meshFiles)
        console.timeLog("URDF Import", "Converted")
        detectAndTagWheels(assembly)
        console.timeLog("URDF Import", "Tagged Wheels")
        applyConservativeURDFImport(assembly)
        console.timeLog("URDF Import", "Imported")

        return assembly
    }

    throw new Error(`Unsupported file extension: .${ext ?? "unknown"}`)
}
