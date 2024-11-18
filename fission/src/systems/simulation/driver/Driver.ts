import { mirabuf } from "@/proto/mirabuf"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"

type DriverType = "hinge" | "wheel" | "slider" | "unknown"

export type DriverID = {
    type: DriverType,
    name?: string,
    guid: string,
}

export function makeDriverID(constraint: MechanismConstraint): DriverID {
    let driverType: DriverType = "unknown"
    switch (constraint.constraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
            driverType = "hinge"
            break
        case JOLT.EConstraintSubType_Slider:
            driverType = "slider"
            break
        case JOLT.EConstraintSubType_Vehicle:
            driverType = "wheel"
            break
    }

    return {
        type: driverType,
        name: constraint.info?.name ?? undefined,
        guid: constraint.info?.GUID ?? "unknown",
    }
}

abstract class Driver {
    private _id: DriverID
    private _info?: mirabuf.IInfo

    constructor(id: DriverID, info?: mirabuf.IInfo) {
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

export enum DriverControlMode {
    Velocity = 0,
    Position = 1,
}

export default Driver
