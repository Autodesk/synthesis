import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem, { AxisInput, emptyModifierState } from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;
    private _inputName: string;
    private _rotationalSpeed = 30;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number) {
        super([hingeDriver], [hingeStimulus]);
        this._hingeDriver = hingeDriver;

        this._inputName = "joint " + jointIndex;

        // TODO: load inputs from mira
        InputSystem.allInputs.push(new AxisInput(this._inputName, "Digit" + jointIndex.toString(), "Digit" + jointIndex.toString(), -1, 
            false, false, emptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }));
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
       this._hingeDriver.targetVelocity = rotationalVelocity; 
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getInput(this._inputName)*this._rotationalSpeed);
    }
}

export default GenericArmBehavior;