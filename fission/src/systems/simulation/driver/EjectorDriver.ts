import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import { type NoraNumber, NoraTypes } from "../Nora"
import Driver, { type DriverID } from "./Driver"

class EjectorDriver extends Driver {
    public value: number

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this.value = 0.0
    }

    public update(_deltaT: number): void {
        this._assembly.ejectorActive = this.value > 0.5
    }

    public setReceiverValue(val: NoraNumber): void {
        this.value = val
    }
    public getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    public displayName(): string {
        return "Ejector"
    }
}

export default EjectorDriver
