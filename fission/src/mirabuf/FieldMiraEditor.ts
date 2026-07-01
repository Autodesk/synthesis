import { mirabuf } from "@/proto/mirabuf"
import type { FieldPreferences, RobotPreferences } from "@/systems/preferences/PreferenceTypes"

export interface DevtoolMiraData {
    "synthesis:field_preferences": FieldPreferences
    "synthesis:robot_preferences": RobotPreferences
    "devtool:a": unknown
    "devtool:b": unknown
    "devtool:test": unknown
    "devtool:keep": unknown
    "devtool:drop": unknown
    "devtool:bad": unknown
    "devtool:foo": unknown
    // additional devtool keys to be added in future
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
    setUserData<K extends keyof DevtoolMiraData>(key: K, value: DevtoolMiraData[K]): void {
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
    getAllSynthesisKeys(): string[] {
        return Object.keys(this._parts.userData!.data!).filter(k => k.startsWith("synthesis:"))
    }
}
