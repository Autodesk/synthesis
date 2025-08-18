import path from "node:path";
import * as fs from "fs/promises"
import {hashBuffer} from "./src/util/Utility.ts";


const basepath = "public/Downloadables/Mira"
const keys = ["fields", "robots", "private"] as const

export type ManifestFileType = Record<typeof keys[number], { filename: string, hash: string }[]>

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