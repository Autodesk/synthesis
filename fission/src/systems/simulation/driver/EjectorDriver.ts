import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import Driver, { type DriverID } from "./Driver"
import { bool, noraType, type NoraValueOf } from "../Nora"

export const EJECTOR_TYPE = noraType([bool("Eject")])

class EjectorDriver extends Driver<typeof EJECTOR_TYPE> {
    private _value: boolean

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this._value = false
    }

    public update(_deltaT: number): void {
        this._assembly.ejectorActive = this._value
    }

    protected receiveValue([val]: NoraValueOf<typeof EJECTOR_TYPE>): void {
        this._value = val.value
    }

    public get receiverType() {
        return EJECTOR_TYPE
    }

    public displayName(): string {
        return "Ejector"
    }
}

export default EjectorDriver
