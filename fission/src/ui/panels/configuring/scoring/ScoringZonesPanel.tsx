import { useEffect, useMemo, useState } from "react"
import { usePanelControlContext } from "@/ui/PanelContext"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { AiOutlinePlus } from "react-icons/ai"
import { IoPencil, IoTrashBin } from "react-icons/io5"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { Box } from "@mui/material"
import { FaBasketball } from "react-icons/fa6"

const AddIcon = <AiOutlinePlus size={"1.25rem"} />
const DeleteIcon = <IoTrashBin size={"1.25rem"} />
const EditIcon = <IoPencil size={"1.25rem"} />

type ScoringZoneRowProps = {
    zone: ScoringZonePreferences
    field: MirabufSceneObject
    save: () => void
    openPanel: (id: string) => void
    deleteZone: () => void
}

export class SelectedZone {
    public static field: MirabufSceneObject | undefined
    public static zone: ScoringZonePreferences
}

const saveZones = (zones: ScoringZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones

    PreferencesSystem.savePreferences()
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, save, field, openPanel, deleteZone }) => {
    return (
        <Box component={"div"} display={"flex"} justifyContent={"space-between"} alignItems={"center"} gap={"1rem"}>
            <Stack direction={StackDirection.Horizontal} spacing={8} justify="start">
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack direction={StackDirection.Vertical} spacing={4} justify={"center"} className="w-max">
                    <Label size={LabelSize.Small}>{zone.name}</Label>
                    <Label size={LabelSize.Small}>
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
                <Button
                    value={EditIcon}
                    onClick={() => {
                        SelectedZone.zone = zone
                        SelectedZone.field = field
                        save()
                        openPanel("zone-config")
                    }}
                    colorOverrideClass="bg-accept-button hover:brightness-90"
                />
                <Button
                    value={DeleteIcon}
                    onClick={() => {
                        deleteZone()
                        save()
                    }}
                    colorOverrideClass="bg-cancel-button hover:brightness-90"
                />
            </Box>
        </Box>
    )
}

const ScoringZonesPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel, closePanel } = usePanelControlContext()

    const [selectedField, setSelectedField] = useState<MirabufSceneObject | undefined>(SelectedZone.field)
    const [zones, setZones] = useState<ScoringZonePreferences[] | undefined>(
        SelectedZone.field?.fieldPreferences?.scoringZones
    )

    const fields = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.FIELD
            }
            return false
        }) as MirabufSceneObject[]

        return assemblies
    }, [])

    useEffect(() => {
        closePanel("zone-config")

        World.PhysicsSystem.HoldPause()

        return () => {
            World.PhysicsSystem.ReleasePause()
        }
    }, [])

    return (
        <Panel
            name="Scoring Zones"
            icon={<FaBasketball />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            cancelEnabled={false}
            acceptName="Close"
            onAccept={() => {
                SelectedZone.field = undefined
                saveZones(zones, selectedField)
            }}
        >
            {selectedField == undefined || zones == undefined ? (
                <>
                    <Label>Select a field</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {fields.map(mirabufSceneObject => {
                            return (
                                <Button
                                    value={mirabufSceneObject.assemblyName}
                                    onClick={() => {
                                        setSelectedField(mirabufSceneObject)
                                        setZones(mirabufSceneObject.fieldPreferences?.scoringZones)
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                </>
            ) : (
                <>
                    {zones?.length > 0 ? (
                        <ScrollView className="flex flex-col gap-4">
                            {zones.map((zonePrefs: ScoringZonePreferences, i: number) => (
                                <ScoringZoneRow
                                    key={i}
                                    zone={(() => {
                                        return zonePrefs
                                    })()}
                                    field={selectedField}
                                    openPanel={openPanel}
                                    save={() => saveZones(zones, selectedField)}
                                    deleteZone={() => {
                                        setZones(zones.filter((_, idx) => idx !== i))
                                    }}
                                />
                            ))}
                        </ScrollView>
                    ) : (
                        <Label>No scoring zones</Label>
                    )}
                    <Button
                        value={AddIcon}
                        onClick={() => {
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

                            //zones.push(newZone)
                            SelectedZone.zone = newZone
                            SelectedZone.field = selectedField

                            openPanel("zone-config")
                        }}
                    />
                </>
            )}
        </Panel>
    )
}

export default ScoringZonesPanel
