import { beforeEach, describe, expect, test, vi } from "vitest"

// These tests deliberately use the real async-mutex (APSTesting.test.ts mocks it as a pass-through), because
// the mutex is not reentrant and nested locking only shows up with the real thing.

vi.mock("@/aps/APSConfig", () => ({
    CLIENT_ID: "test_client_id",
    CALLBACK_URL: "https://callback.test/",
    AUTHENTICATION_BASE_URL: "https://auth.test",
    DATA_MANAGEMENT_BASE_URL: "https://data.test",
    USERINFO_BASE_URL: "https://userinfo.test",
}))

vi.mock("@/systems/World", () => ({
    default: {
        get analyticsSystem() {
            return { event: vi.fn(), exception: vi.fn() }
        },
    },
}))

const mockWindowOpen = vi.fn()
Object.defineProperty(window, "open", { value: mockWindowOpen, writable: true, configurable: true })

const mockFetch = vi.fn()
globalThis.fetch = mockFetch as typeof fetch

import APS from "@/aps/APS"

const response = (data: unknown, ok = true) => ({ ok, json: vi.fn().mockResolvedValue(data) })

const withTimeout = <T>(p: Promise<T>, ms = 2000) =>
    Promise.race([p, new Promise<never>((_, reject) => setTimeout(() => reject(new Error("deadlock")), ms))])

describe("APS refreshAuthToken with a real mutex", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        localStorage.clear()
        APS.userInfo = undefined
    })

    test("legacy session whose userinfo call fails after refresh re-prompts instead of hanging", async () => {
        mockFetch
            .mockResolvedValueOnce(
                response({ access_token: "new", refresh_token: "r", expires_in: 3600, token_type: 1 })
            )
            .mockResolvedValueOnce(response({ developerMessage: "insufficient scope" }, false))

        const result = await withTimeout(APS.refreshAuthToken("old_refresh", true))

        expect(result).toBe(true)
        expect(mockWindowOpen).toHaveBeenCalledWith(expect.stringContaining("/authentication/v2/authorize"))
    })

    test("failed refresh with relog opens the sign in popup without hanging", async () => {
        mockFetch.mockResolvedValueOnce(response({ error: "invalid_grant" }, false))

        const result = await withTimeout(APS.refreshAuthToken("bad_refresh", true))

        expect(result).toBe(false)
        expect(mockWindowOpen).toHaveBeenCalledTimes(1)
    })

    test("network error during refresh opens the sign in popup without hanging", async () => {
        mockFetch.mockRejectedValueOnce(new Error("network down"))

        const result = await withTimeout(APS.refreshAuthToken("r", true))

        expect(result).toBe(false)
        expect(mockWindowOpen).toHaveBeenCalledTimes(1)
    })
})
