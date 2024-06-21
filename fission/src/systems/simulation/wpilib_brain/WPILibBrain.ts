/* eslint-disable @typescript-eslint/no-explicit-any */
import Mechanism from "@/systems/physics/Mechanism";
import Brain from "../Brain";

import WPILibWSWorker from './WPILibWSWorker?worker'

const worker = new WPILibWSWorker()

abstract class DeviceType {
    protected _device: string
    
    constructor(device: string) {
        this._device = device
    }

    public abstract Update(data: any): void
}

class Solenoid extends DeviceType {
    constructor(device: string) {
        super(device)
    }

    public Update(data: any): void {
        
    }
}
const solenoids: Map<string, Solenoid> = new Map()

class SimDevice extends DeviceType {
    constructor(device: string) {
        super(device)
    }

    public Update(data: any): void {

    }
}
const simDevices: Map<string, SimDevice> = new Map()

class SparkMax extends SimDevice {

    private _sparkMaxId: number;

    constructor(device: string) {
        super(device)

        console.debug('Spark Max Constructed')
        
        if (device.match(/spark max/i)?.length ?? 0 > 0) {
            const endPos = device.indexOf(']')
            const startPos = device.indexOf('[')
            this._sparkMaxId = parseInt(device.substring(startPos + 1, endPos))
        } else {
            throw new Error('Unrecognized Device ID')
        }
    }

    public Update(data: any): void {
        super.Update(data)

        Object.entries(data).forEach(x => {
            // if (x[0].startsWith('<')) {
            //     console.debug(`${x[0]} -> ${x[1]}`)
            // }

            switch (x[0]) {
                case '':

                    break
                default:
                    console.debug(`[${this._sparkMaxId}] ${x[0]} -> ${x[1]}`)
                    break
            }
        })
    }

    public SetPosition(val: number) {
        worker.postMessage(
            {
                command: 'update',
                data: { type: 'simdevice', device: this._device, data: { '>Position': val } }
            }
        )
    }
}

worker.addEventListener('message', (eventData: MessageEvent) => {
    let data: any | undefined;
    try {
        data = JSON.parse(eventData.data)
    } catch (e) {
        console.warn(`Failed to parse data:\n${JSON.stringify(eventData.data)}`)
    }
    
    if (!data) {
        // console.log('No data, bailing out')
        return
    }

    switch (data.type.toLowerCase()) {
        case 'solenoid':
            if (!solenoids.has(data.device)) {
                solenoids.set(data.device, new Solenoid(data.device))
            }
            solenoids.get(data.device)?.Update(data.data)
            break
        case 'simdevice':
            // console.debug(`SimDevice:\n${JSON.stringify(data, null, '\t')}`)
            if (!simDevices.has(data.device)) {
                if (data.device.match(/spark max/i)) {
                    simDevices.set(data.device, new SparkMax(data.device))
                } else {
                    simDevices.set(data.device, new SimDevice(data.device))
                }
            }
            simDevices.get(data.device)?.Update(data.data)
            break
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