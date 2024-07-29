import Driver from "../driver/Driver"
import { SimCAN, SimPWM, SimType } from "./WPILibBrain"

export abstract class SimOutputGroup {
    public name: string
    public ports: number[]
    public drivers: Driver[]
    public type: SimType

    public constructor(name: string, ports: number[], drivers: Driver[], type: SimType) {
        this.name = name
        this.ports = ports
        this.drivers = drivers
        this.type = type
    }

    public abstract Update(deltaT: number): void
}

export class PWMOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, "PWM")
    }

    public Update(_deltaT: number) {
        // let average = 0;
        for (const port of this.ports) {
            const speed = SimPWM.GetSpeed(`${port}`) ?? 0
            // average += speed;
            console.log(port, speed)
        }
        // average /= this.ports.length

        // this.drivers.forEach(d => {
        //     (d as WheelDriver).targetWheelSpeed = average * 40
        //     d.Update(_deltaT)
        // })
    }
}

export class CANOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, "CANMotor")
    }

    public Update(_deltaT: number) {
        for (const port of this.ports) {
            const device = SimCAN.GetDeviceWithID(port, this.type)
            console.log(port, device)
        }
    }
}
