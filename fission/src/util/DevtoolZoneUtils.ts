import FieldMiraEditor from "@/mirabuf/FieldMiraEditor"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type {
    BaseZonePreferences,
    ProtectedZonePreferences,
    ScoringZonePreferences,
} from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"

export type ZoneType = "scoring" | "protected"

/**
 * Checks if two zones share the same origin (name, alliance, parent, position).
 * Epsilon-tolerant for float drift from protobuf round-trips.
 * Use only for origin detection (isZoneFromDevtools), not for destructive targeting.
 */
export function sameZoneOrigin(zone1: BaseZonePreferences, zone2: BaseZonePreferences): boolean {
    if (zone1.name !== zone2.name) return false
    if (zone1.alliance !== zone2.alliance) return false
    if (zone1.parentNode !== zone2.parentNode) return false
    const d1 = zone1.deltaTransformation
    const d2 = zone2.deltaTransformation
    if (d1.length !== d2.length) return false
    for (let i = 0; i < d1.length; i++) {
        if (Math.abs(d1[i] - d2[i]) > 1e-6) return false
    }
    return true
}

/**
 * Full equality check for scoring zones, including zone-specific fields.
 * Use for destructive targeting (remove/modify) and deduplication.
 * originalZone in addUserZoneToDevtools is a pre-edit clone; sameScoringZone is intentional
 * there; do not weaken to sameZoneOrigin, which could match a different zone sharing position.
 */
export function sameScoringZone(a: ScoringZonePreferences, b: ScoringZonePreferences): boolean {
    return (
        sameZoneOrigin(a, b) &&
        a.points === b.points &&
        a.destroyGamepiece === b.destroyGamepiece &&
        a.persistentPoints === b.persistentPoints
    )
}

/**
 * Checks if a zone was originally defined in the field file by comparing it with the cached field data.
 */
export function isZoneFromDevtools(zone: BaseZonePreferences, zoneType: ZoneType): boolean {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) return false

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) return false

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        return false
    }

    const devtoolZones = editor.getUserData("devtool:scoring_zones")
    if (!devtoolZones) return false

    return devtoolZones.some(devZone => sameZoneOrigin(devZone, zone))
}

/**
 * Removes a zone from the field file cache permanently.
 * Fails before mutating any state if preconditions are not met.
 */
export async function removeZoneFromDevtools(zone: ScoringZonePreferences, zoneType: "scoring"): Promise<void>
export async function removeZoneFromDevtools(zone: ProtectedZonePreferences, zoneType: "protected"): Promise<void>
export async function removeZoneFromDevtools(
    zone: ScoringZonePreferences | ProtectedZonePreferences,
    zoneType: ZoneType
): Promise<void> {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) throw new Error("No field loaded")

    const cacheId = field.cacheId
    if (!cacheId) throw new Error("Field has no cacheId; cannot persist removal")

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) throw new Error("No field parts found")

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        throw new Error("Protected zone field file removal not yet implemented")
    }

    const devtoolZones = editor.getUserData("devtool:scoring_zones")
    if (!devtoolZones) throw new Error("No devtool zones in cache; nothing to remove")

    const scoringZone = zone as ScoringZonePreferences
    const targetExists = devtoolZones.some(dz => sameScoringZone(dz, scoringZone))
    if (!targetExists) throw new Error(`Zone "${zone.name}" not found in devtool cache`)

    const filteredZones = devtoolZones.filter(devZone => !sameScoringZone(devZone, scoringZone))
    if (filteredZones.length === 0) {
        editor.removeUserData("devtool:scoring_zones")
    } else {
        editor.setUserData("devtool:scoring_zones", filteredZones)
    }

    if (field.fieldPreferences) {
        field.fieldPreferences.scoringZones = field.fieldPreferences.scoringZones.filter(
            z => !sameScoringZone(z, scoringZone)
        )
        PreferencesSystem.savePreferences?.()
        field.updateScoringZones()
    }

    const assembly = field.mirabufInstance.parser.assembly
    const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
    if (!success) {
        throw new Error("Failed to persist changes to cache")
    }
}

/**
 * Modifies a zone in the field file cache permanently by replacing it with updated data.
 * Fails before mutating any state if preconditions are not met.
 */
