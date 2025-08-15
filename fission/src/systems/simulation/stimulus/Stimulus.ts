import type { mirabuf } from "@/proto/mirabuf"
import type { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import type { NoraType, NoraTypes } from "../Nora"
import type { SimSupplier } from "../wpilib_brain/SimDataFlow"

export enum StimulusType {
    STIM_CHASSIS_ACCEL = "Stim_ChassisAccel",
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

abstract class Stimulus implements SimSupplier {
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

    public abstract getSupplierType(): NoraTypes
    public abstract getSupplierValue(): NoraType
    public abstract displayName(): string
}

export default Stimulus
