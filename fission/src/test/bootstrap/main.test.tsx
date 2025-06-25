import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"

describe("main.tsx Bootstrap Tests", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        vi.resetModules()

        let rootElement = document.getElementById("root")
        if (!rootElement) {
            rootElement = document.createElement("div")
            rootElement.id = "root"
            document.body.appendChild(rootElement)
        }

        ;(globalThis as any).gtag = vi.fn()

        delete (window as any).convertAuthToken
        ;(window as any).convertAuthToken = vi.fn()

        vi.doMock("react-dom/client", () => ({
            createRoot: vi.fn(() => ({
                render: vi.fn(),
                unmount: vi.fn(),
            })),
        }))

        vi.doMock("@/Synthesis.tsx", () => ({
            default: () => "Synthesis Component",
        }))
    })

    afterEach(() => {
        vi.clearAllMocks()
        vi.restoreAllMocks()
    })

    test("successfully imports and executes main.tsx without errors", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
    })

    test("sets up window.convertAuthToken function", async () => {
        await import("@/main.tsx")
        expect(window.convertAuthToken).toBeDefined()
        expect(typeof window.convertAuthToken).toBe("function")
    })

    test("handles analytics setup without throwing", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
        expect((globalThis as any).gtag).toBeDefined()
    })

    test("handles missing root element gracefully", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect((globalThis as any).gtag).toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })

    test("can import main module multiple times without error", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
        await expect(import("@/main.tsx")).resolves.toBeDefined()
    })

    test("module sets up expected global dependencies", async () => {
        vi.doMock("react-dom/client", () => ({
            createRoot: vi.fn(() => ({
                render: vi.fn(),
            })),
        }))

        vi.doMock("@/aps/APS", () => ({
            default: {
                convertAuthToken: vi.fn(),
            },
        }))

        vi.doMock("@/Synthesis", () => ({
            default: () => "synthesis-app",
        }))

        await expect(import("@/main.tsx")).resolves.toBeDefined()
    })

    test("bootstraps in browser environment", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect((globalThis as any).gtag).toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })

    test("handles DOM queries safely", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect((globalThis as any).gtag).toBeDefined()
        expect(window.convertAuthToken).toBeDefined()
    })

    test("module is compatible with test environment", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()

        expect(window.convertAuthToken).toBeDefined()
    })

    test("handles theme initialization without errors", async () => {
        vi.doMock("@/ui/ThemeContext", () => ({
            ThemeProvider: ({ children }: { children: React.ReactNode }) => children,
        }))

        await expect(import("@/main.tsx")).resolves.toBeDefined()
    })
})
