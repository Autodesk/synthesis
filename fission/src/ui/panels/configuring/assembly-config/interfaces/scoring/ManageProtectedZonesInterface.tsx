import { Stack } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import { AddButton, DeleteButton, EditButton } from "@/ui/components/StyledComponents"

const saveZones = (zones: ProtectedZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.protectedZones = zones

    PreferencesSystem.savePreferences()
    field.updateProtectedZones()
}

type ProtectedZoneRowProps = {
    zone: ProtectedZonePreferences
    save: () => void
    deleteZone: () => void
    selectZone: (zone: ProtectedZonePreferences) => void
}

const ProtectedZoneRow: React.FC<ProtectedZoneRowProps> = ({ zone, save, deleteZone, selectZone }) => {
    return (
        <Stack justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction="row" gap={8}>
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack gap={4} className="w-max">
                    <Label size="sm">{zone.name}</Label>
                    <Label size="sm">
                        {zone.penaltyPoints} {zone.penaltyPoints === 1 ? "penalty point" : "penalty points"}
                    </Label>
                </Stack>
            </Stack>
            <Stack direction="row-reverse" gap={"0.5rem"} justifyContent={"center"} alignItems={"center"}>
                {EditButton(() => {
                    selectZone(zone)
                    save()
                })}

                {DeleteButton(() => {
                    deleteZone()
                })}
            </Stack>
        </Stack>
    )
}

interface ProtectedZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ProtectedZonePreferences[]
    selectZone: (zone: ProtectedZonePreferences) => void
}

const ManageZonesInterface: React.FC<ProtectedZonesProps> = ({ selectedField, initialZones, selectZone }) => {
    const [zones, setZones] = useState<ProtectedZonePreferences[]>(initialZones)

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

    return (
        <>
            {zones?.length > 0 ? (
                <ScrollView>
                    <Stack gap={4}>
                        {zones.map((zonePrefs: ProtectedZonePreferences, i: number) => (
                            <ProtectedZoneRow
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
                            />
                        ))}
                    </Stack>
                </ScrollView>
            ) : (
                <Label size="md">No protected zones</Label>
            )}
            {AddButton(() => {
                if (zones === undefined) return

                const newZone: ProtectedZonePreferences = {
                    name: "New Protected Zone",
                    alliance: "blue",
                    penaltyPoints: 5,
                    parentNode: undefined,
                    contactType: ContactType.ROBOT_ENTERS,
                    activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP, MatchModeType.ENDGAME],
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                }

                saveZones(zones, selectedField)

                selectZone(newZone)
            })}
        </>
    )
}

export default ManageZonesInterface
