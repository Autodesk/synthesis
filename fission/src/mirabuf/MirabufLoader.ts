import { Data, downloadData } from "@/aps/APSDataManagement"
import { mirabuf } from "@/proto/mirabuf"
import { globalAddToast } from "@/components/GlobalUIControls"
import World from "@/systems/World"
import Pako from "pako"

const MIRABUF_LOCALSTORAGE_GENERATION_KEY = "Synthesis Nonce Key"
const MIRABUF_LOCALSTORAGE_GENERATION = "4543246"

export type MirabufCacheID = string

export interface MirabufCacheInfo {
    id: MirabufCacheID
    miraType: MiraType
    cacheKey: string
    buffer?: ArrayBuffer
    name?: string
    thumbnailStorageID?: string
}

export interface MirabufRemoteInfo {
    displayName: string
    src: string
}

type MapCache = { [id: MirabufCacheID]: MirabufCacheInfo }

const robotsDirName = "Robots"
const fieldsDirName = "Fields"
const root = await navigator.storage.getDirectory()
const robotFolderHandle = await root.getDirectoryHandle(robotsDirName, { create: true })
const fieldFolderHandle = await root.getDirectoryHandle(fieldsDirName, { create: true })

export let backUpRobots: MapCache = {}
export let backUpFields: MapCache = {}

export const canOPFS = await (async () => {
    try {
        if (robotFolderHandle.name == robotsDirName) {
            robotFolderHandle.entries
            robotFolderHandle.keys

            const fileHandle = await robotFolderHandle.getFileHandle("0", { create: true })
            const writable = await fileHandle.createWritable()
            await writable.close()
            await fileHandle.getFile()

            robotFolderHandle.removeEntry(fileHandle.name)

            return true
        } else {
            console.log(`No access to OPFS`)
            return false
        }
    } catch (e) {
        console.log(`No access to OPFS`)

        // Copy-pasted from RemoveAll()
        for await (const key of robotFolderHandle.keys()) {
            robotFolderHandle.removeEntry(key)
        }
        for await (const key of fieldFolderHandle.keys()) {
            fieldFolderHandle.removeEntry(key)
        }

        window.localStorage.setItem(robotsDirName, "{}")
        window.localStorage.setItem(fieldsDirName, "{}")

        backUpRobots = {}
        backUpFields = {}

        return false
    }
})()

export function unzipMira(buff: Uint8Array): Uint8Array {
    // Check if file is gzipped via magic gzip numbers 31 139
    if (buff[0] == 31 && buff[1] == 139) {
        return Pako.ungzip(buff)
    } else {
        return buff
    }
}

class MirabufCachingService {
    // Request deduplication: track ongoing fetch requests to prevent race conditions
    private static _ongoingRequests = new Map<string, Promise<MirabufCacheInfo | undefined>>()

    // Storage operation locks to prevent concurrent storage race conditions
    private static _storageOperations = new Map<string, Promise<MirabufCacheInfo | undefined>>()
    private static _idCounter = 0

