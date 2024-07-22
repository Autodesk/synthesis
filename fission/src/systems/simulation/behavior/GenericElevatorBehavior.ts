import SliderDriver from "../driver/SliderDriver"
import SliderStimulus from "../stimulus/SliderStimulus"
import Behavior from "./Behavior"
import InputSystem from "@/systems/input/InputSystem"

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver
    private _inputName: string
    private _assemblyName: string
    private _assemblyIndex: number

    private _linearSpeed = 2.5

    public get sliderDriver(): SliderDriver {
        return this._sliderDriver
    }

    constructor(
        sliderDriver: SliderDriver,
        sliderStimulus: SliderStimulus,
        jointIndex: number,
        assemblyName: string,
        assemblyIndex: number
    ) {
        super([sliderDriver], [sliderStimulus])

        this._sliderDriver = sliderDriver
        this._inputName = "joint " + jointIndex
        this._assemblyName = assemblyName
        this._assemblyIndex = assemblyIndex
    }

    // Changes the elevators target position
    moveElevator(linearVelocity: number) {
        this._sliderDriver.targetVelocity = linearVelocity
    }

    public Update(_: number): void {
        this.moveElevator(
            InputSystem.getInput(this._inputName, this._assemblyName, this._assemblyIndex) * this._linearSpeed
        )
    }
}

export default GenericElevatorBehavior
