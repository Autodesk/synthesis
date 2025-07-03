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
export {}

console.log("TEST")
interface Messages {
    selectJoint: [undefined, FusionJoint]
    selectGamepiece: [undefined, FusionGamepiece]
    export: [ExporterConfig, undefined]
    init: [undefined, { mass: number }]
}
export async function sendData<A extends keyof Messages>(
    action: A,
    body: Messages[A][0]
): Promise<Messages[A][1] | undefined> {
    const resp = await window.adsk.fusionSendData(action, JSON.stringify(body))
    try {
        return JSON.parse(resp)
    } catch (error) {
        console.error({ error, resp })
        return undefined
    }
}
interface FusionJoint {
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
    return await sendData("selectJoint", undefined)
}

export interface FusionGamepiece {
    name: string
    guid: string
    entityToken: string
    mass: number
    entityIDs: string[]
}
export async function selectGamepiece(): Promise<FusionGamepiece | undefined> {
    if (!window.adsk) {
        return new Promise<any>(resolve => {
            setTimeout(() => {
                const token = Math.random().toString(36).substring(2, 15)
                resolve({
                    entityToken: token,
                    name: "Component " + token.substring(0, 2).toUpperCase(),
                    mass: Math.round(Math.random() * 100) / 10,
                    guid: token + "_" + Math.random().toString(36).substring(2, 15),
                    entityIDs: [
                        token,
                        Math.random().toString(36).substring(2, 15),
                        Math.random().toString(36).substring(2, 15),
                    ],
                })
            }, 2000)
        })
    }
    return await sendData("selectGamepiece", undefined)
}

function updateMessage(messageString: string) {
    // Message is sent from the add-in as a JSON string.
    const messageData = JSON.parse(messageString)

    // Update a paragraph with the data passed in.
    document.getElementById("fusionMessage")!.innerHTML =
        `<b>Your text</b>: ${messageData.myText} <br/>` +
        `<b>Your expression</b>: ${messageData.myExpression} <br/>` +
        `<b>Your value</b>: ${messageData.myValue}`
}

window.fusionJavaScriptHandler = {
    handle: function (action, data) {
        try {
            if (action === "updateMessage") {
                updateMessage(data)
            } else if (action === "debugger") {
                debugger
            } else {
                return `Unexpected command type: ${action}`
            }
        } catch (e) {
            console.log(e)
            console.log(`Exception caught with command: ${action}, data: ${data}`)
        }
        return "OK"
    },
}
