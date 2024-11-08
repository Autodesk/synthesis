import { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import SliderDriver from "../../driver/SliderDriver"
import SliderStimulus from "../../stimulus/SliderStimulus"
import SequenceableBehavior from "./SequenceableBehavior"

class GenericElevatorBehavior extends SequenceableBehavior {
    private _sliderDriver: SliderDriver

    public get sliderDriver(): SliderDriver {
        return this._sliderDriver
    }

    constructor(
        sliderDriver: SliderDriver,
        sliderStimulus: SliderStimulus,
        jointIndex: number,
        brainIndex: number,
        sequentialConfig: SequentialBehaviorPreferences | undefined
    ) {
        super(jointIndex, brainIndex, [sliderDriver], [sliderStimulus], sequentialConfig)

        this._sliderDriver = sliderDriver
    }

    applyInput = (direction: number) => {
        this._sliderDriver.accelerationDirection = direction
    }
}

export default GenericElevatorBehavior
