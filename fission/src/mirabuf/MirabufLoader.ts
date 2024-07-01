import { mirabuf } from "@/proto/mirabuf"
import Pako from "pako"

type MiraCache = { [id: string] : string}

const robotsDirName = "Robots"
const fieldsDirName = "Fields"
const root = await navigator.storage.getDirectory()
const robotFolderHandle = await root.getDirectoryHandle(robotsDirName, { create: true })
const fieldFolderHandle = await root.getDirectoryHandle(fieldsDirName, { create: true })

export function UnzipMira(buff: Uint8Array): Uint8Array {
    // Check if file is gzipped via magic gzip numbers 31 139
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff)
    } else {
        return buff
    }
}

export async function LoadMirabufRemote(fetchLocation: string, type: MiraType, blobHashID?: string): Promise<mirabuf.Assembly | undefined> {
    const map = GetMiraCacheMap(type)

    // blobHashID for ImportLocalMira
    const targetID = map ? ( blobHashID ? map[blobHashID] : map[fetchLocation]) : undefined

    if (targetID != undefined && map != undefined) {
        console.log("Loading mira from cache")
        return (await LoadMirabufCache(fetchLocation, targetID, type, map)) ?? LoadAndCacheMira(fetchLocation, type, blobHashID)
    } else {
        console.log("Loading and caching new mira")
        return await LoadAndCacheMira(fetchLocation, type, blobHashID)
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

    window.localStorage.removeItem(robotsDirName)
    window.localStorage.removeItem(fieldsDirName)
}

export function GetMiraCacheMap(type: MiraType): MiraCache | undefined {
    const miraJSON = window.localStorage.getItem(type == MiraType.ROBOT ? robotsDirName : fieldsDirName)

    if (miraJSON != undefined) {
        console.log("mirabuf JSON found")
        console.log(miraJSON)
        return JSON.parse(miraJSON)
    } else {
        console.log("mirabuf JSON not found")
        return undefined
    }
}

async function LoadAndCacheMira(fetchLocation: string, type: MiraType, blobHashID?: string): Promise<mirabuf.Assembly | undefined> {
    try {
        // Grab file remote
        const miraBuff = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            .then(x => x.blob())
            .then(x => x.arrayBuffer())
        const byteBuffer = UnzipMira(new Uint8Array(miraBuff))
        const assembly = mirabuf.Assembly.decode(byteBuffer)

        // Store in OPFS
        const backupID = Date.now().toString()
        const fileHandle = await (type == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(
            backupID,
            { create: true }
        )
        const writable = await fileHandle.createWritable()
        await writable.write(miraBuff)
        await writable.close()

        // Local cache map
        const map: MiraCache = GetMiraCacheMap(type) ?? {}
        map[blobHashID ?? fetchLocation] = backupID
        window.localStorage.setItem(type == MiraType.ROBOT ? robotsDirName : fieldsDirName, JSON.stringify(map))

        console.log(assembly)
        return assembly
    } catch (e) {
        console.error("Failed to load and cache mira " + e)
        return undefined
    }
}

async function LoadMirabufCache(
    fetchLocation: string,
    targetID: string,
    type: MiraType,
    map: MiraCache
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
        console.error("Failed to find file from OPFS" + e)

        // Remove from localStorage list
        delete map[fetchLocation]
        window.localStorage.setItem(type == MiraType.ROBOT ? robotsDirName : fieldsDirName, JSON.stringify(map))

        return undefined
    }
}

export enum MiraType {
    ROBOT,
    FIELD,
}
