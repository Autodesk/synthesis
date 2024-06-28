import DefaultPreferences from "./DefaultPreferences";

class PreferencesSystem {
    private static _preferences: { [key: string]: Object }

    public static setPreference<T extends Object>(key: string, preference: T) {
        if (this._preferences == undefined)
            this.loadPreferences()

        this._preferences[key] = preference
    }

    public static getPreference<T>(key: string): T | undefined {
        if (this._preferences == undefined)
            this.loadPreferences()

        const customPref = this._preferences[key]

        if (customPref != undefined)
            return customPref as T

        const defaultPref = DefaultPreferences.getDefaultPreference(key)

        if (defaultPref != undefined) {
            return defaultPref as T
        }

        return undefined;
    }

    public static loadPreferences() {
        //throw new Error("Not implemented")
        this._preferences = {}
        // TODO
    }

    public static savePreferences() {
        if (this._preferences == undefined)
            console.log("Preferences not loaded!")

        const prefsString = JSON.stringify(this._preferences)

        console.log("Saving prefs: " + prefsString)
    }

}

export default PreferencesSystem;