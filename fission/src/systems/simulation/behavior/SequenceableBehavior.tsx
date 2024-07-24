import { SequentialBehaviorPreferences } from "@/systems/preferences/PreferenceTypes"
import Driver from "../driver/Driver"
import Stimulus from "../stimulus/Stimulus"
import Behavior from "./Behavior"
import InputSystem from "@/systems/input/InputSystem"

abstract class SequenceableBehavior extends Behavior {
    private _jointIndex: number
    private _assemblyName: string
    private _assemblyIndex: number
    private _sequentialConfig: SequentialBehaviorPreferences | undefined

    abstract maxVelocity: number

    public get jointIndex(): number {
        return this._jointIndex
    }

    constructor(
        jointIndex: number,
        assemblyName: string,
        assemblyIndex: number,
        drivers: Driver[],
        stimuli: Stimulus[],
        sequentialConfig: SequentialBehaviorPreferences | undefined
    ) {
        super(drivers, stimuli)

        this._jointIndex = jointIndex
        this._assemblyName = assemblyName
        this._assemblyIndex = assemblyIndex
        this._sequentialConfig = sequentialConfig
    }

    abstract applyInput: (velocity: number) => void

    public Update(_: number): void {
        const inputName = "joint " + (this._sequentialConfig?.parentJointIndex ?? this._jointIndex)
        const inverted = this._sequentialConfig?.inverted ?? false

        this.applyInput(
            InputSystem.getInput(inputName, this._assemblyName, this._assemblyIndex) *
                this.maxVelocity *
                (inverted ? -1 : 1)
        )
    }
}

export default SequenceableBehavior
