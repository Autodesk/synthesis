import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem, { emptyModifierState } from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;

    private _positiveInput: string;
    private _negativeInput: string;
    
    private _rotationalSpeed = 30;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number) {
        super([hingeDriver], [hingeStimulus]);
        this._hingeDriver = hingeDriver;

        this._positiveInput = "jointPositive" + jointIndex;
        this._negativeInput = "jointNegative" + jointIndex;

        // TODO: load inputs from mira
        InputSystem.allInputs[this._positiveInput] = { name: this._positiveInput, keybind: jointIndex.toString(), isGlobal: true, modifiers: emptyModifierState };
        InputSystem.allInputs[this._negativeInput] = { name: this._negativeInput, keybind: jointIndex.toString(), isGlobal: true, 
            modifiers: { ctrl: false, alt: true, shift: false, meta: false } };
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
       this._hingeDriver.targetVelocity = rotationalVelocity; 
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getAxis(this._positiveInput, this._negativeInput)*this._rotationalSpeed);
    }
}

export default GenericArmBehavior;