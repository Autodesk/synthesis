import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import FieldMiraEditor from "@/mirabuf/FieldMiraEditor"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences, ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"

export type ZoneType = "scoring" | "protected"

/**
 * Checks if a zone was originally defined in the field file by comparing it with the cached field data.
 */
export function isZoneFromDevtools(
    zone: ScoringZonePreferences | ProtectedZonePreferences,
    zoneType: ZoneType
): boolean {
    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (!field) return false

    const parts = field.mirabufInstance.parser.assembly.data?.parts
    if (!parts) return false

    const editor = new FieldMiraEditor(parts)

    if (zoneType === "scoring") {
        const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
        if (!devtoolZones) return false

        return devtoolZones.some(
            devZone =>
                devZone.name === zone.name &&
                devZone.alliance === zone.alliance &&
                devZone.parentNode === zone.parentNode &&
                JSON.stringify(devZone.deltaTransformation) === JSON.stringify(zone.deltaTransformation)
        )
    } else {
        // For protected zones, we'd need to add field file support first
        // For now, return false as protected zones don't have field file support yet
        return false
    }
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

    if (zoneType === "scoring") {
        const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
        if (!devtoolZones) return

        // Remove the zone from field file data
        const filteredZones = devtoolZones.filter(
            devZone =>
                !(
                    devZone.name === zone.name &&
                    devZone.alliance === zone.alliance &&
                    devZone.parentNode === zone.parentNode &&
                    JSON.stringify(devZone.deltaTransformation) === JSON.stringify(zone.deltaTransformation)
                )
        )

        // Update the field file data
        if (filteredZones.length === 0) {
            editor.removeUserData("devtool:scoring_zones")
        } else {
            editor.setUserData("devtool:scoring_zones", filteredZones)
        }

        // Update field preferences to match the filtered field file data
        if (field.fieldPreferences) {
            field.fieldPreferences.scoringZones = filteredZones
            PreferencesSystem.savePreferences?.()
            field.updateScoringZones()
        }

        // Persist changes to cache
        const assembly = field.mirabufInstance.parser.assembly
        const cacheId = field.cacheId
        if (cacheId) {
            const success = await MirabufCachingService.persistDevtoolChanges(cacheId, MiraType.FIELD, assembly)
            if (!success) {
                throw new Error("Failed to persist changes to cache")
            }
        }
    } else {
        throw new Error("Protected zone field file removal not yet implemented")
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

    if (zoneType === "scoring") {
        const devtoolZones = editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
        if (!devtoolZones) return

        // Find and replace the zone in field file data
        const updatedZones = devtoolZones.map(devZone => {
            if (
                devZone.name === originalZone.name &&
                devZone.alliance === originalZone.alliance &&
                devZone.parentNode === originalZone.parentNode &&
                JSON.stringify(devZone.deltaTransformation) === JSON.stringify(originalZone.deltaTransformation)
            ) {
                return modifiedZone as ScoringZonePreferences
            }
            return devZone
        })

        editor.setUserData("devtool:scoring_zones", updatedZones)

        if (field.fieldPreferences) {
            field.fieldPreferences.scoringZones = updatedZones
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
    } else {
        throw new Error("Protected zone field file modification not yet implemented")
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

    if (zoneType === "scoring") {
        return editor.getUserData("devtool:scoring_zones") as ScoringZonePreferences[] | undefined
    } else {
        return undefined
    }
}
