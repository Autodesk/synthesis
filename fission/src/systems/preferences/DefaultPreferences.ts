class DefaultPreferences {
    private static _defaultPreferences: { [key: string]: Object } = {
        "screenMode": "Windowed",
        "qualitySettings": "High",
        "zoomSensitivity": 15,
        "pitchSensitivity": 10,
        "yawSensitivity": 3,
        "reportAnalytics": false,
        "useMetric": false,
        "renderScoringZones": true,
    }

    public static getDefaultPreference(key: string) {
        return this._defaultPreferences[key]
    }
}

export default DefaultPreferences