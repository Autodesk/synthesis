import { Global_SetAlert } from "./GlobalUtils.tsx"
import type { FusionJoint } from "./joints.ts"
import type { ExporterConfig } from "./types.ts"

declare global {
    interface Window {
        adsk: {
            fusionSendData(action: string, body: string): Promise<string>
        }
        fusionJavaScriptHandler: {
            /* The return value is a string and is passed back to your add-in as the return argument of the sendInfoToHTML method. Returning an empty string is interpreted as an error, so you should always return something in both success and failure cases.*/
            handle(action: string, body: string): string
        }
    }
}

type Empty = Record<PropertyKey, never>

interface InitResponse {
    calculatedMass: number
    options: ExporterConfig
    jointData: FusionJoint[]
    gamepieceData: FusionGamepiece[]
    tagData: FusionBody[]
}

interface Messages {
    selectJoint: [Empty, FusionJoint]
    selectGamepiece: [Empty, FusionGamepiece[]]
    selectBody: [Empty, FusionBody]
    export: [ExporterConfig, Empty]
    save: [ExporterConfig, Empty]
    init: [Empty, InitResponse]
}

const errorMatchers: { text: string; cb: () => void }[] = [
    {
        text: "selections.size() > 0",
        cb: () => {
            Global_SetAlert("info", "Selection cancelled")
        },
    },
]

export async function sendData<A extends keyof Messages>(
    action: A,
    body: Messages[A][0]
): Promise<(Messages[A][1] & { _err?: string }) | undefined> {
    console.log({ action, body: JSON.stringify(body) })
    const resp = await window.adsk.fusionSendData(action, JSON.stringify(body))
    if (resp === "") {
        Global_SetAlert("error", "Fusion did not respond. Try restarting the application")
        return undefined
    }
    try {
        const parsed = JSON.parse(resp) as Messages[A][1] & { _err?: string }
        if (parsed._err !== undefined) {
            const wasHandled = errorMatchers.some(matcher => {
                if (parsed._err?.includes(matcher.text)) {
                    matcher.cb()
                    return true
                }
                return false
            })
            if (!wasHandled) {
                console.error({ action, errorResponse: parsed._err })
            }

            return undefined
        }
        return parsed as Messages[A][1]
    } catch (error) {
        console.error({ error, resp })
        return undefined
    }
}

export async function sendDataAndToast<A extends keyof Messages>(
    action: A,
    body: Messages[A][0] & object,
    sucessMsg: string,
    failureMsg = "Fusion did not respond. Try restarting the application"
): Promise<Messages[A][1] | undefined> {
    const resp = await sendData(action, body)

    if (resp === undefined) {
        Global_SetAlert("error", failureMsg)
    } else {
        Global_SetAlert("info", sucessMsg)
    }
    return resp
}

export interface FusionGamepiece {
    name: string
    occurrenceToken: string
    mass: number
    entityIDs: string[]
}
export async function selectGamepiece(): Promise<FusionGamepiece[] | undefined> {
    if (import.meta.env.DEV && typeof window.adsk === "undefined") {
        return new Promise<FusionGamepiece[]>(resolve => {
            setTimeout(() => {
                const token = Math.random().toString(36).substring(2, 15)
                resolve([
                    {
                        occurrenceToken: `${token}_${Math.random().toString(36).substring(2, 15)}`,
                        name: `Component ${token.substring(0, 2).toUpperCase()}`,
                        mass: Math.round(Math.random() * 100) / 10,
                        entityIDs: [
                            token,
                            Math.random().toString(36).substring(2, 15),
                            Math.random().toString(36).substring(2, 15),
                        ],
                    },
                ])
            }, 2000)
        })
    }
    return await sendData("selectGamepiece", {})
}

export interface FusionBody {
    entityToken: string
    name: string
    componentName: string
}
export async function selectBody(): Promise<FusionBody | undefined> {
    if (import.meta.env.DEV && typeof window.adsk === "undefined") {
        return new Promise<FusionBody>(resolve => {
            setTimeout(() => {
                const token = Math.random().toString(36).substring(2, 15)
                resolve({
                    entityToken: token,
                    name: `Body ${token.substring(0, 2).toUpperCase()}`,
                    componentName: `Component ${token.substring(2, 3).toUpperCase()}`,
                })
            }, 2000)
        })
    }
    return await sendData("selectBody", {})
}
window.fusionJavaScriptHandler = {
    handle: (action, data) => {
        console.log({ action, data })
        return "OK"
    },
}
