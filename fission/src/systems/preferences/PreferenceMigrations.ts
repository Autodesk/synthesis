
import { Preferences } from "./PreferenceTypes"

type Migration = (prefs: any) => any

// Migrations are written to be forward-compatible
const migrations: { [key: string]: Migration } = {
    "7.2.0": prefs => {
        prefs.version = "7.2.0"
        return prefs
    },
}

export function migratePreferences(prefs: Partial<Preferences>): Partial<Preferences> {
    const version = prefs.version ?? "0.0.0"

    const versionsToMigrate = Object.keys(migrations).sort()

    let migratedPrefs = { ...prefs }
    for (const v of versionsToMigrate) {
        if (version < v) {
            migratedPrefs = migrations[v](migratedPrefs)
        }
    }

    return migratedPrefs
}
