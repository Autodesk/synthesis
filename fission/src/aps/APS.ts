import { Mutex } from "async-mutex"
import { AUTHENTICATION_BASE_URL, CALLBACK_URL, CLIENT_ID, USERINFO_BASE_URL } from "@/aps/APSConfig"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

// openid and the profapi scopes are required by the userinfo endpoint for the name, email and picture
const APS_SCOPES = "data:read openid profapi:core-std-profile:read profapi:img-profile:read"
const APS_AUTH_KEY = "aps_auth"
const APS_USER_INFO_KEY = "aps_user_info"

const ENDPOINT_AUTODESK_AUTHENTICATION_AUTHORIZE = `${AUTHENTICATION_BASE_URL}/authentication/v2/authorize`
const ENDPOINT_AUTODESK_AUTHENTICATION_TOKEN = `${AUTHENTICATION_BASE_URL}/authentication/v2/token`
const ENDPOINT_AUTODESK_AUTHENTICATION_REVOKE = `${AUTHENTICATION_BASE_URL}/authentication/v2/revoke`
const ENDPOINT_AUTODESK_USERINFO = `${USERINFO_BASE_URL}/userinfo`

// biome-ignore-start lint/style/useNamingConvention: returned from api
export interface APSAuth {
    access_token: string
    refresh_token: string
    expires_in: number
    expires_at: number
    token_type: number
}
// biome-ignore-end lint/style/useNamingConvention: returned from api

export interface APSUserInfo {
    name: string
    picture: string
    givenName: string
    email: string
}

const randomBase64Url = (numBytes: number): string => {
    const bytes = crypto.getRandomValues(new Uint8Array(numBytes))
    return base64UrlEncode(bytes)
}

const base64UrlEncode = (bytes: Uint8Array): string =>
    btoa(String.fromCharCode(...bytes))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "")

const challengeFor = async (verifier: string): Promise<string> => {
    const digest = await crypto.subtle.digest("SHA-256", new TextEncoder().encode(verifier))
    return base64UrlEncode(new Uint8Array(digest))
}

// biome-ignore lint/style/useNamingConvention: this is pascal case
class APS {
    static authCode: string | undefined = undefined
    static requestMutex: Mutex = new Mutex()

    // PKCE verifier and state for the login in progress. Only ever held in memory, so only this window can redeem the code.
    private static _pendingLogin: { verifier: string; state: string } | undefined = undefined

    private static _numApsCalls = new Map<string, number>()
    public static get numApsCalls() {
        return this._numApsCalls
    }
    public static incApsCalls(endpoint: string) {
        this._numApsCalls.set(endpoint, (this._numApsCalls.get(endpoint) ?? 0) + 1)
    }
    public static resetNumApsCalls() {
        this._numApsCalls.clear()
    }

    private static get _auth(): APSAuth | undefined {
        const res = window.localStorage.getItem(APS_AUTH_KEY)
        try {
            return res ? JSON.parse(res) : undefined
        } catch (e) {
            console.warn(`Failed to parse stored APS auth data: ${e}`)
            return undefined
        }
    }

    private static set _auth(a: APSAuth | undefined) {
        window.localStorage.removeItem(APS_AUTH_KEY)
        if (a) {
            window.localStorage.setItem(APS_AUTH_KEY, JSON.stringify(a))
            World.analyticsSystem?.event("APS Login")
        }
        this.userInfo = undefined
    }

    /**
     * Sets the timestamp at which the access token expires
     *
     * @param {number} expiresAt - When the token expires
     */
    static setExpiresAt(expiresAt: number) {
        if (this._auth) this._auth.expires_at = expiresAt
    }

    /**
     * Returns whether the user is signed in
     * @returns {boolean} Whether the user is signed in
     */
    static async isSignedIn(): Promise<boolean> {
        const auth = await this.getAuth()
        return !!auth
    }

    /**
     * Returns the auth data of the current user. See {@link APSAuth}
     * @returns {(APSAuth | undefined)} Auth data of the current user
     */
    static async getAuth(): Promise<APSAuth | undefined> {
        const auth = this._auth
        if (!auth) return undefined

        if (Date.now() > auth.expires_at) {
            console.debug("Expired. Refreshing...")
            await this.refreshAuthToken(auth.refresh_token, false)
        }
        return this._auth
    }

