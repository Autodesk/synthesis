/* eslint-disable @typescript-eslint/no-explicit-any */
import Mechanism from "@/systems/physics/Mechanism";
import Brain from "../Brain";

import WPILibWSWorker from './WPILibWSWorker?worker'

const worker = new WPILibWSWorker()

export const SIM_MAP_UPDATE_EVENT = "ws/sim-map-update"

// abstract class DeviceType {
//     protected _device: string
    
//     constructor(device: string) {
//         this._device = device
//     }

//     public abstract Update(data: any): void
// }

// class Solenoid extends DeviceType {
//     constructor(device: string) {
//         super(device)
//     }

//     public Update(data: any): void {
        
//     }
// }
// const solenoids: Map<string, Solenoid> = new Map()

// class SimDevice extends DeviceType {
//     constructor(device: string) {
//         super(device)
//     }

//     public Update(data: any): void {

//     }
// }
// const simDevices: Map<string, SimDevice> = new Map()

// class SparkMax extends SimDevice {

//     private _sparkMaxId: number;

//     constructor(device: string) {
//         super(device)

//         console.debug('Spark Max Constructed')
        
//         if (device.match(/spark max/i)?.length ?? 0 > 0) {
//             const endPos = device.indexOf(']')
//             const startPos = device.indexOf('[')
//             this._sparkMaxId = parseInt(device.substring(startPos + 1, endPos))
//         } else {
//             throw new Error('Unrecognized Device ID')
//         }
//     }

//     public Update(data: any): void {
//         super.Update(data)

//         Object.entries(data).forEach(x => {
//             // if (x[0].startsWith('<')) {
//             //     console.debug(`${x[0]} -> ${x[1]}`)
//             // }

//             switch (x[0]) {
//                 case '':

//                     break
//                 default:
//                     console.debug(`[${this._sparkMaxId}] ${x[0]} -> ${x[1]}`)
//                     break
//             }
//         })
//     }

//     public SetPosition(val: number) {
//         worker.postMessage(
//             {
//                 command: 'update',
//                 data: { type: 'simdevice', device: this._device, data: { '>Position': val } }
//             }
//         )
//     }
// }

export const pwmMap = new Map<string, any>()
export const simDeviceMap = new Map<string, any>()
export const canMotorMap = new Map<string, any>()

worker.addEventListener('message', (eventData: MessageEvent) => {
    let data: any | undefined;
    try {
        if (typeof(eventData.data) == 'object') {
            data = eventData.data
        } else {
            data = JSON.parse(eventData.data)
        }
    } catch (e) {
        console.warn(`Failed to parse data:\n${JSON.stringify(eventData.data)}`)
    }
    
    if (!data || !data.type) {
        console.log('No data, bailing out')
        return
    }

    // console.debug(data)

    const device = data.device
    const updateData = data.data

    switch (data.type.toLowerCase()) {
        case 'pwm': { // ESLint wants curly brackets apparently. Doesn't like scoped variables with only colon?
            console.debug('pwm')
            const currentData = pwmMap.get(device) ?? {}
            Object.entries(updateData).forEach(kvp => currentData[kvp[0]] = kvp[1])
            pwmMap.set(device, currentData)

            window.dispatchEvent(new Event(SIM_MAP_UPDATE_EVENT))
            break
        }
        case 'solenoid':
            console.debug('solenoid')
            break
        case 'simdevice': {
            console.debug('simdevice')
            const currentData = simDeviceMap.get(device) ?? {}
            Object.entries(updateData).forEach(kvp => currentData[kvp[0]] = kvp[1])
            simDeviceMap.set(device, currentData)

            window.dispatchEvent(new Event(SIM_MAP_UPDATE_EVENT))
            break
        }
        case 'canmotor': {
            console.debug('canmotor')
            const currentData = canMotorMap.get(device) ?? {}
            Object.entries(updateData).forEach(kvp => currentData[kvp[0]] = kvp[1])
            canMotorMap.set(device, currentData)

            window.dispatchEvent(new Event(SIM_MAP_UPDATE_EVENT))
            break
        }
        default:
            // console.debug(`Unrecognized Message:\n${data}`)
            break
    }
})

class WPILibBrain extends Brain {

    constructor(mech: Mechanism) {
        super(mech)
    }

    public Update(_: number): void { }
    
    public Enable(): void {
        worker.postMessage({ command: 'connect' })
    }

    public Disable(): void {
        worker.postMessage({ command: 'disconnect' })
    }

}

export default WPILibBrain