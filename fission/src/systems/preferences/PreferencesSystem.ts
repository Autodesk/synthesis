import { DefaultPreferences, DefaultRobotPreferences, GlobalPreference, RobotPreferences, RobotPreferencesKey } from "./PreferenceTypes"

class PreferencesSystem {
    private static _preferences: { [key: string]: Object }
    private static _localStorageKey = "Preferences"

    public static setGlobalPreference<T extends Object>(key: GlobalPreference, preference: T) {
        if (this._preferences == undefined) this.loadPreferences()

        this._preferences[key] = preference
    }

    private static getPreference<T>(key: string): T | undefined {
        if (this._preferences == undefined) 
            this.loadPreferences()

        return this._preferences[key] as T

        // if (customPref != undefined) 
        //     return customPref as T

        // return undefined
    }

    public static getGlobalPreference<T>(key: GlobalPreference): T {
        const customPref = this.getPreference<T>(key);
        if (customPref != undefined) 
            return customPref;

        const defaultPref = DefaultPreferences[key]
        if (defaultPref != undefined)
        
            return defaultPref as T

        throw new Error("Preference '" + key + "' is not assigned a default!")
    }

    public static getRobotPreferences(miraName: string): RobotPreferences {
        const allRoboPrefs = this.getPreference<{[key: string]: RobotPreferences}>(RobotPreferencesKey);
        if (allRoboPrefs == undefined || allRoboPrefs[miraName] == undefined)
            return DefaultRobotPreferences()

        return allRoboPrefs[miraName]
    }

    public static getFieldPreferences(miraName: string) {

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
        } catch (e) {
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
        this._preferences = {}
        console.log("Cleared preferences")
    }
}

export default PreferencesSystem