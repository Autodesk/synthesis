import WheelDriver from "@/systems/simulation/driver/WheelDriver"
import WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus"
import Behavior from "@/systems/simulation/behavior/Behavior"
import WPILibBrain, { SimCAN, SimGeneric, simMap } from "../../wpilib_brain/WPILibBrain"

class WPILibArcadeDriveBehavior extends Behavior {
    private leftWheels: WheelDriver[]
    private rightWheels: WheelDriver[]

    private _driveSpeed = 30
    private _turnSpeed = 30

    constructor(
        leftWheels: WheelDriver[],
        rightWheels: WheelDriver[],
        leftStimuli: WheelRotationStimulus[],
        rightStimuli: WheelRotationStimulus[],
    ) {
        super(leftWheels.concat(rightWheels), leftStimuli.concat(rightStimuli))

        this.leftWheels = leftWheels
        this.rightWheels = rightWheels
    }

    public Update(_: number): void {
        // TODO: better checks for device field existence
        // TODO: differentiate between fields for CAN, PWM (<percentOutput vs <motorVoltage)
        this.leftWheels.forEach(wheel => wheel.targetWheelSpeed = SimGeneric.Get<number>(wheel.deviceType!, wheel.device!, "<percentOutput", 0)! * this._driveSpeed);
        this.rightWheels.forEach(wheel => wheel.targetWheelSpeed = SimGeneric.Get<number>(wheel.deviceType!, wheel.device!, "<percentOutput", 0)! * this._driveSpeed);
    }
}

export default WPILibArcadeDriveBehavior
