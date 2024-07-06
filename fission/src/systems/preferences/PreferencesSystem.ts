import { DefaultFieldPreferences, DefaultGlobalPreferences, DefaultRobotPreferences, FieldPreferences, FieldPreferencesKey, GlobalPreference, RobotPreferences, RobotPreferencesKey } from "./PreferenceTypes"

class PreferencesSystem {
    private static _preferences: { [key: string]: Object }
    private static _localStorageKey = "Preferences"

    /** Sets a global preference to be a value of a specific type */
    public static setGlobalPreference<T extends Object>(key: GlobalPreference, preference: T) {
        if (this._preferences == undefined) this.loadPreferences()

        this._preferences[key] = preference
    }

    /** Gets any preference from the preferences map */
    private static getPreference<T>(key: string): T | undefined {
        if (this._preferences == undefined) this.loadPreferences()

        return this._preferences[key] as T
    }

    /** Gets a global preference, or it's default value if it does not exist in the preferences map */
    public static getGlobalPreference<T>(key: GlobalPreference): T {
        const customPref = this.getPreference<T>(key)
        if (customPref != undefined) return customPref

        const defaultPref = DefaultGlobalPreferences[key]
        if (defaultPref != undefined) return defaultPref as T

        throw new Error("Preference '" + key + "' is not assigned a default!")
    }

    /** Gets a RobotPreferences object for a robot of a specific mira name */
    public static getRobotPreferences(miraName: string): RobotPreferences {
        const allRoboPrefs = this.getAllRobotPreferences()

        if (allRoboPrefs[miraName] == undefined) {
            const defaultPrefs = DefaultRobotPreferences()
            allRoboPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        return allRoboPrefs[miraName]
    }

    /** Gets preferences for every robot in local storage */
    public static getAllRobotPreferences(): { [key: string]: RobotPreferences } {
        let allRoboPrefs = this.getPreference<{ [key: string]: RobotPreferences }>(RobotPreferencesKey)

        if (allRoboPrefs == undefined) {
            allRoboPrefs = {}
            this._preferences[RobotPreferencesKey] = allRoboPrefs
        }

        return allRoboPrefs
    }

    /** Gets a FieldPreferences object for a robot of a specific mira name */
    public static getFieldPreferences(miraName: string): FieldPreferences {
        const allFieldPrefs = this.getAllFieldPreferences()

        if (allFieldPrefs[miraName] == undefined) {
            const defaultPrefs = DefaultFieldPreferences()
            allFieldPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        return allFieldPrefs[miraName]
    }

    /** Gets preferences for every robot in local storage */
    public static getAllFieldPreferences(): { [key: string]: FieldPreferences } {
        let allFieldPrefs = this.getPreference<{ [key: string]: FieldPreferences }>(RobotPreferencesKey)

        if (allFieldPrefs == undefined) {
            allFieldPrefs = {}
            this._preferences[FieldPreferencesKey] = allFieldPrefs
        }

        return allFieldPrefs
    }

    /** Load all preferences from local storage */
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
    }

    /** Save all preferences to local storage */
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
    }

    /** Remove all preferences from local storage */
    public static clearPreferences() {
        window.localStorage.removeItem(this._localStorageKey)
        this._preferences = {}
        console.log("Cleared preferences")
    }
}

export default PreferencesSystem