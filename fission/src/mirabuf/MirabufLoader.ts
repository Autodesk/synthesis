import Pako from "pako"
import { type Data, downloadData } from "@/aps/APSDataManagement"
import { globalAddToast } from "@/components/GlobalUIControls"
import { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import { hashBuffer } from "@/util/Utility.ts"

const MIRABUF_LOCALSTORAGE_GENERATION_KEY = "Synthesis Nonce Key"
const MIRABUF_LOCALSTORAGE_GENERATION = "978534"

export interface MirabufCacheInfo {
    hash: string
    name: string
    miraType: MiraType
    remotePath?: string
    thumbnailStorageID?: string
}

export interface MirabufRemoteInfo {
    displayName: string
    src: string
}

const localStorageEntryName = "MirabufAssets"
let root: FileSystemDirectoryHandle
let fsHandle: FileSystemDirectoryHandle

export const canOPFS = await (async () => {
    try {
        root = await navigator.storage.getDirectory()
        fsHandle = await root.getDirectoryHandle(localStorageEntryName, {
            create: true,
        })
        if (fsHandle.name == localStorageEntryName) {
            const fileHandle = await fsHandle.getFileHandle("0", {
                create: true,
            })
            const writable = await fileHandle.createWritable()
            await writable.close()
            await fileHandle.getFile()

            await fsHandle.removeEntry(fileHandle.name)

            return true
        } else {
            console.log(`No access to OPFS`)
            return false
        }
    } catch (_e) {
        console.log(`No access to OPFS`)
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

class CacheMap {
    private _map: Map<string, MirabufCacheInfo> = new Map()

    private static isMirabufCacheInfo(data: unknown): data is MirabufCacheInfo {
        return typeof data === "object" && data != null && "hash" in data && "name" in data && "miraType" in data
    }

    public async load() {
        const lookup = window.localStorage.getItem(localStorageEntryName)

        if (lookup == null) {
            this.save()
            return
        }
        const parsed = JSON.parse(lookup) as unknown
        if (!Array.isArray(parsed)) {
            console.warn("Invalid localstorage cache")
            this.save()
            return
        }
        if (!canOPFS) {
            console.warn("no OPFS, can't load from cache")
            return
        }
        await Promise.all(
            parsed.map(async (data: unknown) => {
                if (!CacheMap.isMirabufCacheInfo(data)) {
                    console.warn("malformed mirabuf cache info", data)
                    return
                }
                const hasFile = await fsHandle
                    .getFileHandle(data.hash)
                    .then(() => true)
                    .catch(() => false)
                if (!hasFile) {
                    console.warn(`Could not find ${data.hash} (${data.name}) in OPFS`)
                    return
                }
                this._map.set(data.hash, data)
            })
        )
    }

    public save() {
        window.localStorage.setItem(localStorageEntryName, JSON.stringify([...this._map.values()]))
    }

    public get(hash: string): Readonly<MirabufCacheInfo> | undefined {
        return this._map.get(hash)
    }

    public add(entry: MirabufCacheInfo): void {
        if (this._map.has(entry.hash)) {
            console.warn("attempting to add existing element to CacheMap")
            return
        }
        this._map.set(entry.hash, entry)
        this.save()
    }

    public store(entry: MirabufCacheInfo): void {
        this._map.set(entry.hash, entry)
        this.save()
    }

    public remove(hash: string): void {
        this._map.delete(hash)
        this.save()
    }

    public getAll(miraType?: MiraType): MirabufCacheInfo[] {
        const all = [...this._map.values()]
        if (miraType != null) {
            return all.filter(x => x.miraType == miraType)
        }
        return all
    }

    public update(hash: string, updater: (item: MirabufCacheInfo) => void) {
        const val = this._map.get(hash)
        if (val) {
            updater(val)
            this.save()
        } else {
            console.warn("Could not find item to update")
        }
    }

    public clear() {
        this._map.clear()
        this.save()
    }
}

class MirabufCachingService {
    private static _cacheMap = new CacheMap()
    private static _inMemoryCache: Record<string, ArrayBuffer | undefined> = {}
    static {
        if (
            (window.localStorage.getItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY) ?? "") != MIRABUF_LOCALSTORAGE_GENERATION
        ) {
            window.localStorage.setItem(MIRABUF_LOCALSTORAGE_GENERATION_KEY, MIRABUF_LOCALSTORAGE_GENERATION)
            this.removeAll().catch(console.error)
            this._cacheMap.clear()
        } else {
            this._cacheMap.load().then(() => {
                if (canOPFS) {
                    setTimeout(() => this.clearExtraAssets()) // make sure nothing extra got left behind due to preferences clearing / whatever
                }
            })
        }
    }

    private static async clearExtraAssets() {
        const files = fsHandle.keys()
        for await (const filename of files) {
            if (!this._cacheMap.get(filename)) {
                await fsHandle.removeEntry(filename)
            }
        }
    }

    /**
     * Cache remote Mirabuf file
     *
     * @param {string} fetchLocation Location of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     * @param {string} name Optional display name for the cached file.
     *
     * @returns {Promise<MirabufCacheInfo | undefined>} Promise with the result of the promise. Metadata on the mirabuf file if successful, undefined if not.
     */
    public static async cacheRemote(
        fetchLocation: string,
        miraType: MiraType,
        name?: string,
        expectedHash?: string
    ): Promise<MirabufCacheInfo | undefined> {
        try {
            // grab file remote
            const resp = await fetch(encodeURI(fetchLocation), import.meta.env.DEV ? { cache: "no-store" } : undefined)
            if (!resp.ok) throw new Error(`${resp.status} ${resp.statusText}`)

            const miraBuff = await resp.arrayBuffer()
            name ??= this.assemblyFromBuffer(miraBuff).info?.name ?? fetchLocation

            World.analyticsSystem?.event("Remote Download", {
                assemblyName: name,
                type: miraType === MiraType.ROBOT ? "robot" : "field",
                fileSize: miraBuff.byteLength,
            })

            const cached = await MirabufCachingService.storeInCache(miraBuff, {
                miraType,
                name,
                remotePath: fetchLocation,
            })

            if (expectedHash != null && cached?.hash != null && cached?.hash != expectedHash) {
                globalAddToast("warning", "Hash Mismatch", `Try downloading again`)
                console.warn("mismatched hashes", expectedHash, cached?.hash)
                // await this.remove(cached.hash)
            }

            if (cached) return cached

            globalAddToast("error", "Cache Fallback", `Unable to cache “${fetchLocation}”. Using raw buffer instead.`)

            // fallback: return raw buffer wrapped in MirabufCacheInfo
            return {
                hash: await hashBuffer(miraBuff),
                miraType: miraType,
                name: name,
            }
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

        const miraBuff = await downloadData(data)
        if (!miraBuff) {
            console.error("Failed to download file")
            return undefined
        }

        World.analyticsSystem?.event("APS Download", {
            type: miraType == MiraType.ROBOT ? "robot" : "field",
            fileSize: miraBuff.byteLength,
        })

        return await MirabufCachingService.storeInCache(miraBuff, {
            miraType,
            name: this.assemblyFromBuffer(miraBuff).info?.name ?? "Unknown APS",
        })
    }

    /**
     * Caches and gets local Mirabuf file with cache info
     *
     * @param {ArrayBuffer} buffer ArrayBuffer of Mirabuf file.
     * @param {MiraType} miraType Type of Mirabuf Assembly.
     *
     * @returns {Promise<{assembly: mirabuf.Assembly, cacheInfo: MirabufCacheInfo} | undefined>} Promise with the result of the promise. Assembly and cache info of the mirabuf file if successful, undefined if not.
     */
    public static async cacheLocalAndReturn(
        buffer: ArrayBuffer,
        miraType: MiraType
    ): Promise<{ assembly: mirabuf.Assembly; cacheInfo: MirabufCacheInfo } | undefined> {
        const assembly = this.assemblyFromBuffer(buffer)
        const hash = await hashBuffer(buffer)

        World.analyticsSystem?.event("Local Upload", {
            fileSize: buffer.byteLength,
            key: hash,
            type: miraType == MiraType.ROBOT ? "robot" : "field",
        })
        if (assembly.dynamic && miraType == MiraType.FIELD) {
            globalAddToast("warning", "Cannot import robot assembly as a field")
            return
        }

        if (!assembly.dynamic && miraType != MiraType.FIELD) {
            globalAddToast("warning", "Cannot import field assembly as a robot")
            return
        }

        const info = await MirabufCachingService.storeAssemblyInCache(assembly, { miraType })
        if (!info) return

        return { assembly, cacheInfo: info }
    }

    /**
     * Gets a given Mirabuf file from the cache
     */
    public static async get(hash: string): Promise<mirabuf.Assembly | undefined> {
        try {
            const encodedData = await this.getEncoded(hash)
            if (!encodedData) {
                return
            }
            const { buffer, info } = encodedData
            // If we have buffer, get assembly
            const assembly = this.assemblyFromBuffer(buffer)
            if (info != null) {
                if (!info.name) {
                    this._cacheMap.update(hash, v => {
                        v.name = assembly?.info?.name ?? ""
                    })
                }

                World.analyticsSystem?.event("Cache Get", {
                    key: info.hash,
                    type: info.miraType == MiraType.ROBOT ? "robot" : "field",
                    assemblyName: info.name,
                    fileSize: buffer.byteLength,
                })
            }
            return assembly
        } catch (e) {
            console.error(`Failed to find file\n${e}`)
            return undefined
        }
    }

    public static async getEncoded(
        hash: string
    ): Promise<{ buffer: ArrayBuffer; info?: MirabufCacheInfo } | undefined> {
        try {
            const info = this._cacheMap.get(hash)

            const memCache = this._inMemoryCache[hash]
            if (memCache) {
                console.log(`Retrieved ${info?.name ?? hash} from memory`)
                return { buffer: memCache, info }
            }
            if (info == null) {
                console.warn(`${hash} not found in cache`)
                return
            }
            if (canOPFS) {
                const fileHandle = await fsHandle.getFileHandle(hash, {
                    create: false,
                })
                const buffer = await fileHandle
                    .getFile()
                    .then(x => x.arrayBuffer())
                    .catch((e: DOMException) => {
                        if (e.name != "FileNotFound") console.error("Error accessing OPFS", { error: e, hash })
                        return undefined
                    })
                if (!buffer) {
                    console.warn(`Could not find ${hash} in OPFS`)
                    return undefined
                }
                this._inMemoryCache[hash] = buffer
                console.log(`Retrieved ${info?.name ?? hash} from OPFS`)
                return { buffer: buffer, info }
            }
            console.warn("Could not find assembly for hash", hash, info)
            return
        } catch (e) {
            console.error("could not get encoded assembly", e)
            return undefined
        }
    }

    public static getAll(miraType?: MiraType) {
        return this._cacheMap.getAll(miraType)
    }

    /**
     * Removes a given Mirabuf item from the cache
     */
    public static async remove(hash: string): Promise<boolean> {
        try {
            const info = this._cacheMap.get(hash)

            this._cacheMap.remove(hash)
            delete this._inMemoryCache[hash]
            if (canOPFS) {
                await fsHandle.removeEntry(hash)
            }
            if (!info) {
                console.warn("couldn't find cached item to remove", hash)
                return true
            }
            World.analyticsSystem?.event("Cache Remove", {
                key: info.hash,
                type: info.miraType == MiraType.ROBOT ? "robot" : "field",
                assemblyName: info.name,
            })
            console.log(`Removed ${hash} from cache`)
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
        // remove old separated localstorage keys
        localStorage.removeItem("Robots")
        localStorage.removeItem("Fields")
        localStorage.removeItem("Pieces")
        if (canOPFS) {
            // Remove old separated directories
            root.removeEntry("Robots", { recursive: true }).catch(() => {})
            root.removeEntry("Fields", { recursive: true }).catch(() => {})
            root.removeEntry("Pieces", { recursive: true }).catch(() => {})

            for await (const key of fsHandle.keys()) {
                await fsHandle.removeEntry(key).catch(e => console.warn("could not remove file", key, e))
            }
        }
        Object.keys(this._inMemoryCache).forEach(key => delete this._inMemoryCache[key])
        this._cacheMap.clear()
    }

    public static async storeAssemblyInCache(
        assembly: mirabuf.Assembly,
        extra: Omit<MirabufCacheInfo, "hash" | "name"> & { name?: string }
    ) {
        const buffer = mirabuf.Assembly.encode(assembly).finish()

        return await this.storeInCache(buffer.buffer as ArrayBuffer, {
            ...extra,
            name: extra.name ?? assembly.info?.name ?? "Unknown",
        })
    }

    // Optional name for when assembly is being decoded anyway like in CacheAndGetLocal()
    private static async storeInCache(
        buffer: ArrayBuffer,
        extra: Omit<MirabufCacheInfo, "hash">
    ): Promise<MirabufCacheInfo | undefined> {
        try {
            const hash = await hashBuffer(buffer)

            this._inMemoryCache[hash] = buffer
            const existing = this._cacheMap.get(hash)
            extra = { ...extra, ...existing }

            // Local cache map
            const info: MirabufCacheInfo = {
                ...extra,
                hash: hash,
            }

            // Store buffer
            if (!canOPFS) return info

            // Store in OPFS
            const fileHandle = await fsHandle.getFileHandle(info.hash, { create: true })
            const writable = await fileHandle.createWritable()
            await writable.write(buffer)
            await writable.close()

            this._cacheMap.store(info)

            World.analyticsSystem?.event("Cache Store", {
                assemblyName: info.name,
                key: info.hash,
                type: info.miraType == MiraType.ROBOT ? "robot" : "field",
                fileSize: buffer.byteLength,
            })
            console.log(`Added cache entry for ${hash}`)
            return info
        } catch (e) {
            console.error("Failed to cache mira " + e)
            World.analyticsSystem?.exception("Failed to store in cache")
            return undefined
        }
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
