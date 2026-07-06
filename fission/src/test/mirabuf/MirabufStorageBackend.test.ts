import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"
import { initStorageBackend, type MirabufStorageBackend } from "../../mirabuf/MirabufStorageBackend"

// ─── Helpers ─────────────────────────────────────────────────────────────────

/** Build a deterministic ArrayBuffer containing a given byte pattern. */
function makeBuffer(...bytes: number[]): ArrayBuffer {
    return new Uint8Array(bytes).buffer
}

/** Assert that two ArrayBuffers have identical byte contents. */
function buffersEqual(a: ArrayBuffer, b: ArrayBuffer): boolean {
    const va = new Uint8Array(a)
    const vb = new Uint8Array(b)
    if (va.length !== vb.length) return false
    return va.every((byte, i) => byte === vb[i])
}

// ─── Shared contract tests ────────────────────────────────────────────────────

/**
 * The full backend contract.  Both OPFSBackend and IndexedDBBackend must
 * pass every test here, proving behavioural parity between them.
 */
function describeBackendContract(label: string, getBackend: () => MirabufStorageBackend) {
    describe(label, () => {
        let backend: MirabufStorageBackend

        beforeEach(async () => {
            backend = getBackend()
            await backend.removeAll()
        })

        afterEach(async () => {
            await backend.removeAll()
        })

        // ── hasFile ──────────────────────────────────────────────────────────

        describe("hasFile", () => {
            test("returns false for a hash that was never written", async () => {
                expect(await backend.hasFile("nonexistent-hash")).toBe(false)
            })

            test("returns true after writing a file", async () => {
                const hash = "hash-hasfile-written"
                await backend.writeFile(hash, makeBuffer(1, 2, 3))
                expect(await backend.hasFile(hash)).toBe(true)
            })

            test("returns false after the file is removed", async () => {
                const hash = "hash-hasfile-removed"
                await backend.writeFile(hash, makeBuffer(4, 5, 6))
                await backend.removeFile(hash)
                expect(await backend.hasFile(hash)).toBe(false)
            })
        })

        // ── readFile ─────────────────────────────────────────────────────────

        describe("readFile", () => {
            test("returns undefined for a hash that was never written", async () => {
                expect(await backend.readFile("nonexistent-hash")).toBeUndefined()
            })

            test("round-trips small binary data exactly", async () => {
                const hash = "hash-small"
                const original = makeBuffer(0xde, 0xad, 0xbe, 0xef)
                await backend.writeFile(hash, original)
                const retrieved = await backend.readFile(hash)
                expect(retrieved).toBeDefined()
                expect(buffersEqual(original, retrieved!)).toBe(true)
            })

            test("round-trips a larger buffer (64 KiB) without corruption", async () => {
                const hash = "hash-large"
                const size = 64 * 1024
                const src = new Uint8Array(size)
                for (let i = 0; i < size; i++) src[i] = i & 0xff
                await backend.writeFile(hash, src.buffer)
                const retrieved = await backend.readFile(hash)
                expect(retrieved).toBeDefined()
                expect(buffersEqual(src.buffer, retrieved!)).toBe(true)
            })

            test("overwrites previous data when the same hash is written again", async () => {
                const hash = "hash-overwrite"
                const first = makeBuffer(1, 1, 1)
                const second = makeBuffer(2, 2, 2)
                await backend.writeFile(hash, first)
                await backend.writeFile(hash, second)
                const retrieved = await backend.readFile(hash)
                expect(retrieved).toBeDefined()
                expect(buffersEqual(second, retrieved!)).toBe(true)
            })

            test("returns undefined after the file is removed", async () => {
                const hash = "hash-read-after-remove"
                await backend.writeFile(hash, makeBuffer(7, 8, 9))
                await backend.removeFile(hash)
                expect(await backend.readFile(hash)).toBeUndefined()
            })
        })

        // ── removeFile ───────────────────────────────────────────────────────

        describe("removeFile", () => {
            test("is a no-op for a hash that does not exist (does not throw)", async () => {
                await expect(backend.removeFile("ghost-hash")).resolves.toBeUndefined()
            })

            test("removes only the targeted file, leaving others intact", async () => {
                const hashA = "hash-keep-a"
                const hashB = "hash-remove-b"
                await backend.writeFile(hashA, makeBuffer(10))
                await backend.writeFile(hashB, makeBuffer(20))

                await backend.removeFile(hashB)

                expect(await backend.hasFile(hashA)).toBe(true)
                expect(await backend.hasFile(hashB)).toBe(false)
            })
        })

        // ── listFiles ────────────────────────────────────────────────────────

        describe("listFiles", () => {
            test("returns an empty array when the store is empty", async () => {
                expect(await backend.listFiles()).toEqual([])
            })

            test("returns the hashes of all stored files", async () => {
                const hashes = ["list-alpha", "list-beta", "list-gamma"]
                for (const h of hashes) {
                    await backend.writeFile(h, makeBuffer(0))
                }
                const listed = await backend.listFiles()
                expect(listed.sort()).toEqual(hashes.sort())
            })

            test("reflects removals immediately", async () => {
                const hashes = ["list-del-a", "list-del-b"]
                for (const h of hashes) await backend.writeFile(h, makeBuffer(0))
                await backend.removeFile("list-del-a")
                expect(await backend.listFiles()).toEqual(["list-del-b"])
            })
        })

        // ── removeAll ────────────────────────────────────────────────────────

        describe("removeAll", () => {
            test("clears all stored files", async () => {
                await backend.writeFile("ra-1", makeBuffer(1))
                await backend.writeFile("ra-2", makeBuffer(2))
                await backend.writeFile("ra-3", makeBuffer(3))

                await backend.removeAll()

                expect(await backend.listFiles()).toEqual([])
                expect(await backend.hasFile("ra-1")).toBe(false)
                expect(await backend.hasFile("ra-2")).toBe(false)
                expect(await backend.hasFile("ra-3")).toBe(false)
            })

            test("is safe to call on an already-empty store", async () => {
                await expect(backend.removeAll()).resolves.toBeUndefined()
            })

            test("allows new writes after clearing", async () => {
                await backend.writeFile("before-clear", makeBuffer(5))
                await backend.removeAll()
                await backend.writeFile("after-clear", makeBuffer(6))
                expect(await backend.hasFile("after-clear")).toBe(true)
            })
        })

        // ── multi-entry isolation ─────────────────────────────────────────────

        describe("multi-entry isolation", () => {
            test("stores distinct buffers under distinct hashes independently", async () => {
                const pairs = [
                    { hash: "iso-1", data: makeBuffer(0x01) },
                    { hash: "iso-2", data: makeBuffer(0x02) },
                    { hash: "iso-3", data: makeBuffer(0x03) },
                ]
                for (const { hash, data } of pairs) await backend.writeFile(hash, data)

                for (const { hash, data } of pairs) {
                    const retrieved = await backend.readFile(hash)
                    expect(buffersEqual(data, retrieved!), `data mismatch for ${hash}`).toBe(true)
                }
            })
        })
    })
}

