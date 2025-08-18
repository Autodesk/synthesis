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
 * Checks if two zones are equal by comparing their common base properties
 */
function zonesEqual(zone1: BaseZonePreferences, zone2: BaseZonePreferences): boolean {
    return (
        zone1.name === zone2.name &&
        zone1.alliance === zone2.alliance &&
        zone1.parentNode === zone2.parentNode &&
        JSON.stringify(zone1.deltaTransformation) === JSON.stringify(zone2.deltaTransformation)
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

    const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
    if (!devtoolZones) return false

    return devtoolZones.some(devZone => zonesEqual(devZone, zone))
}

/**
 * Removes a zone from the field file cache permanently.
 */
export async function removeZoneFromDevtools(
    zone: ScoringZonePreferences | ProtectedZonePreferences,
    zoneType: ZoneType
): Promise<void> {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) throw new Error("No field loaded")

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) throw new Error("No field parts found")

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        throw new Error("Protected zone field file removal not yet implemented")
    }

    const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
    if (!devtoolZones) return

    const filteredZones = devtoolZones.filter(devZone => !zonesEqual(devZone, zone))
    if (filteredZones.length === 0) {
        editor.removeUserData("devtool:scoring_zones")
    } else {
        editor.setUserData("devtool:scoring_zones", filteredZones)
    }

    if (field.fieldPreferences) {
        field.fieldPreferences.scoringZones = filteredZones
        PreferencesSystem.savePreferences?.()
        field.updateScoringZones()
    }

    const assembly = field.mirabufInstance.parser.assembly
    const cacheId = field.cacheId
    if (cacheId) {
        const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
        if (!success) {
            throw new Error("Failed to persist changes to cache")
        }
    }
}

/**
 * Modifies a zone in the field file cache permanently by replacing it with updated data.
 */
export async function modifyZoneInDevtools(
    originalZone: ScoringZonePreferences | ProtectedZonePreferences,
    modifiedZone: ScoringZonePreferences | ProtectedZonePreferences,
    zoneType: ZoneType
): Promise<void> {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) throw new Error("No field loaded")

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) throw new Error("No field parts found")

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        throw new Error("Protected zone field file modification not yet implemented")
    }

    const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
    if (!devtoolZones) return

    // Find and replace the zone in field file data
    const updatedZones = devtoolZones.map(devZone => {
        if (zonesEqual(devZone, originalZone)) {
            return modifiedZone
        }
        return devZone
    })

    editor.setUserData("devtool:scoring_zones", updatedZones as ScoringZonePreferences[])

    if (field.fieldPreferences) {
        field.fieldPreferences.scoringZones = updatedZones as ScoringZonePreferences[]
        PreferencesSystem.savePreferences?.()
        field.updateScoringZones()
    }

    const assembly = field.mirabufInstance.parser.assembly
    const cacheId = field.cacheId
    if (cacheId) {
        const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
        if (!success) {
            throw new Error("Failed to persist changes to cache")
        }
    }
}

/**
 * Gets all zones that exist in the field file for a given type.
 */
export function getDevtoolZones(zoneType: ZoneType): ScoringZonePreferences[] | ProtectedZonePreferences[] | undefined {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) return undefined

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) return undefined

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "protected") {
        return undefined
    }

    return editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
}
