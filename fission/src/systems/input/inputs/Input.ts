import { DriveType } from "@/systems/simulation/behavior/Behavior"
import { TouchControlsAxes } from "@/ui/components/TouchControls"
import type { InputName, KeyDescriptor, ModifierState } from "../InputTypes"
import type { KeyCode } from "../KeyboardTypes"

const inputDriveTypeAssociations: Partial<Record<InputName, DriveType>> = {
    arcadeDrive: DriveType.ARCADE,
    arcadeTurn: DriveType.ARCADE,
    tankLeft: DriveType.TANK,
    tankRight: DriveType.TANK,
    swerveForward: DriveType.SWERVE,
    swerveStrafe: DriveType.SWERVE,
    swerveTurn: DriveType.SWERVE,
    swerveResetFieldForward: DriveType.SWERVE,
}

/** Represents any user input */
export default abstract class Input {
    public inputName: InputName

    /** @param {string} inputName - The name given to this input to identify its purpose. */
    protected constructor(inputName: InputName) {
        this.inputName = inputName
    }

    // Returns the current value of the input. Range depends on input type
    abstract getValue(useGamepad: boolean, useTouchControls: boolean, playerSlot?: number): number

    /**
     * @param {number} playerSlot - The logical player slot the owning scheme reads from.
     */
    abstract keysUsed(playerSlot?: number): KeyDescriptor[]

    protected describeKey(id: KeyCode, modifiers?: ModifierState): KeyDescriptor {
        if (id == "") {
            return null
        }
        if (!modifiers) {
            return id as KeyDescriptor
        }
        for (const key in modifiers) {
            if (modifiers[key as keyof ModifierState]) {
                id += `_${key}`
            }
        }
        return `${inputDriveTypeAssociations[this.inputName] ?? ""}_${id}` as KeyDescriptor
    }
    protected describeGamepadBtn(button: number, playerSlot: number = 0): KeyDescriptor {
        if (button == -1) {
            return null
        }
        return `slot${playerSlot}_${inputDriveTypeAssociations[this.inputName] ?? ""}_gamepadBtn${button}` as KeyDescriptor
    }
    protected describeGamepadAxis(axis: number, playerSlot: number = 0): KeyDescriptor {
        if (axis == -1) {
            return null
        }
        return `slot${playerSlot}_${inputDriveTypeAssociations[this.inputName] ?? ""}_gamepadAxis${axis}` as KeyDescriptor
    }
    protected describeTouchAxis(axis: TouchControlsAxes): KeyDescriptor {
        if (axis == TouchControlsAxes.NONE) {
            return null
        }
        return `${inputDriveTypeAssociations[this.inputName] ?? ""}_touchAxis${axis.valueOf()}` as KeyDescriptor
    }
}
