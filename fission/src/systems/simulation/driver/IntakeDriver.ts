import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import Driver, { type DriverID } from "./Driver"
import { BaseUnit, DerivativeOrder, noraType, type NoraValueOf, num, } from "../Nora"

const INTAKE_TYPE = noraType([num(BaseUnit.ANGLE, DerivativeOrder.ZERO)])

class IntakeDriver extends Driver<typeof INTAKE_TYPE> {
    public value: number

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this.value = 0.0
    }

    public update(_deltaT: number): void {
        this._assembly.intakeActive = this.value > 0.5
    }

    protected receiveValue([val]: NoraValueOf<typeof INTAKE_TYPE>): void {
        this.value = val
    }

    public get receiverType() {
        return INTAKE_TYPE
    }

    public displayName(): string {
        return "Intake"
    }
}

export default IntakeDriver
