import { useCallback } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import type { Panel } from "@/ui/helpers/UIProviderHelpers"
import ManageZonesBase from "../zones/ManageZonesBase"

interface ProtectedZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ProtectedZonePreferences[]
    selectZone: (zone: ProtectedZonePreferences) => void
    // biome-ignore lint/suspicious/noExplicitAny: Panel generics are intentionally widened
    panel?: Panel<any, any>
}

const ManageZonesInterface: React.FC<ProtectedZonesProps> = ({ selectedField, initialZones, selectZone, panel }) => {
    const persistZones = useCallback((zones: ProtectedZonePreferences[], field: MirabufSceneObject) => {
        const prefs = field.fieldPreferences
        if (prefs) prefs.protectedZones = zones
        field.updateProtectedZones()
    }, [])

    return (
        <ManageZonesBase
            selectedField={selectedField}
            initialZones={initialZones}
            selectZone={selectZone}
            getListItem={zone => ({
                name: zone.name,
                alliance: zone.alliance,
                pointsLabel: `${zone.penaltyPoints} ${zone.penaltyPoints === 1 ? "penalty point" : "penalty points"}`,
            })}
            persistZones={persistZones}
            createNewZone={() => ({
                name: "New Protected Zone",
                alliance: "blue",
                penaltyPoints: 5,
                parentNode: undefined,
                contactType: ContactType.ROBOT_ENTERS,
                activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME],
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            })}
            emptyLabel="No protected zones"
            panel={panel}
        />
    )
}

export default ManageZonesInterface
