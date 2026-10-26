import type { BaseType, BaseUnit, DerivativeOrder, NoraBaseValueOf, NoraValueOf } from "../../Nora"
import type EncoderStimulus from "../../stimulus/EncoderStimulus"
import { WHEEL_STIMULUS_TYPE } from "../../stimulus/WheelStimulus"
import type { SimReceiver } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { CANENCODER_POSITION, CANENCODER_VELOCITY, SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export const CAN_ENCODER_TYPE = WHEEL_STIMULUS_TYPE

export default class SimCANEncoder {
    private constructor() {}

    public static setVelocity(
        device: string,
        velocity: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ONE
        }>
    ): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_VELOCITY, velocity.value)
    }

    public static setPosition(
        device: string,
        position: NoraBaseValueOf<{
            type: BaseType.NUMBER
            unit: BaseUnit.ANGLE
            order: DerivativeOrder.ZERO
        }>
    ): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_POSITION, position.value)
    }

    public static genReceiver(device: string): SimReceiver<typeof CAN_ENCODER_TYPE> {
        return {
            receiverType: CAN_ENCODER_TYPE,
            setReceiverValue: ([count, rate]: NoraValueOf<typeof CAN_ENCODER_TYPE>) => {
                SimCANEncoder.setPosition(device, count)
                SimCANEncoder.setVelocity(device, rate)
            },
        }
    }
}

export class SimEncoderInput extends SimInput {
    private _stimulus: EncoderStimulus<typeof CAN_ENCODER_TYPE>

    constructor(device: string, stimulus: EncoderStimulus<typeof CAN_ENCODER_TYPE>) {
        super(device)
        this._stimulus = stimulus
    }

    public update(_deltaT: number) {
        SimCANEncoder.setPosition(this._device, this._stimulus.positionValue)
        SimCANEncoder.setVelocity(this._device, this._stimulus.velocityValue)
    }
}
