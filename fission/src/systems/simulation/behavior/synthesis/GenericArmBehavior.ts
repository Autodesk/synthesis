import HingeDriver from "@/systems/simulation/driver/HingeDriver"
import HingeStimulus from "@/systems/simulation/stimulus/HingeStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver
    private _inputName: string
    private _brainIndex: number

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number, brainIndex: number) {
        super([hingeDriver], [hingeStimulus])

        this._hingeDriver = hingeDriver
        this._inputName = "joint " + jointIndex
        this._brainIndex = brainIndex
    }

    // Sets the arm's acceleration direction
    rotateArm(input: number) {
        this._hingeDriver.accelerationDirection = input
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getInput(this._inputName, this._brainIndex))
    }
}

export default GenericArmBehavior
