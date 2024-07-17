import PreferencesSystem from "../preferences/PreferencesSystem"
import DefaultInputs from "./DefaultInputs"
import InputSystem, { Input } from "./InputSystem"

export type InputScheme = {
    schemeName: string
    descriptiveName: string
    customized: boolean
    usesGamepad: boolean
    inputs: Input[]
}

class InputSchemeManager {
    private static _defaultInputSchemes: InputScheme[] = [
        DefaultInputs.ernie,
        DefaultInputs.luna,
        DefaultInputs.jax,
        DefaultInputs.hunter,
        DefaultInputs.carmela,
    ]

    /** Fetches custom input schemes from preferences manager */
    private static get _customInputSchemes(): InputScheme[] {
        return PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")
    }

    /** Creates an array of every input scheme that is either a default or customized by the user. Custom themes will appear on top. */
    public static get AVAILABLE_INPUT_SCHEMES(): InputScheme[] {
        // Start with custom input schemes
        const availableSchemes = this._customInputSchemes

        // Add default schemes that have not been customized
        this._defaultInputSchemes.forEach(defaultScheme => {
            if (availableSchemes.includes(defaultScheme)) return

            availableSchemes.push(defaultScheme)
        })

        // Remove schemes that are in use
        const schemesInUse = Array.from(InputSystem.brainIndexSchemeMap.values())
        availableSchemes.filter(scheme => !schemesInUse.includes(scheme))

        return InputSchemeManager._defaultInputSchemes
    }
}

export default InputSchemeManager
