import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class ArcadeDriveBehavior extends Behavior {
    private leftWheels: WheelDriver[]
    private rightWheels: WheelDriver[]
    private _brainIndex: number

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

        // this.leftWheels = leftWheels.map(wheel => {
        //     wheel.maxForce = wheel.maxForce == 50 ? 500 : wheel.maxForce
        //     wheel.maxVelocity = wheel.maxVelocity < 15 ? 150 : wheel.maxVelocity
        //     return wheel
        // })
        // this.rightWheels = rightWheels.map(wheel => {
        //     wheel.maxForce = wheel.maxForce == 50 ? 500 : wheel.maxForce
        //     wheel.maxVelocity = wheel.maxVelocity < 15 ? 150 : wheel.maxVelocity
        //     return wheel
        // })
        this.rightWheels = rightWheels
        this.leftWheels = leftWheels

        this._brainIndex = brainIndex

        // console.log("left")
        // this.leftWheels.forEach(wheel => {
        //     console.log(`force: ${wheel.maxForce}`)
        // })
        // console.log("right")
        // this.rightWheels.forEach(wheel => {
        //     console.log(`force: ${wheel.maxForce}`)
        // })
    }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(driveInput: number, turnInput: number) {
        // if (driveInput != 0 || turnInput != 0) {
        //     console.log(`driveInput: ${driveInput} turnInput: ${turnInput}`)
        // }
        let leftDirection = Math.min(1, Math.max(-1, driveInput + turnInput))
        let rightDirection = Math.min(1, Math.max(-1, driveInput - turnInput))
        // if (leftDirection != 0 || rightDirection != 0) {
        //     console.log(`left: ${leftDirection} right: ${rightDirection}`)
        // }

        this.leftWheels.forEach(wheel => (wheel.accelerationDirection = leftDirection))
        this.rightWheels.forEach(wheel => (wheel.accelerationDirection = rightDirection))
    }

    public Update(_: number): void {
        this.DriveSpeeds(
            InputSystem.getInput("arcadeDrive", this._brainIndex),
            InputSystem.getInput("arcadeTurn", this._brainIndex)
        )
    }
}

export default ArcadeDriveBehavior
