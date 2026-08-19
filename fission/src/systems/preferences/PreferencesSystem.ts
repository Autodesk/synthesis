import {
    defaultFieldPreferences,
    defaultGraphicsPreferences,
    defaultRobotPreferences,
    defaultUserPreferences,
    FIELD_PREFERENCE_KEY,
    type FieldPreferences,
    GRAPHICS_PREFERENCE_KEY,
    type GraphicsPreferences,
    type Preferences,
    ROBOT_PREFERENCE_KEY,
    type RobotPreferences,
    USER_PREFERENCE_KEY,
    type UserPreference,
    type UserPreferences,
} from "./PreferenceTypes"

/** An event that's triggered when a preference is changed. */
export class UserPreferenceEvent<K extends UserPreference> extends Event {
    public prefName: K
    public prefValue: UserPreferences[K]

    /**
     * @param {UserPreference} prefName - The name of the preference that has just been updated.
     * @param {unknown} prefValue - The new value this preference was set to.
     */
    constructor(prefName: K, prefValue: UserPreferences[K]) {
        super("preferenceChanged")
        this.prefName = prefName
        this.prefValue = prefValue
    }
}

/** The preference system handles loading, saving, and updating all user managed data saved in local storage. */
class PreferencesSystem {
    private static _preferences: Partial<Preferences>
    private static _localStorageKey = "Preferences"

    /** Event dispatched when a specific user preference is updated, returns a function to unsubscribe */
    public static addPreferenceEventListener<P extends UserPreference>(
        preference: P,
        callback: (e: UserPreferenceEvent<P>) => void
    ) {
        const cb: EventListener = event => {
            if ((event as UserPreferenceEvent<UserPreference>).prefName == preference) {
                callback(event as UserPreferenceEvent<P>)
            }
        }
        window.addEventListener("preferenceChanged", cb)
        return () => {
            window.removeEventListener("preferenceChanged", cb)
        }
    }

    /** Gets any preference from the preferences map */
    private static getPreferenceFamily<K extends keyof Preferences, V extends Preferences[K] & object>(
        key: K,
        defaultValue: V
    ): Preferences[K]
    private static getPreferenceFamily<K extends keyof Preferences>(key: K): Preferences[K] | undefined
    private static getPreferenceFamily<K extends keyof Preferences>(
        key: K,
        defaultValue?: Preferences[K] & object
    ): Preferences[K] | undefined {
        if (this._preferences == undefined) this.loadPreferences()
        if (defaultValue !== undefined) {
            this._preferences[key] = { ...defaultValue, ...(this._preferences[key] ?? {}) }
        }
        return this._preferences[key]
    }

    private static get _robotPreferences(): Preferences[typeof ROBOT_PREFERENCE_KEY] {
        return this.getPreferenceFamily(ROBOT_PREFERENCE_KEY, {})
    }

    private static get _fieldPreferences(): Preferences[typeof FIELD_PREFERENCE_KEY] {
        return this.getPreferenceFamily(FIELD_PREFERENCE_KEY, {})
    }

    private static get _userPreferences(): Preferences[typeof USER_PREFERENCE_KEY] {
        return this.getPreferenceFamily(USER_PREFERENCE_KEY, defaultUserPreferences())
    }

    /**
     * Gets a user preference, or its default value if it does not exist in the preferences map
     */
    public static getUserPreference<K extends UserPreference>(key: K): UserPreferences[K] {
        return this._userPreferences[key] ?? defaultUserPreferences()[key]
    }

    /**
     * Sets a global preference to be a value of a specific type
     */
    public static setUserPreference<K extends UserPreference>(key: K, value: UserPreferences[K]) {
        this._userPreferences[key] = value
        window.dispatchEvent(new UserPreferenceEvent(key, value))
    }

    /**
     * Resolves whether an asset is currently favorited.
     *
     * @param {string} hash - The asset's content hash.
     * @param {boolean} isDefaultFavorite - Whether the asset is a built-in default favorite.
     */
    public static isFavoriteAsset(hash: string, isDefaultFavorite = false): boolean {
        const status = this.getUserPreference("AssemblyFavoriteStatus")[hash]
        if (status === "favorited") return true
        if (status === "unfavorited") return false
        return isDefaultFavorite
    }

