import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class ArcadeDriveBehavior extends Behavior {
    private _leftWheels: WheelDriver[]
    private _rightWheels: WheelDriver[]
    private _brainIndex: number

    public get wheels(): WheelDriver[] {
        return this._leftWheels.concat(this._rightWheels)
    }

    constructor(
        leftWheels: WheelDriver[],
        rightWheels: WheelDriver[],
        leftStimuli: WheelRotationStimulus[],
        rightStimuli: WheelRotationStimulus[],
        brainIndex: number
    ) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli))

        this._leftWheels = leftWheels
        this._rightWheels = rightWheels
        this._brainIndex = brainIndex
    }

    // Sets the drivetrains target linear and rotational velocity
    private driveSpeeds(driveInput: number, turnInput: number) {
        const leftDirection = Math.min(1, Math.max(-1, driveInput + turnInput))
        const rightDirection = Math.min(1, Math.max(-1, driveInput - turnInput))

        this._leftWheels.forEach(wheel => (wheel.accelerationDirection = leftDirection))
        this._rightWheels.forEach(wheel => (wheel.accelerationDirection = rightDirection))
    }

    public update(_: number): void {
        this.driveSpeeds(
            InputSystem.getInput("arcadeDrive", this._brainIndex),
            InputSystem.getInput("arcadeTurn", this._brainIndex)
        )
    }
}

export default ArcadeDriveBehavior
