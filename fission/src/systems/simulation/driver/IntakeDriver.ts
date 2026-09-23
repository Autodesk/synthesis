import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import Driver, { type DriverID } from "./Driver"
import { noraType, type NoraValueOf, bool } from "../Nora"

const INTAKE_TYPE = noraType([bool("Intake")])

class IntakeDriver extends Driver<typeof INTAKE_TYPE> {
    public value: boolean

    private _assembly: MirabufSceneObject

    public constructor(id: DriverID, assembly: MirabufSceneObject, info?: mirabuf.IInfo) {
        super(id, info)

        this._assembly = assembly
        this.value = false
    }

    public update(_deltaT: number): void {
        this._assembly.intakeActive = this.value
    }

    protected receiveValue([val]: NoraValueOf<typeof INTAKE_TYPE>): void {
        this.value = val.value
    }

    public get receiverType() {
        return INTAKE_TYPE
    }

    public displayName(): string {
        return "Intake"
    }
}

export default IntakeDriver
