import { Box, Stack } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import { AddButton, DeleteButton, EditButton } from "@/ui/components/StyledComponents"
import DevtoolZoneRemovalModal from "@/ui/modals/DevtoolZoneRemovalModal"
import { isZoneFromDevtools, removeZoneFromDevtools } from "@/util/DevtoolZoneUtils"

const saveZones = (zones: ScoringZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones

    PreferencesSystem.savePreferences()
    field.updateScoringZones()
}

type ScoringZoneRowProps = {
    zone: ScoringZonePreferences
    save: () => void
    deleteZone: () => void
    selectZone: (zone: ScoringZonePreferences) => void
    onShowConfirmation: (zone: ScoringZonePreferences) => void
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, save, deleteZone, selectZone, onShowConfirmation }) => {
    const handleDeleteClick = () => {
        if (isZoneFromDevtools(zone, "scoring")) {
            onShowConfirmation(zone)
        } else {
            deleteZone()
        }
    }

    return (
        <Stack justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction="row" gap={8}>
                <Box
                    className={`w-12 h-12 rounded-lg`}
                    sx={{
                        bgcolor: zone.alliance === "red" ? "redAlliance.main" : "blueAlliance.main",
                    }}
                />
                <Stack direction="row" gap={4} className="w-max">
                    <Label size="sm">{zone.name}</Label>
                    <Label size="sm">
                        {zone.points} {zone.points === 1 ? "point" : "points"}
                    </Label>
                </Stack>
            </Stack>
            <Stack direction={"row-reverse"} gap={"0.25rem"} justifyContent={"center"} alignItems={"center"}>
                {EditButton(() => {
                    selectZone(zone)
                    save()
                })}

                {DeleteButton(() => {
                    handleDeleteClick()
                })}
            </Stack>
        </Stack>
    )
}

interface ScoringZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
    selectZone: (zone: ScoringZonePreferences) => void
}

const ManageZonesInterface: React.FC<ScoringZonesProps> = ({ selectedField, initialZones, selectZone }) => {
    const [zones, setZones] = useState<ScoringZonePreferences[]>(initialZones)
    const [confirmationModal, setConfirmationModal] = useState<{
        isOpen: boolean
        zone: ScoringZonePreferences | null
        zoneIndex: number
    }>({ isOpen: false, zone: null, zoneIndex: -1 })

    const saveEvent = useCallback(() => {
        saveZones(zones, selectedField)
    }, [zones, selectedField])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.removeListener(saveEvent)
        }
    }, [saveEvent])

    useEffect(() => {
        saveZones(zones, selectedField)

        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [selectedField, zones])

    const handleShowConfirmation = (zone: ScoringZonePreferences) => {
        const zoneIndex = zones.indexOf(zone)
        setConfirmationModal({ isOpen: true, zone, zoneIndex })
    }

    const handleTemporaryRemoval = () => {
        if (confirmationModal.zoneIndex >= 0) {
            const newZones = zones.filter((_, idx) => idx !== confirmationModal.zoneIndex)
            setZones(newZones)
            saveZones(newZones, selectedField)
        }
    }

    const handlePermanentRemoval = async () => {
        if (confirmationModal.zone) {
            await removeZoneFromDevtools(confirmationModal.zone, "scoring")
            const updatedZones = selectedField.fieldPreferences?.scoringZones ?? []
            setZones(updatedZones)
        }
    }

    const handleCloseConfirmation = () => {
        setConfirmationModal({ isOpen: false, zone: null, zoneIndex: -1 })
    }

    return (
        <>
            {zones?.length > 0 ? (
                <ScrollView>
                    <Stack gap={4}>
                        {zones.map((zonePrefs: ScoringZonePreferences, i: number) => (
                            <ScoringZoneRow
                                key={i}
                                zone={(() => {
                                    return zonePrefs
                                })()}
                                save={() => saveZones(zones, selectedField)}
                                deleteZone={() => {
                                    setZones(zones.filter((_, idx) => idx !== i))
                                    saveZones(
                                        zones.filter((_, idx) => idx !== i),
                                        selectedField
                                    )
                                }}
                                selectZone={selectZone}
                                onShowConfirmation={handleShowConfirmation}
                            />
                        ))}
                    </Stack>
                </ScrollView>
            ) : (
                <Label size="md">No scoring zones</Label>
            )}
            {AddButton(() => {
                if (zones === undefined) return

                const newZone: ScoringZonePreferences = {
                    name: "New Scoring Zone",
                    alliance: "blue",
                    parentNode: undefined,
                    points: 0,
                    destroyGamepiece: false,
                    persistentPoints: false,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                }

                saveZones(zones, selectedField)

                selectZone(newZone)
            })}

            <DevtoolZoneRemovalModal
                isOpen={confirmationModal.isOpen}
                onClose={handleCloseConfirmation}
                zoneType="scoring"
                zoneName={confirmationModal.zone?.name ?? ""}
                onTemporaryRemoval={handleTemporaryRemoval}
                onPermanentRemoval={handlePermanentRemoval}
            />
        </>
    )
}

export default ManageZonesInterface
