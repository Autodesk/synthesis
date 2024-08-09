import { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import HingeDriver from "../../driver/HingeDriver"
import HingeStimulus from "../../stimulus/HingeStimulus"
import SequenceableBehavior from "./SequenceableBehavior"

class GenericArmBehavior extends SequenceableBehavior {
    private _hingeDriver: HingeDriver

    public get hingeDriver(): HingeDriver {
        return this._hingeDriver
    }

    maxVelocity: number = 6

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

    applyInput = (velocity: number) => {
        this._hingeDriver.targetVelocity = velocity
    }
}

export default GenericArmBehavior
