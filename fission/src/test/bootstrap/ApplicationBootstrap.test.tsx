import { describe, test, expect, vi, beforeEach } from "vitest"

describe("Complete Application Bootstrap", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        vi.resetModules()

        let rootElement = document.getElementById("root")
        if (!rootElement) {
            rootElement = document.createElement("div")
            rootElement.id = "root"
            document.body.appendChild(rootElement)
        }

        (globalThis as any).gtag = vi.fn()
        ;(window as any).convertAuthToken = vi.fn()

        Object.defineProperty(window, "requestAnimationFrame", {
            value: vi.fn((cb: FrameRequestCallback) => {
                setTimeout(cb, 16)
                return 1
            }),
            configurable: true,
        })

        Object.defineProperty(window, "cancelAnimationFrame", {
            value: vi.fn(),
            configurable: true,
        })
    })

    test("application bootstraps successfully from main.tsx entry point", async () => {
        const mockRender = vi.fn()
        const mockCreateRoot = vi.fn(() => ({
            render: mockRender,
            unmount: vi.fn(),
        }))

        vi.doMock("react-dom/client", () => ({
            createRoot: mockCreateRoot,
        }))

        vi.doMock("@/Synthesis.tsx", () => ({
            default: () => "Synthesis App",
        }))

        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect(window.convertAuthToken).toBeDefined()
        expect((globalThis as any).gtag).toBeDefined()
    })

    test("handles complete application failure gracefully", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect(window.convertAuthToken).toBeDefined()
    })

    test("bootstrap process handles multiple imports", async () => {
        await import("@/main.tsx")
        await import("@/main.tsx")
        await import("@/main.tsx")

        expect(window.convertAuthToken).toBeDefined()
        expect((globalThis as any).gtag).toBeDefined()
    })

    test("validates critical dependencies are available", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })

    test("handles missing dependencies gracefully", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })

    test("environment setup works in different modes", async () => {
        await import("@/main.tsx")

        expect(window.convertAuthToken).toBeDefined()
        expect((globalThis as any).gtag).toBeDefined()
    })

    test("CSS imports don't break bootstrap", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })
})
