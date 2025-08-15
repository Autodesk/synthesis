import { afterEach, beforeEach, describe, expect, test, vi } from "vitest"

// Mock dependencies before importing APS
vi.mock("@/systems/World", () => ({
    default: {
        AnalyticsSystem: {
            Event: vi.fn(),
            Exception: vi.fn(),
        },
    },
}))

vi.mock("async-mutex", () => ({
    Mutex: vi.fn(() => ({
        runExclusive: vi.fn(fn => fn()),
    })),
}))

// Mock window.open
const mockWindowOpen = vi.fn()
Object.defineProperty(window, "open", {
    value: mockWindowOpen,
    writable: true,
    configurable: true,
})

// Mock fetch with proper typing
const mockFetch = vi.fn()
globalThis.fetch = mockFetch as typeof fetch
// Also mock window.fetch if it exists
if (typeof window !== "undefined") {
    window.fetch = mockFetch
}

// Mock document.dispatchEvent
const mockDispatchEvent = vi.fn()
Object.defineProperty(document, "dispatchEvent", {
    value: mockDispatchEvent,
    writable: true,
    configurable: true,
})

// Mock Date.now for consistent testing
const mockNow = 1700000000000 // Fixed timestamp
vi.spyOn(Date, "now").mockReturnValue(mockNow)

// Import APS after setting up mocks
import APS, { type APSAuth, type APSUserInfo } from "@/aps/APS"

// Helper function to create proper fetch response mock
const createMockResponse = (data: unknown, ok: boolean = true) => ({
    ok,
    json: vi.fn().mockResolvedValue(data),
})