    /**
     * Returns the auth data of the current user or prompts them to sign in if they haven't. See {@link APSAuth} and {@link APS#refreshAuthToken}
     * @returns {Promise<APSAuth | undefined>} Promise that resolves to the auth data
     */
    static async getAuthOrLogin(): Promise<APSAuth | undefined> {
        const auth = this._auth
        if (!auth) {
            this.requestAuthCode()
            return undefined
        }

        if (Date.now() > auth.expires_at) {
            await this.refreshAuthToken(auth.refresh_token, true)
        }
        return this._auth
    }

    static get userInfo(): APSUserInfo | undefined {
        const res = window.localStorage.getItem(APS_USER_INFO_KEY)

        try {
            return res ? (JSON.parse(res) as APSUserInfo) : undefined
        } catch (e) {
            console.warn(`Failed to parse stored APS user info: ${e}`)
            return undefined
        }
    }

    static set userInfo(info: APSUserInfo | undefined) {
        window.localStorage.removeItem(APS_USER_INFO_KEY)
        if (info) {
            window.localStorage.setItem(APS_USER_INFO_KEY, JSON.stringify(info))
        }
        EventSystem.dispatch("APSUserInfoUpdate")
    }

    /**
     * Logs the user out by setting their auth data to undefined and revoking their auth token.
     */
    static async logout() {
        await this.revokeTokenPublic()
        this._auth = undefined
    }

    /*
     * Revokes the users token
     *
     * The client should be public since we're an spa
     * Endpoint documentation:
     * https://aps.autodesk.com/en/docs/oauth/v2/reference/http/revoke-POST/
     */
    static async revokeTokenPublic(): Promise<boolean> {
        const headers = {
            "Content-Type": "application/x-www-form-urlencoded",
        }
        const opts = {
            method: "POST",
            headers: headers,
            body: new URLSearchParams([
                ["token", this._auth?.access_token],
                ["token_type_hint", "access_token"],
                ["client_id", CLIENT_ID],
            ] as string[][]),
        }
        const res = await fetch(ENDPOINT_AUTODESK_AUTHENTICATION_REVOKE, opts)
        if (!res.ok) {
            console.log("Failed to revoke auth token:\n")
            return false
        }
        console.log("Revoked auth token")
        return true
    }

    /**
     * Prompts the user to sign in, which will retrieve the auth code.
     */
    static async requestAuthCode() {
        await this.requestMutex.runExclusive(async () => {
            try {
                const verifier = randomBase64Url(32)
                const state = randomBase64Url(16)
                const challenge = await challengeFor(verifier)
                // Starting a new login replaces any old one, so an older popup will fail the state check
                this._pendingLogin = { verifier, state }

                const params = new URLSearchParams({
                    response_type: "code",
                    client_id: CLIENT_ID,
                    redirect_uri: CALLBACK_URL,
                    scope: APS_SCOPES,
                    nonce: Date.now().toString(),
                    prompt: "login",
                    code_challenge: challenge,
                    code_challenge_method: "S256",
                    state: state,
                })

                if (APS.userInfo) {
                    params.append("authoptions", encodeURIComponent(JSON.stringify({ id: APS.userInfo.email })))
                }

                const url = `${ENDPOINT_AUTODESK_AUTHENTICATION_AUTHORIZE}?${params.toString()}`

                window.open(url)
            } catch (e) {
                console.error(e)
                World.analyticsSystem?.exception("APS Login Failure")
                globalAddToast("error", "Error signing in.", "Please try again.")
            }
        })
    }

