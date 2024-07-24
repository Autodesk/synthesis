import { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import SliderDriver from "../driver/SliderDriver"
import SliderStimulus from "../stimulus/SliderStimulus"
import SequenceableBehavior from "./SequenceableBehavior"

class GenericElevatorBehavior extends SequenceableBehavior {
    private _sliderDriver: SliderDriver

    maxVelocity = 6

    public get sliderDriver(): SliderDriver {
        return this._sliderDriver
    }

    constructor(
        sliderDriver: SliderDriver,
        sliderStimulus: SliderStimulus,
        jointIndex: number,
        assemblyName: string,
        assemblyIndex: number,
        sequentialConfig: SequentialBehaviorPreferences | undefined
    ) {
        super(jointIndex, assemblyName, assemblyIndex, [sliderDriver], [sliderStimulus], sequentialConfig)

        this._sliderDriver = sliderDriver
    }

    applyInput = (velocity: number) => {
        this._sliderDriver.targetVelocity = velocity
    }
}

export default GenericElevatorBehavior
