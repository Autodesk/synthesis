import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder } from "./WPILibBrain"

export interface SimInput {
    Update: (deltaT: number) => void
}

export class SimEncoderInput implements SimInput {
    private _device: string
    private _stimulus: EncoderStimulus
    private _conversionFactor: number

    constructor(device: string, stimulus: EncoderStimulus, conversionFactor: number) {
        this._device = device
        this._stimulus = stimulus
        this._conversionFactor = conversionFactor
    }

    public Update(_deltaT: number) {
        SimCANEncoder.SetRawInputPosition(`${this._device}`, this._stimulus.positionValue * this._conversionFactor)
    }
}
