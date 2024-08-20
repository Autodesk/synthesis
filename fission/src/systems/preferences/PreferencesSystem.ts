import {
    DefaultFieldPreferences,
    DefaultGlobalPreferences,
    DefaultRobotPreferences,
    FieldPreferences,
    FieldPreferencesKey,
    GlobalPreference,
    RobotPreferences,
    RobotPreferencesKey,
} from "./PreferenceTypes"

/** An event that's triggered when a preference is changed. */
export class PreferenceEvent extends Event {
    public prefName: GlobalPreference
    public prefValue: unknown

    /**
     * @param {GlobalPreference} prefName - The name of the preference that has just been updated.
     * @param {unknown} prefValue - The new value this preference was set to.
     */
    constructor(prefName: GlobalPreference, prefValue: unknown) {
        super("preferenceChanged")
        this.prefName = prefName
        this.prefValue = prefValue
    }
}

/** The preference system handles loading, saving, and updating all user managed data saved in local storage. */
class PreferencesSystem {
    private static _preferences: { [key: string]: unknown }
    private static _localStorageKey = "Preferences"

    /** Event dispatched when any global preference is updated */
    public static addEventListener(callback: (e: PreferenceEvent) => void) {
        window.addEventListener("preferenceChanged", callback as EventListener)
    }

    /** Gets any preference from the preferences map */
    private static getPreference<T>(key: string): T | undefined {
        if (this._preferences == undefined) this.loadPreferences()

        return this._preferences[key] as T
    }

    /**
     * Gets a global preference, or it's default value if it does not exist in the preferences map
     *
     * @param {GlobalPreference} key - The name of the preference to get.
     * @returns {T} The value of this preference casted to type T.
     */
    public static getGlobalPreference<T>(key: GlobalPreference): T {
        const customPref = this.getPreference<T>(key)
        if (customPref != undefined) return customPref

        const defaultPref = DefaultGlobalPreferences[key]
        if (defaultPref != undefined) return defaultPref as T

        throw new Error("Preference '" + key + "' is not assigned a default!")
    }

    /**
     * Sets a global preference to be a value of a specific type
     *
     * @param {GlobalPreference} key - The name of the preference to set.
     * @param {T} value - The value to set the preference to.
     */
    public static setGlobalPreference<T>(key: GlobalPreference, value: T) {
        if (this._preferences == undefined) this.loadPreferences()

        window.dispatchEvent(new PreferenceEvent(key, value))
        this._preferences[key] = value
    }

    /**
     * @param {string} miraName - The name of the robot assembly to get preference for.
     * @returns {RobotPreferences} Robot preferences found for the given robot, or default robot preferences if none are found.
     */
    public static getRobotPreferences(miraName: string): RobotPreferences {
        const allRoboPrefs = this.getAllRobotPreferences()

        if (allRoboPrefs[miraName] == undefined) {
            const defaultPrefs = DefaultRobotPreferences()
            allRoboPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        return allRoboPrefs[miraName]
    }

    /** Sets the RobotPreferences object for the robot of a specific mira name */
    public static setRobotPreferences(miraName: string, value: RobotPreferences) {
        const allRoboPrefs = this.getAllRobotPreferences()
        allRoboPrefs[miraName] = value
    }

    /** @returns Preferences for every robot that was found in local storage. */
    public static getAllRobotPreferences(): { [key: string]: RobotPreferences } {
        let allRoboPrefs = this.getPreference<{ [key: string]: RobotPreferences }>(RobotPreferencesKey)

        if (allRoboPrefs == undefined) {
            allRoboPrefs = {}
            this._preferences[RobotPreferencesKey] = allRoboPrefs
        }

        return allRoboPrefs
    }

    /**
     * @param {string} miraName - The name of the field assembly to get preference for.
     * @returns {FieldPreferences} Field preferences found for the given field, or default field preferences if none are found.
     */
    public static getFieldPreferences(miraName: string): FieldPreferences {
        const allFieldPrefs = this.getAllFieldPreferences()

        if (allFieldPrefs[miraName] == undefined) {
            const defaultPrefs = DefaultFieldPreferences()
            allFieldPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        return allFieldPrefs[miraName]
    }

    /** @returns Preferences for every field that was found in local storage. */
    public static getAllFieldPreferences(): { [key: string]: FieldPreferences } {
        let allFieldPrefs = this.getPreference<{ [key: string]: FieldPreferences }>(FieldPreferencesKey)

        if (allFieldPrefs == undefined) {
            allFieldPrefs = {}
            this._preferences[FieldPreferencesKey] = allFieldPrefs
        }

        return allFieldPrefs
    }

    /** Loads all preferences from local storage. */
    public static loadPreferences() {
        const loadedPrefs = window.localStorage.getItem(this._localStorageKey)

        if (loadedPrefs == undefined) {
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

    /** Saves all preferences to local storage. */
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

    /** Removes all preferences from local storage. */
    public static clearPreferences() {
        window.localStorage.removeItem(this._localStorageKey)
        this._preferences = {}
        console.log("Cleared all preferences")
    }
}

export default PreferencesSystem
