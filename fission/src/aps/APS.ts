import { Random } from "@/util/Random"

const APS_AUTH_KEY = "aps_auth"
const APS_USER_INFO_KEY = "aps_user_info"

export const APS_USER_INFO_UPDATE_EVENT = "aps_user_info_update"

const delay = 1000
const authCodeTimeout = 200000

const CLIENT_ID = 'GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU'

let lastCall = Date.now()

interface APSAuth {
    access_token: string;
    refresh_token: string;
    expires_in: number;
    expires_at: number;
    token_type: number;
}

interface APSUserInfo {
    name: string
    picture: string
    givenName: string
}

class APS {
    static authCode: string | undefined = undefined

    static get auth(): APSAuth | undefined {
        const res = window.localStorage.getItem(APS_AUTH_KEY)
        try {
            return res ? JSON.parse(res) : undefined;
        } catch (e) {
            console.warn(`Failed to parse stored APS auth data: ${e}`)
            return undefined
        }
    }

    static set auth(a: APSAuth | undefined) {
        window.localStorage.removeItem(APS_AUTH_KEY)
        if (a) {
            window.localStorage.setItem(APS_AUTH_KEY, JSON.stringify(a))
        }
        this.userInfo = undefined
    }

    static async getAuth(): Promise<APSAuth | undefined> {
        const auth = this.auth;
        if (!auth) return new Promise((resolve) => resolve(undefined));

        if (Date.now() > auth.expires_at) {
            await this.refreshAuthToken(auth.refresh_token);
        }
        return new Promise((resolve) => resolve(this.auth));
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
        if (Date.now() - lastCall > delay) {
            lastCall = Date.now()
            const callbackUrl = import.meta.env.DEV
                ? `http://localhost:3000${import.meta.env.BASE_URL}`
                : `https://synthesis.autodesk.com${import.meta.env.BASE_URL}`

            const challenge = await this.codeChallenge();

            const params = new URLSearchParams({
                'response_type': 'code',
                'client_id': CLIENT_ID,
                'redirect_uri': callbackUrl,
                'scope': 'data:read',
                'nonce': Date.now().toString(),
                'prompt': 'login',
                'code_challenge': challenge,
                'code_challenge_method': 'S256'
            })

            window.open(`https://developer.api.autodesk.com/authentication/v2/authorize?${params.toString()}`)

            const searchStart = Date.now()
            const func = () => {
                if (Date.now() - searchStart > authCodeTimeout) {
                    console.debug("Auth Code Timeout")
                    return
                }

                if (this.authCode) {
                    const code = this.authCode
                    this.authCode = undefined

                    this.convertAuthToken(code)
                } else {
                    setTimeout(func, 500)
                }
            }
            func()
        }
    }

    static async refreshAuthToken(refresh_token: string) {
        let now
        if ((now = Date.now()) - lastCall > delay) {
            lastCall = now
            const res = await fetch('https://developer.api.autodesk.com/authentication/v2/token', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({
                    'client_id': CLIENT_ID,
                    'grant_type': 'refresh_token',
                    'refresh_token': refresh_token,
                    'scope': 'data:read',
                })
            }).then(res => res.json());
            res.expires_at = res.expires_in + Date.now()
            this.auth = res as APSAuth
        }
    }

    static async convertAuthToken(code: string) {
        const authUrl = import.meta.env.DEV ? `http://localhost:3003/api/aps/code/` : `https://synthesis.autodesk.com/api/aps/code/`
        fetch(`${authUrl}?code=${code}`).then(x => x.json()).then(x => {
            const auth = x.response as APSAuth;
            auth.expires_at = auth.expires_in + Date.now()
            this.auth = auth
        }).then(async () => {
            console.log('Preloading user info')
            const auth = await this.getAuth();
            if (auth) {
                this.loadUserInfo(auth).then(async () => {
                    if (APS.userInfo) {
                        MainHUD_AddToast('info', 'ADSK Login', `Hello, ${APS.userInfo.givenName}`)
                    }
                })
            }
        })
    }

    static async loadUserInfo(auth: APSAuth) {
        console.log("Loading user information")
        await fetch("https://api.userprofile.autodesk.com/userinfo", {
            method: "GET",
            headers: {
                Authorization: auth.access_token,
            },
        })
            .then(x => x.json())
            .then(x => {
                const info: APSUserInfo = {
                    name: x.name,
                    givenName: x.given_name,
                    picture: x.picture,
                }

                this.userInfo = info
            })
    }

    static async codeChallenge() {
        const endpoint = import.meta.env.DEV ? 'http://localhost:3003/api/aps/challenge/' : 'https://synthesis.autodesk.com/api/aps/challenge/';
        const res = await fetch(endpoint)
        const json = await res.json()
        return json['challenge']
    }
}

Window.prototype.setAuthCode = (code: string) => {
    APS.authCode = code
}

export default APS
