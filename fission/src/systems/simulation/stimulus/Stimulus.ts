import type { mirabuf } from "@/proto/mirabuf"
import type { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import type { SimSupplier } from "../wpilib_brain/SimDataFlow"
import { type NoraValueOf, serializeNoraType, valueMatchesType, type NoraType, type NoraValue } from "../Nora"

export enum StimulusType {
    STIM_ACCEL = "Stim_Accel",
    STIM_GYRO = "Stim_Gyro",
    STIM_ENCODER = "Stim_Encoder",
    STIM_UNKNOWN = "Stim_Unknown",
}

export type StimulusID = {
    type: StimulusType
    name?: string
    guid: string
}

export function makeStimulusID(constraint: MechanismConstraint): StimulusID {
    let stimulusType: StimulusType = StimulusType.STIM_UNKNOWN
    switch (constraint.primaryConstraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
        case JOLT.EConstraintSubType_Slider:
        case JOLT.EConstraintSubType_Vehicle:
            stimulusType = StimulusType.STIM_ENCODER
            break
    }

    return {
        type: stimulusType,
        name: constraint.info?.name ?? undefined,
        guid: constraint.info?.GUID ?? "unknown",
    }
}

abstract class Stimulus<T extends NoraType = NoraType> implements SimSupplier {
    private _id: StimulusID
    private _info?: mirabuf.IInfo

    constructor(id: StimulusID, info?: mirabuf.IInfo) {
        this._id = id
        this._info = info
    }

    public abstract update(deltaT: number): void

    public get id() {
        return this._id
    }

    public get idStr() {
        return JSON.stringify(this._id)
    }

    public get info() {
        return this._info
    }

    public abstract get supplierType(): T

    public getSupplierValue(): NoraValue {
        const val = this.supplyValue()

        if (!valueMatchesType(val, this.supplierType))
            throw new Error(
                `${this.displayName()}: supplied value of length ${val.length} does not match supplier type ${serializeNoraType(this.supplierType)}`
            )
        
        return val
    }

    protected abstract supplyValue(): NoraValueOf<T>

    public abstract displayName(): string
}

export default Stimulus
