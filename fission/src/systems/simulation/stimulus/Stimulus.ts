import { mirabuf } from "@/proto/mirabuf"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import { NoraType, NoraTypes } from "../Nora"
import { SimSupplier } from "../wpilib_brain/SimDataFlow"

export enum StimulusType {
    Stim_ChassisAccel = "Stim_ChassisAccel",
    Stim_Encoder = "Stim_Encoder",
    Stim_Unknown = "Stim_Unknown",
}

export type StimulusID = {
    type: StimulusType
    name?: string
    guid: string
}

export function makeStimulusID(constraint: MechanismConstraint): StimulusID {
    let stimulusType: StimulusType = StimulusType.Stim_Unknown
    switch (constraint.constraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
        case JOLT.EConstraintSubType_Slider:
        case JOLT.EConstraintSubType_Vehicle:
            stimulusType = StimulusType.Stim_Encoder
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

    public abstract Update(deltaT: number): void

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
    public abstract DisplayName(): string
}

export default Stimulus
