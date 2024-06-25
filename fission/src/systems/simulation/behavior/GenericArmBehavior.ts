import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem, { AxisInput, emptyModifierState } from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;
    private _inputName: string;
    private _rotationalSpeed = 30;

    private _assemblyName: string;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number, assemblyName: string) {
        super([hingeDriver], [hingeStimulus]);
        this._hingeDriver = hingeDriver;
        this._inputName = "joint " + jointIndex;

        this._assemblyName = assemblyName;

        // TODO: load inputs from mira
        InputSystem.allInputs.get(this._assemblyName)!.push(new AxisInput(this._inputName, "Digit" + jointIndex.toString(), "Digit" + jointIndex.toString(), -1, 
            false, false, -1, -1, false, emptyModifierState, { ctrl: false, alt: false, shift: true, meta: false }));
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
       this._hingeDriver.targetVelocity = rotationalVelocity; 
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getInput(this._inputName, this._assemblyName)*this._rotationalSpeed);
    }
}

export default GenericArmBehavior;