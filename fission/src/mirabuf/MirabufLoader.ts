import { Data, downloadData } from "@/aps/APSDataManagement"
import { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import Pako from "pako"

const MIRABUF_LOCALSTORAGE_GENERATION_KEY = "Synthesis Nonce Key"
const MIRABUF_LOCALSTORAGE_GENERATION = "4543246"

export type MirabufCacheID = string

export interface MirabufCacheInfo {
    id: MirabufCacheID
    cacheKey: string
    miraType: MiraType
    name?: string
    thumbnailStorageID?: string
}

export interface MirabufRemoteInfo {
    displayName: string
    src: string
}

type MapCache = { [id: string]: MirabufCacheInfo }
type BackUpBufferCache = { [id: string]: ArrayBuffer }

// Local Storage
const robotMapDirName = "RobotMap"
const fieldMapDirName = "FieldMap"
const backUpRobotsDirName = "BackUpRobots"
const backUpFieldsDirName = "BackUpFields"

// OPFS
const robotsDirName = "Robots"
const fieldsDirName = "Fields"
const root = await navigator.storage.getDirectory()
const robotFolderHandle = await root.getDirectoryHandle(robotsDirName, { create: true })
const fieldFolderHandle = await root.getDirectoryHandle(fieldsDirName, { create: true })

const canOPFS = await (async() => {
    try {
        console.log(`trying OPFS`)

        if (robotFolderHandle.name == robotsDirName) {
            robotFolderHandle.entries
            robotFolderHandle.keys

            const fileHandle = await robotFolderHandle.getFileHandle(
                "0",
                { create: true }
            )
            const writable = await fileHandle.createWritable() 
            await writable.close()
            await fileHandle.getFile()

            robotFolderHandle.removeEntry(fileHandle.name)

            console.log(`yes OPFS`)
            return true
        } else {
            console.log(`no OPFS`)
            return false
        }
    } catch (e) {
        console.log(`no OPFS`)
        return false
    }
})()

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
     * @param {MiraType} miraType Type of Mirabuf Assembly
     *
     * @returns {MapCache} Map of cached keys and paired MirabufCacheInfo
     */
    public static GetCacheMap(miraType: MiraType): MapCache {
        if (
            (window.localStorage.getItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY) ?? "") == MIRABUF_LOCALSTORAGE_GENERATION
        ) {
            window.localStorage.setItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY, MIRABUF_LOCALSTORAGE_GENERATION)
            window.localStorage.setItem(robotMapDirName, "{}")
            window.localStorage.setItem(fieldMapDirName, "{}")
            return {}
        }

        const key = miraType == MiraType.ROBOT ? robotMapDirName : fieldMapDirName
        const map = window.localStorage.getItem(key)

        if (map) {
            return JSON.parse(map)
        } else {
            window.localStorage.setItem(key, "{}")
            return {}
        }
    }

    /**
     * Get back up cache from local storage. Used if no access to OPFS (Safari and iOS)
     * @param miraType Type of Mirabuf Assembly
     * @returns IDs and buffers
     */
    public static GetBackUpCache(miraType: MiraType): BackUpBufferCache {
        if (
            (window.localStorage.getItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY) ?? "") == MIRABUF_LOCALSTORAGE_GENERATION
        ) {
            window.localStorage.setItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY, MIRABUF_LOCALSTORAGE_GENERATION)
            window.localStorage.setItem(robotMapDirName, "{}")
            window.localStorage.setItem(fieldMapDirName, "{}")
            return {}
        }

        const key = miraType == MiraType.ROBOT ? backUpRobotsDirName : backUpFieldsDirName
        const map = window.localStorage.getItem(key)

        if (map) {
            return JSON.parse(map)
        } else {
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
    public static async CacheRemote(fetchLocation: string, miraType?: MiraType): Promise<MirabufCacheInfo | undefined> {
        if (miraType) {
            const map = MirabufCachingService.GetCacheMap(miraType)
            const target = map[fetchLocation]

            if (target) {
                return target
            }
        }

        // Grab file remote
        const miraBuff = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            .then(x => x.blob())
            .then(x => x.arrayBuffer())
        World.AnalyticsSystem?.Event("Remote Download", {
            type: miraType == MiraType.ROBOT ? "robot" : "field",
            fileSize: miraBuff.byteLength,
        })
        return await MirabufCachingService.StoreInCache(fetchLocation, miraBuff, miraType)
    }

    public static async CacheAPS(data: Data, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        if (!data.href) {
            console.error("Data has no href")
            return undefined
        }

        const map = MirabufCachingService.GetCacheMap(miraType)
        const target = map[data.id]

        if (target) {
            return target
        }

        const miraBuff = await downloadData(data)
        if (!miraBuff) {
            console.error("Failed to download file")
            return undefined
        }

        World.AnalyticsSystem?.Event("APS Download", {
            type: miraType == MiraType.ROBOT ? "robot" : "field",
            fileSize: miraBuff.byteLength,
        })

        return await MirabufCachingService.StoreInCache(data.id, miraBuff, miraType)
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
        const key = await this.HashBuffer(buffer)

        const map = MirabufCachingService.GetCacheMap(miraType)
        const target = map[key]

        if (target) {
            return target
        }

        return await MirabufCachingService.StoreInCache(key, buffer, miraType)
    }

    /**
     * Caches metadata (name or thumbnailStorageID) for a key
     *
     * @param {string} key Key to the given Mirabuf file entry in the caching service. Obtainable via GetCacheMaps().
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * @param {string} name (Optional) Name of Mirabuf Assembly.
     * @param {string} thumbnailStorageID (Optional) ID of the the thumbnail storage for the Mirabuf Assembly.
     */
    public static async CacheInfo(
        key: string,
        miraType: MiraType,
        name?: string,
        thumbnailStorageID?: string
    ): Promise<boolean> {
        try {
            const map: MapCache = this.GetCacheMap(miraType)
            const id = map[key].id
            const _name = map[key].name
            const _thumbnailStorageID = map[key].thumbnailStorageID
            const info: MirabufCacheInfo = {
                id: id,
                cacheKey: key,
                miraType: miraType,
                name: name ?? _name,
                thumbnailStorageID: thumbnailStorageID ?? _thumbnailStorageID,
            }
            map[key] = info
            window.localStorage.setItem(miraType == MiraType.ROBOT ? robotMapDirName : fieldMapDirName, JSON.stringify(map))
            return true
        } catch (e) {
            console.error(`Failed to cache info\n${e}`)
            return false
        }
    }

    /**
     * Caches and gets local Mirabuf file
     *
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     *
     * @returns {Promise<mirabufAssembly | undefined>} Promise with the result of the promise. Assembly of the mirabuf file if successful, undefined if not.
     */
    public static async CacheAndGetLocal(
        buffer: ArrayBuffer,
        miraType: MiraType
    ): Promise<mirabuf.Assembly | undefined> {
        const key = await this.HashBuffer(buffer)
        const map = MirabufCachingService.GetCacheMap(miraType)
        const target = map[key]
        const assembly = this.AssemblyFromBuffer(buffer)

        if (!target) {
            await MirabufCachingService.StoreInCache(key, buffer, miraType, assembly.info?.name ?? undefined)
        }

        return assembly
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
            // If we have access to OPFS, get file handle from OPFS
            const fileHandle = canOPFS ? await (miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(
                id,
                {
                    create: false,
                }
            ) : undefined

            // If we have OPFS file, get buffer from OPFS, otherwise get from local storage
            const buff = fileHandle ? await fileHandle.getFile().then(x => x.arrayBuffer()) : this.GetBackUpCache(miraType)[id]

            if (buff) {
                const assembly = this.AssemblyFromBuffer(buff)
                World.AnalyticsSystem?.Event("Cache Get", {
                    key: id,
                    type: miraType == MiraType.ROBOT ? "robot" : "field",
                    assemblyName: assembly.info!.name!,
                    fileSize: buff.byteLength,
                })
                return assembly
            } else {
                console.error(`Failed to find file for id: ${id}`)
            }
        } catch (e) {
            console.error(`Failed to find file\n${e}`)
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
            const map = this.GetCacheMap(miraType)
            if (map) {
                delete map[key]
                window.localStorage.setItem(
                    miraType == MiraType.ROBOT ? robotMapDirName : fieldMapDirName,
                    JSON.stringify(map)
                )
            }

            if (canOPFS) {
                const dir = miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
                await dir.removeEntry(id)
            } else {
                const backUpCache = this.GetBackUpCache(miraType)
                if (backUpCache) {
                    delete backUpCache[key]
                    window.localStorage.setItem(
                        miraType == MiraType.ROBOT ? backUpRobotsDirName : backUpFieldsDirName,
                        JSON.stringify(backUpCache)
                    )
                }
            }

            World.AnalyticsSystem?.Event("Cache Remove", {
                key: key,
                type: miraType == MiraType.ROBOT ? "robot" : "field",
            })
            return true
        } catch (e) {
            console.error(`Failed to remove\n${e}`)
            World.AnalyticsSystem?.Exception("Failed to remove mirabuf from cache")
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

        window.localStorage.removeItem(robotMapDirName)
        window.localStorage.removeItem(fieldMapDirName)
        window.localStorage.removeItem(backUpRobotsDirName)
        window.localStorage.removeItem(backUpFieldsDirName)
    }

    // Optional name for when assembly is being decoded anyway like in CacheAndGetLocal()
    private static async StoreInCache(
        key: string,
        miraBuff: ArrayBuffer,
        miraType?: MiraType,
        name?: string
    ): Promise<MirabufCacheInfo | undefined> {
        const backupID = Date.now().toString()
        try {
            if (!miraType) {
                console.log("Double loading")
                miraType = this.AssemblyFromBuffer(miraBuff).dynamic ? MiraType.ROBOT : MiraType.FIELD
            }

            // Local cache map
            const map: MapCache = this.GetCacheMap(miraType)
            const info: MirabufCacheInfo = {
                id: backupID,
                cacheKey: key,
                miraType: miraType,
                name: name,
            }
            map[key] = info
            window.localStorage.setItem(miraType == MiraType.ROBOT ? robotMapDirName : fieldMapDirName, JSON.stringify(map))

            World.AnalyticsSystem?.Event("Cache Store", {
                name: name ?? "-",
                key: key,
                type: miraType == MiraType.ROBOT ? "robot" : "field",
                fileSize: miraBuff.byteLength,
            })

            if (canOPFS) {
                // Store in OPFS
                const fileHandle = await (miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle).getFileHandle(
                    backupID,
                    { create: true }
                )
                const writable = await fileHandle.createWritable()
                await writable.write(miraBuff)
                await writable.close()
            } else {
                // Store in localStorage
                const cache: BackUpBufferCache = this.GetBackUpCache(miraType)
                cache[key] = miraBuff
                window.localStorage.setItem(miraType == MiraType.ROBOT ? backUpRobotsDirName : backUpFieldsDirName, JSON.stringify(cache))
            }

            return info
        } catch (e) {
            console.error("Failed to cache mira " + e)
            World.AnalyticsSystem?.Exception("Failed to store in cache")
            return undefined
        }
    }

    private static async HashBuffer(buffer: ArrayBuffer): Promise<string> {
        const hashBuffer = await crypto.subtle.digest("SHA-256", buffer)
        let hash = ""
        new Uint8Array(hashBuffer).forEach(x => (hash = hash + String.fromCharCode(x)))
        return btoa(hash).replace(/\+/g, "-").replace(/\//g, "_").replace(/=/g, "")
    }

    private static AssemblyFromBuffer(buffer: ArrayBuffer): mirabuf.Assembly {
        return mirabuf.Assembly.decode(UnzipMira(new Uint8Array(buffer)))
    }
}

export enum MiraType {
    ROBOT = 1,
    FIELD,
}

export default MirabufCachingService