export async function modifyZoneInDevtools(
    originalZone: ScoringZonePreferences,
    modifiedZone: ScoringZonePreferences,
    zoneType: "scoring"
): Promise<void>
export async function modifyZoneInDevtools(
    originalZone: ProtectedZonePreferences,
    modifiedZone: ProtectedZonePreferences,
    zoneType: "protected"
): Promise<void>
export async function modifyZoneInDevtools(
    originalZone: ScoringZonePreferences | ProtectedZonePreferences,
    modifiedZone: ScoringZonePreferences | ProtectedZonePreferences,
    zoneType: ZoneType
): Promise<void> {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) throw new Error("No field loaded")

    const cacheId = field.cacheId
    if (!cacheId) throw new Error("Field has no cacheId; cannot persist modification")

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) throw new Error("No field parts found")

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        throw new Error("Protected zone field file modification not yet implemented")
    }

    const devtoolZones = editor.getUserData("devtool:scoring_zones")
    if (!devtoolZones) throw new Error("No devtool zones in cache; nothing to modify")

    const scoringOriginal = originalZone as ScoringZonePreferences
    const scoringModified = modifiedZone as ScoringZonePreferences

    const targetExists = devtoolZones.some(dz => sameScoringZone(dz, scoringOriginal))
    if (!targetExists) throw new Error(`Zone "${originalZone.name}" not found in devtool cache`)

    const updatedZones = devtoolZones.map(devZone =>
        sameScoringZone(devZone, scoringOriginal) ? scoringModified : devZone
    )
    editor.setUserData("devtool:scoring_zones", updatedZones)

    if (field.fieldPreferences) {
        field.fieldPreferences.scoringZones = field.fieldPreferences.scoringZones.map(z =>
            sameScoringZone(z, scoringOriginal) ? scoringModified : z
        )
        PreferencesSystem.savePreferences?.()
        field.updateScoringZones()
    }

    const assembly = field.mirabufInstance.parser.assembly
    const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
    if (!success) {
        throw new Error("Failed to persist changes to cache")
    }
}

/**
 * Automatically caches user-created or modified zones to the field file for persistence across reloads.
 * Fails before mutating any state if preconditions are not met.
 */
export async function addUserZoneToDevtools(
    zone: ScoringZonePreferences,
    originalZone: ScoringZonePreferences | undefined,
    zoneType: "scoring"
): Promise<void>
export async function addUserZoneToDevtools(
    zone: ProtectedZonePreferences,
    originalZone: ProtectedZonePreferences | undefined,
    zoneType: "protected"
): Promise<void>
export async function addUserZoneToDevtools(
    zone: ScoringZonePreferences | ProtectedZonePreferences,
    originalZone: ScoringZonePreferences | ProtectedZonePreferences | undefined,
    zoneType: ZoneType
): Promise<void> {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) throw new Error("No field loaded")

    const cacheId = field.cacheId
    if (!cacheId) throw new Error("Field has no cacheId; cannot persist zone addition")

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) throw new Error("No field parts found")

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        throw new Error("Protected zone field file addition not yet implemented")
    }

    const scoringZone = zone as ScoringZonePreferences
    const devtoolZones = editor.getUserData("devtool:scoring_zones") || []

    let updated = false

    if (originalZone) {
        // originalZone is a pre-edit clone; sameScoringZone matches the exact cached entry
        const existingIndex = devtoolZones.findIndex(devZone =>
            sameScoringZone(devZone, originalZone as ScoringZonePreferences)
        )
        if (existingIndex >= 0) {
            devtoolZones[existingIndex] = scoringZone
            updated = true
        }
    }

    if (!updated) {
        const zoneExists = devtoolZones.some(devZone => sameScoringZone(devZone, scoringZone))
        if (!zoneExists) {
            devtoolZones.push(scoringZone)
        }
    }

    editor.setUserData("devtool:scoring_zones", devtoolZones)

    // fieldPreferences.scoringZones is already up to date from save(); overwriting it here
    // with only devtool zones would drop any user zones not yet in the cache.
    const assembly = field.mirabufInstance.parser.assembly
    const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
    if (!success) {
        throw new Error("Failed to persist changes to cache")
    }
}

/**
 * Gets all zones that exist in the field file for a given type.
 */
export function getDevtoolZones(zoneType: "scoring"): ScoringZonePreferences[] | undefined
export function getDevtoolZones(zoneType: "protected"): ProtectedZonePreferences[] | undefined
export function getDevtoolZones(zoneType: ZoneType): ScoringZonePreferences[] | ProtectedZonePreferences[] | undefined {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) return undefined

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) return undefined

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        return undefined
    }

    return editor.getUserData("devtool:scoring_zones")
}
