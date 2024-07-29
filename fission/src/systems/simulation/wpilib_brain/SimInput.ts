import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder } from "./WPILibBrain"

export interface SimInput {
    Update: (deltaT: number) => void
}

export class Encoder implements SimInput {
    private _device: number
    private _stimulus: EncoderStimulus

    constructor(device: number, stimulus: EncoderStimulus) {
        this._device = device
        this._stimulus = stimulus
    }

    public Update(_deltaT: number) {
        SimCANEncoder.SetRawInputPosition(`${this._device}`, this._stimulus.positionValue)
    }
}