    /**
     * Refreshes the access token using our refresh token.
     * @param {string} refreshToken - The refresh token from our auth data
     *
     * @returns If the promise returns true, that means the auth token is currently available. If not, it means it
     *           is not readily available, although one may be in the works
     */
    static async refreshAuthToken(refreshToken: string, shouldRelog: boolean): Promise<boolean> {
        return this.requestMutex.runExclusive(async () => {
            try {
                APS.incApsCalls("authentication-v2-token")
                const res = await fetch(ENDPOINT_AUTODESK_AUTHENTICATION_TOKEN, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                    },
                    body: new URLSearchParams({
                        client_id: CLIENT_ID,
                        grant_type: "refresh_token",
                        refresh_token: refreshToken,
                        scope: APS_SCOPES,
                    }),
                })
                const json = await res.json()
                if (!res.ok) {
                    if (shouldRelog) {
                        globalAddToast("warning", "Must Re-signin.", `${json.userMessage}`)
                        this._auth = undefined
                        await this.requestAuthCode()
                        return false
                    } else {
                        return false
                    }
                }
                json.expires_at = json.expires_in * 1000 + Date.now()
                this._auth = json as APSAuth
                if (this._auth) {
                    await this.loadUserInfo(this._auth)
                    if (APS.userInfo) {
                        globalAddToast("info", "ADSK Login", `Hello, ${APS.userInfo.givenName}`)
                    }
                }
                return true
            } catch (_e) {
                World.analyticsSystem?.exception("APS Login Failure")
                globalAddToast("error", "Error signing in.", "Please try again.")
                this._auth = undefined
                await this.requestAuthCode()
                return false
            }
        })
    }

    /**
     * Fetches the auth data from Autodesk using the auth code.
     * @param {string} code - The auth code
     * @param {string | null} state - The state returned by the authorize redirect, which must match our pending login
     */
    static async convertAuthToken(code: string, state: string | null) {
        let retryLogin = false

        const pending = this._pendingLogin
        // Each verifier is only good for one redemption
        this._pendingLogin = undefined
        if (!pending || !state || state !== pending.state) {
            console.error("APS login state mismatch")
            World.analyticsSystem?.exception("APS Login Failure: State Mismatch")
            globalAddToast("error", "Error signing in.", "Please try again.")
            return
        }

        try {
            APS.incApsCalls("authentication-v2-token")
            const res = await fetch(ENDPOINT_AUTODESK_AUTHENTICATION_TOKEN, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                },
                body: new URLSearchParams({
                    client_id: CLIENT_ID,
                    grant_type: "authorization_code",
                    code: code,
                    code_verifier: pending.verifier,
                    redirect_uri: CALLBACK_URL,
                }),
            })
            const json = await res.json()
            if (!res.ok) {
                World.analyticsSystem?.exception("APS Login Failure")
                globalAddToast("error", "Error signing in.", `${json.userMessage ?? json.error_description ?? ""}`)
                this._auth = undefined
                return
            }
            const authRes = json as APSAuth
            authRes.expires_at = authRes.expires_in * 1000 + Date.now()
            this._auth = authRes
            console.log("Preloading user info")
            const auth = await this.getAuth()
            if (auth) {
                await this.loadUserInfo(auth)
                if (APS.userInfo) {
                    globalAddToast("info", "ADSK Login", `Hello, ${APS.userInfo.givenName}`)
                }
            } else {
                console.error("Couldn't get auth data.")
                retryLogin = true
            }
        } catch (e) {
            console.error(e)
            retryLogin = true
        }
        if (retryLogin) {
            this._auth = undefined
            World.analyticsSystem?.exception("APS Login Failure")
            globalAddToast("error", "Error signing in.", "Please try again.")
        }
    }

    /**
     * Fetches user information using the auth data. See {@link APSAuth}
     * @param {APSAuth} auth - The auth data
     */
    static async loadUserInfo(auth: APSAuth) {
        console.log("Loading user information")
        try {
            APS.incApsCalls("userinfo")
            const res = await fetch(ENDPOINT_AUTODESK_USERINFO, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: "Bearer " + auth.access_token,
                },
            })
            const json = await res.json()
            if (!res.ok) {
                World.analyticsSystem?.exception("APS Failure: User Info")
                globalAddToast("error", "Error fetching user data.", `${json.userMessage}`)
                this._auth = undefined
                await this.requestAuthCode()
                return
            }
            this.userInfo = {
                name: json.name,
                givenName: json.given_name,
                picture: json.picture,
                email: json.email,
            }
        } catch (e) {
            console.error(e)
            World.analyticsSystem?.exception("APS Login Failure: User Info")
            globalAddToast("error", "Error signing in.", "Please try again.")
            this._auth = undefined
        }
    }
}

export default APS
