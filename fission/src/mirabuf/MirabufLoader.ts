import { mirabuf } from "@/proto/mirabuf"
import Pako from "pako"

const MIRABUF_LOCALSTORAGE_GENERATION_KEY = "Synthesis Nonce Key"
const MIRABUF_LOCALSTORAGE_GENERATION = "4543246"

export type MirabufCacheID = string;

export interface MirabufCacheInfo {
    id: MirabufCacheID
    cacheKey: string
    miraType: MiraType
    name?: string
    thumbnailStorageID?: string
}

type MiraCache = { [id: string] : MirabufCacheInfo }

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

class MirabufCachingService {

    /**
     * Get the map of mirabuf keys and paired MirabufCacheInfo from local storage
     * 
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * 
     * @returns {MiraCache} Map of cached keys and paired MirabufCacheInfo
     */
    public static GetCacheMap(miraType: MiraType): MiraCache {
        if ((window.localStorage.getItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY) ?? "") == MIRABUF_LOCALSTORAGE_GENERATION) {
            window.localStorage.setItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY, MIRABUF_LOCALSTORAGE_GENERATION)
            window.localStorage.setItem(robotsDirName, "{}")
            window.localStorage.setItem(fieldsDirName, "{}")
            return {}
        }

        const key = miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName
        const map = window.localStorage.getItem(key)

        if (map) {
            console.log("mirabuf JSON found")
            console.log(map)
            return JSON.parse(map)
        } else {
            console.log("mirabuf JSON not found. Creating blank cache")
            window.localStorage.setItem(key, "{}")
            return {}
        }
    }

    /**
     * Cache remote Mirabuf file
     * 
     * @param {string} fetchLocation Location of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * 
     * @returns {Promise<MirabufCacheInfo | undefined>} Promise with the result of the promise. Metadata on the mirabuf file if successful, undefined if not.
     */
    public static async CacheRemote(fetchLocation: string, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        const map = MirabufCachingService.GetCacheMap(miraType)
        const target = map[fetchLocation]
    
        if (target) {
            console.log("Mira in cache")
            return target
        }
            
        console.log("Caching new mira")

        // Grab file remote
        const miraBuff = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            .then(x => x.blob())
            .then(x => x.arrayBuffer())
        return await MirabufCachingService.StoreInCache(fetchLocation, miraBuff, miraType)
    }

    /**
     * Cache local Mirabuf file
     * 
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * 
     * @returns {Promise<MirabufCacheInfo | undefined>} Promise with the result of the promise. Metadata on the mirabuf file if successful, undefined if not.
     */
    public static async CacheLocal(buffer: ArrayBuffer, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        const hashBuffer = await crypto.subtle.digest("SHA-256",  buffer)
        let hash = ""
        new Uint8Array(hashBuffer).forEach(x => (hash = hash + String.fromCharCode(x)))

        const map = MirabufCachingService.GetCacheMap(miraType)
        const target = map[hash]
    
        if (target) {
            console.log("Mira in cache")
            return target
        }

        return await MirabufCachingService.StoreInCache(hash, hashBuffer, miraType)
    }
    
    /**
     * Gets a given Mirabuf file from the cache
     * 
     * @param {MirabufCacheID} id ID to the given Mirabuf file in the caching service. Obtainable via GetCacheMaps().
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * 
     * @returns {Promise<mirabufAssembly | undefined>} Promise with the result of the promise. Assembly of the mirabuf file if successful, undefined if not.
     */
    public static async Get(id: MirabufCacheID, miraType: MiraType): Promise<mirabuf.Assembly | undefined> {
        try {
            const fileHandle =
                await (miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(id, {
                    create: false,
                })
    
            // Get assembly from file
            if (fileHandle) {
                const buff = await fileHandle.getFile().then(x => x.arrayBuffer())
                const assembly = mirabuf.Assembly.decode(UnzipMira(new Uint8Array(buff)))
                console.log(assembly)
                return assembly
            } else {
                console.error(`Failed to get file handle for ID: ${id}`)
                return undefined
            }
        } catch (e) {
            console.error(`Failed to find file from OPFS\n${e}`)
            return undefined
        }
    }

    /**
     * Removes a given Mirabuf file from the cache
     * 
     * @param {string} key Key to the given Mirabuf file entry in the caching service. Obtainable via GetCacheMaps().
     * @param {MirabufCacheID} id ID to the given Mirabuf file in the caching service. Obtainable via GetCacheMaps().
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * 
     * @returns {Promise<boolean>} Promise with the result of the promise. True if successful, false if not.
     */
    public static async Remove(key: string, id: MirabufCacheID, miraType: MiraType): Promise<boolean> {
        try {
            window.localStorage.removeItem(key)

            const dir = miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
            await dir.removeEntry(id)

            return true
        } catch (e) {
            console.error(`Failed to remove\n${e}`)
            return false
        }
    }

    /**
     * Removes all Mirabuf files from the caching services. Mostly for debugging purposes.
     */
    public static async RemoveAll() {
        for await (const key of robotFolderHandle.keys()) {
            robotFolderHandle.removeEntry(key)
        }
        for await (const key of fieldFolderHandle.keys()) {
            fieldFolderHandle.removeEntry(key)
        }
    
        window.localStorage.removeItem(robotsDirName)
        window.localStorage.removeItem(fieldsDirName)
    }

    private static async StoreInCache(key: string, miraBuff: ArrayBuffer, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        // Store in OPFS
        const backupID = Date.now().toString()
        try {
            const fileHandle = await (miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(
                backupID,
                { create: true }
            )
            const writable = await fileHandle.createWritable()
            await writable.write(miraBuff)
            await writable.close()
    
            // Local cache map
            const map: MiraCache = this.GetCacheMap(miraType)
            const info: MirabufCacheInfo = {
                id: backupID,
                cacheKey: key,
                miraType: miraType
            }
            map[key] = info
            window.localStorage.setItem(miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName, JSON.stringify(map))
            
            return info
        } catch (e) {
            console.log("Failed to cache mira " + e)
            return undefined
        }
    }
}


export enum MiraType {
    ROBOT,
    FIELD,
}

export default MirabufCachingService
