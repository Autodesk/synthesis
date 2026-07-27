import path from "node:path"
import * as fs from "fs/promises"
import type { ManifestFileType } from "./manifest"
import { hashBuffer, hexStringToUint8Array, unzipMira } from "@/util/Utility"
import { mirabuf } from "@/proto/mirabuf"
import { v4 as uuidV4 } from "uuid"
import FieldMiraEditor from "@/mirabuf/FieldMiraEditor.ts"

const BASE_PATH = "public/Downloadables/Mira"
const MAP: ManifestFileType = { fields: [], private: [], robots: [] }

const DIRS = Object.keys(MAP) as (keyof typeof MAP)[]

async function main() {
    for (const dirname of DIRS) {
        const list = MAP[dirname]
        for await (const file of await fs.opendir(path.join(BASE_PATH, dirname))) {
            if (file.isDirectory() || !file.name.endsWith(".mira")) {
                continue
            }
            const originalPath = path.join(file.parentPath, file.name)
            const data = await fs.readFile(originalPath)
            const originalHash = await hashBuffer(data.buffer as ArrayBuffer)

            const assembly = mirabuf.Assembly.decode(unzipMira(new Uint8Array(data.buffer)))

            // Add GUID because the exporter doesn't
            if (!assembly.info?.GUID?.match(/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}/)) {
                assembly.info!.GUID = uuidV4({ random: hexStringToUint8Array(originalHash).slice(0, 16) })
                console.log("Generated GUID for", file.name, "->", assembly.info!.GUID)
            }

            // Normalize Mira assembly name to match file name
            assembly.info!.name = file.name.replace(".mira", "").replace("_", " ")
            const name = assembly.info!.name! + ".mira"

            // Auto migrate old system
            const fieldEditor = new FieldMiraEditor(assembly.data!.parts!)
            const fieldPrefs = fieldEditor.getUserData("synthesis:field_preferences")
            if (fieldPrefs) {
                fieldEditor.migrateDevtoolFieldData(fieldPrefs)
            }

            const robotPrefs = fieldEditor.getUserData("synthesis:robot_preferences")
            if (robotPrefs) {
                fieldEditor.migrateDevtoolRobotData(robotPrefs)
            }

            const updated = mirabuf.Assembly.encode(assembly).finish()
            const updatedHash = await hashBuffer(updated.buffer as ArrayBuffer)

            // Update only if changes are made (avoid updating modification times otherwise)
            if (originalHash !== updatedHash) {
                const newPath = path.join(file.parentPath, name)
                await fs.writeFile(newPath, updated)
                if (newPath !== originalPath) {
                    await fs.rm(originalPath)
                }
            }
            list.push({ filename: name, hash: updatedHash })
        }
    }
    await fs.writeFile(path.join(BASE_PATH, "manifest.json"), JSON.stringify(MAP))
}

main().catch(console.error)
