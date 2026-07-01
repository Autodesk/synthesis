import {Box, Stack} from "@mui/material"
import {useCallback, useEffect, useState} from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import {PAUSE_REF_ASSEMBLY_CONFIG} from "@/systems/physics/PhysicsTypes"
import type {Alliance} from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import ScrollView from "@/ui/components/ScrollView"
import {AddButton, DeleteButton, EditButton} from "@/ui/components/StyledComponents"
import type {BaseZonePreferences} from "./ZoneConfigBase"

export type ZoneListItem = {
    name: string
    alliance: Alliance
    pointsLabel?: string
}

export type ManageZonesBaseProps<TZone extends BaseZonePreferences> = {
    selectedField: MirabufSceneObject
    initialZones: TZone[]
    selectZone: (zone: TZone) => void
    /** Returns the props needed to render a row for a given zone */
    getListItem: (zone: TZone) => ZoneListItem
    /** Apply the new list to the field preferences and persist, and trigger any field updates */
    persistZones: (zones: TZone[], field: MirabufSceneObject) => void
    /** Create a sensible default new zone */
    createNewZone: () => TZone
    /** Label shown when the zones list is empty */
    emptyLabel?: string
}

function saveZonesGeneric<TZone extends BaseZonePreferences>(
    zones: TZone[] | undefined,
    field: MirabufSceneObject | undefined,
    persistZones: (zones: TZone[], field: MirabufSceneObject) => void
) {
    if (!zones || !field) return
    persistZones(zones, field)
    field.savePreferences()
}

export default function ManageZonesBase<TZone extends BaseZonePreferences>(props: ManageZonesBaseProps<TZone>) {
    const {
        selectedField,
        initialZones,
        selectZone,
        getListItem,
        persistZones,
        createNewZone,
        emptyLabel = "No zones",
    } = props
    const [zones, setZones] = useState<TZone[]>(initialZones)

    const saveEvent = useCallback(() => {
        saveZonesGeneric(zones, selectedField, persistZones)
    }, [zones, selectedField, persistZones])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    useEffect(() => {
        saveZonesGeneric(zones, selectedField, persistZones)
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [selectedField, zones, persistZones])

    return (
        <>
            {zones?.length > 0 ? (
                <ScrollView>
                    <Stack gap={4}>
                        {zones.map((zonePrefs: TZone, i: number) => {
                            const item = getListItem(zonePrefs)
                            return (
                                <Stack
                                    key={`${item.name}-${item.alliance}-${i}`}
                                    justifyContent={"space-between"}
                                    alignItems={"center"}
                                    gap={"1rem"}
                                >
                                    <Stack direction="row" gap={8}>
                                        <Box
                                            className={`w-12 h-12 rounded-lg`}
                                            sx={{
                                                bgcolor:
                                                    item.alliance === "red" ? "redAlliance.main" : "blueAlliance.main",
                                            }}
                                        />
                                        <Stack direction="row" gap={4} className="w-max">
                                            <Label size="sm">{item.name}</Label>
                                            {item.pointsLabel ? <Label size="sm">{item.pointsLabel}</Label> : null}
                                        </Stack>
                                    </Stack>
                                    <Stack
                                        direction={"row-reverse"}
                                        gap={"0.25rem"}
                                        justifyContent={"center"}
                                        alignItems={"center"}
                                    >
                                        {EditButton(() => {
                                            selectZone(zonePrefs)
                                            saveZonesGeneric(zones, selectedField, persistZones)
                                        })}
                                        {DeleteButton(() => {
                                            const newZones = zones.filter((_, idx) => idx !== i)
                                            setZones(newZones)
                                            saveZonesGeneric(newZones, selectedField, persistZones)
                                        })}
                                    </Stack>
                                </Stack>
                            )
                        })}
                    </Stack>
                </ScrollView>
            ) : (
                <Label size="md">{emptyLabel}</Label>
            )}
            {AddButton(() => {
                const newZone = createNewZone()
                saveZonesGeneric(zones, selectedField, persistZones)
                selectZone(newZone)
            })}
        </>
    )
}
