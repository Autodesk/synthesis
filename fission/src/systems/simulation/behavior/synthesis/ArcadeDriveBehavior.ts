import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class ArcadeDriveBehavior extends Behavior {
    private leftWheels: WheelDriver[]
    private rightWheels: WheelDriver[]
    private _brainIndex: number

    private _driveSpeed = 30
    private _turnSpeed = 30

    public get wheels(): WheelDriver[] {
        return this.leftWheels.concat(this.rightWheels)
    }

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
    private DriveSpeeds(linearVelocity: number, rotationVelocity: number) {
        const leftSpeed = linearVelocity + rotationVelocity
        const rightSpeed = linearVelocity - rotationVelocity

        this.leftWheels.forEach(wheel => (wheel.targetWheelSpeed = leftSpeed))
        this.rightWheels.forEach(wheel => (wheel.targetWheelSpeed = rightSpeed))
    }

    public Update(_: number): void {
        const driveInput = InputSystem.getInput("arcadeDrive", this._brainIndex)
        const turnInput = InputSystem.getInput("arcadeTurn", this._brainIndex)

        this.DriveSpeeds(driveInput * this._driveSpeed, turnInput * this._turnSpeed)
    }
}

export default ArcadeDriveBehavior
