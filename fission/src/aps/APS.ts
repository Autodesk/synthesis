import { MainHUD_AddToast } from "@/components/MainHUD"
import { Random } from "@/util/Random"

const APS_AUTH_KEY = 'aps_auth'
const APS_USER_INFO_KEY = 'aps_user_info'

export const APS_USER_INFO_UPDATE_EVENT = 'aps_user_info_update'

const delay = 1000
const authCodeTimeout = 200000

const CLIENT_ID = 'GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU'
const CHARACTERS = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'

let lastCall = Date.now()

interface APSAuth {
    access_token: string;
    refresh_token: string;
    expires_in: number;
    token_type: number;
}

interface APSUserInfo {
    name: string;
    picture: string;
    givenName: string;
}

class APS {

    static authCode: string | undefined = undefined

    static get auth(): APSAuth | undefined {
        const res = window.localStorage.getItem(APS_AUTH_KEY)
        try {
            return res ? JSON.parse(res) as APSAuth : undefined
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

    static get userInfo(): APSUserInfo | undefined {
        const res = window.localStorage.getItem(APS_USER_INFO_KEY)

        try {
            return res ? JSON.parse(res) as APSUserInfo : undefined
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

            const [ codeVerifier, codeChallenge ] = await this.codeChallenge();

            const dataParams = [
                ['response_type', 'code'],
                ['client_id', CLIENT_ID],
                ['redirect_uri', callbackUrl],
                ['scope', 'data:read'],
                ['nonce', Date.now().toString()],
                ['prompt', 'login'],
                ['code_challenge', codeChallenge],
                ['code_challenge_method', 'S256']
            ]
            const data = dataParams.map(x => `${x[0]}=${encodeURIComponent(x[1])}`).join('&')

            window.open(`https://developer.api.autodesk.com/authentication/v2/authorize?${data}`)
            
            const searchStart = Date.now()
            const func = () => {
                if (Date.now() - searchStart > authCodeTimeout) {
                    console.debug('Auth Code Timeout')
                    return
                }

                if (this.authCode) {
                    const code = this.authCode;
                    this.authCode = undefined;

                    this.convertAuthToken(code, codeVerifier)
                } else {
                    setTimeout(func, 500)
                }
            }
            func()
        }
    }

    static async convertAuthToken(code: string, codeVerifier: string) {
        const authUrl = import.meta.env.DEV ? `http://localhost:3003/api/aps/code/` : `https://synthesis.autodesk.com/api/aps/code/`
        fetch(`${authUrl}?code=${code}&code_verifier=${codeVerifier}`).then(x => x.json()).then(x => {
            this.auth = x.response as APSAuth;
        }).then(() => {
            console.log('Preloading user info')
            if (this.auth) {
                this.loadUserInfo(this.auth!).then(async () => {
                    if (APS.userInfo) {
                        MainHUD_AddToast('info', 'ADSK Login', `Hello, ${APS.userInfo.givenName}`)
                    }
                })
            }
        })
    }

    static async loadUserInfo(auth: APSAuth) {
        console.log('Loading user information')
        await fetch('https://api.userprofile.autodesk.com/userinfo', {
            method: 'GET',
            headers: {
                'Authorization': auth.access_token
            }
        }).then(x => x.json()).then(x => {

            const info: APSUserInfo = {
                name: x.name,
                givenName: x.given_name,
                picture: x.picture
            }

            this.userInfo = info;
        })
    }

    static async codeChallenge() {
        const codeVerifier = this.genRandomString(50)
    
        const msgBuffer = new TextEncoder().encode(codeVerifier);
        const hashBuffer = await crypto.subtle.digest('SHA-256', msgBuffer);
        
        let str = '';
        (new Uint8Array(hashBuffer)).forEach(x => str = str + String.fromCharCode(x))
        const codeChallenge = btoa(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '')
    
        return [ codeVerifier, codeChallenge ]
    }

    static genRandomString(len: number): string {
        const s: string[] = []
        for (let i = 0; i < len; i++) {
            const c = CHARACTERS.charAt(Math.abs(Random() * 10000) % CHARACTERS.length)
            s.push(c)
        }

        const targetLocation = 'somewhere'

        const fetchLocations = JSON.parse(window.localStorage.getItem('fetchLocations') ?? "{}")
        let existingLocation = fetchLocations[targetLocation]
        if (!existingLocation) {
            // Load mirabuf file, get uuid
            existingLocation = 0
            fetchLocations[targetLocation] = existingLocation
            window.localStorage.setItem('fetchLocation', JSON.stringify(existingLocation))
        }

        // No matter what, existing location will have the uuid of the loaded mirabuf file
        
        return s.join('')
    }
}

Window.prototype.setAuthCode = (code: string) => {
    APS.authCode = code
}

export default APS