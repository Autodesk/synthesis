import { random } from "@/util/Random"
import PreferencesSystem from "../preferences/PreferencesSystem"
import DefaultInputs from "./DefaultInputs"
import InputSystem, { AxisInput, ButtonInput, Input, KeyDescriptor } from "./InputSystem"
import { DriveType } from "@/systems/simulation/behavior/Behavior.ts"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"

export type InputScheme = {
    schemeName: string
    descriptiveName: string
    customized: boolean
    usesGamepad: boolean
    usesTouchControls: boolean
    supportedDrivetrains: DriveType[]
    inputs: Input[]
}

export enum InputSchemeUseType {
    IN_USE, // bound to a robot
    CONFLICT, // has keys overlapping with a bound scheme
    AVAILABLE, // no overlap and not bound
}

export type InputSchemeAvailability = { scheme: InputScheme; status: InputSchemeUseType }

class InputSchemeManager {
    // References to the current custom schemes to avoid parsing every time they are requested
    private static _customSchemes: InputScheme[] | undefined

    /** Fetches custom input schemes from preferences manager */
    public static get customInputSchemes(): InputScheme[] {
        if (this._customSchemes) return this._customSchemes

        // Load schemes from preferences and parse into objects
        this._customSchemes = PreferencesSystem.getGlobalPreference("InputSchemes")
        this._customSchemes.forEach(scheme => this.parseScheme(scheme))

        return this._customSchemes
    }

    /** Registers a new custom scheme */
    public static addCustomScheme(scheme: InputScheme) {
        this.customInputSchemes.push(scheme)
    }

    /** Parses a schemes inputs into working Input instances */
    private static parseScheme(rawInputs: InputScheme) {
        for (let i = 0; i < rawInputs.inputs.length; i++) {
            const rawInput = rawInputs.inputs[i]
            let parsedInput: Input

            if ((rawInput as ButtonInput).keyCode != undefined) {
                const rawButton = rawInput as ButtonInput

                parsedInput = new ButtonInput(
                    rawButton.inputName,
                    rawButton.keyCode,
                    rawButton.gamepadButton,
                    rawButton.keyModifiers
                )
            } else {
                const rawAxis = rawInput as AxisInput

                parsedInput = new AxisInput(
                    rawAxis.inputName,
                    rawAxis.posKeyCode,
                    rawAxis.negKeyCode,
                    rawAxis.gamepadAxisNumber,
                    rawAxis.joystickInverted,
                    rawAxis.useGamepadButtons,
                    rawAxis.posGamepadButton,
                    rawAxis.negGamepadButton,
                    rawAxis.touchControlAxis,
                    rawAxis.posKeyModifiers,
                    rawAxis.negKeyModifiers
                )
            }

            rawInputs.inputs[i] = parsedInput
        }
    }

    public static defaultInputSchemes: InputScheme[] = DefaultInputs.defaultInputCopies

    public static resetDefaultSchemes() {
        this.defaultInputSchemes = DefaultInputs.defaultInputCopies
        this._customSchemes = undefined
    }

    /** Creates an array of every input scheme that is either a default or customized by the user. Custom themes will appear on top. */
    public static get allInputSchemes(): InputScheme[] {
        // Start with custom input schemes
        const allSchemes: InputScheme[] = []

        this.customInputSchemes.forEach(s => allSchemes.push(s))

        // Add default schemes if they have not been customized
        this.defaultInputSchemes.forEach(defaultScheme => {
            if (
                allSchemes.some(s => {
                    return s.schemeName === defaultScheme.schemeName
                })
            )
                return

            allSchemes.push(defaultScheme)
        })

        return allSchemes
    }

    /** Creates an array of every input scheme that is not currently in use by a robot */
    private static get _availableInputSchemes(): InputSchemeAvailability[] {
        const allSchemes = this.allInputSchemes

        // Remove schemes that have conflicts
        const usedKeyMap = new Set<KeyDescriptor>()
        const result: Record<string, InputSchemeAvailability> = {}
        for (const scheme of InputSystem.brainIndexSchemeMap.values()) {
            result[scheme.schemeName] = { scheme, status: InputSchemeUseType.IN_USE }
            scheme?.inputs?.forEach(input => {
                input.keysUsed.forEach(key => {
                    if (key != null) {
                        usedKeyMap.add(key)
                    }
                })
            })
        }

        allSchemes.forEach(scheme => {
            if (scheme.inputs.some(input => input.keysUsed.some(k => usedKeyMap.has(k)))) {
                result[scheme.schemeName] ??= { scheme, status: InputSchemeUseType.CONFLICT }
            } else {
                result[scheme.schemeName] = { scheme, status: InputSchemeUseType.AVAILABLE }
            }
        })
        return Object.values(result)
    }

    /** Creates an array of every input scheme that is not currently in use by a robot */
    public static availableInputSchemesByType(driveType?: DriveType): InputSchemeAvailability[] {
        const allSchemes = this._availableInputSchemes
        if (driveType == null) {
            return allSchemes
        }
        return allSchemes.filter(entry => entry.scheme.supportedDrivetrains.includes(driveType))
    }

    /** Creates an array of every input scheme that is not currently in use by a robot */
    public static availableInputSchemesByBrain(brainIndex: number): InputSchemeAvailability[] {
        const driveType = SynthesisBrain.brainIndexMap.get(brainIndex)?.driveType
        return this.availableInputSchemesByType(driveType)
    }

    /** @returns a random available robot name */
    public static get randomAvailableName(): string {
        const usedNames = this.allInputSchemes.map(s => s.schemeName)

        const randomName = () => {
            const index = Math.floor(random() * DefaultInputs.NAMES.length)
            return DefaultInputs.NAMES[index]
        }

        let name = randomName()
        while (usedNames.includes(name)) name = randomName()

        return name
    }

    /** Save all schemes that have been customized to local storage via preferences */
    public static saveSchemes() {
        const customizedSchemes = this.allInputSchemes.filter(s => {
            return s.customized
        })

        PreferencesSystem.setGlobalPreference("InputSchemes", customizedSchemes)
        PreferencesSystem.savePreferences()
    }
}

export default InputSchemeManager
