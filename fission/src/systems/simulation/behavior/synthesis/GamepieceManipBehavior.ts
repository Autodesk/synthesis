import Behavior from "@/systems/simulation/behavior/Behavior"
import InputSystem from "@/systems/input/InputSystem"
import EjectorDriver from "../../driver/EjectorDriver"
import IntakeDriver from "../../driver/IntakeDriver"

class GamepieceManipBehavior extends Behavior {
    private _brainIndex: number

    private _ejector: EjectorDriver
    private _intake: IntakeDriver

    constructor(ejector: EjectorDriver, intake: IntakeDriver, brainIndex: number) {
        super([ejector, intake], [])

        this._brainIndex = brainIndex
        this._ejector = ejector
        this._intake = intake
    }

    public Update(_: number): void {
        this._ejector.value = InputSystem.getInput("eject", this._brainIndex)
        this._intake.value = InputSystem.getInput("intake", this._brainIndex)
    }
}

export default GamepieceManipBehavior
