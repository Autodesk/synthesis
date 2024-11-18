import { mirabuf } from "@/proto/mirabuf"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"

type StimulusType = "chassis" | "encoder" | "unknown"

export type StimulusID = {
    type: StimulusType,
    name?: string,
    guid: string,
}

export function makeStimulusID(constraint: MechanismConstraint): StimulusID {
    let stimulusType: StimulusType = "unknown"
    switch (constraint.constraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
        case JOLT.EConstraintSubType_Slider:
        case JOLT.EConstraintSubType_Vehicle:
            stimulusType = "encoder"
            break
    }

    return {
        type: stimulusType,
        name: constraint.info?.name ?? undefined,
        guid: constraint.info?.GUID ?? "unknown",
    }
}

abstract class Stimulus {
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

    public get info() {
        return this._info
    }

    public abstract DisplayName(): string
}

export default Stimulus
