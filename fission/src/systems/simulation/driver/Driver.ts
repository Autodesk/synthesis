import { mirabuf } from "@/proto/mirabuf"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import { NoraType, NoraTypes } from "../Nora"
import { SimReceiver } from "../wpilib_brain/SimDataFlow"

export enum DriverType {
    DRIV_HINGE = "Driv_Hinge",
    DRIV_WHEEL = "Driv_Wheel",
    DRIV_SLIDER = "Driv_Slider",
    DRIV_INTAKE = "Driv_Intake",
    DRIV_EJECTOR = "Driv_Ejector",
    DRIV_UNKNOWN = "Driv_Unknown",
}

export type DriverID = {
    type: DriverType
    name?: string
    guid: string
}

export function makeDriverID(constraint: MechanismConstraint): DriverID {
    let driverType: DriverType = DriverType.DRIV_UNKNOWN
    switch (constraint.primaryConstraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
            driverType = DriverType.DRIV_HINGE
            break
        case JOLT.EConstraintSubType_Slider:
            driverType = DriverType.DRIV_SLIDER
            break
        case JOLT.EConstraintSubType_Vehicle:
            driverType = DriverType.DRIV_WHEEL
            break
    }

    return {
        type: driverType,
        name: constraint.info?.name ?? undefined,
        guid: constraint.info?.GUID ?? "unknown",
    }
}

abstract class Driver implements SimReceiver {
    private _id: DriverID
    private _info?: mirabuf.IInfo

    constructor(id: DriverID, info?: mirabuf.IInfo) {
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

    public abstract setReceiverValue(val: NoraType): void
    public abstract getReceiverType(): NoraTypes
    public abstract displayName(): string
}

export enum DriverControlMode {
    VELOCITY = 0,
    POSITION = 1,
}

export default Driver
