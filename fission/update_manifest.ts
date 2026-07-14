import path from "node:path"
import * as fs from "fs/promises"
import type { ManifestFileType } from "./manifest"
import { hashBuffer, hexStringToUint8Array, unzipMira } from "@/util/Utility"
import { mirabuf } from "@/proto/mirabuf"
import { v4 as uuidV4 } from "uuid"
import FieldMiraEditor from "@/mirabuf/FieldMiraEditor.ts"

const basepath = "public/Downloadables/mira"
const map: ManifestFileType = { fields: [], private: [], robots: [] }

const dirs = Object.keys(map) as (keyof typeof map)[]

/**
 * Derive the competition year from an asset's (normalized) name.
 * Prefers an explicitly parenthesized year (e.g. "KitBot (2024)"), otherwise
 * falls back to the last four-digit 19xx/20xx run in the string (e.g. "FRC Field 2026 v2").
 * Returns undefined when no year is present so the asset lands in the "Other" group.
 */
function parseYear(name: string): number | undefined {
    const parenthesized = name.match(/\((19|20)\d{2}\)/g)
    if (parenthesized) {
        return Number(parenthesized[parenthesized.length - 1].replace(/[()]/g, ""))
    }
    const loose = name.match(/(19|20)\d{2}/g)
    if (loose) {
        return Number(loose[loose.length - 1])
    }
    return undefined
}

/**
 * Look for a sibling thumbnail image at "<dir>/thumbnails/<name>.png".
 * Returns the path relative to the asset directory, or undefined when absent.
 */
async function resolveThumbnail(dirname: string, name: string): Promise<string | undefined> {
    const relative = `thumbnails/${name.replace(/\.mira$/, ".png")}`
    return fs
        .access(path.join(basepath, dirname, relative))
        .then(() => relative)
        .catch(() => undefined)
}

async function main() {
    for (const dirname of dirs) {
        const list = map[dirname]
        for await (const file of await fs.opendir(path.join(basepath, dirname))) {
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
            list.push({
                filename: name,
                hash: updatedHash,
                year: parseYear(name),
                thumbnail: await resolveThumbnail(dirname, name),
            })
        }
    }
    await fs.writeFile(path.join(basepath, "manifest.json"), JSON.stringify(map))
}

main().catch(console.error)
