import { afterEach, assert, beforeEach, describe, expect, type MockedFunction, test, vi } from "vitest"
import MirabufLoader, { MiraType } from "../../mirabuf/MirabufLoader"

vi.mock("@/systems/World", () => ({
    default: {
        get analyticsSystem() {
            return { event: vi.fn(), exception: vi.fn() }
        },
    },
}))

// // Polyfill btoa for Uint8Array to base64 (browser compatible, no Buffer)
// function uint8ToBase64(bytes: Uint8Array): string {
//     let binary = ""
//     for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i])
//     return btoa(binary)
// }
//
// globalThis.btoa =
//     globalThis.btoa ||
//     ((str: string) => {
//         return uint8ToBase64(new TextEncoder().encode(str))
//     })

describe("MirabufLoader", () => {
    let fetchMock: MockedFunction<typeof fetch>
    let unhandledRejectionHandler: ((event: PromiseRejectionEvent) => void) | undefined

    beforeEach(() => {
        vi.spyOn(console, "log").mockImplementation(() => {})
        vi.spyOn(console, "warn").mockImplementation(() => {})
        vi.spyOn(console, "error").mockImplementation(() => {})
        vi.spyOn(console, "debug").mockImplementation(() => {})
    })
    afterEach(() => {
        vi.restoreAllMocks()
        vi.unstubAllGlobals()
    })
    describe("Fake Fetch", () => {
        beforeEach(() => {
            fetchMock = vi.fn() as MockedFunction<typeof fetch>
            vi.stubGlobal("fetch", fetchMock)
            // Suppress unhandled NotFoundError rejections
            unhandledRejectionHandler = (event: PromiseRejectionEvent) => {
                if (event.reason && event.reason.name === "NotFoundError") {
                    event.preventDefault()
                }
            }
            window.addEventListener("unhandledrejection", unhandledRejectionHandler)
        })

        afterEach(() => {
            if (unhandledRejectionHandler) {
                window.removeEventListener("unhandledrejection", unhandledRejectionHandler)
            }
        })

        test("CacheRemote returns fallback on cache failure (GH-1141)", async () => {
            const buffer = new ArrayBuffer(8)
            fetchMock.mockResolvedValue(new Response(buffer, { status: 200 }))

            const result = await MirabufLoader.cacheRemote("/fake/path", MiraType.ROBOT)
            expect(result).toBeDefined()
            expect(result).toHaveProperty("miraType", MiraType.ROBOT)
            expect(result).toHaveProperty("remotePath", "/fake/path")

            if ("buffer" in result!) expect(result.buffer).toBeInstanceOf(ArrayBuffer)
        })

        test("CacheRemote caches and returns info on success", async () => {
            const buffer = new ArrayBuffer(8)
            fetchMock.mockResolvedValue(new Response(buffer, { status: 200 }))
            const result = await MirabufLoader.cacheRemote("/fake/path", MiraType.ROBOT)
            expect(result).toBeDefined()
        })
    })

    describe("Real Fetch", () => {
        beforeEach(async () => {
            await MirabufLoader.removeAll()
        })
        const tests: [string, MiraType][] = [
            ["/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT],
            ["/api/mira/fields/FRC Field 2023_v7.mira", MiraType.FIELD],
        ]
        test.for(tests)("Loads Asset ($0)", async ([url, miratype]) => {
            const info = await MirabufLoader.cacheRemote(url, miratype)
            expect(info).toBeDefined()
            expect(info?.miraType).toBe(miratype)
            expect(info?.name).toMatchSnapshot()
            expect(info?.hash).toMatchSnapshot()
            const assembly = await MirabufLoader.get(info!.hash)
            expect(assembly).toBeDefined()
            expect(assembly?.info?.name).toBe(info!.name)
            expect(MirabufLoader.getAll()).toStrictEqual([info])
            expect(MirabufLoader.getAll(miratype)).toStrictEqual([info])
            expect(MirabufLoader.getAll(miratype == MiraType.FIELD ? MiraType.ROBOT : MiraType.FIELD)).toStrictEqual([])
        })

        test("Remove All Cleans Up", async () => {
            const field1 = await MirabufLoader.cacheRemote("/api/mira/fields/FRC Field 2023_v7.mira", MiraType.FIELD)
            const robot1 = await MirabufLoader.cacheRemote("/api/mira/robots/Dozer_v9.mira", MiraType.ROBOT)
            assert.exists(field1)
            assert.exists(robot1)
            expect(MirabufLoader.getAll()).toHaveLength(2)
            await MirabufLoader.removeAll()
            expect(MirabufLoader.getAll()).toHaveLength(0)
            assert.notExists(await MirabufLoader.get(field1.hash))
            assert.notExists(await MirabufLoader.get(robot1.hash))

            const opfsRoot = await navigator.storage.getDirectory()
            for await (const dir of opfsRoot.keys()) {
                const handle = await opfsRoot.getDirectoryHandle(dir)
                for await (const key of handle.keys()) {
                    expect.fail(key, "", "Directory should be empty", "does not exist")
                }
            }
        })
    })
})
