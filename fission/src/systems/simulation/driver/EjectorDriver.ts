import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import Driver, { type DriverID } from "./Driver"
import { BaseUnit, DerivativeOrder, noraType, num, type NoraValueOf } from "../Nora"

const EJECTOR_TYPE = noraType([num(BaseUnit.NONE, DerivativeOrder.ZERO)])

class EjectorDriver extends Driver<typeof EJECTOR_TYPE> {
    private _value: number

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this._value = 0.0
    }

    public update(_deltaT: number): void {
        this._assembly.ejectorActive = this._value > 0.5
    }

    protected receiveValue([val]: NoraValueOf<typeof EJECTOR_TYPE>): void {
        this._value = val
    }

    public get receiverType() {
        return EJECTOR_TYPE
    }

    public displayName(): string {
        return "Ejector"
    }
}

export default EjectorDriver
