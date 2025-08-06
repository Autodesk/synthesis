import type { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import type HingeDriver from "../../driver/HingeDriver"
import type HingeStimulus from "../../stimulus/HingeStimulus"
import SequenceableBehavior from "./SequenceableBehavior"

class GenericArmBehavior extends SequenceableBehavior {
    private _hingeDriver: HingeDriver

    public get hingeDriver(): HingeDriver {
        return this._hingeDriver
    }

    constructor(
        hingeDriver: HingeDriver,
        hingeStimulus: HingeStimulus,
        jointIndex: number,
        brainIndex: number,
        sequentialConfig: SequentialBehaviorPreferences | undefined
    ) {
        super(jointIndex, brainIndex, [hingeDriver], [hingeStimulus], sequentialConfig)

        this._hingeDriver = hingeDriver
    }

    applyInput = (direction: number) => {
        this._hingeDriver.accelerationDirection = direction
    }
}

export default GenericArmBehavior
