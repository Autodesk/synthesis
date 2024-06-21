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
        this.DriveSpeeds(InputSystem.getInput("arcadeDrive")*this._driveSpeed, 
            InputSystem.getInput("arcadeTurn")*this._turnSpeed);

        // TODO: Joystick control only for testing
        //this.DriveSpeeds(-InputSystem.getGamepadAxis(1)*this._driveSpeed, InputSystem.getGamepadAxis(0)*this._driveSpeed);
    }
}

export default ArcadeDriveBehavior;