describe("APS Authentication System", () => {
    const originalConsoleLog = console.log
    const originalConsoleError = console.error
    const originalConsoleWarn = console.warn
    const originalConsoleDebug = console.debug

    const mockAuth: APSAuth = {
        access_token: "test_access_token",
        refresh_token: "test_refresh_token",
        expires_in: 3600,
        expires_at: mockNow + 3600000,
        token_type: 1,
    }

    const mockUserInfo: APSUserInfo = {
        name: "Test User",
        picture: "https://example.com/pic.jpg",
        givenName: "Test",
        email: "test@example.com",
    }

    beforeEach(() => {
        // Clear localStorage and reset mocks
        localStorage.clear()
        vi.clearAllMocks()

        // Reset APS state
        APS.resetNumApsCalls()
        APS.authCode = undefined

        // Mock console methods
        console.log = vi.fn()
        console.error = vi.fn()
        console.warn = vi.fn()
        console.debug = vi.fn()
    })

    afterEach(() => {
        vi.clearAllMocks()
        console.log = originalConsoleLog
        console.error = originalConsoleError
        console.warn = originalConsoleWarn
        console.debug = originalConsoleDebug
    })

    describe("End-to-End User Journeys", () => {
        // These test complete workflows and method interactions
        test("successful authentication journey from login to logout", async () => {
            // === SCENARIO 1: User starts with no authentication ===

            // 1. Initially not signed in
            expect(await APS.isSignedIn()).toBe(false)
            expect(APS.userInfo).toBeUndefined()

            // 2. User clicks login - request auth code
            mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "challenge_123" }))

            await APS.requestAuthCode()

            // Should open authorization window
            expect(mockWindowOpen).toHaveBeenCalledTimes(1)
            expect(mockWindowOpen).toHaveBeenCalledWith(
                expect.stringContaining("https://developer.api.autodesk.com/authentication/v2/authorize")
            )

            // 3. User authorizes and comes back with auth code
            const authCode = "auth_code_123"

            // Mock the token exchange endpoint
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    response: {
                        access_token: "fresh_access_token",
                        refresh_token: "fresh_refresh_token",
                        expires_in: 3600,
                        token_type: 1,
                    },
                })
            )

            // Mock user info loading
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    name: mockUserInfo.name,
                    given_name: mockUserInfo.givenName,
                    picture: mockUserInfo.picture,
                    email: mockUserInfo.email,
                })
            )

            await APS.convertAuthToken(authCode)

            // 4. Verify user is now authenticated
            expect(await APS.isSignedIn()).toBe(true)
            expect(APS.userInfo).toEqual(mockUserInfo)

            // Verify auth data was stored
            const storedAuth = localStorage.getItem("aps_auth")
            expect(storedAuth).toBeTruthy()
            expect(storedAuth).toContain("fresh_access_token")

            // Verify user info was stored
            const storedUserInfo = localStorage.getItem("aps_user_info")
            expect(storedUserInfo).toBeTruthy()
            expect(JSON.parse(storedUserInfo!)).toEqual({
                name: mockUserInfo.name,
                givenName: mockUserInfo.givenName,
                picture: mockUserInfo.picture,
                email: mockUserInfo.email,
            })

            // === SCENARIO 2: User makes authenticated requests ===

            // 5. User can now make authenticated requests
            const auth = await APS.getAuth()
            expect(auth).toBeDefined()
            expect(auth?.access_token).toBe("fresh_access_token")

            // === SCENARIO 3: Token expires and gets refreshed ===

            // 6. Simulate token expiration
            localStorage.setItem(
                "aps_auth",
                JSON.stringify({
                    access_token: "expired_token",
                    refresh_token: "fresh_refresh_token",
                    expires_in: 3600,
                    expires_at: mockNow - 1000, // Expired 1 second ago
                    token_type: 1,
                })
            )

            // Mock successful token refresh
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    access_token: "refreshed_access_token",
                    refresh_token: "refreshed_refresh_token",
                    expires_in: 3600,
                    token_type: 1,
                })
            )

            // Mock user info loading after refresh
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    name: mockUserInfo.name,
                    given_name: mockUserInfo.givenName,
                    picture: mockUserInfo.picture,
                    email: mockUserInfo.email,
                })
            )

            const refreshedAuth = await APS.getAuth()
            expect(refreshedAuth).toBeDefined()
            expect(refreshedAuth?.access_token).toBe("refreshed_access_token")

            // Should have refreshed the token
            expect(mockFetch).toHaveBeenCalledWith(
                "https://developer.api.autodesk.com/authentication/v2/token",
                expect.objectContaining({
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: expect.any(URLSearchParams),
                })
            )

            // === SCENARIO 4: User logs out ===

            // 7. User logs out
            localStorage.setItem(
                "aps_auth",
                JSON.stringify({
                    access_token: "current_token",
                    refresh_token: "current_refresh_token",
                    expires_in: 3600,
                    expires_at: mockNow + 3600000,
                    token_type: 1,
                })
            )

            // Mock successful token revocation
            mockFetch.mockResolvedValueOnce(createMockResponse({}))

            await APS.logout()

            // Should have revoked the token
            expect(mockFetch).toHaveBeenCalledWith(
                "https://developer.api.autodesk.com/authentication/v2/revoke",
                expect.objectContaining({
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: expect.any(URLSearchParams),
                })
            )

            // Should have cleared auth data
            expect(localStorage.getItem("aps_auth")).toBeNull()

            // 8. Verify user is logged out
            expect(await APS.isSignedIn()).toBe(false)
        })

        test("handles authentication failure gracefully", async () => {
            // === SCENARIO: Authentication fails at various points ===

            // 1. Code challenge fails
            mockFetch.mockRejectedValueOnce(new Error("Network error"))

            await APS.requestAuthCode()

            // Should not open window on failure - but check that codeChallenge was called
            expect(mockFetch).toHaveBeenCalledWith("/api/aps/challenge")
            // Reset mock calls for next test
            mockFetch.mockClear()
            mockWindowOpen.mockClear()

            // 2. Token exchange fails
            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "invalid_grant",
                        userMessage: "Invalid authorization code",
                    },
                    false
                )
            )

            await APS.convertAuthToken("invalid_code")

            // Should remain unauthenticated
            expect(await APS.isSignedIn()).toBe(false)

            // 3. User info loading fails
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    response: mockAuth,
                })
            )

            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "unauthorized",
                        userMessage: "Token invalid",
                    },
                    false
                )
            )

            // Mock the re-login flow
            mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "challenge_456" }))

            await APS.convertAuthToken("valid_code")

            // Should trigger re-login flow
            expect(mockWindowOpen).toHaveBeenCalledWith(
                expect.stringContaining("https://developer.api.autodesk.com/authentication/v2/authorize")
            )
        })

        test("handles token refresh failure and triggers re-authentication", async () => {
            // === SCENARIO: Token refresh fails, user needs to re-authenticate ===

            // 1. Set up expired token
            localStorage.setItem(
                "aps_auth",
                JSON.stringify({
                    access_token: "expired_token",
                    refresh_token: "invalid_refresh_token",
                    expires_in: 3600,
                    expires_at: mockNow - 1000,
                    token_type: 1,
                })
            )

            // 2. Mock failed token refresh
            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "invalid_grant",
                        userMessage: "Refresh token expired",
                    },
                    false
                )
            )

            // 3. Mock successful code challenge for re-auth
            mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "new_challenge" }))

            // 4. Try to get auth with shouldRelog = true
            const result = await APS.refreshAuthToken("invalid_refresh_token", true)

            // Should fail and trigger re-login
            expect(result).toBe(false)
            expect(mockWindowOpen).toHaveBeenCalledWith(
                expect.stringContaining("https://developer.api.autodesk.com/authentication/v2/authorize")
            )
        })

        test("user returns to app after existing session", async () => {
            // === SCENARIO: User returns to app with existing valid session ===

            // 1. Simulate existing valid session in localStorage
            localStorage.setItem(
                "aps_auth",
                JSON.stringify({
                    access_token: "existing_token",
                    refresh_token: "existing_refresh_token",
                    expires_in: 3600,
                    expires_at: mockNow + 1800000, // Expires in 30 minutes
                    token_type: 1,
                })
            )
            localStorage.setItem("aps_user_info", JSON.stringify(mockUserInfo))

            // 2. User should be immediately signed in
            expect(await APS.isSignedIn()).toBe(true)
            expect(APS.userInfo).toEqual(mockUserInfo)

            // 3. Getting auth should return existing token without refresh
            const auth = await APS.getAuth()
            expect(auth?.access_token).toBe("existing_token")

            // Should not have made any network requests
            expect(mockFetch).not.toHaveBeenCalled()
        })

        test("API call tracking works throughout authentication flow", async () => {
            // === SCENARIO: Verify API calls are tracked properly ===

            // 1. Reset counters
            APS.resetNumApsCalls()
            expect(APS.numApsCalls.size).toBe(0)

            // 2. Mock requests for complete flow
            mockFetch
                .mockResolvedValueOnce(createMockResponse({ challenge: "challenge" }))
                .mockResolvedValueOnce(createMockResponse({ response: mockAuth }))
                .mockResolvedValueOnce(
                    createMockResponse({
                        name: mockUserInfo.name,
                        given_name: mockUserInfo.givenName,
                        picture: mockUserInfo.picture,
                        email: mockUserInfo.email,
                    })
                )

            // 3. Perform authentication flow
            await APS.requestAuthCode()
            await APS.convertAuthToken("test_code")

            // 4. Verify API calls were tracked
            expect(APS.numApsCalls.get("userinfo")).toBe(1)
            expect(APS.numApsCalls.size).toBeGreaterThan(0)

            // 5. Test refresh flow tracking
            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    access_token: "new_token",
                    refresh_token: "new_refresh",
                    expires_in: 3600,
                    token_type: 1,
                })
            )

            mockFetch.mockResolvedValueOnce(
                createMockResponse({
                    name: mockUserInfo.name,
                    given_name: mockUserInfo.givenName,
                    picture: mockUserInfo.picture,
                    email: mockUserInfo.email,
                })
            )

            await APS.refreshAuthToken("test_refresh", false)

            // Should have tracked the refresh call
            expect(APS.numApsCalls.get("authentication-v2-token")).toBe(1)
            expect(APS.numApsCalls.get("userinfo")).toBe(2) // Called twice now
        })
    })

    describe("Individual Method Behavior", () => {
        // These test specific method inputs, outputs, and edge cases

        describe("Core Authentication Methods", () => {
            test("getAuth returns undefined when no auth data", async () => {
                const result = await APS.getAuth()
                expect(result).toBeUndefined()
            })

            test("getAuth returns auth data when valid and not expired", async () => {
                const validAuth = { ...mockAuth, expires_at: mockNow + 1000000 }
                localStorage.setItem("aps_auth", JSON.stringify(validAuth))

                const result = await APS.getAuth()
                expect(result).toEqual(validAuth)
            })

            test("logout calls revoke token and clears auth data", async () => {
                localStorage.setItem("aps_auth", JSON.stringify(mockAuth))
                mockFetch.mockResolvedValueOnce(createMockResponse({}))

                await APS.logout()

                expect(mockFetch).toHaveBeenCalledWith(
                    "https://developer.api.autodesk.com/authentication/v2/revoke",
                    expect.objectContaining({
                        method: "POST",
                    })
                )
                // Verify auth data was cleared
                expect(localStorage.getItem("aps_auth")).toBeNull()
            })

            test("requestAuthCode generates correct authorization URL", async () => {
                // This tests URL parameters that integration tests don't check
                mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "test_challenge" }))

                await APS.requestAuthCode()

                expect(mockWindowOpen).toHaveBeenCalledWith(
                    expect.stringContaining("https://developer.api.autodesk.com/authentication/v2/authorize")
                )

                const callArgs = mockWindowOpen.mock.calls[0][0]
                expect(callArgs).toContain("response_type=code")
                // Fix URL encoding issue - scope gets URL encoded
                expect(callArgs).toContain("scope=data%3Aread")
                expect(callArgs).toContain("code_challenge=test_challenge")
            })

            test("codeChallenge fetches challenge from correct endpoint", async () => {
                // This tests specific return value and endpoint
                mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "test_challenge" }))

                const result = await APS.codeChallenge()

                expect(result).toBe("test_challenge")
                expect(mockFetch).toHaveBeenCalledWith("/api/aps/challenge")
            })

            test("convertAuthToken makes correct API call", async () => {
                // This tests specific endpoint and parameters
                mockFetch.mockResolvedValueOnce(
                    createMockResponse({
                        response: mockAuth,
                    })
                )

                // Mock loadUserInfo
                mockFetch.mockResolvedValueOnce(
                    createMockResponse({
                        name: mockUserInfo.name,
                        given_name: mockUserInfo.givenName,
                        picture: mockUserInfo.picture,
                        email: mockUserInfo.email,
                    })
                )

                await APS.convertAuthToken("test_auth_code")

                expect(mockFetch).toHaveBeenCalledWith(expect.stringContaining("/api/aps/code?code=test_auth_code"))
            })

            test("refreshAuthToken successfully refreshes token", async () => {
                const newAuth = { ...mockAuth, access_token: "new_token" }
                mockFetch.mockResolvedValueOnce(createMockResponse(newAuth))

                // Mock loadUserInfo
                mockFetch.mockResolvedValueOnce(
                    createMockResponse({
                        name: mockUserInfo.name,
                        given_name: mockUserInfo.givenName,
                        picture: mockUserInfo.picture,
                        email: mockUserInfo.email,
                    })
                )

                const result = await APS.refreshAuthToken("test_refresh_token", false)

                expect(result).toBe(true)
                expect(mockFetch).toHaveBeenCalledWith(
                    "https://developer.api.autodesk.com/authentication/v2/token",
                    expect.objectContaining({
                        method: "POST",
                        body: expect.any(URLSearchParams),
                    })
                )
            })

            test("loadUserInfo calls correct endpoint with auth header", async () => {
                // This tests specific API call structure
                mockFetch.mockResolvedValueOnce(
                    createMockResponse({
                        name: mockUserInfo.name,
                        given_name: mockUserInfo.givenName,
                        picture: mockUserInfo.picture,
                        email: mockUserInfo.email,
                    })
                )

                await APS.loadUserInfo(mockAuth)

                expect(mockFetch).toHaveBeenCalledWith(
                    "https://api.userprofile.autodesk.com/userinfo",
                    expect.objectContaining({
                        method: "GET",
                        headers: {
                            Authorization: mockAuth.access_token,
                        },
                    })
                )
            })
        })
    })

    describe("Authentication state", () => {
        test("isSignedIn returns false when no auth data", async () => {
            expect(await APS.isSignedIn()).toBe(false)
        })

        test("isSignedIn returns true when valid auth data exists", async () => {
            localStorage.setItem("aps_auth", JSON.stringify(mockAuth))
            expect(await APS.isSignedIn()).toBe(true)
        })

        test("handles corrupted auth data gracefully", async () => {
            localStorage.setItem("aps_auth", "invalid json")
            expect(await APS.isSignedIn()).toBe(false)
        })
    })

    describe("Error handling", () => {
        test("handles code challenge failure", async () => {
            mockFetch.mockRejectedValueOnce(new Error("Network error"))

            const result = await APS.codeChallenge()

            expect(result).toBeUndefined()
        })

        test("handles conversion failure", async () => {
            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "invalid_code",
                        userMessage: "Invalid authorization code",
                    },
                    false
                )
            )

            await APS.convertAuthToken("invalid_code")

            // The function should handle the error gracefully
            expect(mockFetch).toHaveBeenCalled()
        })

        test("handles user info load failure", async () => {
            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "unauthorized",
                        userMessage: "Invalid access token",
                    },
                    false
                )
            )

            // Mock codeChallenge for re-login
            mockFetch.mockResolvedValueOnce(createMockResponse({ challenge: "test_challenge" }))

            await APS.loadUserInfo(mockAuth)

            // Should trigger re-login flow
            expect(mockWindowOpen).toHaveBeenCalled()
        })
    })
})
