import { MainHUD_AddToast } from "@/components/MainHUD"
import { Random } from "@/util/Random"
import { getCookie, removeCookie, setCookie } from "typescript-cookie"

let lastCall = Date.now()

const delay = 1000
const authCodeTimeout = 200000

const CLIENT_ID = 'GCxaewcLjsYlK8ud7Ka9AKf9dPwMR3e4GlybyfhAK2zvl3tU'

const CHARACTERS = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'

interface APSAuth {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
}

interface APSUserInfo {
    name: string;
    picture: string;
    givenName: string;
}

class APS {

    static authCode: string | undefined = undefined
    static auth: APSAuth | undefined = undefined
    static latestUserInfo: APSUserInfo | undefined = undefined

    static async removeCookieTest() {
        setCookie('code', 'hello')
        if (getCookie('code')) {
            removeCookie('code')
            if (getCookie('code')) {
                console.log('Failed to remove cookie')
            } else {
                console.log('Cookie Test Passed')
            }
        } else {
            console.log('Failed to set cookie')
        }
    }

    static async requestAuthCode() {
        if (Date.now() - lastCall > delay) {
            lastCall = Date.now()
            const callbackUrl = import.meta.env.DEV ? `http://localhost:3000${import.meta.env.BASE_URL}` : `https://synthesis.autodesk.com${import.meta.env.BASE_URL}`
            console.debug(`Setting callback url to '${callbackUrl}'`)
            removeCookie('code')

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
            this.auth = { accessToken: x.response.access_token, expiresIn: x.response.expires_in, refreshToken: x.response.refresh_token }
            console.log(x)
        }).then(() => {
            if (this.auth)
                this.getUserData(this.auth!)
        })
    }

    static async getUserData(auth: APSAuth) {
        fetch('https://api.userprofile.autodesk.com/userinfo', {
            method: 'GET',
            headers: {
                'Authorization': auth.accessToken
            }
        }).then(x => x.json()).then(x => {

            this.latestUserInfo = {
                name: x.name,
                givenName: x.given_name,
                picture: x.picture
            }

            MainHUD_AddToast('info', 'ADSK Login', `Hello, ${this.latestUserInfo.givenName}`)
            console.log(`Hello, ${this.latestUserInfo.givenName}`)
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
        
        return s.join('')
    }
}

Window.prototype.setAuthCode = (code: string) => {
    APS.authCode = code
}

export default APS