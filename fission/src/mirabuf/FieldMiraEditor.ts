import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { ContactType } from "@/mirabuf/ZoneTypes.ts"
import { mirabuf } from "@/proto/mirabuf"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes.ts"
import {
    defaultFieldPreferences,
    type FieldPreferences,
    type ProtectedZonePreferences,
    type ScoringZonePreferences,
} from "@/systems/preferences/PreferenceTypes"

export interface DevtoolMiraData {
    "devtool:scoring_zones": ScoringZonePreferences[]
    "devtool:protected_zones": ProtectedZonePreferences[]
    "devtool:spawn_locations": FieldPreferences["spawnLocations"]
    "devtool:a": unknown
    "devtool:b": unknown
    "devtool:test": unknown
    "devtool:keep": unknown
    "devtool:drop": unknown
    "devtool:bad": unknown
    "devtool:foo": unknown
    // additional devtool keys to be added in future
}

export const devtoolHandlers = {
    "devtool:scoring_zones": {
        get(field) {
            return field.fieldPreferences?.scoringZones ?? defaultFieldPreferences().scoringZones
        },
        set(field, val) {
            val ??= defaultFieldPreferences().scoringZones
            if (!field.fieldPreferences || !this.validate(val)) {
                console.warn("validation failed", val, field.fieldPreferences)
                return
            }
            field.fieldPreferences.scoringZones = val
            field.updateScoringZones()
        },
        validate(val): val is ScoringZonePreferences[] {
            if (!Array.isArray(val)) return false
            return val.every(
                z =>
                    typeof z === "object" &&
                    z !== null &&
                    typeof z.name === "string" &&
                    (z.alliance === "red" || z.alliance === "blue") &&
                    (typeof z.parentNode === "string" || z.parentNode === undefined) &&
                    typeof z.points === "number" &&
                    typeof z.destroyGamepiece === "boolean" &&
                    typeof z.persistentPoints === "boolean" &&
                    Array.isArray(z.deltaTransformation)
            )
        },
    },
    "devtool:protected_zones": {
        get(field) {
            return field.fieldPreferences?.protectedZones ?? defaultFieldPreferences().protectedZones
        },
        set(field, val) {
            val ??= defaultFieldPreferences().protectedZones
            if (!field.fieldPreferences || !this.validate(val)) {
                console.warn("validation failed", val, field.fieldPreferences)
                return
            }
            field.fieldPreferences.protectedZones = val
            field.updateProtectedZones()
        },
        validate(val): val is ProtectedZonePreferences[] {
            if (!Array.isArray(val)) return false
            return val.every(
                z =>
                    typeof z === "object" &&
                    z !== null &&
                    typeof z.name === "string" &&
                    (z.alliance === "red" || z.alliance === "blue") &&
                    (typeof z.parentNode === "string" || z.parentNode === undefined) &&
                    typeof z.penaltyPoints === "number" &&
                    typeof z.contactType === "string" &&
                    Object.values(ContactType).includes(z.contactType as ContactType) &&
                    Array.isArray(z.activeDuring) &&
                    z.activeDuring.every(
                        (v: unknown) =>
                            typeof v === "string" && Object.values(MatchModeType).includes(v as MatchModeType)
                    ) &&
                    Array.isArray(z.deltaTransformation)
            )
        },
    },
    "devtool:spawn_locations": {
        get(field) {
            return field.fieldPreferences?.spawnLocations ?? defaultFieldPreferences().spawnLocations
        },
        set(field, val) {
            val ??= defaultFieldPreferences().spawnLocations
            if (!field.fieldPreferences || !this.validate(val)) {
                console.warn("validation failed", val, field.fieldPreferences)
                return
            }
            field.fieldPreferences.spawnLocations = val
        },
        validate(val: unknown): val is FieldPreferences["spawnLocations"] {
            const isStructureCorrect =
                typeof val === "object" &&
                val != null &&
                "red" in val &&
                "blue" in val &&
                "default" in val &&
                "hasConfiguredLocations" in val

            if (!isStructureCorrect) return false
            return (["red", "blue"] as const).every(v => {
                const obj = val[v]
                if (!(typeof obj === "object" && obj != null && 1 in obj && 2 in obj && 3 in obj)) return false
                return ([1, 2, 3] as const).every(v => {
                    const spawnposition = obj[v]
                    return (
                        typeof spawnposition == "object" &&
                        spawnposition != null &&
                        "pos" in spawnposition &&
                        "yaw" in spawnposition &&
                        Array.isArray(spawnposition["pos"]) &&
                        spawnposition["pos"].length == 3 &&
                        typeof spawnposition["yaw"] == "number"
                    )
                })
            })
        },
    },
} as const satisfies Partial<{
    [K in keyof DevtoolMiraData]: {
        get(field: MirabufSceneObject): DevtoolMiraData[K]
        set(field: MirabufSceneObject, val: unknown | null): void
        validate(val: unknown): val is DevtoolMiraData[K]
    }
}>

export type DevtoolKey = keyof typeof devtoolHandlers
export const devtoolKeys = Object.keys(devtoolHandlers) as DevtoolKey[]

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
    getAllDevtoolKeys(): string[] {
        return Object.keys(this._parts.userData!.data!).filter(k => k.startsWith("devtool:"))
    }
}
