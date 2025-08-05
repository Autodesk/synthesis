import InputSystem from "@/systems/input/InputSystem"
import type { InputName } from "@/systems/input/InputTypes"
import type { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import type Driver from "../../driver/Driver"
import type Stimulus from "../../stimulus/Stimulus"
import Behavior from "../Behavior"

abstract class SequenceableBehavior extends Behavior {
    private _jointIndex: number
    private _brainIndex: number
    private _sequentialConfig: SequentialBehaviorPreferences | undefined

    public get jointIndex(): number {
        return this._jointIndex
    }

    constructor(
        jointIndex: number,
        brainIndex: number,
        drivers: Driver[],
        stimuli: Stimulus[],
        sequentialConfig: SequentialBehaviorPreferences | undefined
    ) {
        super(drivers, stimuli)

        this._jointIndex = jointIndex
        this._brainIndex = brainIndex
        this._sequentialConfig = sequentialConfig
    }

    abstract applyInput: (velocity: number) => void

    public update(_: number): void {
        const inputName: InputName = `joint ${this._sequentialConfig?.parentJointIndex ?? this._jointIndex}`
        const inverted = this._sequentialConfig?.inverted ?? false

        this.applyInput(InputSystem.getInput(inputName, this._brainIndex) * (inverted ? -1 : 1))
    }
}

export default SequenceableBehavior
