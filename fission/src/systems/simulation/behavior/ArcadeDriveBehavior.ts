import WheelDriver from "../driver/WheelDriver"
import WheelRotationStimulus from "../stimulus/WheelStimulus"
import Behavior from "./Behavior"
import InputSystem from "@/systems/input/InputSystem"

class ArcadeDriveBehavior extends Behavior {
    private leftWheels: WheelDriver[]
    private rightWheels: WheelDriver[]
    private _assemblyName: string

    private _driveSpeed = 30
    private _turnSpeed = 30

    constructor(
        leftWheels: WheelDriver[],
        rightWheels: WheelDriver[],
        leftStimuli: WheelRotationStimulus[],
        rightStimuli: WheelRotationStimulus[],
        assemblyName: string
    ) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli))

        this.leftWheels = leftWheels
        this.rightWheels = rightWheels
        this._assemblyName = assemblyName
    }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(linearVelocity: number, rotationVelocity: number) {
        const leftSpeed = linearVelocity + rotationVelocity
        const rightSpeed = linearVelocity - rotationVelocity

        this.leftWheels.forEach(wheel => (wheel.targetWheelSpeed = leftSpeed))
        this.rightWheels.forEach(wheel => (wheel.targetWheelSpeed = rightSpeed))
    }

    public Update(_: number): void {
        const driveInput = InputSystem.getInput("arcadeDrive", this._assemblyName)
        const turnInput = InputSystem.getInput("arcadeTurn", this._assemblyName)

        this.DriveSpeeds(driveInput * this._driveSpeed, turnInput * this._turnSpeed)
    }
}

export default ArcadeDriveBehavior
