import DefaultPreferences from "./DefaultPreferences"

class PreferencesSystem {
    private static _preferences: { [key: string]: Object }
    private static _localStorageKey = "Preferences"

    public static setPreference<T extends Object>(key: string, preference: T) {
        if (this._preferences == undefined)
            this.loadPreferences()

        this._preferences[key] = preference
    }

    public static getPreference<T>(key: string): T {
        if (this._preferences == undefined)
            this.loadPreferences()

        const customPref = this._preferences[key]

        if (customPref != undefined)
            return customPref as T

        const defaultPref = DefaultPreferences.getDefaultPreference(key)

        if (defaultPref != undefined) {
            return defaultPref as T
        }

        throw new Error("Preference '" + key + "' is not assigned a default!")
    }

    public static loadPreferences() {
        const loadedPrefs = window.localStorage.getItem(this._localStorageKey)

        if (loadedPrefs == undefined) {
            console.log("Preferences not found in local storage!")
            this._preferences = {}
            return
        }

        try {
            this._preferences = JSON.parse(loadedPrefs)
        }
        catch(e) {
            console.error(e)
            this._preferences = {}
        }

        console.log("Loaded prefs: " + loadedPrefs)
    }

    public static savePreferences() {
        if (this._preferences == undefined) {
            console.log("Preferences not loaded!")
            return
        }

        const prefsString = JSON.stringify(this._preferences)

        if (prefsString == undefined) {
            console.log("Preferences loaded but undefined")
            return
        }

        window.localStorage.setItem(this._localStorageKey, prefsString)

        console.log("Saved prefs: " + prefsString)
    }

    public static clearPreferences() {
        window.localStorage.removeItem(this._localStorageKey)
        this._preferences = {};
        console.log("Cleared preferences")
    }
}

export default PreferencesSystem