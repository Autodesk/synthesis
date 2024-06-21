import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import Behavior from "./Behavior";
import InputSystem, { AxisInput, emptyModifierState } from "@/systems/input/InputSystem";

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver;
    private _inputName: string;
    private _linearSpeed = 10;

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, jointIndex: number) {
        super([sliderDriver], [sliderStimulus]);
        this._sliderDriver = sliderDriver;

        this._inputName = "joint " + jointIndex;

        // TODO: load inputs from mira
        InputSystem.allInputs.push(new AxisInput(this._inputName, "Digit" + jointIndex.toString(), "Digit" + jointIndex.toString(), -1, 
            false, false, emptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }));
    }

    // Changes the elevators target position
    moveElevator(linearVelocity: number) {
        this._sliderDriver.targetVelocity = linearVelocity; 
    }

    public Update(_: number): void {
        this.moveElevator(InputSystem.getInput(this._inputName)*this._linearSpeed); 
    }
}

export default GenericElevatorBehavior;