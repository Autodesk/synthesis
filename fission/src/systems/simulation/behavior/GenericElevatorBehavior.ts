import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import Behavior from "./Behavior";
import InputSystem, { emptyModifierState } from "@/systems/input/InputSystem";

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver;

    private _positiveInput: string;
    private _negativeInput: string;

    private _linearSpeed = 2.5;

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, jointIndex: number) {
        super([sliderDriver], [sliderStimulus]);
        this._sliderDriver = sliderDriver;

        this._positiveInput = "joint " + jointIndex + " Positive";
        this._negativeInput = "joint " + jointIndex + " Negative";

        // TODO: load inputs from mira
        InputSystem.allInputs[this._positiveInput] = { name: this._positiveInput, keyCode: "Digit" + jointIndex.toString(), isGlobal: false, modifiers: emptyModifierState };
        InputSystem.allInputs[this._negativeInput] = { name: this._negativeInput, keyCode: "Digit" + jointIndex.toString(), isGlobal: false, 
            modifiers: { ctrl: false, alt: false, shift: true, meta: false } };
    }

    // Changes the elevators target position
    moveElevator(linearVelocity: number) {
        this._sliderDriver.targetVelocity = linearVelocity; 
    }

    public Update(_: number): void {
        this.moveElevator(InputSystem.GetAxis(this._positiveInput, this._negativeInput)*this._linearSpeed); 
    }
}

export default GenericElevatorBehavior;