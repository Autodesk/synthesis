import Driver from "../driver/Driver"
import HingeDriver from "../driver/HingeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
import { SimCAN, SimPWM, SimType } from "./WPILibBrain"

// TODO: Averaging is probably not the right solution (if we want large output groups)
// We can keep averaging, but we need a better ui for creating one to one (or just small) output groups
// The issue is that if a drivetrain is one output group, then each driver is given the average of all the motors
// We instead want a system where every driver gets (a) unique motor(s) that control it
// That way a single driver might get the average of two motors or something, if it has two motors to control it
// A system where motors a drivers are visually "linked" with "threads" in the UI would work well in my opinion
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
                d.accelerationDirection = average
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.accelerationDirection = average
            }
            d.Update(_deltaT)
        })
    }
}

export class CANOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, SimType.CANMotor)
    }

    public Update(deltaT: number): void {
        const average =
            this.ports.reduce((sum, port) => {
                const device = SimCAN.GetDeviceWithID(port, SimType.CANMotor)
                return sum + (device?.get("<percentOutput") ?? 0)
            }, 0) / this.ports.length

        this.drivers.forEach(d => {
            if (d instanceof WheelDriver) {
                d.accelerationDirection = average
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.accelerationDirection = average
            }
            d.Update(deltaT)
        })
    }
}
