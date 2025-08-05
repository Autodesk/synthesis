import type { NoraNumber2 } from "../../Nora"
import type EncoderStimulus from "../../stimulus/EncoderStimulus"
import type { SimReceiver } from "../SimDataFlow"
import { SimInput } from "../SimInput"
import { receiverTypeMap } from "../WPILibState"
import { CANENCODER_POSITION, CANENCODER_VELOCITY, SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimCANEncoder {
    private constructor() {}

    public static setVelocity(device: string, velocity: number): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_VELOCITY, velocity)
    }

    public static setPosition(device: string, position: number): boolean {
        return SimGeneric.set(SimType.CAN_ENCODER, device, CANENCODER_POSITION, position)
    }

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.CAN_ENCODER]!,
            setReceiverValue: ([count, rate]: NoraNumber2) => {
                SimCANEncoder.setPosition(device, count)
                SimCANEncoder.setVelocity(device, rate)
            },
        }
    }
}

export class SimEncoderInput extends SimInput {
    private _stimulus: EncoderStimulus

    constructor(device: string, stimulus: EncoderStimulus) {
        super(device)
        this._stimulus = stimulus
    }

    public update(_deltaT: number) {
        SimCANEncoder.setPosition(this._device, this._stimulus.positionValue)
        SimCANEncoder.setVelocity(this._device, this._stimulus.velocityValue)
    }
}
