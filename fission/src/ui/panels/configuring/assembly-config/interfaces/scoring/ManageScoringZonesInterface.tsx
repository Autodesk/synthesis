import { useCallback, useEffect, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Box } from "@mui/material"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"
import { AddButtonInteractiveColor, DeleteButton, EditButton } from "@/ui/components/StyledComponents"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"

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
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, save, deleteZone, selectZone }) => {
    return (
        <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction={StackDirection.HORIZONTAL} spacing={8} justify="start">
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack direction={StackDirection.VERTICAL} spacing={4} justify={"center"} className="w-max">
                    <Label size={LabelSize.SMALL}>{zone.name}</Label>
                    <Label size={LabelSize.SMALL}>
                        {zone.points} {zone.points == 1 ? "point" : "points"}
                    </Label>
                </Stack>
            </Stack>
            <Box
                component={"div"}
                display={"flex"}
                flexDirection={"row-reverse"}
                gap={"0.25rem"}
                justifyContent={"center"}
                alignItems={"center"}
            >
                {EditButton(() => {
                    selectZone(zone)
                    save()
                })}

                {DeleteButton(() => {
                    deleteZone()
                })}
            </Box>
        </Box>
    )
}

interface ScoringZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
    selectZone: (zone: ScoringZonePreferences) => void
}

const ManageZonesInterface: React.FC<ScoringZonesProps> = ({ selectedField, initialZones, selectZone }) => {
    const [zones, setZones] = useState<ScoringZonePreferences[]>(initialZones)

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
                <ScrollView className="flex flex-col gap-4">
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
                        />
                    ))}
                </ScrollView>
            ) : (
                <Label>No scoring zones</Label>
            )}
            {AddButtonInteractiveColor(() => {
                if (zones == undefined) return

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
        </>
    )
}

export default ManageZonesInterface
