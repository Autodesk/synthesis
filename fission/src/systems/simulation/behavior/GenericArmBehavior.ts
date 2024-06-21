import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem, { ButtonInput } from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;

    private _positiveInput: string;
    private _negativeInput: string;
    
    private _rotationalSpeed = 30;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number) {
        super([hingeDriver], [hingeStimulus]);
        this._hingeDriver = hingeDriver;

        this._positiveInput = "joint " + jointIndex + " Positive";
        this._negativeInput = "joint " + jointIndex + " Negative";

        // TODO: load inputs from mira
        InputSystem.allInputs.push(new ButtonInput(this._positiveInput, "Digit" + jointIndex.toString()));
        InputSystem.allInputs.push(new ButtonInput(this._negativeInput, "Digit" + jointIndex.toString(), false, false, 
            { ctrl: false, alt: false, shift: true, meta: false } ));
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
       this._hingeDriver.targetVelocity = rotationalVelocity; 
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getButtonAxis(this._positiveInput, this._negativeInput)*this._rotationalSpeed);
    }
}

export default GenericArmBehavior;