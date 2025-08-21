import crypto from "node:crypto"
import path from "node:path";
import * as fs from "fs/promises"
import type {ManifestFileType} from "./manifest";

const basepath = "public/Downloadables/Mira"
const map: ManifestFileType = {fields: [], private: [], robots: []}

const dirs = Object.keys(map) as (keyof typeof map)[]

async function main() {
    for (const dirname of dirs) {
        const list = map[dirname]
        for await (const file of await fs.opendir(path.join(basepath, dirname))) {
            if (file.isDirectory() || !file.name.endsWith(".mira")) {
                continue
            }

            const data = await fs.readFile(path.join(file.parentPath, file.name))
            list.push({filename: file.name, hash: await hashBuffer(data.buffer as ArrayBuffer)})
        }
    }
    await fs.writeFile(path.join(basepath, "manifest.json"), JSON.stringify(map))
}

main().catch(console.error)

export async function hashBuffer(buffer:ArrayBuffer) {
    const hashBuffer = await crypto.subtle.digest("SHA-1", buffer)
    return Array.from(new Uint8Array(hashBuffer))
        .map(x => x.toString(16))
        .join("")
}