import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;
    private _inputName: string;
    private _assemblyName: string;

    private _rotationalSpeed = 6;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number, assemblyName: string) {
        super([hingeDriver], [hingeStimulus]);
        
        this._hingeDriver = hingeDriver;
        this._inputName = "joint " + jointIndex;
        this._assemblyName = assemblyName;
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
        this._hingeDriver.targetVelocity = rotationalVelocity
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getInput(this._inputName, this._assemblyName)*this._rotationalSpeed);
    }
}

export default GenericArmBehavior
