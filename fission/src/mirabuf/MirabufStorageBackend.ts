export interface MirabufStorageBackend {
    hasFile(hash: string): Promise<boolean>
    readFile(hash: string): Promise<ArrayBuffer | undefined>
    writeFile(hash: string, buffer: ArrayBuffer): Promise<void>
    removeFile(hash: string): Promise<void>
    listFiles(): Promise<string[]>
    removeAll(): Promise<void>
}

const OPFS_DIR_NAME = "MirabufAssets"
const OPFS_PROBE_NAME = "__opfs_probe__"

class OPFSBackend implements MirabufStorageBackend {
    constructor(
        private root: FileSystemDirectoryHandle,
        private dir: FileSystemDirectoryHandle
    ) {}

    async hasFile(hash: string): Promise<boolean> {
        return this.dir
            .getFileHandle(hash)
            .then(() => true)
            .catch(() => false)
    }

    async readFile(hash: string): Promise<ArrayBuffer | undefined> {
        try {
            const fileHandle = await this.dir.getFileHandle(hash, { create: false })
            const file = await fileHandle.getFile()
            return await file.arrayBuffer()
        } catch (e) {
            const err = e as DOMException
            if (err.name !== "NotFoundError") {
                console.error("Error reading from OPFS", { error: e, hash })
            }
            return undefined
        }
    }

    async writeFile(hash: string, buffer: ArrayBuffer): Promise<void> {
        const fileHandle = await this.dir.getFileHandle(hash, { create: true })
        const writable = await fileHandle.createWritable()
        await writable.write(buffer)
        await writable.close()
    }

    async removeFile(hash: string): Promise<void> {
        await this.dir.removeEntry(hash).catch(() => {})
    }

    async listFiles(): Promise<string[]> {
        const keys: string[] = []
        for await (const name of this.dir.keys()) {
            keys.push(name)
        }
        return keys
    }

    async removeAll(): Promise<void> {
        // Remove legacy separated directories
        this.root.removeEntry("Robots", { recursive: true }).catch(() => {})
        this.root.removeEntry("Fields", { recursive: true }).catch(() => {})
        this.root.removeEntry("Pieces", { recursive: true }).catch(() => {})

        for await (const key of this.dir.keys()) {
            await this.dir.removeEntry(key).catch(e => console.warn("could not remove file", key, e))
        }
    }
}

const IDB_DB_NAME = "MirabufCache"
const IDB_STORE_NAME = "assemblies"
const IDB_VERSION = 1

class IndexedDBBackend implements MirabufStorageBackend {
    constructor(private db: IDBDatabase) {}

    async hasFile(hash: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readonly")
            const store = tx.objectStore(IDB_STORE_NAME)
            // Use getKey instead of get to avoid loading the full blob into memory
            const req = store.getKey(hash)
            req.onsuccess = () => resolve(req.result !== undefined)
            req.onerror = () => reject(req.error)
        })
    }

    async readFile(hash: string): Promise<ArrayBuffer | undefined> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readonly")
            const store = tx.objectStore(IDB_STORE_NAME)
            const req = store.get(hash)
            req.onsuccess = () => {
                const result = req.result as { hash: string; data: ArrayBuffer } | undefined
                resolve(result?.data)
            }
            req.onerror = () => reject(req.error)
        })
    }

    async writeFile(hash: string, buffer: ArrayBuffer): Promise<void> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readwrite")
            const store = tx.objectStore(IDB_STORE_NAME)
            const req = store.put({ hash, data: buffer })
            req.onsuccess = () => resolve()
            req.onerror = () => reject(req.error)
        })
    }

    async removeFile(hash: string): Promise<void> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readwrite")
            const store = tx.objectStore(IDB_STORE_NAME)
            const req = store.delete(hash)
            req.onsuccess = () => resolve()
            req.onerror = () => reject(req.error)
        })
    }

    async listFiles(): Promise<string[]> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readonly")
            const store = tx.objectStore(IDB_STORE_NAME)
            const req = store.getAllKeys()
            req.onsuccess = () => resolve(req.result as string[])
            req.onerror = () => reject(req.error)
        })
    }

    async removeAll(): Promise<void> {
        return new Promise((resolve, reject) => {
            const tx = this.db.transaction(IDB_STORE_NAME, "readwrite")
            const store = tx.objectStore(IDB_STORE_NAME)
            const req = store.clear()
            req.onsuccess = () => resolve()
            req.onerror = () => reject(req.error)
        })
    }
}

/** Try to open an IndexedDB database. Returns the DB handle or null. */
function openIndexedDB(): Promise<IDBDatabase | null> {
    return new Promise(resolve => {
        try {
            const req = indexedDB.open(IDB_DB_NAME, IDB_VERSION)
            req.onupgradeneeded = () => {
                const db = req.result
                if (!db.objectStoreNames.contains(IDB_STORE_NAME)) {
                    db.createObjectStore(IDB_STORE_NAME, { keyPath: "hash" })
                }
            }
            req.onsuccess = () => resolve(req.result)
            req.onerror = () => {
                console.warn("IndexedDB open failed", req.error)
                resolve(null)
            }
        } catch {
            console.warn("IndexedDB not available")
            resolve(null)
        }
    })
}

/** Try to create an OPFS backend. Returns null if createWritable() is not supported. */
async function tryOPFS(): Promise<OPFSBackend | null> {
    try {
        const root = await navigator.storage.getDirectory()
        const dir = await root.getDirectoryHandle(OPFS_DIR_NAME, { create: true })
        if (dir.name !== OPFS_DIR_NAME) return null

        // checking if createWriteable() is supported; Safari (< 26) is missing this
        const testHandle = await dir.getFileHandle(OPFS_PROBE_NAME, { create: true })
        try {
            const writable = await testHandle.createWritable()
            await writable.close()
        } finally {
            await dir.removeEntry(OPFS_PROBE_NAME).catch(() => {})
        }

        return new OPFSBackend(root, dir)
    } catch {
        return null
    }
}

/**
 * Initialize the best available storage backend.
 * Tries OPFS first (Chrome/Edge/Firefox), then IndexedDB (Safari/fallback), then null.
 */
export async function initStorageBackend(): Promise<MirabufStorageBackend | null> {
    const opfs = await tryOPFS()
    if (opfs) {
        console.log("Mirabuf cache: using OPFS backend")
        return opfs
    }

    // Fall back to IndexedDB
    const db = await openIndexedDB()
    if (db) {
        console.log("Mirabuf cache: using IndexedDB backend")
        return new IndexedDBBackend(db)
    }

    // 3. No persistent storage available
    console.warn("Mirabuf cache: no persistent storage backend available")
    return null
}
