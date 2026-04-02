import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageZonesBase, { type ManageZonesBaseProps } from "../zones/ManageZonesBase"

function persistProtectedZones(zones: ProtectedZonePreferences[], field: MirabufSceneObject) {
    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.protectedZones = zones
    field.updateProtectedZones()
}


interface ProtectedZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ProtectedZonePreferences[]
    selectZone: (zone: ProtectedZonePreferences) => void
}

const ManageProtectedZonesInterface: React.FC<ProtectedZonesProps> = ({ selectedField, initialZones, selectZone }) => {
    const baseProps: ManageZonesBaseProps<ProtectedZonePreferences> = {
        selectedField,
        initialZones,
        selectZone,
        getListItem: zone => ({
            name: zone.name,
            alliance: zone.alliance,
            pointsLabel: `${zone.penaltyPoints} ${zone.penaltyPoints === 1 ? "penalty point" : "penalty points"}`,
        }),
        persistZones: persistProtectedZones,
        createNewZone: () => ({
            name: "New Protected Zone",
            alliance: "blue",
            penaltyPoints: 5,
            parentNode: undefined,
            contactType: ContactType.ROBOT_ENTERS,
            activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME],
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
        }),
        emptyLabel: "No protected zones",
    }

    return <ManageZonesBase {...baseProps} />
}

export default ManageProtectedZonesInterface
