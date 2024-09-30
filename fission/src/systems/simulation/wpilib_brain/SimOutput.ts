import Driver from "../driver/Driver"
import HingeDriver from "../driver/HingeDriver"
import SliderDriver from "../driver/SliderDriver"
import WheelDriver from "../driver/WheelDriver"
import { SimAO, SimCAN, SimDIO, SimPWM, SimType } from "./WPILibBrain"

export abstract class SimOutput {
    constructor(protected _name: string) {}

    public abstract Update(deltaT: number): void

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
                return sum + (device?.get("<percentOutput") as number | undefined ?? 0)
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

export class SimDigitalOutput extends SimOutput {
    /**
     * Creates a Simulation Digital Input/Output object.
     *
     * @param device Device ID
     */
    constructor(name: string) {
        super(name)
    }

    public SetValue(value: boolean) {
        SimDIO.SetValue(this._name, value)
    }

    public GetValue(): boolean {
        return SimDIO.GetValue(this._name)
    }

    public Update(_deltaT: number) {}
}

export class SimAnalogOutput extends SimOutput {
    public constructor(name: string) {
        super(name)
    }

    public GetVoltage(): number {
        return SimAO.GetVoltage(this._name)
    }

    public Update(_deltaT: number) {}
}
