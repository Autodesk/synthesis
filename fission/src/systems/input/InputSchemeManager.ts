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

class InputSchemeParser {
    private static _customSchemes: InputScheme[]

    /** Fetches custom input schemes from preferences manager */
    public static get customInputSchemes(): InputScheme[] {
        if (this._customSchemes) return this._customSchemes

        // Load schemes from preferences and parse into objects
        const inputSchemes = PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")
        inputSchemes.forEach(scheme => this.parseScheme(scheme))

        this._customSchemes = inputSchemes
        return this._customSchemes
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
                    rawButton.isGlobal,
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
                    rawAxis.isGlobal,
                    rawAxis.posKeyModifiers,
                    rawAxis.negKeyModifiers
                )
            }

            rawInputs.inputs[i] = parsedInput
        }
    }
}

class InputSchemeManager {
    private static _defaultInputSchemes: InputScheme[] = [
        DefaultInputs.ernie,
        DefaultInputs.luna,
        DefaultInputs.jax,
        DefaultInputs.hunter,
        DefaultInputs.carmela,
    ]

    /** Creates an array of every input scheme that is either a default or customized by the user. Custom themes will appear on top. */
    public static get availableInputSchemes(): InputScheme[] {
        // Start with custom input schemes
        const availableSchemes: InputScheme[] = []

        InputSchemeParser.customInputSchemes.forEach(s => availableSchemes.push(s))

        // Add default schemes if they have not been customized
        this._defaultInputSchemes.forEach(defaultScheme => {
            if (
                availableSchemes.some(s => {
                    return s.schemeName === defaultScheme.schemeName
                })
            )
                return

            availableSchemes.push(defaultScheme)
        })

        // Remove schemes that are in use
        const schemesInUse = Array.from(InputSystem.brainIndexSchemeMap.values())
        return availableSchemes.filter(scheme => !schemesInUse.includes(scheme))
    }

    public static saveSchemes() {
        const customizedSchemes = Array.from(InputSystem.brainIndexSchemeMap.values()).filter(s => {
            return s.customized
        })

        PreferencesSystem.setGlobalPreference("InputSchemes", customizedSchemes)
        PreferencesSystem.savePreferences()
    }
}

export default InputSchemeManager
