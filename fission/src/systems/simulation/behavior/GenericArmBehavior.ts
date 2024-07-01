import HingeDriver from "../driver/HingeDriver"
import HingeStimulus from "../stimulus/HingeStimulus"
import Behavior from "./Behavior"
import InputSystem, { emptyModifierState } from "@/systems/input/InputSystem"

class GenericArmBehavior extends Behavior {
    private _hingeDriver: HingeDriver

    private _positiveInput: string
    private _negativeInput: string

    private _rotationalSpeed = 6

    private _rotationalSpeed = 30

    constructor(hingeDriver: HingeDriver, hingeStimulus: HingeStimulus, jointIndex: number) {
        super([hingeDriver], [hingeStimulus])
        this._hingeDriver = hingeDriver

        this._positiveInput = "joint " + jointIndex + " Positive"
        this._negativeInput = "joint " + jointIndex + " Negative"

        // TODO: load inputs from mira
        InputSystem.allInputs[this._positiveInput] = {
            name: this._positiveInput,
            keyCode: "Digit" + jointIndex.toString(),
            isGlobal: false,
            modifiers: emptyModifierState,
        }
        InputSystem.allInputs[this._negativeInput] = {
            name: this._negativeInput,
            keyCode: "Digit" + jointIndex.toString(),
            isGlobal: false,
            modifiers: { ctrl: false, alt: false, shift: true, meta: false },
        }
    }

    // Sets the arms target rotational velocity
    rotateArm(rotationalVelocity: number) {
        this._hingeDriver.targetVelocity = rotationalVelocity
    }

    public Update(_: number): void {
        this.rotateArm(InputSystem.GetAxis(this._positiveInput, this._negativeInput) * this._rotationalSpeed)
    }
}

export default GenericArmBehavior
