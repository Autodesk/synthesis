import { mirabuf } from "@/proto/mirabuf"

abstract class Stimulus {
    private _info?: mirabuf.IInfo

    constructor(info?: mirabuf.IInfo) {
        this._info = info
    }

    public abstract Update(deltaT: number): void

    public get info() {
        return this._info
    }
}

export default Stimulus
