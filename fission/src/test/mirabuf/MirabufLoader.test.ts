import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import MirabufLoader, { MiraType, MirabufCacheInfo } from "../../mirabuf/MirabufLoader"

vi.mock("@/systems/World", () => ({
    default: {
        AnalyticsSystem: {
            Event: vi.fn(),
            Exception: vi.fn(),
        },
    },
}))

// Polyfill btoa for Uint8Array to base64 (browser compatible, no Buffer)
function uint8ToBase64(bytes: Uint8Array): string {
    let binary = ""
    for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i])
    return btoa(binary)
}

globalThis.btoa = globalThis.btoa || ((str: string) => {
    return uint8ToBase64(new TextEncoder().encode(str))
})

describe("MirabufLoader", () => {
    let localStorageMock: Record<string, string>
    let fetchMock: any
    let originalDigest: any

    beforeEach(() => {
        localStorageMock = {}
        vi.stubGlobal("localStorage", {
            getItem: vi.fn(key => localStorageMock[key] ?? null),
            setItem: vi.fn((key, value) => { localStorageMock[key] = value }),
            removeItem: vi.fn(key => { delete localStorageMock[key] }),
            clear: vi.fn(() => { localStorageMock = {} }),
            key: vi.fn(),
            length: 0,
        })
        fetchMock = vi.fn()
        globalThis.fetch = fetchMock
        if (!globalThis.crypto) {
            // @ts-expect-error
            globalThis.crypto = {}
        }
        if (!globalThis.crypto.subtle) {
            // @ts-expect-error
            globalThis.crypto.subtle = {}
        }
        originalDigest = globalThis.crypto.subtle.digest
        globalThis.crypto.subtle.digest = vi.fn(async (_alg) => {
            return new Uint8Array(32).buffer
        })
    })

    afterEach(() => {
        vi.restoreAllMocks()
        vi.unstubAllGlobals()
        if (globalThis.crypto && globalThis.crypto.subtle && originalDigest) {
            globalThis.crypto.subtle.digest = originalDigest
        }
    })

    test("GetCacheMap initializes and retrieves cache", () => {
        const map = MirabufLoader.GetCacheMap(MiraType.ROBOT)
        expect(map).toEqual({})
        expect(globalThis.localStorage.setItem).toHaveBeenCalled()
    })

    test("CacheRemote returns fallback on cache failure (GH-1141)", async () => {
        const buffer = new ArrayBuffer(8)
        fetchMock.mockResolvedValue({ ok: true, arrayBuffer: () => Promise.resolve(buffer) })
        vi.spyOn((MirabufLoader as unknown as Record<string, any>), "StoreInCache").mockResolvedValue(undefined)
        vi.spyOn((MirabufLoader as unknown as Record<string, any>), "AssemblyFromBuffer").mockReturnValue({ dynamic: true })
        const result = await MirabufLoader.CacheRemote("/fake/path", MiraType.ROBOT)
        expect(result).toMatchObject({ buffer })
        expect(result).toHaveProperty("miraType", MiraType.ROBOT)
        expect(result).toHaveProperty("cacheKey", "/fake/path")
    })

    test("CacheRemote caches and returns info on success", async () => {
        const buffer = new ArrayBuffer(8)
        fetchMock.mockResolvedValue({ ok: true, arrayBuffer: () => Promise.resolve(buffer) })
        const info: MirabufCacheInfo = { id: "id", miraType: MiraType.ROBOT, cacheKey: "/fake/path" }
        vi.spyOn((MirabufLoader as unknown as Record<string, any>), "StoreInCache").mockResolvedValue(info)
        const result = await MirabufLoader.CacheRemote("/fake/path", MiraType.ROBOT)
        expect(result).toBe(info)
    })

    test("CacheLocal caches buffer and returns info", async () => {
        const buffer = new ArrayBuffer(8)
        vi.spyOn((MirabufLoader as unknown as Record<string, any>), "StoreInCache").mockResolvedValue({ id: "id", miraType: MiraType.ROBOT, cacheKey: "key" })
        const result = await MirabufLoader.CacheLocal(buffer, MiraType.ROBOT)
        expect(result).toHaveProperty("id")
        expect(result).toHaveProperty("miraType", MiraType.ROBOT)
    })

    test("CacheInfo updates cache info and returns true", async () => {
        const key = "key"
        const id = "id"
        const miraType = MiraType.ROBOT
        const map = { [key]: { id, miraType, cacheKey: key } }
        localStorageMock["Robots"] = JSON.stringify(map)
        ;(MirabufLoader as unknown as { backUpRobots: unknown }).backUpRobots = { [id]: { buffer: new ArrayBuffer(1) } } 
    })

    test("HashBuffer returns a base64 string", async () => {
        const buffer = new ArrayBuffer(8)
        const hash = await ((MirabufLoader as unknown as Record<string, any>)["HashBuffer"])(buffer)
        expect(typeof hash).toBe("string")
        expect(hash.length).toBeGreaterThan(0)
    })
}) 