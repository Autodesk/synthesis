import { Mutex } from "async-mutex"
import World from "@/systems/World"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

const APS_AUTH_KEY = "aps_auth"
const APS_USER_INFO_KEY = "aps_user_info"

export const APS_USER_INFO_UPDATE_EVENT = "aps_user_info_update"

const CLIENT_ID = "GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU"

const ENDPOINT_SYNTHESIS_CODE = `/api/aps/code`
export const ENDPOINT_SYNTHESIS_CHALLENGE = `/api/aps/challenge`

const ENDPOINT_AUTODESK_AUTHENTICATION_AUTHORIZE = "https://developer.api.autodesk.com/authentication/v2/authorize"
const ENDPOINT_AUTODESK_AUTHENTICATION_TOKEN = "https://developer.api.autodesk.com/authentication/v2/token"
const ENDPOINT_AUTODESK_AUTHENTICATION_REVOKE = "https://developer.api.autodesk.com/authentication/v2/revoke"
const ENDPOINT_AUTODESK_USERINFO = "https://api.userprofile.autodesk.com/userinfo"

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

// biome-ignore lint/style/useNamingConvention: this is pascal case
class APS {
    static authCode: string | undefined = undefined
    static requestMutex: Mutex = new Mutex()

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

        document.dispatchEvent(new Event(APS_USER_INFO_UPDATE_EVENT))
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
            const callbackUrl = import.meta.env.DEV
                ? `http://localhost:3000${import.meta.env.BASE_URL}`
                : `https://synthesis.autodesk.com${import.meta.env.BASE_URL}`

            try {
                const challenge = await this.codeChallenge()

                const params = new URLSearchParams({
                    response_type: "code",
                    client_id: CLIENT_ID,
                    redirect_uri: callbackUrl,
                    scope: "data:read",
                    nonce: Date.now().toString(),
                    prompt: "login",
                    code_challenge: challenge,
                    code_challenge_method: "S256",
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
                        scope: "data:read",
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
     */
    static async convertAuthToken(code: string) {
        let retryLogin = false

        const callbackUrl = import.meta.env.DEV
            ? `http://localhost:3000${import.meta.env.BASE_URL}`
            : `https://synthesis.autodesk.com${import.meta.env.BASE_URL}`

        try {
            const res = await fetch(
                `${ENDPOINT_SYNTHESIS_CODE}?code=${code}&redirect_uri=${encodeURIComponent(callbackUrl)}`
            )
            const json = await res.json()
            if (!res.ok) {
                World.analyticsSystem?.exception("APS Login Failure")
                globalAddToast("error", "Error signing in.", `${json.userMessage}`)
                this._auth = undefined
                return
            }
            const authRes = json.response as APSAuth
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
                    Authorization: auth.access_token,
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
            const info: APSUserInfo = {
                name: json.name,
                givenName: json.given_name,
                picture: json.picture,
                email: json.email,
            }

            this.userInfo = info
        } catch (e) {
            console.error(e)
            World.analyticsSystem?.exception("APS Login Failure: User Info")
            globalAddToast("error", "Error signing in.", "Please try again.")
            this._auth = undefined
        }
    }

    /**
     * Fetches the code challenge from our server for requesting the auth code.
     */
    static async codeChallenge() {
        try {
            const res = await fetch(ENDPOINT_SYNTHESIS_CHALLENGE)
            const json = await res.json()
            return json["challenge"]
        } catch (e) {
            console.error(e)
            World.analyticsSystem?.exception("APS Login Failure: Code Challenge")
            globalAddToast("error", "Error signing in.", "Please try again.")
        }
    }
}

export default APS
