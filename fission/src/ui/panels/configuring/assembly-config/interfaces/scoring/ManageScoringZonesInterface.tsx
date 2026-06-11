import { useCallback, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import DevtoolZoneModificationModal from "@/ui/modals/DevtoolZoneModificationModal"
import type { Panel } from "@/ui/helpers/UIProviderHelpers"
import { isZoneFromDevtools, removeZoneFromDevtools } from "@/util/DevtoolZoneUtils"
import ManageZonesBase from "../zones/ManageZonesBase"

interface ScoringZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
    selectZone: (zone: ScoringZonePreferences) => void
    // biome-ignore lint/suspicious/noExplicitAny: Panel generics are intentionally widened
    panel?: Panel<any, any>
}

const ManageZonesInterface: React.FC<ScoringZonesProps> = ({ selectedField, initialZones, selectZone, panel }) => {
    // refreshKey forces ManageZonesBase to remount after a permanent removal.
    // ManageZonesBase initializes local zone state from initialZones once on mount;
    // permanent removal updates fieldPreferences externally, so a remount is required
    // to pick up the updated zone list.
    const [refreshKey, setRefreshKey] = useState(0)
    const [confirmModal, setConfirmModal] = useState<{
        isOpen: boolean
        zone: ScoringZonePreferences | null
        confirmDelete: (() => void) | null
    }>({ isOpen: false, zone: null, confirmDelete: null })

    const closeConfirmModal = useCallback(() => {
        setConfirmModal({ isOpen: false, zone: null, confirmDelete: null })
    }, [])

    const persistZones = useCallback((zones: ScoringZonePreferences[], field: MirabufSceneObject) => {
        const prefs = field.fieldPreferences
        if (prefs) prefs.scoringZones = zones
        field.updateScoringZones()
    }, [])

    const handleBeforeDelete = useCallback((zone: ScoringZonePreferences, confirmDelete: () => void) => {
        if (isZoneFromDevtools(zone, "scoring")) {
            setConfirmModal({ isOpen: true, zone, confirmDelete })
        } else {
            confirmDelete()
        }
    }, [])

    const handlePermanentRemoval = async () => {
        if (!confirmModal.zone) return
        await removeZoneFromDevtools(confirmModal.zone, "scoring")
        closeConfirmModal()
        setRefreshKey(k => k + 1)
    }

    return (
        <>
            <ManageZonesBase
                key={refreshKey}
                selectedField={selectedField}
                initialZones={selectedField.fieldPreferences?.scoringZones ?? initialZones}
                selectZone={selectZone}
                getListItem={zone => ({
                    name: zone.name,
                    alliance: zone.alliance,
                    pointsLabel: `${zone.points} ${zone.points === 1 ? "point" : "points"}`,
                })}
                persistZones={persistZones}
                createNewZone={() => ({
                    name: "New Scoring Zone",
                    alliance: "blue",
                    parentNode: undefined,
                    points: 0,
                    destroyGamepiece: false,
                    persistentPoints: false,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                })}
                emptyLabel="No scoring zones"
                onBeforeDelete={handleBeforeDelete}
                panel={panel}
            />
            <DevtoolZoneModificationModal
                isOpen={confirmModal.isOpen}
                onClose={closeConfirmModal}
                mode="remove"
                zoneType="scoring"
                zoneName={confirmModal.zone?.name ?? ""}
                onTemporaryAction={() => {
                    confirmModal.confirmDelete?.()
                    closeConfirmModal()
                }}
                onPermanentAction={handlePermanentRemoval}
            />
        </>
    )
}

export default ManageZonesInterface
