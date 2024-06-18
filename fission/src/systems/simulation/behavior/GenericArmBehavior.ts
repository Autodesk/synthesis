import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import Behavior from "./Behavior";
import InputSystem from "@/systems/input/InputSystem";

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver;

    private _rotationalSpeed = 30;

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus) {
        super([hingeDriver], [hingeStimulus]);
        this._hingeDriver = hingeDriver;
    }

    rotateArm(rotationalVelocity: number) {
       this._hingeDriver.targetVelocity = rotationalVelocity; 
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.getAxis("armPositive", "armNegative")*this._rotationalSpeed);
    }
}

export default GenericArmBehavior;