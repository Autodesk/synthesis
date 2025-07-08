import { type ExporterConfig, JointType } from "./types.ts"

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
interface Messages {
    selectJoint: [Empty, FusionJoint]
    selectGamepiece: [Empty, FusionGamepiece]
    export: [ExporterConfig, Empty]
    save: [ExporterConfig, Empty]
    init: [
        Empty,
        { calculatedMass: number; options: ExporterConfig; jointData: FusionJoint[]; gamepieceData: FusionGamepiece[] },
    ]
}

export async function sendData<A extends keyof Messages>(
    action: A,
    body: Messages[A][0] & object
): Promise<Messages[A][1] | undefined> {
    console.log({ action, body: JSON.stringify(body) })
    const resp = await window.adsk.fusionSendData(action, JSON.stringify(body))

    try {
        return JSON.parse(resp)
    } catch (error) {
        console.error({ error, resp })
        return undefined
    }
}

export interface FusionJoint {
    entityToken: string
    name: string
    jointType: JointType
}
export async function selectJoint(): Promise<FusionJoint | undefined> {
    if (!window.adsk) {
        return new Promise<any>(resolve => {
            setTimeout(() => {
                const jointType = Math.round(1 + Math.random())
                const token = Math.random().toString(36).substring(2, 15)
                resolve({
                    entityToken: token,
                    name: (jointType == 1 ? "Revolute" : "Slider") + " " + token.substring(0, 2).toUpperCase(),
                    jointType: jointType,
                })
            }, 2000)
        })
    }
    return await sendData("selectJoint", {})
}

export interface FusionGamepiece {
    name: string
    occurrenceToken: string
    mass: number
    entityIDs: string[]
}
export async function selectGamepiece(): Promise<FusionGamepiece | undefined> {
    if (!window.adsk) {
        return new Promise<FusionGamepiece>(resolve => {
            setTimeout(() => {
                const token = Math.random().toString(36).substring(2, 15)
                resolve({
                    occurrenceToken: token + "_" + Math.random().toString(36).substring(2, 15),
                    name: "Component " + token.substring(0, 2).toUpperCase(),
                    mass: Math.round(Math.random() * 100) / 10,
                    entityIDs: [
                        token,
                        Math.random().toString(36).substring(2, 15),
                        Math.random().toString(36).substring(2, 15),
                    ],
                })
            }, 2000)
        })
    }
    return await sendData("selectGamepiece", {})
}

window.fusionJavaScriptHandler = {
    handle: function (action, data) {
        console.log({ action, data })
        return "OK"
    },
}
