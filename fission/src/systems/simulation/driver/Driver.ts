import type { mirabuf } from "@/proto/mirabuf"
import type { MechanismConstraint } from "@/systems/physics/Mechanism"
import JOLT from "@/util/loading/JoltSyncLoader"
import type { SimReceiver } from "../wpilib_brain/SimDataFlow"
import { type NoraType, type NoraValue, valueMatchesType, type NoraValueOf, serializeNoraType } from "../Nora"

export enum DriverType {
    HINGE = "Driv_Hinge",
    WHEEL = "Driv_Wheel",
    SLIDER = "Driv_Slider",
    INTAKE = "Driv_Intake",
    EJECTOR = "Driv_Ejector",
    UNKNOWN = "Driv_Unknown",
}

export type DriverID = {
    type: DriverType
    name?: string
    guid: string
}

export function makeDriverID(constraint: MechanismConstraint): DriverID {
    let driverType: DriverType = DriverType.UNKNOWN
    switch (constraint.primaryConstraint.GetSubType()) {
        case JOLT.EConstraintSubType_Hinge:
            driverType = DriverType.HINGE
            break
        case JOLT.EConstraintSubType_Slider:
            driverType = DriverType.SLIDER
            break
        case JOLT.EConstraintSubType_Vehicle:
            driverType = DriverType.WHEEL
            break
    }

    return {
        type: driverType,
        name: constraint.info?.name ?? undefined,
        guid: constraint.info?.GUID ?? "unknown",
    }
}

abstract class Driver<T extends NoraType = NoraType> implements SimReceiver {
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

    public abstract get receiverType(): T

    public setReceiverValue(val: NoraValue): void {
        if (!valueMatchesType(val, this.receiverType))
            throw new Error(
                `${this.displayName()}: value of length ${val.length} does not match receiver type ${serializeNoraType(this.receiverType)}`
            )

        this.receiveValue(val as NoraValueOf<T>)
    }

    /**
     * SAFETY: When overriding, it's okay to cast `val` to the right type, since `setReceiverValue` checks it
     *
     * NOTE: remember that NoraValue is an array comprised of all associated base values
     */
    protected abstract receiveValue(val: NoraValueOf<T>): void

    public abstract displayName(): string
}

export enum DriverControlMode {
    VELOCITY = 0,
    POSITION = 1,
}

export default Driver
