import { mirabuf } from "@/proto/mirabuf"

abstract class Driver {
    private _info?: mirabuf.IInfo

    constructor(info?: mirabuf.IInfo) {
        this._info = info
    }

    public abstract Update(deltaT: number): void

    public get info() {
        return this._info
    }
}

export enum DriverControlMode {
    Velocity = 0,
    Position = 1,
}

export default Driver