    /**
     * Favorites or un-favorites an asset and persists the change.
     *
     * @param {string} hash - The asset's content hash.
     * @param {boolean} favorite - The desired favorite state.
     * @param {boolean} isDefaultFavorite - Whether the asset is a built-in default favorite.
     */
    public static setFavoriteAsset(hash: string, favorite: boolean, isDefaultFavorite = false) {
        const status = { ...this.getUserPreference("AssemblyFavoriteStatus") }

        if (favorite === isDefaultFavorite) {
            // Desired state already matches the default — no override needed.
            delete status[hash]
        } else {
            status[hash] = favorite ? "favorited" : "unfavorited"
        }

        this.setUserPreference("AssemblyFavoriteStatus", status)
        this.savePreferences()
    }

    /**
     * @param {string} assemblyId - The name of the robot assembly to get preference for.
     * @returns {RobotPreferences} Robot preferences found for the given robot, or default robot preferences if none are found.
     */
    public static getRobotPreferences(assemblyId: string): RobotPreferences {
        const mergedPrefs = { ...defaultRobotPreferences(), ...(this._robotPreferences[assemblyId] ?? {}) }
        this._robotPreferences[assemblyId] = mergedPrefs

        return mergedPrefs
    }

    /** Sets the RobotPreferences object for the robot of a specific mira name */
    public static setRobotPreferences(assemblyId: string, value: RobotPreferences) {
        this._robotPreferences[assemblyId] = value
    }

    /** Sets the FieldPreferences object for the field of a specific mira name */
    public static setFieldPreferences(assemblyId: string, value: FieldPreferences) {
        this._fieldPreferences[assemblyId] = value
    }

    /**
     * @param {string} assemblyId - The name of the field assembly to get preference for.
     * @returns {FieldPreferences} Field preferences found for the given field, or default field preferences if none are found.
     */
    public static getFieldPreferences(assemblyId: string): FieldPreferences {
        const mergedPrefs = { ...defaultFieldPreferences(), ...(this._fieldPreferences[assemblyId] ?? {}) }
        this._fieldPreferences[assemblyId] = mergedPrefs
        return mergedPrefs
    }

    public static hasFieldPreferences(assemblyId: string): boolean {
        return this._fieldPreferences[assemblyId] !== undefined
    }

    public static hasRobotPreferences(assemblyId: string): boolean {
        return this._robotPreferences[assemblyId] !== undefined
    }

    /** Gets simulation quality preferences */
    public static getGraphicsPreferences(): GraphicsPreferences {
        let graphicsPrefs = this.getPreferenceFamily(GRAPHICS_PREFERENCE_KEY)

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

    public static setGraphicsPreferences(g: GraphicsPreferences) {
        this._preferences[GRAPHICS_PREFERENCE_KEY] = g
        this.savePreferences()
    }
    public static graphicsPreferencesAreLow(): boolean {
        const current = this.getGraphicsPreferences()
        return current.fancyShadows === false && current.antiAliasing === false
    }

    /** Loads all preferences from local storage. */
    public static loadPreferences() {
        const loadedPrefs = window.localStorage.getItem(this._localStorageKey)

        if (loadedPrefs == undefined) {
            this._preferences = {}
            return
        }

        try {
            const saved: Preferences & UserPreferences = JSON.parse(loadedPrefs)
            saved[USER_PREFERENCE_KEY] ??= defaultUserPreferences()
            const unmigratedKeys = Object.keys(defaultUserPreferences())
                .filter(key => key in saved) // If the key is in the top level preferences, it hasn't been migrated
                .map(key => key as UserPreference)

            unmigratedKeys.forEach(key => {
                const userPreferences = saved[USER_PREFERENCE_KEY] as Record<UserPreference, unknown>
                userPreferences[key] = saved[key]

                delete saved[key]
            })

            this._preferences = saved
            if (unmigratedKeys.length > 0) {
                this.savePreferences()
            }
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
        Object.entries(this._userPreferences).forEach(([key, value]) => {
            window.dispatchEvent(new UserPreferenceEvent(key as UserPreference, value))
        })
    }

    /** Removes all preferences from local storage. */
    public static clearPreferences() {
        window.localStorage.removeItem(this._localStorageKey)
        this._preferences = {}
    }
}

export default PreferencesSystem