// ─── Run contract tests against each backend ──────────────────────────────────

describe("MirabufStorageBackend", () => {
    // Silence expected console noise from backend initialisation.
    beforeEach(() => {
        vi.spyOn(console, "log").mockImplementation(() => {})
        vi.spyOn(console, "warn").mockImplementation(() => {})
        vi.spyOn(console, "error").mockImplementation(() => {})
    })
    afterEach(() => {
        vi.restoreAllMocks()
    })

    // ── OPFS backend (Chromium / Firefox only) ────────────────────────────────

    describe("OPFSBackend", () => {
        // initStorageBackend() picks OPFS when createWritable() is available.
        let opfsBackend: MirabufStorageBackend | null = null

        beforeEach(async () => {
            opfsBackend = await initStorageBackend()
        })

        test("is selected as the backend when createWritable() is available", async () => {
            // If OPFS is available (Chromium/Firefox in CI), the backend must be non-null.
            // The test environment (Playwright browser) supports OPFS, so we always expect it.
            expect(opfsBackend).not.toBeNull()
        })

        describeBackendContract("OPFSBackend contract", () => opfsBackend!)
    })

    // ── IndexedDB backend (Safari-equivalent path) ────────────────────────────

    describe("IndexedDBBackend", () => {
        // Safari (< 26) supports OPFS but lacks createWritable(), so initStorageBackend()
        // falls back to IndexedDB. We simulate that here by making OPFS unavailable and
        // then run the shared contract against the *real* IndexedDBBackend the factory
        // returns, so the class under test is genuinely exercised (no reimplementation).
        let idbBackend: MirabufStorageBackend

        beforeEach(async () => {
            vi.spyOn(navigator.storage, "getDirectory").mockRejectedValue(
                new DOMException("Not supported", "NotSupportedError")
            )
            const backend = await initStorageBackend()
            expect(backend).not.toBeNull()
            idbBackend = backend!
        })

        describeBackendContract("IndexedDBBackend contract", () => idbBackend)
    })

    // ── initStorageBackend() factory ──────────────────────────────────────────

    describe("initStorageBackend", () => {
        test("returns a non-null backend in a standard browser environment", async () => {
            const backend = await initStorageBackend()
            expect(backend).not.toBeNull()
        })

        test("returned backend is functional end-to-end", async () => {
            const backend = await initStorageBackend()
            expect(backend).not.toBeNull()

            const hash = "factory-e2e"
            const data = makeBuffer(0xca, 0xfe)

            await backend!.writeFile(hash, data)
            expect(await backend!.hasFile(hash)).toBe(true)

            const retrieved = await backend!.readFile(hash)
            expect(buffersEqual(data, retrieved!)).toBe(true)

            await backend!.removeFile(hash)
            expect(await backend!.hasFile(hash)).toBe(false)
        })

        test("falls back to IndexedDB when OPFS createWritable() is unavailable", async () => {
            // Simulate Safari by patching navigator.storage.getDirectory to throw.
            const originalGetDirectory = navigator.storage.getDirectory.bind(navigator.storage)
            vi.spyOn(navigator.storage, "getDirectory").mockRejectedValue(
                new DOMException("Not supported", "NotSupportedError")
            )

            const backend = await initStorageBackend()

            // Even without OPFS, the factory must return a working backend (IndexedDB).
            expect(backend).not.toBeNull()

            const hash = "idb-fallback-hash"
            const data = makeBuffer(0x11, 0x22, 0x33)
            await backend!.writeFile(hash, data)
            expect(await backend!.hasFile(hash)).toBe(true)
            const retrieved = await backend!.readFile(hash)
            expect(buffersEqual(data, retrieved!)).toBe(true)

            await backend!.removeFile(hash)

            // Restore
            vi.spyOn(navigator.storage, "getDirectory").mockImplementation(originalGetDirectory)
        })

        test("returns null when both OPFS and IndexedDB are unavailable", async () => {
            vi.spyOn(navigator.storage, "getDirectory").mockRejectedValue(new DOMException("Not supported"))
            vi.spyOn(globalThis, "indexedDB", "get").mockReturnValue(undefined as unknown as IDBFactory)

            const backend = await initStorageBackend()
            expect(backend).toBeNull()
        })
    })
})
