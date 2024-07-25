import SliderDriver from "@/systems/simulation/driver/SliderDriver"
import SliderStimulus from "@/systems/simulation/stimulus/SliderStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver
    private _inputName: string
    private _brainIndex: number

    private _linearSpeed = 2.5

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, jointIndex: number, brainIndex: number) {
        super([sliderDriver], [sliderStimulus])

        this._sliderDriver = sliderDriver
        this._inputName = "joint " + jointIndex
        this._brainIndex = brainIndex
    }

    // Changes the elevators target position
    moveElevator(linearVelocity: number) {
        this._sliderDriver.targetVelocity = linearVelocity
    }

    public Update(_: number): void {
        this.moveElevator(InputSystem.getInput(this._inputName, this._brainIndex) * this._linearSpeed)
    }
}

export default GenericElevatorBehavior
