import Driver from "../driver/Driver"
import HingeDriver from "../driver/HingeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
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
        super(name, ports, drivers, SimType.PWM)
    }

    public Update(_deltaT: number) {
        const average =
            this.ports.reduce((sum, port) => {
                const speed = SimPWM.GetSpeed(`${port}`) ?? 0
                console.debug(port, speed)
                return sum + speed
            }, 0) / this.ports.length

        this.drivers.forEach(d => {
            if (d instanceof WheelDriver) {
                d.targetWheelSpeed = average * 40
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.targetVelocity = average * 40
            }
            d.Update(_deltaT)
        })
    }
}

export class CANOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, SimType.CANMotor)
    }

    // Averaging is probably not the right solution
    public Update(deltaT: number): void {
        const average =
            this.ports.reduce((sum, port) => {
                const device = SimCAN.GetDeviceWithID(port, SimType.CANMotor)
                return sum + device["<percentOutput"] ?? 0
            }, 0) / this.ports.length

        this.drivers.forEach(d => {
            if (d instanceof WheelDriver) {
                d.targetWheelSpeed = average * 40
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.targetVelocity = average * 40
            }
            d.Update(deltaT)
        })
    }
}
