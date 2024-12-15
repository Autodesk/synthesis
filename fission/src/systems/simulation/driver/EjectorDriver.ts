import { mirabuf } from "@/proto/mirabuf"
import { NoraNumber, NoraTypes } from "../Nora"
import Driver, { DriverID } from "./Driver"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

class EjectorDriver extends Driver {
    public value: number

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this.value = 0.0
    }

    public Update(_deltaT: number): void {
        this._assembly.ejectorActive = this.value > 0.5
    }

    public setReceiverValue(val: NoraNumber): void {
        this.value = val
    }
    public getReceiverType(): NoraTypes {
        return NoraTypes.Number
    }
    public DisplayName(): string {
        return "Ejector"
    }
}

export default EjectorDriver
