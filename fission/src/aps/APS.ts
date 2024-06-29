import { MainHUD_AddToast } from "@/ui/components/MainHUD"
import { Mutex } from "async-mutex"

const APS_AUTH_KEY = "aps_auth"
const APS_USER_INFO_KEY = "aps_user_info"

export const APS_USER_INFO_UPDATE_EVENT = "aps_user_info_update"

const CLIENT_ID = "GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU"

interface APSAuth {
    access_token: string
    refresh_token: string
    expires_in: number
    expires_at: number
    token_type: number
}

interface APSUserInfo {
    name: string
    picture: string
    givenName: string
    email: string
}

class APS {
    static authCode: string | undefined = undefined
    static requestMutex: Mutex = new Mutex()

    private static get auth(): APSAuth | undefined {
        const res = window.localStorage.getItem(APS_AUTH_KEY)
        try {
            return res ? JSON.parse(res) : undefined
        } catch (e) {
            console.warn(`Failed to parse stored APS auth data: ${e}`)
            return undefined
        }
    }

    private static set auth(a: APSAuth | undefined) {
        window.localStorage.removeItem(APS_AUTH_KEY)
        if (a) {
            window.localStorage.setItem(APS_AUTH_KEY, JSON.stringify(a))
        }
        this.userInfo = undefined
    }

    static async getAuth(): Promise<APSAuth | undefined> {
        const auth = this.auth
        if (!auth) return undefined

        if (Date.now() > auth.expires_at) {
            await this.refreshAuthToken(auth.refresh_token)
        }
        return this.auth
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

    static async logout() {
        this.auth = undefined
    }

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

                const url = `https://developer.api.autodesk.com/authentication/v2/authorize?${params.toString()}`

                window.open(url, "_self")
            } catch (e) {
                console.error(e)
                MainHUD_AddToast("error", "Error signing in.", "Please try again.")
            }
        })
    }

    static async refreshAuthToken(refresh_token: string) {
        await this.requestMutex.runExclusive(async () => {
            try {
                const res = await fetch("https://developer.api.autodesk.com/authentication/v2/token", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                    },
                    body: new URLSearchParams({
                        client_id: CLIENT_ID,
                        grant_type: "refresh_token",
                        refresh_token: refresh_token,
                        scope: "data:read",
                    }),
                })
                const json = await res.json()
                if (!res.ok) {
                    MainHUD_AddToast("error", "Error signing in.", json.userMessage)
                    this.auth = undefined
                    await this.requestAuthCode()
                    return
                }
                json.expires_at = json.expires_in + Date.now()
                this.auth = json as APSAuth
                if (this.auth) {
                    await this.loadUserInfo(this.auth)
                    if (APS.userInfo) {
                        MainHUD_AddToast("info", "ADSK Login", `Hello, ${APS.userInfo.givenName}`)
                    }
                }
            } catch (e) {
                MainHUD_AddToast("error", "Error signing in.", "Please try again.")
                this.auth = undefined
                await this.requestAuthCode()
            }
        })
    }

    static async convertAuthToken(code: string) {
        const authUrl = import.meta.env.DEV
            ? `http://localhost:3003/api/aps/code/`
            : `https://synthesis.autodesk.com/api/aps/code/`
        let retry_login = false
        try {
            const res = await fetch(`${authUrl}?code=${code}`)
            const json = await res.json()
            if (!res.ok) {
                MainHUD_AddToast("error", "Error signing in.", json.userMessage)
                this.auth = undefined
                return
            }
            const auth_res = json.response as APSAuth
            auth_res.expires_at = auth_res.expires_in + Date.now()
            this.auth = auth_res
            console.log("Preloading user info")
            const auth = await this.getAuth()
            if (auth) {
                await this.loadUserInfo(auth)
                if (APS.userInfo) {
                    MainHUD_AddToast("info", "ADSK Login", `Hello, ${APS.userInfo.givenName}`)
                }
            } else {
                console.error("Couldn't get auth data.")
                retry_login = true
            }
        } catch (e) {
            console.error(e)
            retry_login = true
        }
        if (retry_login) {
            this.auth = undefined
            MainHUD_AddToast("error", "Error signing in.", "Please try again.")
        }
    }

    static async loadUserInfo(auth: APSAuth) {
        console.log("Loading user information")
        try {
            const res = await fetch("https://api.userprofile.autodesk.com/userinfo", {
                method: "GET",
                headers: {
                    Authorization: auth.access_token,
                },
            })
            const json = await res.json()
            if (!res.ok) {
                MainHUD_AddToast("error", "Error fetching user data.", json.userMessage)
                this.auth = undefined
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
            MainHUD_AddToast("error", "Error signing in.", "Please try again.")
            this.auth = undefined
        }
    }

    static async codeChallenge() {
        try {
            const endpoint = import.meta.env.DEV
                ? "http://localhost:3003/api/aps/challenge/"
                : "https://synthesis.autodesk.com/api/aps/challenge/"
            const res = await fetch(endpoint)
            const json = await res.json()
            return json["challenge"]
        } catch (e) {
            console.error(e)
            MainHUD_AddToast("error", "Error signing in.", "Please try again.")
        }
    }
}

Window.prototype.setAuthCode = (code: string) => {
    APS.authCode = code
}

export default APS
