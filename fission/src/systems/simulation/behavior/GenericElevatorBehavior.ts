import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import Behavior from "./Behavior";
import InputSystem from "@/systems/input/InputSystem";

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver;
    private _inputName: string;
    private _assemblyName: string;

    private _linearSpeed = 2.5;

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, jointIndex: number, assemblyName: string) {
        super([sliderDriver], [sliderStimulus]);
        
        this._sliderDriver = sliderDriver;
        this._inputName = "joint " + jointIndex;
        this._assemblyName = assemblyName;
    }

    // Changes the elevators target position
    moveElevator(linearVelocity: number) {
        this._sliderDriver.targetVelocity = linearVelocity; 
    }

    public Update(_: number): void {
        this.moveElevator(InputSystem.getInput(this._inputName, this._assemblyName) * this._linearSpeed); 
    }
}

export default GenericElevatorBehavior