    /**
     * Get the map of mirabuf keys and paired MirabufCacheInfo from local storage
     *
     * @param {MiraType} miraType Type of Mirabuf Assembly
     *
     * @returns {MapCache} Map of cached keys and paired MirabufCacheInfo
     */
    public static getCacheMap(miraType: MiraType): MapCache {
        if (
            (window.localStorage.getItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY) ?? "") != MIRABUF_LOCALSTORAGE_GENERATION
        ) {
            window.localStorage.setItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY, MIRABUF_LOCALSTORAGE_GENERATION)
            this.removeAll()
            return {}
        }

        const key = miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName
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
    public static async cacheRemote(fetchLocation: string, miraType?: MiraType): Promise<MirabufCacheInfo | undefined> {
        if (miraType !== undefined) {
            const map = MirabufCachingService.getCacheMap(miraType)
            const target = map[fetchLocation]
            if (target) return target
        }

        // Check if there's already an ongoing request for this URL
        const requestKey = `${fetchLocation}:${miraType ?? "unknown"}`
        const ongoingRequest = this._ongoingRequests.get(requestKey)
        if (ongoingRequest) {
            return ongoingRequest
        }

        const requestPromise = this._fetchAndCache(fetchLocation, miraType)
        this._ongoingRequests.set(requestKey, requestPromise)
        requestPromise.finally(() => {
            this._ongoingRequests.delete(requestKey)
        })

        return requestPromise
    }

    private static async _fetchAndCache(
        fetchLocation: string,
        miraType?: MiraType
    ): Promise<MirabufCacheInfo | undefined> {
        try {
            // grab file remote
            const resp = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            if (!resp.ok) throw new Error(`${resp.status} ${resp.statusText}`)

            const miraBuff = await resp.arrayBuffer()

            World.analyticsSystem?.event("Remote Download", {
                type: miraType === MiraType.ROBOT ? "robot" : "field",
                fileSize: miraBuff.byteLength,
            })

            const cached = await MirabufCachingService.storeInCache(fetchLocation, miraBuff, miraType)

            if (cached) {
                // Verify that the cached data is actually retrievable
                const verification = await MirabufCachingService.get(cached.id, cached.miraType)
                if (verification) {
                    return cached
                } else {
                    console.warn(
                        `Storage verification failed for "${fetchLocation}" - data not retrievable despite successful cache operation`
                    )
                }
            }

            console.warn(`Primary caching failed for "${fetchLocation}", creating emergency fallback`)
            globalAddToast("error", "Cache Fallback", `Unable to cache "${fetchLocation}". Using raw buffer instead.`)

            // Emergency fallback: return raw buffer wrapped in MirabufCacheInfo
            const fallbackId = `${Date.now()}_${++this._idCounter}_fallback`
            const resolvedMiraType =
                miraType ?? (this.assemblyFromBuffer(miraBuff).dynamic ? MiraType.ROBOT : MiraType.FIELD)

            const fallbackInfo: MirabufCacheInfo = {
                id: fallbackId,
                miraType: resolvedMiraType,
                cacheKey: fetchLocation,
                buffer: miraBuff,
            }

            // Store fallback in memory cache so get() can find it later
            const cache = resolvedMiraType == MiraType.ROBOT ? backUpRobots : backUpFields
            cache[fallbackId] = fallbackInfo

            // Also try to update localStorage for future reference
            try {
                const map: MapCache = this.getCacheMap(resolvedMiraType)
                map[fetchLocation] = {
                    id: fallbackId,
                    miraType: resolvedMiraType,
                    cacheKey: fetchLocation,
                    name: `Fallback: ${fetchLocation}`,
                }
                window.localStorage.setItem(
                    resolvedMiraType == MiraType.ROBOT ? robotsDirName : fieldsDirName,
                    JSON.stringify(map)
                )
            } catch (lsError) {
                console.warn(`Fallback localStorage update failed:`, lsError)
            }

            return fallbackInfo
        } catch (e) {
            console.warn("Caching failed", e)
            return undefined
        }
    }

    public static async cacheAPS(data: Data, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        if (!data.href) {
            console.error("Data has no href")
            return undefined
        }

        const map = MirabufCachingService.getCacheMap(miraType)
        const target = map[data.id]

        if (target) {
            return target
        }

        const miraBuff = await downloadData(data)
        if (!miraBuff) {
            console.error("Failed to download file")
            return undefined
        }

        World.analyticsSystem?.event("APS Download", {
            type: miraType == MiraType.ROBOT ? "robot" : "field",
            fileSize: miraBuff.byteLength,
        })

        return await MirabufCachingService.storeInCache(data.id, miraBuff, miraType)
    }

    /**
     * Cache local Mirabuf file
     *
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     *
     * @returns {Promise<MirabufCacheInfo | undefined>} Promise with the result of the promise. Metadata on the mirabuf file if successful, undefined if not.
     */
    public static async cacheLocal(buffer: ArrayBuffer, miraType: MiraType): Promise<MirabufCacheInfo | undefined> {
        const key = await this.hashBuffer(buffer)

        const map = MirabufCachingService.getCacheMap(miraType)
        const target = map[key]

        if (target) {
            return target
        }

        return await MirabufCachingService.storeInCache(key, buffer, miraType)
    }

    /**
     * Caches metadata (name or thumbnailStorageID) for a key
     *
     * @param {string} key Key to the given Mirabuf file entry in the caching service. Obtainable via GetCacheMaps().
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * @param {string} name (Optional) Name of Mirabuf Assembly.
     * @param {string} thumbnailStorageID (Optional) ID of the the thumbnail storage for the Mirabuf Assembly.
     */
    public static async cacheInfo(
        key: string,
        miraType: MiraType,
        name?: string,
        thumbnailStorageID?: string
    ): Promise<boolean> {
        try {
            const map: MapCache = this.getCacheMap(miraType)
            const id = map[key].id
            const buffer = miraType == MiraType.ROBOT ? backUpRobots[id].buffer : backUpFields[id].buffer
            const defaultName = map[key].name
            const defaultStorageID = map[key].thumbnailStorageID
            const info: MirabufCacheInfo = {
                id: id,
                cacheKey: key,
                miraType: miraType,
                buffer: buffer,
                name: name ?? defaultName,
                thumbnailStorageID: thumbnailStorageID ?? defaultStorageID,
            }
            map[key] = info
            miraType == MiraType.ROBOT ? (backUpRobots[id] = info) : (backUpFields[id] = info)
            window.localStorage.setItem(miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName, JSON.stringify(map))
            return true
        } catch (e) {
            console.error(`Failed to cache info\n${e}`)
            return false
        }
    }

    /**
     * Caches and gets local Mirabuf file with cache info
     *
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     *
     * @returns {Promise<{assembly: mirabuf.Assembly, cacheInfo: MirabufCacheInfo} | undefined>} Promise with the result of the promise. Assembly and cache info of the mirabuf file if successful, undefined if not.
     */
    public static async cacheAndGetLocalWithInfo(
        buffer: ArrayBuffer,
        miraType: MiraType
    ): Promise<{ assembly: mirabuf.Assembly; cacheInfo: MirabufCacheInfo } | undefined> {
        const key = await this.hashBuffer(buffer)
        const map = MirabufCachingService.getCacheMap(miraType)
        const target = map[key]
        const assembly = this.assemblyFromBuffer(buffer)

        // Check if assembly has devtool data and update name accordingly
        let displayName = assembly.info?.name ?? undefined
        if (assembly.data?.parts?.userData?.data) {
            const devtoolKeys = Object.keys(assembly.data.parts.userData.data).filter(k => k.startsWith("devtool:"))
            if (devtoolKeys.length > 0) {
                displayName = displayName ? `Edited ${displayName}` : "Edited Field"
            }
        }

        if (!target) {
            const cacheInfo = await MirabufCachingService.storeInCache(key, buffer, miraType, displayName)
            if (cacheInfo) {
                return { assembly, cacheInfo }
            }
        } else {
            // Update existing cache info with new name if it has devtool data
            if (displayName && displayName !== target.name) {
                await MirabufCachingService.cacheInfo(key, miraType, displayName)
                target.name = displayName
            }
            return { assembly, cacheInfo: target }
        }

        return undefined
    }

    /**
     * Caches and gets local Mirabuf file
     *
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     *
     * @returns {Promise<mirabufAssembly | undefined>} Promise with the result of the promise. Assembly of the mirabuf file if successful, undefined if not.
     */
    public static async cacheAndGetLocal(
        buffer: ArrayBuffer,
        miraType: MiraType
    ): Promise<mirabuf.Assembly | undefined> {
        const key = await this.hashBuffer(buffer)
        const map = MirabufCachingService.getCacheMap(miraType)
        const target = map[key]
        const assembly = this.assemblyFromBuffer(buffer)

        // Check if assembly has devtool data and update name accordingly
        let displayName = assembly.info?.name ?? undefined
        if (assembly.data?.parts?.userData?.data) {
            const devtoolKeys = Object.keys(assembly.data.parts.userData.data).filter(k => k.startsWith("devtool:"))
            if (devtoolKeys.length > 0) {
                displayName = displayName ? `Edited ${displayName}` : "Edited Field"
            }
        }

        if (!target) {
            await MirabufCachingService.storeInCache(key, buffer, miraType, displayName)
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
    public static async get(id: MirabufCacheID, miraType: MiraType): Promise<mirabuf.Assembly | undefined> {
        // Retry logic to handle race conditions where storage might still be in progress
        const maxRetries = 3
        const retryDelay = 50 // ms

        for (let attempt = 0; attempt < maxRetries; attempt++) {
            try {
                const cache = miraType == MiraType.ROBOT ? backUpRobots : backUpFields
                let buff: ArrayBuffer | undefined

                // Try memory cache first (most reliable and fastest)
                if (cache[id]?.buffer) {
                    buff = cache[id].buffer
                } else if (canOPFS) {
                    // Try OPFS as fallback
                    try {
                        const fileHandle = await (
                            miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
                        ).getFileHandle(id, { create: false })
                        const file = await fileHandle.getFile()
                        buff = await file.arrayBuffer()

                        // If we found it in OPFS but not in memory, update memory cache for next time
                        if (buff && !cache[id]?.buffer) {
                            if (cache[id]) {
                                cache[id].buffer = buff
                            } else {
                                // Create a minimal cache entry if it doesn't exist
                                cache[id] = {
                                    id: id,
                                    miraType: miraType,
                                    cacheKey: `opfs_recovered_${id}`,
                                    buffer: buff,
                                }
                            }
                        }
                    } catch (opfsError) {
                        console.debug(`OPFS retrieval failed for ${id}:`, opfsError)
                    }
                }

                // If we have buffer, return assembly
                if (buff) {
                    const assembly = this.assemblyFromBuffer(buff)
                    World.analyticsSystem?.event("Cache Get", {
                        key: id,
                        type: miraType == MiraType.ROBOT ? "robot" : "field",
                        assemblyName: assembly.info!.name!,
                        fileSize: buff.byteLength,
                    })
                    return assembly
                }

                // If we didn't find the buffer and this isn't the last attempt, wait and retry
                if (attempt < maxRetries - 1) {
                    console.debug(
                        `Cache miss for ${id}, retrying in ${retryDelay}ms (attempt ${attempt + 1}/${maxRetries})`
                    )
                    await new Promise(resolve => setTimeout(resolve, retryDelay))
                    continue
                }

                // Last attempt failed
                console.error(`Failed to find arrayBuffer for id: ${id} after ${maxRetries} attempts`)
                return undefined
            } catch (e) {
                console.error(`Failed to find file for id ${id} (attempt ${attempt + 1}):`, e)

                // If this is the last attempt, give up
                if (attempt === maxRetries - 1) {
                    return undefined
                }

                // Wait before retrying
                await new Promise(resolve => setTimeout(resolve, retryDelay))
            }
        }

        return undefined
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
    public static async remove(key: string, id: MirabufCacheID, miraType: MiraType): Promise<boolean> {
        try {
            const map = this.getCacheMap(miraType)
            if (map) {
                delete map[key]
                window.localStorage.setItem(
                    miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName,
                    JSON.stringify(map)
                )
            }

            if (canOPFS) {
                const dir = miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
                await dir.removeEntry(id)
            }

            const backUpCache = miraType == MiraType.ROBOT ? backUpRobots : backUpFields
            if (backUpCache) {
                delete backUpCache[id]
            }

            World.analyticsSystem?.event("Cache Remove", {
                key: key,
                type: miraType == MiraType.ROBOT ? "robot" : "field",
            })
            return true
        } catch (e) {
            console.error(`Failed to remove\n${e}`)
            World.analyticsSystem?.exception("Failed to remove mirabuf from cache")
            return false
        }
    }

    /**
     * Removes all Mirabuf files from the caching services. Mostly for debugging purposes.
     */
    public static async removeAll() {
        if (canOPFS) {
            for await (const key of robotFolderHandle.keys()) {
                robotFolderHandle.removeEntry(key)
            }
            for await (const key of fieldFolderHandle.keys()) {
                fieldFolderHandle.removeEntry(key)
            }
        }

        window.localStorage.setItem(robotsDirName, "{}")
        window.localStorage.setItem(fieldsDirName, "{}")

        backUpRobots = {}
        backUpFields = {}
    }

    /**
     * Persists devtool changes back to the cache by re-encoding the assembly
     *
     * @param {MirabufCacheID} id ID of the cached mirabuf file
     * @param {MiraType} miraType Type of Mirabuf Assembly
     * @param {mirabuf.Assembly} assembly The updated assembly with devtool changes
     *
     * @returns {Promise<boolean>} Promise with the result. True if successful, false if not.
     */
    public static async persistDevtoolChanges(
        id: MirabufCacheID,
        miraType: MiraType,
        assembly: mirabuf.Assembly
    ): Promise<boolean> {
        try {
            // Re-encode the assembly with devtool changes
            const updatedBuffer = mirabuf.Assembly.encode(assembly).finish()

            // Update the cached buffer
            const cache = miraType == MiraType.ROBOT ? backUpRobots : backUpFields
            if (cache[id]) {
                cache[id].buffer = updatedBuffer
            }

            // Update OPFS if available
            if (canOPFS) {
                const fileHandle = await (
                    miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
                ).getFileHandle(id, { create: false })
                const writable = await fileHandle.createWritable()
                await writable.write(updatedBuffer)
                await writable.close()
            }

            World.analyticsSystem?.event("Devtool Cache Persist", {
                key: id,
                type: miraType == MiraType.ROBOT ? "robot" : "field",
                assemblyName: assembly.info?.name ?? "unknown",
                fileSize: updatedBuffer.byteLength,
            })

            return true
        } catch (e) {
            console.error("Failed to persist devtool changes", e)
            World.analyticsSystem?.exception("Failed to persist devtool changes to cache")
            return false
        }
    }

    // Optional name for when assembly is being decoded anyway like in CacheAndGetLocal()
    private static async storeInCache(
        key: string,
        miraBuff: ArrayBuffer,
        miraType?: MiraType,
        name?: string
    ): Promise<MirabufCacheInfo | undefined> {
        // Check if there's already an ongoing storage operation for this key
        const storageKey = `${key}:${miraType ?? "unknown"}`
        const ongoingStorage = this._storageOperations.get(storageKey)
        if (ongoingStorage) {
            return ongoingStorage
        }

        const storagePromise = this._performStoreInCache(key, miraBuff, miraType, name)
        this._storageOperations.set(storageKey, storagePromise)

        storagePromise.finally(() => {
            this._storageOperations.delete(storageKey)
        })

        return storagePromise
    }

    private static async _performStoreInCache(
        key: string,
        miraBuff: ArrayBuffer,
        miraType?: MiraType,
        name?: string
    ): Promise<MirabufCacheInfo | undefined> {
        try {
            // Generate unique ID using timestamp + counter to avoid collisions
            const backupID = `${Date.now()}_${++this._idCounter}`

            if (!miraType) {
                console.debug("Double loading")
                miraType = this.assemblyFromBuffer(miraBuff).dynamic ? MiraType.ROBOT : MiraType.FIELD
            }

            // Create the cache info that will be used consistently across all storage locations
            const cacheInfo: MirabufCacheInfo = {
                id: backupID,
                miraType: miraType,
                cacheKey: key,
                name: name,
            }

            const memoryInfo: MirabufCacheInfo = {
                id: backupID,
                miraType: miraType,
                cacheKey: key,
                buffer: miraBuff,
                name: name,
            }

            // Store in memory cache FIRST (most reliable)
            const cache = miraType == MiraType.ROBOT ? backUpRobots : backUpFields
            cache[backupID] = memoryInfo

            World.analyticsSystem?.event("Cache Store", {
                name: name ?? "-",
                key: key,
                type: miraType == MiraType.ROBOT ? "robot" : "field",
                fileSize: miraBuff.byteLength,
            })

            // Store in OPFS (can fail silently)
            if (canOPFS) {
                try {
                    const fileHandle = await (
                        miraType == MiraType.ROBOT ? robotFolderHandle : fieldFolderHandle
                    ).getFileHandle(backupID, { create: true })
                    const writable = await fileHandle.createWritable()
                    await writable.write(miraBuff)
                    await writable.close()
                } catch (opfsError) {
                    console.warn(`OPFS storage failed for ${key}, using fallback:`, opfsError)
                }
            }

            // Update localStorage cache map LAST (after memory cache is secured)
            try {
                const map: MapCache = this.getCacheMap(miraType)
                map[key] = cacheInfo
                window.localStorage.setItem(
                    miraType == MiraType.ROBOT ? robotsDirName : fieldsDirName,
                    JSON.stringify(map)
                )
            } catch (lsError) {
                console.warn(`localStorage update failed for ${key}:`, lsError)
            }

            return cacheInfo
        } catch (e) {
            console.error("Failed to cache mira " + e)
            World.analyticsSystem?.exception("Failed to store in cache")
            return undefined
        }
    }

    private static async hashBuffer(buffer: ArrayBuffer): Promise<string> {
        const hashBuffer = await crypto.subtle.digest("SHA-256", buffer)
        let hash = ""
        new Uint8Array(hashBuffer).forEach(x => (hash = hash + String.fromCharCode(x)))
        return btoa(hash).replace(/\+/g, "-").replace(/\//g, "_").replace(/=/g, "")
    }

    private static assemblyFromBuffer(buffer: ArrayBuffer): mirabuf.Assembly {
        return mirabuf.Assembly.decode(unzipMira(new Uint8Array(buffer)))
    }
}

export enum MiraType {
    ROBOT = 1,
    FIELD,
}

export default MirabufCachingService
