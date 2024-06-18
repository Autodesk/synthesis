import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import Behavior from "./Behavior";
import InputSystem from "@/systems/input/InputSystem";

class GenericElevatorBehavior extends Behavior {
    private _sliderDriver: SliderDriver;

    private _linearSpeed = 1;

    private _inverted: boolean;

    constructor(sliderDriver: SliderDriver, sliderStimulus: SliderStimulus, inverted: boolean) {
        super([sliderDriver], [sliderStimulus]);
        this._sliderDriver = sliderDriver;
        this._inverted = inverted;
    }

    moveElevator(positionDelta: number) {
       this._sliderDriver.targetPosition += positionDelta * (this._inverted ? -1 : 1); 
    }

    public Update(deltaT: number): void {
        this.moveElevator(InputSystem.getAxis("elevatorPositive", "elevatorNegative")*this._linearSpeed*deltaT);
    }
}

export default GenericElevatorBehavior;