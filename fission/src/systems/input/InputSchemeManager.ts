import PreferencesSystem from "../preferences/PreferencesSystem"
import DefaultInputs from "./DefaultInputs"
import InputSystem, { AxisInput, ButtonInput, Input } from "./InputSystem"

export type InputScheme = {
    schemeName: string
    descriptiveName: string
    customized: boolean
    usesGamepad: boolean
    inputs: Input[]
}

class InputSchemeManager {
    private static _customSchemes: InputScheme[] | undefined

    /** Fetches custom input schemes from preferences manager */
    public static get customInputSchemes(): InputScheme[] {
        if (this._customSchemes) return this._customSchemes

        // Load schemes from preferences and parse into objects
        const inputSchemes = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")
        inputSchemes.forEach(scheme => this.parseScheme(scheme))

        this._customSchemes = inputSchemes
        return this._customSchemes
    }

    public static addCustomScheme(scheme: InputScheme) {
        this.customInputSchemes.push(scheme)
    }

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
    public static get availableInputSchemes(): InputScheme[] {
        const allSchemes = this.allInputSchemes

        // Remove schemes that are in use
        const schemesInUse = Array.from(InputSystem.brainIndexSchemeMap.values())
        return allSchemes.filter(scheme => !schemesInUse.includes(scheme))
    }

    /** @returns a random available robot name */
    public static get randomAvailableName(): string {
        const usedNames = this.availableInputSchemes.map(s => s.schemeName)

        const randomName = () => {
            const index = Math.floor(Math.random() * DefaultInputs.NAMES.length)
            return DefaultInputs.NAMES[index]
        }

        let name = randomName()
        while (usedNames.includes(name)) name = randomName()

        return name
    }

    /** Save all schemes that have been customized to local storage via preferences */
    public static saveSchemes() {
        const customizedSchemes = Array.from(InputSystem.brainIndexSchemeMap.values()).filter(s => {
            return s.customized
        })

        // Save default schemes that have been customized if a customized version does not already exist
        this.defaultInputSchemes.forEach(s => {
            if (!s.customized) return

            if (
                customizedSchemes.some(c => {
                    if (c.schemeName === s.schemeName) return true
                })
            )
                return

            customizedSchemes.push(s)
        })

        PreferencesSystem.setGlobalPreference("InputSchemes", customizedSchemes)
        PreferencesSystem.savePreferences()
    }

    /** Returns a copy of a scheme without references to the original in any way */
    public static copyScheme(scheme: InputScheme): InputScheme {
        const copiedInputs: Input[] = []
        scheme.inputs.forEach(i => copiedInputs.push(i.getCopy()))

        return {
            schemeName: scheme.schemeName,
            descriptiveName: scheme.descriptiveName,
            customized: scheme.customized,
            usesGamepad: scheme.usesGamepad,
            inputs: copiedInputs,
        }
    }
}

export default InputSchemeManager
