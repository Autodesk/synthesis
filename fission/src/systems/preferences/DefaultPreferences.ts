class DefaultPreferences {
    private static _defaultPreferences: { [key: string]: Object } = {
        "renderScoringZones": false,
        "recordAnalytics": true
    }

    public static getDefaultPreference(key: string) {
        return this._defaultPreferences[key]
    }
}

export default DefaultPreferences