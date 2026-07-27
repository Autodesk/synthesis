import { mirabuf } from "@/proto/mirabuf"
import {
    defaultFieldPreferences,
    type FieldPreferences,
    type ProtectedZonePreferences,
    type RobotPreferences,
    type ScoringZonePreferences,
} from "@/systems/preferences/PreferenceTypes"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"

export interface DevtoolMiraData {
    "synthesis:field_preferences": FieldPreferences
    "synthesis:robot_preferences": RobotPreferences
    "devtool:scoring_zones": ScoringZonePreferences[]
    "devtool:protected_zones": ProtectedZonePreferences[]
    "devtool:spawn_locations": FieldPreferences["spawnLocations"]
    "devtool:robot_ejector": RobotPreferences["ejector"]
    "devtool:robot_intake": RobotPreferences["intake"]
    // additional devtool keys to be added in future
}

export type SynthesisDevtoolKey = keyof DevtoolMiraData & `synthesis:${string}`

export type DevtoolHandlerMap = {
    [K in SynthesisDevtoolKey]: {
        get(object: MirabufSceneObject): DevtoolMiraData[K]
        set(object: MirabufSceneObject, val: DevtoolMiraData[K]): void
    }
}

export const DEVTOOL_HANDLERS: DevtoolHandlerMap = {
    "synthesis:field_preferences": {
        get(object) {
            return object.fieldPreferences ?? defaultFieldPreferences()
        },
        set(object, value) {
            PreferencesSystem.setFieldPreferences(object.assemblyId, value)
            object.loadPreferences(false)
            object.updateProtectedZones()
            object.updateScoringZones()
        },
    },
    "synthesis:robot_preferences": {
        get(object) {
            return object.robotPreferences
        },
        set(object, value) {
            PreferencesSystem.setRobotPreferences(object.assemblyId, value)
            object.loadPreferences(false)
            object.updateIntakeSensor()
        },
    },
}

/**
 * Utility for reading and writing developer tool data in the mira file's UserData field.
 * Docs: https://www.mirabuf.dev/#mirabuf.UserData
 */
export default class FieldMiraEditor {
    private _parts: mirabuf.IParts

    constructor(parts: mirabuf.IParts) {
        this._parts = parts
        if (!this._parts.userData) {
            this._parts.userData = mirabuf.UserData.create({ data: {} })
        }
        if (!this._parts.userData.data) {
            this._parts.userData.data = {}
        }
    }

    migrateDevtoolFieldData(prefs: FieldPreferences): void {
        const scoringZones = this.getUserData("devtool:scoring_zones")
        if (scoringZones !== undefined) {
            prefs.scoringZones = scoringZones
        }
        const protectedZones = this.getUserData("devtool:protected_zones")
        if (protectedZones !== undefined) {
            prefs.protectedZones = protectedZones
        }
        const spawnLocations = this.getUserData("devtool:spawn_locations")
        if (spawnLocations !== undefined) {
            prefs.spawnLocations = spawnLocations
        }

        this.removeUserData("devtool:scoring_zones")
        this.removeUserData("devtool:protected_zones")
        this.removeUserData("devtool:spawn_locations")
    }

    migrateDevtoolRobotData(prefs: RobotPreferences): void {
        const intakeSettings = this.getUserData("devtool:robot_intake")
        if (intakeSettings !== undefined) {
            prefs.intake = intakeSettings
        }
        const ejectorSettings = this.getUserData("devtool:robot_ejector")
        if (ejectorSettings !== undefined) {
            prefs.ejector = ejectorSettings
        }
        this.removeUserData("devtool:robot_intake")
        this.removeUserData("devtool:robot_ejector")
    }
    /**
     * Get parsed data for a devtool key (e.g., 'devtool:scoring_zones').
     */
    getUserData<K extends keyof DevtoolMiraData>(key: K): DevtoolMiraData[K] | undefined {
        const raw = this._parts.userData!.data![key]
        if (!raw) return undefined
        try {
            return JSON.parse(raw)
        } catch {
            return undefined
        }
    }

    /**
     * Set data for a devtool key. Value will be stringified as JSON.
     */
    setUserData<K extends keyof DevtoolMiraData & `synthesis:${string}`>(key: K, value: DevtoolMiraData[K]): void {
        this._parts.userData!.data![key] = JSON.stringify(value)
    }

    /**
     * Remove a devtool key from userData.
     */
    removeUserData(key: keyof DevtoolMiraData): void {
        delete this._parts.userData!.data![key]
    }

    /**
     * Get all devtool keys currently in userData.
     */
    getSynthesisKeys(): SynthesisDevtoolKey[] {
        return Object.keys(this._parts.userData!.data!)
            .filter(k => k.startsWith("synthesis:"))
            .map(key => key as SynthesisDevtoolKey)
    }
}
