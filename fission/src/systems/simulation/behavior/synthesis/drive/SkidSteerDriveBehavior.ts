import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import { clamp } from "@/util/Utility.ts"

class SkidSteerDriveBehavior extends DriveBehavior {
    private readonly _leftWheels: WheelDriver[]
    private readonly _rightWheels: WheelDriver[]
    private readonly _brainIndex: number
    public isArcade: boolean
    private _debugActiveFrames = 0

    public get wheels(): WheelDriver[] {
        return this._leftWheels.concat(this._rightWheels)
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
        this.isArcade = isArcade
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

        // Throttled diagnostic: once input is non-zero, dump ground-contact/suspension state for one
        // wheel per side every ~60 updates so a drive attempt's log shows whether the wheels are actually
        // touching the ground (no contact -> no traction force regardless of commanded angular velocity).
        if (Math.abs(leftDirection) > 0.02 || Math.abs(rightDirection) > 0.02) {
            this._debugActiveFrames++
            if (this._debugActiveFrames % 60 === 1) {
                const leftInfo = this._leftWheels[0]?.getDebugContactInfo()
                const rightInfo = this._rightWheels[0]?.getDebugContactInfo()
                console.debug(
                    `[SkidSteerDriveBehavior] drive input left=${leftDirection.toFixed(2)} right=${rightDirection.toFixed(2)} -- ` +
                        `leftWheel0=${JSON.stringify(leftInfo)} rightWheel0=${JSON.stringify(rightInfo)}`
                )
            }
        } else {
            this._debugActiveFrames = 0
        }
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
        if (this.isArcade) {
            this.arcadeUpdate()
        } else {
            this.tankUpdate()
        }
    }
}

export default SkidSteerDriveBehavior
