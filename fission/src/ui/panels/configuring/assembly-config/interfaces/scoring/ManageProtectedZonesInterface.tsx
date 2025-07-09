import { useCallback, useEffect, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { Box } from "@mui/material"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"
import { AddButtonInteractiveColor, DeleteButton, EditButton } from "@/ui/components/StyledComponents"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"

const saveZones = (zones: ProtectedZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.protectedZones = zones

    PreferencesSystem.savePreferences()
    field.UpdateProtectedZones()
}

type ProtectedZoneRowProps = {
    zone: ProtectedZonePreferences
    save: () => void
    deleteZone: () => void
    selectZone: (zone: ProtectedZonePreferences) => void
}

const ProtectedZoneRow: React.FC<ProtectedZoneRowProps> = ({ zone, save, deleteZone, selectZone }) => {
    return (
        <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction={StackDirection.Horizontal} spacing={8} justify="start">
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack direction={StackDirection.Vertical} spacing={4} justify={"center"} className="w-max">
                    <Label size={LabelSize.Small}>{zone.name}</Label>
                    <Label size={LabelSize.Small}>
                        {zone.penaltyPoints} {zone.penaltyPoints == 1 ? "penalty point" : "penalty points"}
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
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    useEffect(() => {
        saveZones(zones, selectedField)

        World.PhysicsSystem.HoldPause(PAUSE_REF_ASSEMBLY_CONFIG)

        return () => {
            World.PhysicsSystem.ReleasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [selectedField, zones])

    return (
        <>
            {zones?.length > 0 ? (
                <ScrollView className="flex flex-col gap-4">
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
                </ScrollView>
            ) : (
                <Label>No protected zones</Label>
            )}
            {AddButtonInteractiveColor(() => {
                if (zones == undefined) return

                const newZone: ProtectedZonePreferences = {
                    name: "New Protected Zone",
                    alliance: "blue",
                    penaltyPoints: 0,
                    parentNode: undefined,
                    requireRobotContact: true,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                }

                saveZones(zones, selectedField)

                selectZone(newZone)
            })}
        </>
    )
}

export default ManageZonesInterface
