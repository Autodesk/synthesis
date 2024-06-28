import { mirabuf } from "../proto/mirabuf"
import Pako from "pako"
// import * as fs from "fs"

const robots = "Robots"
const fields = "Fields"
const root = await navigator.storage.getDirectory()
const robotFolderHandle = await root.getDirectoryHandle(robots, { create: true })
const fieldFolderHandle = await root.getDirectoryHandle(fields, { create: true })

export function UnzipMira(buff: Uint8Array): Uint8Array {
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff)
    } else {
        return buff
    }
}

export async function LoadMirabufRemote(fetchLocation: string, type: MiraType): Promise<mirabuf.Assembly | undefined> {
    const map = GetMap(type)
    const targetID = map != undefined ? map[fetchLocation] : undefined

    if (targetID != undefined) {
        console.log("Loading mira from cache")
        return (await LoadMirabufCache(fetchLocation, targetID, type, map)) ?? LoadAndCacheMira(fetchLocation, type)
    } else {
        console.log("Loading and caching new mira")
        return await LoadAndCacheMira(fetchLocation, type)
    }
}

// export function LoadMirabufLocal(fileLocation: string): mirabuf.Assembly {
//     return mirabuf.Assembly.decode(UnzipMira(new Uint8Array(fs.readFileSync(fileLocation))))
// }

export async function ClearMira() {
    for await (const key of robotFolderHandle.keys()) {
        robotFolderHandle.removeEntry(key)
    }
    for await (const key of fieldFolderHandle.keys()) {
        fieldFolderHandle.removeEntry(key)
    }

    window.localStorage.removeItem(robots)
    window.localStorage.removeItem(fields)
}

export function GetMap(type: MiraType): any {
    const miraJSON = window.localStorage.getItem(type == MiraType.ROBOT ? robots : fields)

    if (miraJSON != undefined) {
        console.log("mirabuf JSON found")
        return JSON.parse(miraJSON)
    } else {
        console.log("mirabuf JSON not found")
        return undefined
    }
}

async function LoadAndCacheMira(fetchLocation: string, type: MiraType): Promise<mirabuf.Assembly | undefined> {
    try {
        const backupID = Date.now().toString()

        // Grab file remote
        const miraBuff = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            .then(x => x.blob())
            .then(x => x.arrayBuffer())
        const byteBuffer = UnzipMira(new Uint8Array(miraBuff))
        const assembly = mirabuf.Assembly.decode(byteBuffer)

        // Store in OPFS
        const fileHandle = await (type == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(
            backupID,
            { create: true }
        )
        const writable = await fileHandle.createWritable()
        await writable.write(miraBuff)
        await writable.close()

        // Local cache map
        let map: { [k: string]: string } = GetMap(type) ?? {}
        map[fetchLocation] = backupID
        window.localStorage.setItem(type == MiraType.ROBOT ? robots : fields, JSON.stringify(map))

        console.log(assembly)
        return assembly
    } catch (e) {
        console.error("Failed to load and cache mira")
    }
}

async function LoadMirabufCache(
    fetchLocation: string,
    targetID: string,
    type: MiraType,
    map: { [k: string]: string }
): Promise<mirabuf.Assembly | undefined> {
    try {
        const fileHandle =
            (await (type == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(targetID, {
                create: false,
            })) ?? undefined

        // Get assembly from file
        if (fileHandle != undefined) {
            const buff = await fileHandle.getFile().then(x => x.arrayBuffer())
            const assembly = mirabuf.Assembly.decode(UnzipMira(new Uint8Array(buff)))
            console.log(assembly)
            return assembly
        }
    } catch (e) {
        console.error("Failed to find file from OPFS")

        // Remove from localStorage list
        delete map[fetchLocation]
        window.localStorage.setItem(type == MiraType.ROBOT ? robots : fields, JSON.stringify(map))

        return undefined
    }
}

export enum MiraType {
    ROBOT,
    FIELD,
}
