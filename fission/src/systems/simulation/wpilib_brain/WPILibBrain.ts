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
            console.log(`SimDevice:\n${JSON.stringify(data, null, '\t')}`)
            break
        default:
            // console.debug('Skipping Message')
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