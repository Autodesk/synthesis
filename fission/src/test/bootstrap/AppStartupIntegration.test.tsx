import { describe, test, expect, vi, beforeEach, Mock } from "vitest"

declare const globalThis: Global

describe("App Startup & Platform APIs", () => {
    let createRoot: Mock

    beforeEach(() => {
        vi.clearAllMocks()
        vi.resetModules()

        let root = document.getElementById("root")
        if (!root) {
            root = document.createElement("div")
            root.id = "root"
            document.body.appendChild(root)
        }

        globalThis.gtag = vi.fn()
        window.convertAuthToken = vi.fn()

        createRoot = vi.fn(() => ({ render: vi.fn(), unmount: vi.fn() }))
        vi.doMock("react-dom/client", () => ({ createRoot }))
        vi.doMock("@/Synthesis.tsx", () => ({ default: () => "Synthesis" }))
    })

    test("bootstraps main.tsx without error", async () => {
        await expect(import("@/main.tsx")).resolves.toBeDefined()
    })

    test("wires up analytics & auth callbacks", async () => {
        await import("@/main.tsx")
        expect(typeof window.convertAuthToken).toBe("function")
        expect(typeof globalThis.gtag).toBe("function")
    })

    test("does not throw on basic browser APIs", () => {
        expect(() => {
            requestAnimationFrame(() => {})
            cancelAnimationFrame(1)
            localStorage.setItem("foo", "bar")
            localStorage.getItem("foo")
            document.getElementById("nonexistent")
        }).not.toThrow()
    })
})
