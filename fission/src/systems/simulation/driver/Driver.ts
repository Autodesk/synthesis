import { mirabuf } from "@/proto/mirabuf"
import { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import { NoraType, NoraTypes } from "../Nora"
import { SimReceiver } from "../wpilib_brain/SimDataFlow"

export enum DriverType {
    Driv_Hinge = "Driv_Hinge",
    Driv_Wheel = "Driv_Wheel",
    Driv_Slider = "Driv_Slider",
    Driv_Intake = "Driv_Intake",
    Driv_Ejector = "Driv_Ejector",
    Driv_Unknown = "Driv_Unknown",
}

export type DriverID = {
    type: DriverType
    name?: string
    guid: string
}

export function makeDriverID(constraint: MechanismConstraint): DriverID {
    let driverType: DriverType = DriverType.Driv_Unknown
    switch (constraint.constraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
            driverType = DriverType.Driv_Hinge
            break
        case JOLT.EConstraintSubType_Slider:
            driverType = DriverType.Driv_Slider
            break
        case JOLT.EConstraintSubType_Vehicle:
            driverType = DriverType.Driv_Wheel
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

    public abstract setReceiverValue(val: NoraType): void
    public abstract getReceiverType(): NoraTypes
    public abstract DisplayName(): string
}

export enum DriverControlMode {
    Velocity = 0,
    Position = 1,
}

export default Driver
