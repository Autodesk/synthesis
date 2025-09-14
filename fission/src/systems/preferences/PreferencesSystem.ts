import {
    defaultFieldPreferences,
    defaultGlobalPreferences,
    defaultGraphicsPreferences,
    defaultMotorPreferences,
    defaultRobotPreferences,
    FIELD_PREFERENCE_KEY,
    type FieldPreferences,
    type GlobalPreference,
    type GlobalPreferences,
    GRAPHICS_PREFERENCE_KEY,
    type GraphicsPreferences,
    MOTOR_PREFERENCES_KEY,
    type MotorPreferences,
    type Preferences,
    ROBOT_PREFERENCE_KEY,
    type RobotPreferences,
} from "./PreferenceTypes"

/** An event that's triggered when a preference is changed. */
export class PreferenceEvent<K extends GlobalPreference> extends Event {
    public prefName: K
    public prefValue: GlobalPreferences[K]

    /**
     * @param {GlobalPreference} prefName - The name of the preference that has just been updated.
     * @param {unknown} prefValue - The new value this preference was set to.
     */
    constructor(prefName: K, prefValue: GlobalPreferences[K]) {
        super("preferenceChanged")
        this.prefName = prefName
        this.prefValue = prefValue
    }
}

/** The preference system handles loading, saving, and updating all user managed data saved in local storage. */
class PreferencesSystem {
    private static _preferences: Partial<Preferences>
    private static _localStorageKey = "Preferences"

    /** Event dispatched when a specific global preference is updated, returns a function to unsubscribe */
    public static addPreferenceEventListener<P extends GlobalPreference>(
        preference: P,
        callback: (e: PreferenceEvent<P>) => void
    ) {
        const cb: EventListener = event => {
            if ((event as PreferenceEvent<GlobalPreference>).prefName == preference) {
                callback(event as PreferenceEvent<P>)
            }
        }
        window.addEventListener("preferenceChanged", cb)
        return () => {
            window.removeEventListener("preferenceChanged", cb)
        }
    }

    /** Gets any preference from the preferences map */
    private static getPreference<K extends keyof Preferences>(key: K): Preferences[K] | undefined {
        if (this._preferences == undefined) this.loadPreferences()

        return this._preferences[key]
    }

    /**
     * Gets a global preference, or it's default value if it does not exist in the preferences map
     *
     * @param {GlobalPreference} key - The name of the preference to get.
     * @returns {T} The value of this preference casted to type T.
     */
    public static getGlobalPreference<K extends GlobalPreference>(key: K): GlobalPreferences[K] {
        const customPref = this.getPreference(key)
        if (customPref != undefined) return customPref

        const defaultPref = defaultGlobalPreferences[key]
        if (defaultPref != undefined) return defaultPref

        throw new Error("Preference '" + key + "' is not assigned a default!")
    }

    /**
     * Sets a global preference to be a value of a specific type
     *
     * @param {GlobalPreference} key - The name of the preference to set.
     * @param {T} value - The value to set the preference to.
     */
    public static setGlobalPreference<K extends GlobalPreference>(key: K, value: GlobalPreferences[K]) {
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
            const defaultPrefs = defaultRobotPreferences()
            allRoboPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        const defaultPrefs = defaultRobotPreferences()
        const mergedPrefs = { ...defaultPrefs, ...allRoboPrefs[miraName] }
        allRoboPrefs[miraName] = mergedPrefs

        return mergedPrefs
    }

    /** Sets the RobotPreferences object for the robot of a specific mira name */
    public static setRobotPreferences(miraName: string, value: RobotPreferences) {
        const allRoboPrefs = this.getAllRobotPreferences()
        allRoboPrefs[miraName] = value
    }

    /** Sets the FieldPreferences object for the field of a specific mira name */
    public static setFieldPreferences(miraName: string, value: FieldPreferences) {
        const allFieldPrefs = this.getAllFieldPreferences()
        allFieldPrefs[miraName] = value
    }

    /** Sets the MotorPreferences object for the motor of a specific mira name */
    public static setMotorPreferences(miraName: string, value: MotorPreferences) {
        const allMotorPrefs = this.getAllMotorPreferences()
        allMotorPrefs[miraName] = value
    }

