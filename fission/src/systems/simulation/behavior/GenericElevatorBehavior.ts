import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import Behavior from "./Behavior";
import InputSystem, { emptyModifierState } from "@/systems/input/InputSystem";

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver;

    private _positiveInput: string;
    private _negativeInput: string;

    private _linearSpeed = 1;

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, jointIndex: number) {
        super([sliderDriver], [sliderStimulus]);
        this._sliderDriver = sliderDriver;

        this._positiveInput = "jointPositive" + jointIndex;
        this._negativeInput = "jointNegative" + jointIndex;

        // TODO: load inputs from mira
        InputSystem.allInputs[this._positiveInput] = { name: this._positiveInput, keybind: jointIndex.toString(), isGlobal: true, modifiers: emptyModifierState };
        InputSystem.allInputs[this._negativeInput] = { name: this._negativeInput, keybind: jointIndex.toString(), isGlobal: true, 
            modifiers: { ctrl: false, alt: true, shift: false, meta: false } };
    }

    // Changes the elevators target position
    moveElevator(positionDelta: number) {
       this._sliderDriver.targetPosition += positionDelta; 
    }

    public Update(deltaT: number): void {
        this.moveElevator(InputSystem.getAxis(this._positiveInput, this._negativeInput)*this._linearSpeed*deltaT); 
    }
}

export default GenericElevatorBehavior;