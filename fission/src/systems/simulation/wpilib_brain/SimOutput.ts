import type Driver from "../driver/Driver"
import HingeDriver from "../driver/HingeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
import SimAO from "./sim/SimAO"
import SimCAN from "./sim/SimCAN"
import SimDIO from "./sim/SimDIO"
import SimPWM from "./sim/SimPWM"
import { SimType } from "./WPILibTypes"

export abstract class SimOutput {
    constructor(protected _name: string) {}

    public abstract update(deltaT: number): void

    public get name(): string {
        return this._name
    }
}

export abstract class SimOutputGroup extends SimOutput {
    public ports: number[]
    public drivers: Driver[]
    public type: SimType

    public constructor(name: string, ports: number[], drivers: Driver[], type: SimType) {
        super(name)
        this.ports = ports
        this.drivers = drivers
        this.type = type
    }

    public abstract update(deltaT: number): void
}

export class PWMOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, SimType.PWM)
    }

    public update(deltaT: number) {
        const average =
            this.ports.reduce((sum, port) => {
                const speed = SimPWM.getSpeed(`${port}`) ?? 0
                return sum + speed
            }, 0) / this.ports.length

        this.drivers.forEach(d => {
            if (d instanceof WheelDriver) {
                d.accelerationDirection = average
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.accelerationDirection = average
            }
            d.update(deltaT)
        })
    }
}

export class CANOutputGroup extends SimOutputGroup {
    public constructor(name: string, ports: number[], drivers: Driver[]) {
        super(name, ports, drivers, SimType.CAN_MOTOR)
    }

    public update(deltaT: number): void {
        const average =
            this.ports.reduce((sum, port) => {
                const device = SimCAN.getDeviceWithID(port, SimType.CAN_MOTOR)
                return sum + ((device?.get("<percentOutput") as number | undefined) ?? 0)
            }, 0) / this.ports.length

        this.drivers.forEach(d => {
            if (d instanceof WheelDriver) {
                d.accelerationDirection = average
            } else if (d instanceof HingeDriver || d instanceof SliderDriver) {
                d.accelerationDirection = average
            }
            d.update(deltaT)
        })
    }
}

export class SimDigitalOutput extends SimOutput {
    /**
     * Creates a Simulation Digital Input/Output object.
     *
     * @param device Device ID
     */
    constructor(name: string) {
        super(name)
    }

    public setValue(value: boolean) {
        SimDIO.setValue(this._name, value)
    }

    public getValue(): boolean {
        return SimDIO.getValue(this._name)
    }

    public update(_deltaT: number) {}
}

export class SimAnalogOutput extends SimOutput {
    public constructor(name: string) {
        super(name)
    }

    public getVoltage(): number {
        return SimAO.getVoltage(this._name)
    }

    public update(_deltaT: number) {}
}
