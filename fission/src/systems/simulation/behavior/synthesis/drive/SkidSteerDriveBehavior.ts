import WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import Behavior from "@/systems/simulation/behavior/Behavior.ts"
import { clampValues } from "@/util/Utility.ts"
import InputSystem from "@/systems/input/InputSystem.ts"

class SkidSteerDriveBehavior extends Behavior {
    private readonly leftWheels: WheelDriver[]
    private readonly rightWheels: WheelDriver[]
    private readonly _brainIndex: number
    private readonly isArcade: boolean

    public get wheels(): WheelDriver[] {
        return this.leftWheels.concat(this.rightWheels)
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

        this.leftWheels = leftWheels
        this.rightWheels = rightWheels
        this._brainIndex = brainIndex
        this.isArcade = isArcade
    }

    // Sets the drivetrains target linear and rotational velocity
    protected DriveSpeeds(leftInput: number, rightInput: number) {
        const leftDirection = clampValues(-1, leftInput, 1)
        const rightDirection = clampValues(-1, rightInput, 1)

        this.leftWheels.forEach(wheel => (wheel.accelerationDirection = leftDirection))
        this.rightWheels.forEach(wheel => (wheel.accelerationDirection = rightDirection))
    }

    private arcadeUpdate() {
        const driveInput = InputSystem.getInput("arcadeDrive", this._brainIndex)
        const turnInput = InputSystem.getInput("arcadeTurn", this._brainIndex)

        this.DriveSpeeds(driveInput + turnInput, driveInput - turnInput)
    }
    private tankUpdate() {
        this.DriveSpeeds(
            InputSystem.getInput("tankLeft", this._brainIndex),
            InputSystem.getInput("tankRight", this._brainIndex)
        )
    }

    public Update(_: number): void {
        if (this.isArcade) {
            this.arcadeUpdate()
        } else {
            this.tankUpdate()
        }
    }
}

export default SkidSteerDriveBehavior
