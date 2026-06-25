import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageZonesBase, { type ManageZonesBaseProps } from "../zones/ManageZonesBase"

function persistScoringZones(zones: ScoringZonePreferences[], field: MirabufSceneObject) {
    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones
    field.updateScoringZones()
}

interface ScoringZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
    selectZone: (zone: ScoringZonePreferences) => void
}

const ManageScoringZonesInterface: React.FC<ScoringZonesProps> = ({ selectedField, initialZones, selectZone }) => {
    const baseProps: ManageZonesBaseProps<ScoringZonePreferences> = {
        selectedField,
        initialZones,
        selectZone,
        getListItem: zone => ({
            name: zone.name,
            alliance: zone.alliance,
            pointsLabel: `${zone.points} ${zone.points === 1 ? "point" : "points"}`,
        }),
        persistZones: persistScoringZones,
        createNewZone: () => ({
            name: "New Scoring Zone",
            alliance: "blue",
            parentNode: undefined,
            points: 0,
            destroyGamepiece: false,
            shouldPointsAccumulate: true,
            deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
        }),
        emptyLabel: "No scoring zones",
    }

    return <ManageZonesBase {...baseProps} />
}

export default ManageScoringZonesInterface
