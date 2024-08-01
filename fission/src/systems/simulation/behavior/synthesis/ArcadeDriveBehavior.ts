import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class ArcadeDriveBehavior extends Behavior {
    private leftWheels: WheelDriver[]
    private rightWheels: WheelDriver[]
    private _brainIndex: number

    constructor(
        leftWheels: WheelDriver[],
        rightWheels: WheelDriver[],
        leftStimuli: WheelRotationStimulus[],
        rightStimuli: WheelRotationStimulus[],
        brainIndex: number
    ) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli))

        this.leftWheels = leftWheels
        this.rightWheels = rightWheels
        this._brainIndex = brainIndex
    }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(driveInput: number, turnInput: number) {
        const leftDirection = driveInput + turnInput
        const rightDirection = driveInput - turnInput

        this.leftWheels.forEach(wheel => (wheel.accelerationDirection = leftDirection))
        this.rightWheels.forEach(wheel => (wheel.accelerationDirection = rightDirection))
    }

    public Update(_: number): void {
        this.DriveSpeeds(InputSystem.getInput("arcadeDrive", this._brainIndex), InputSystem.getInput("arcadeTurn", this._brainIndex))
    }
}

export default ArcadeDriveBehavior
