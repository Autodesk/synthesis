import { afterEach, beforeEach, describe, expect, type MockedFunction, test, vi } from "vitest"
import MirabufLoader, { MiraType } from "../../mirabuf/MirabufLoader"

vi.mock("@/systems/World", () => ({
    default: {
        get analyticsSystem() {
            return { event: vi.fn(), exception: vi.fn() }
        },
    },
}))

// Polyfill btoa for Uint8Array to base64 (browser compatible, no Buffer)
function uint8ToBase64(bytes: Uint8Array): string {
    let binary = ""
    for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i])
    return btoa(binary)
}

globalThis.btoa =
    globalThis.btoa ||
    ((str: string) => {
        return uint8ToBase64(new TextEncoder().encode(str))
    })

describe("MirabufLoader", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    let localStorageMock: Record<string, string>
    let fetchMock: MockedFunction<typeof fetch>
    let originalDigest: typeof crypto.subtle.digest
    let unhandledRejectionHandler: ((event: PromiseRejectionEvent) => void) | undefined

    beforeEach(() => {
        localStorageMock = {}
        vi.stubGlobal("localStorage", {
            getItem: vi.fn(key => localStorageMock[key] ?? null),
            setItem: vi.fn((key, value) => {
                localStorageMock[key] = value
            }),
            removeItem: vi.fn(key => {
                delete localStorageMock[key]
            }),
            clear: vi.fn(() => {
                localStorageMock = {}
            }),
            key: vi.fn(),
            length: 0,
        })
        fetchMock = vi.fn() as MockedFunction<typeof fetch>
        globalThis.fetch = fetchMock
        if (!globalThis.crypto) {
            // @ts-expect-error: Polyfill for crypto in test environment
            globalThis.crypto = {}
        }
        if (!globalThis.crypto.subtle) {
            // @ts-expect-error: Polyfill for crypto.subtle in test environment
            globalThis.crypto.subtle = {}
        }
        originalDigest = globalThis.crypto.subtle.digest
        globalThis.crypto.subtle.digest = vi.fn(async (_alg, _data) => {
            return new Uint8Array(32).buffer
        }) as typeof crypto.subtle.digest

        // Mock OPFS APIs (use Object.defineProperty to avoid read-only errors)
        if (typeof navigator !== "undefined") {
            if (!navigator.storage) {
                Object.defineProperty(navigator, "storage", {
                    value: {},
                    configurable: true,
                })
            }
            Object.defineProperty(navigator.storage, "getDirectory", {
                value: vi.fn(async () => ({
                    getDirectoryHandle: vi.fn(async () => ({
                        name: "Robots",
                        getFileHandle: vi.fn(async () => ({
                            createWritable: vi.fn(async () => ({
                                write: vi.fn(),
                                close: vi.fn(),
                            })),
                            getFile: vi.fn(async () => new Blob()),
                            name: "0",
                        })),
                        keys: vi.fn(async function* () {}),
                        removeEntry: vi.fn(),
                        entries: vi.fn(),
                    })),
                })),
                configurable: true,
            })
        }

        console.log = vi.fn()
        console.error = vi.fn()
        console.warn = vi.fn()
        console.debug = vi.fn()

        // Suppress unhandled NotFoundError rejections
        unhandledRejectionHandler = (event: PromiseRejectionEvent) => {
            if (event.reason && event.reason.name === "NotFoundError") {
                event.preventDefault()
            }
        }
        window.addEventListener("unhandledrejection", unhandledRejectionHandler)
    })

    afterEach(() => {
        vi.restoreAllMocks()
        vi.unstubAllGlobals()
        if (globalThis.crypto && globalThis.crypto.subtle && originalDigest) {
            globalThis.crypto.subtle.digest = originalDigest
        }

        console.log = originalConsoleLog
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.debug = originalConsoleDebug

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
        expect(result).toHaveProperty("cacheKey", "/fake/path")

        if ("buffer" in result!) expect(result.buffer).toBeInstanceOf(ArrayBuffer)
    })

    test("CacheRemote caches and returns info on success", async () => {
        const buffer = new ArrayBuffer(8)
        fetchMock.mockResolvedValue(new Response(buffer, { status: 200 }))
        const result = await MirabufLoader.cacheRemote("/fake/path", MiraType.ROBOT)
        expect(result).toBeDefined()
    })

    test("HashBuffer returns a string", async () => {
        const buffer = new ArrayBuffer(8)
        const hash = await MirabufLoader["hashBuffer"](buffer)
        expect(typeof hash).toBe("string")
        expect(hash.length).toBeGreaterThan(0)
    })
})
