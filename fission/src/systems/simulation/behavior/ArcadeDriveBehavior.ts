import WheelDriver from "../driver/WheelDriver";
import WheelRotationStimulus from "../stimulus/WheelStimulus";
import Behavior from "./Behavior";
import InputSystem from "@/systems/input/InputSystem";

class ArcadeDriveBehavior extends Behavior {
    leftWheels: WheelDriver[];
    rightWheels: WheelDriver[];

    private _driveSpeed = 30;
    private _turnSpeed = 30;

    constructor(leftWheels: WheelDriver[], rightWheels: WheelDriver[], leftStimuli: WheelRotationStimulus[], rightStimuli: WheelRotationStimulus[]) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli));
        
        this.leftWheels = leftWheels;
        this.rightWheels = rightWheels;
    }

    // Sets the drivetrains target linear and rotational velocity
    private DriveSpeeds(linearVelocity: number, rotationVelocity: number) {
        const leftSpeed = linearVelocity + rotationVelocity;
        const rightSpeed = linearVelocity - rotationVelocity;
    
        this.leftWheels.forEach((wheel) => wheel.targetWheelSpeed = leftSpeed);
        this.rightWheels.forEach((wheel) => wheel.targetWheelSpeed = rightSpeed);
    }

    public Update(_: number): void {
        const driveInput = InputSystem.getInput("arcadeDrive");
        const turnInput = InputSystem.getInput("arcadeTurn");

        this.DriveSpeeds(driveInput*this._driveSpeed, turnInput*this._turnSpeed);
    }
}

export default ArcadeDriveBehavior;