    /** @returns Preferences for every robot that was found in local storage. */
    public static getAllRobotPreferences(): { [key: string]: RobotPreferences } {
        let allRoboPrefs = this.getPreference(ROBOT_PREFERENCE_KEY)

        if (allRoboPrefs == undefined) {
            allRoboPrefs = {}
            this._preferences[ROBOT_PREFERENCE_KEY] = allRoboPrefs
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
            const defaultPrefs = defaultFieldPreferences()
            allFieldPrefs[miraName] = defaultPrefs
            return defaultPrefs
        }

        const defaultPrefs = defaultFieldPreferences()
        const mergedPrefs = { ...defaultPrefs, ...allFieldPrefs[miraName] }
        allFieldPrefs[miraName] = mergedPrefs

        return mergedPrefs
    }

    /** @returns Preferences for every field that was found in local storage. */
    public static getAllFieldPreferences(): { [key: string]: FieldPreferences } {
        let allFieldPrefs = this.getPreference(FIELD_PREFERENCE_KEY)

        if (allFieldPrefs == undefined) {
            allFieldPrefs = {}
            this._preferences[FIELD_PREFERENCE_KEY] = allFieldPrefs
        }

        return allFieldPrefs
    }

    /**
     * @param {string} miraName - The name of the motor assembly to get preference for.
     * @returns {MotorPreferences} Motor preferences found for the given motor, or default motor preferences if none are found.
     */
    public static getMotorPreferences(miraName: string): MotorPreferences {
        const allMotorPrefs = this.getAllMotorPreferences()

        if (allMotorPrefs[miraName] == undefined) {
            allMotorPrefs[miraName] = defaultMotorPreferences(miraName)
        } else {
            const defaultPrefs = defaultMotorPreferences(miraName)
            allMotorPrefs[miraName] = { ...defaultPrefs, ...allMotorPrefs[miraName] }
        }

        return allMotorPrefs[miraName]
    }

    /** @returns Preferences for every motor that was found in local storage. */
    public static getAllMotorPreferences(): { [key: string]: MotorPreferences } {
        let motorPrefs = this.getPreference(MOTOR_PREFERENCES_KEY)

        if (motorPrefs == undefined) {
            motorPrefs = {}
            this._preferences[MOTOR_PREFERENCES_KEY] = motorPrefs
        }

        return motorPrefs
    }

    /** Gets simulation quality preferences */
    public static getGraphicsPreferences(): GraphicsPreferences {
        let graphicsPrefs = this.getPreference(GRAPHICS_PREFERENCE_KEY)

        if (graphicsPrefs == undefined) {
            graphicsPrefs = defaultGraphicsPreferences()
            this._preferences[GRAPHICS_PREFERENCE_KEY] = graphicsPrefs
        } else {
            const defaultPrefs = defaultGraphicsPreferences()
            graphicsPrefs = { ...defaultPrefs, ...graphicsPrefs }
            this._preferences[GRAPHICS_PREFERENCE_KEY] = graphicsPrefs
        }

        return graphicsPrefs
    }

    /** Resets simulation quality preferences to default values */
    public static resetGraphicsPreferences() {
        this._preferences[GRAPHICS_PREFERENCE_KEY] = defaultGraphicsPreferences()
        this.savePreferences()
    }

    /** Loads all preferences from local storage. */
    public static loadPreferences() {
        const loadedPrefs = window.localStorage.getItem(this._localStorageKey)

        if (loadedPrefs == undefined) {
            this._preferences = {}
            return
        }

        try {
            this._preferences = { ...defaultGlobalPreferences, ...JSON.parse(loadedPrefs) }
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

    public static revertPreferences() {
        PreferencesSystem.loadPreferences()
        Object.entries(this._preferences).forEach(([key, value]) => {
            window.dispatchEvent(
                new PreferenceEvent(key as GlobalPreference, value as GlobalPreferences[GlobalPreference])
            )
        })
    }

    /** Removes all preferences from local storage. */
    public static clearPreferences() {
        window.localStorage.removeItem(this._localStorageKey)
        this._preferences = {}
    }
}

export default PreferencesSystem
