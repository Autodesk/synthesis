import InputSystem from "@/systems/input/InputSystem"
import Behavior from "@/systems/simulation/behavior/Behavior"
import type EjectorDriver from "../../driver/EjectorDriver"
import { EJECTOR_TYPE } from "../../driver/EjectorDriver"
import type IntakeDriver from "../../driver/IntakeDriver"

class GamepieceManipBehavior extends Behavior {
    private _brainIndex: number

    private _ejector: EjectorDriver
    private _intake: IntakeDriver

    private _prevEjectPressed = false

    constructor(ejector: EjectorDriver, intake: IntakeDriver, brainIndex: number) {
        super([ejector, intake], [])

        this._brainIndex = brainIndex
        this._ejector = ejector
        this._intake = intake
    }

    public update(_: number): void {
        const ejectPressed = InputSystem.getInput("eject", this._brainIndex) === 1

        if (ejectPressed && !this._prevEjectPressed)
            this._ejector.setReceiverValue([{ value: 1, baseType: EJECTOR_TYPE[0] }])
        else this._ejector.setReceiverValue([{ value: 0, baseType: EJECTOR_TYPE[0] }])

        this._prevEjectPressed = ejectPressed

        this._intake.value = InputSystem.getInput("intake", this._brainIndex)
    }
}

export default GamepieceManipBehavior
