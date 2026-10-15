import { afterEach, afterAll, beforeEach, describe, expect, test, vi } from "vitest"
import { mockConsole } from "@/test/mocks/Common.ts"
// Mock dependencies before importing APS

vi.mock("@/systems/World", () => ({
    default: {
        get analyticsSystem() {
            return { event: vi.fn(), exception: vi.fn() }
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

// Pulls the params out of the authorize URL that requestAuthCode opened
const lastAuthorizeParams = () => {
    const calls = mockWindowOpen.mock.calls
    return new URL(calls[calls.length - 1][0] as string).searchParams
}

const base64UrlSha256 = async (input: string) => {
    const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(input))
    return btoa(String.fromCharCode(...new Uint8Array(digest)))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "")
}

describe("APS Authentication System", () => {
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

        // Reset APS state
        APS.resetNumApsCalls()
        APS.authCode = undefined

        // Mock console methods
        mockConsole()
    })

    afterEach(() => {
        vi.clearAllMocks()
    })
    afterAll(() => {
        vi.restoreAllMocks()
    })

    describe("End-to-End User Journeys", () => {
        // These test complete workflows and method interactions
        test("successful authentication journey from login to logout", async () => {
            // === SCENARIO 1: User starts with no authentication ===

            // 1. Initially not signed in
            expect(await APS.isSignedIn()).toBe(false)
            expect(APS.userInfo).toBeUndefined()

            // 2. User clicks login - request auth code
            await APS.requestAuthCode()
            const state = lastAuthorizeParams().get("state")!

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
                    access_token: "fresh_access_token",
                    refresh_token: "fresh_refresh_token",
                    expires_in: 3600,
                    token_type: 1,
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

            await APS.convertAuthToken(authCode, state)

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

            // 1. Login starts without touching our server
            await APS.requestAuthCode()
            expect(mockFetch).not.toHaveBeenCalled()
            let state = lastAuthorizeParams().get("state")!
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

            await APS.convertAuthToken("invalid_code", state)

            // Should remain unauthenticated
            expect(await APS.isSignedIn()).toBe(false)

            // 3. User info loading fails
            await APS.requestAuthCode()
            state = lastAuthorizeParams().get("state")!
            mockWindowOpen.mockClear()

            mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth))

            mockFetch.mockResolvedValueOnce(
                createMockResponse(
                    {
                        error: "unauthorized",
                        userMessage: "Token invalid",
                    },
                    false
                )
            )

            await APS.convertAuthToken("valid_code", state)

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

            // 3. Try to get auth with shouldRelog = true
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
            mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth)).mockResolvedValueOnce(
                createMockResponse({
                    name: mockUserInfo.name,
                    given_name: mockUserInfo.givenName,
                    picture: mockUserInfo.picture,
                    email: mockUserInfo.email,
                })
            )

            // 3. Perform authentication flow
            await APS.requestAuthCode()
            await APS.convertAuthToken("test_code", lastAuthorizeParams().get("state"))

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
            expect(APS.numApsCalls.get("authentication-v2-token")).toBe(2) // Code exchange and refresh
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
                await APS.requestAuthCode()

                expect(mockFetch).not.toHaveBeenCalled()
                expect(mockWindowOpen).toHaveBeenCalledWith(
                    expect.stringContaining("https://developer.api.autodesk.com/authentication/v2/authorize")
                )

                const callArgs = mockWindowOpen.mock.calls[0][0]
                expect(callArgs).toContain("response_type=code")
                // Fix URL encoding issue - scope gets URL encoded
                expect(callArgs).toContain("scope=data%3Aread")
                expect(callArgs).toContain("code_challenge_method=S256")
                expect(lastAuthorizeParams().get("state")).toBeTruthy()
                expect(lastAuthorizeParams().get("code_challenge")).toBeTruthy()
            })

            test("authorize URL code_challenge is the S256 hash of the stored verifier", async () => {
                await APS.requestAuthCode()
                const params = lastAuthorizeParams()

                mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth))
                mockFetch.mockResolvedValueOnce(createMockResponse({}))
                await APS.convertAuthToken("test_auth_code", params.get("state"))

                const body = mockFetch.mock.calls[0][1].body as URLSearchParams
                const verifier = body.get("code_verifier")!
                expect(await base64UrlSha256(verifier)).toBe(params.get("code_challenge"))
            })

            test("each login generates a new verifier and state", async () => {
                const verifiers: string[] = []
                const states: string[] = []
                const challenges: string[] = []
                for (let i = 0; i < 2; i++) {
                    await APS.requestAuthCode()
                    const params = lastAuthorizeParams()
                    states.push(params.get("state")!)
                    challenges.push(params.get("code_challenge")!)

                    mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth))
                    mockFetch.mockResolvedValueOnce(createMockResponse({}))
                    await APS.convertAuthToken("code", params.get("state"))
                    verifiers.push((mockFetch.mock.calls[i * 2][1].body as URLSearchParams).get("code_verifier")!)
                }

                expect(states[0]).not.toBe(states[1])
                expect(challenges[0]).not.toBe(challenges[1])
                expect(verifiers[0]).not.toBe(verifiers[1])
            })

            test("convertAuthToken posts to the APS token endpoint with code_verifier and no client_secret", async () => {
                await APS.requestAuthCode()
                const state = lastAuthorizeParams().get("state")

                mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth))
                mockFetch.mockResolvedValueOnce(
                    createMockResponse({
                        name: mockUserInfo.name,
                        given_name: mockUserInfo.givenName,
                        picture: mockUserInfo.picture,
                        email: mockUserInfo.email,
                    })
                )

                await APS.convertAuthToken("test_auth_code", state)

                expect(mockFetch).toHaveBeenCalledWith(
                    "https://developer.api.autodesk.com/authentication/v2/token",
                    expect.objectContaining({ method: "POST", body: expect.any(URLSearchParams) })
                )
                const body = mockFetch.mock.calls[0][1].body as URLSearchParams
                expect(body.get("grant_type")).toBe("authorization_code")
                expect(body.get("code")).toBe("test_auth_code")
                expect(body.get("code_verifier")).toBeTruthy()
                expect(body.get("redirect_uri")).toBeTruthy()
                expect(body.has("client_secret")).toBe(false)
                expect(APS.numApsCalls.get("authentication-v2-token")).toBe(1)
            })

            test("convertAuthToken rejects a wrong or missing state without calling fetch", async () => {
                await APS.requestAuthCode()
                await APS.convertAuthToken("test_auth_code", "wrong_state")
                expect(mockFetch).not.toHaveBeenCalled()

                await APS.requestAuthCode()
                await APS.convertAuthToken("test_auth_code", null)
                expect(mockFetch).not.toHaveBeenCalled()
                expect(await APS.isSignedIn()).toBe(false)
            })

            test("convertAuthToken rejects when no login is pending", async () => {
                await APS.convertAuthToken("test_auth_code", "some_state")
                expect(mockFetch).not.toHaveBeenCalled()
            })

            test("convertAuthToken cannot be redeemed twice", async () => {
                await APS.requestAuthCode()
                const state = lastAuthorizeParams().get("state")

                mockFetch.mockResolvedValueOnce(createMockResponse(mockAuth))
                mockFetch.mockResolvedValueOnce(createMockResponse({}))
                await APS.convertAuthToken("test_auth_code", state)
                mockFetch.mockClear()

                await APS.convertAuthToken("test_auth_code", state)
                expect(mockFetch).not.toHaveBeenCalled()
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
                    expect.stringContaining("autodesk.com"),
                    expect.objectContaining({
                        method: "GET",
                        headers: expect.objectContaining({
                            Authorization: "Bearer " + mockAuth.access_token,
                        }),
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

            await APS.requestAuthCode()
            await APS.convertAuthToken("invalid_code", lastAuthorizeParams().get("state"))

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

            await APS.loadUserInfo(mockAuth)

            // Should trigger re-login flow
            expect(mockWindowOpen).toHaveBeenCalled()
        })
    })
})
