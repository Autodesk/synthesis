import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import FieldMiraEditor from "@/mirabuf/FieldMiraEditor"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences, ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"

export type ZoneType = "scoring" | "protected"

/**
 * Checks if a zone was originally placed by dev tools by comparing it with the cached dev tool data.
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

        return devtoolZones.some(devZone => 
            devZone.name === zone.name &&
            devZone.alliance === zone.alliance &&
            devZone.parentNode === zone.parentNode &&
            JSON.stringify(devZone.deltaTransformation) === JSON.stringify(zone.deltaTransformation)
        )
    } else {
        // For protected zones, we'd need to add dev tool support first
        // For now, return false as protected zones don't have dev tool support yet
        return false
    }
}

/**
 * Removes a zone from dev tools cache permanently.
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

        // Remove the zone from dev tool data
        const filteredZones = devtoolZones.filter(devZone =>
            !(devZone.name === zone.name &&
              devZone.alliance === zone.alliance &&
              devZone.parentNode === zone.parentNode &&
              JSON.stringify(devZone.deltaTransformation) === JSON.stringify(zone.deltaTransformation))
        )

        // Update the dev tool data
        if (filteredZones.length === 0) {
            editor.removeUserData("devtool:scoring_zones")
        } else {
            editor.setUserData("devtool:scoring_zones", filteredZones)
        }

        // Update field preferences to match the filtered dev tool data
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
        throw new Error("Protected zone dev tool removal not yet implemented")
    }
}

/**
 * Gets all zones that exist in dev tools for a given type.
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