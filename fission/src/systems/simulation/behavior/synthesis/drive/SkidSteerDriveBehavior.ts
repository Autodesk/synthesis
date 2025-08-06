import InputSystem from "@/systems/input/InputSystem.ts"
import Behavior from "@/systems/simulation/behavior/Behavior.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import { clamp } from "@/util/Utility.ts"

class SkidSteerDriveBehavior extends Behavior {
    private readonly _leftWheels: WheelDriver[]
    private readonly _rightWheels: WheelDriver[]
    private readonly _brainIndex: number
    private _isArcade: boolean

    public get wheels(): WheelDriver[] {
        return this._leftWheels.concat(this._rightWheels)
    }

    public setIsArcade(isArcade: boolean) {
        this._isArcade = isArcade
    }

    public constructor(
        leftWheels: WheelDriver[],
        rightWheels: WheelDriver[],
        leftStimuli: WheelRotationStimulus[],
        rightStimuli: WheelRotationStimulus[],
        brainIndex: number,
        isArcade: boolean
    ) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli))

        this._leftWheels = leftWheels
        this._rightWheels = rightWheels
        this._brainIndex = brainIndex
        this._isArcade = isArcade
    }

    // Sets the drivetrains target linear and rotational velocity
    protected driveSpeeds(leftInput: number, rightInput: number) {
        const leftDirection = clamp(leftInput, -1, 1)
        const rightDirection = clamp(rightInput, -1, 1)
        this._leftWheels.forEach(wheel => {
            wheel.accelerationDirection = leftDirection
        })
        this._rightWheels.forEach(wheel => {
            wheel.accelerationDirection = rightDirection
        })
    }

    private arcadeUpdate() {
        const driveInput = InputSystem.getInput("arcadeDrive", this._brainIndex)
        const turnInput = InputSystem.getInput("arcadeTurn", this._brainIndex)

        this.driveSpeeds(driveInput + turnInput, driveInput - turnInput)
    }
    private tankUpdate() {
        this.driveSpeeds(
            InputSystem.getInput("tankLeft", this._brainIndex),
            InputSystem.getInput("tankRight", this._brainIndex)
        )
    }

    public update(_: number): void {
        if (this._isArcade) {
            this.arcadeUpdate()
        } else {
            this.tankUpdate()
        }
    }
}

export default SkidSteerDriveBehavior
