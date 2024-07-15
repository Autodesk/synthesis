import { useEffect, useState } from "react"
import { usePanelControlContext } from "@/ui/PanelContext"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { AiOutlinePlus } from "react-icons/ai"
import { IoPencil, IoTrashBin } from "react-icons/io5"

const AddIcon = <AiOutlinePlus size={"1.25rem"} />
const DeleteIcon = <IoTrashBin size={"1.25rem"} />
const EditIcon = <IoPencil size={"1.25rem"} />

type ScoringZoneRowProps = {
    zone: ScoringZonePreferences
    openPanel: (id: string) => void
    deleteZone: () => void
    saveZones: () => void
}

export class SelectedZone {
    public static zone: ScoringZonePreferences;
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, openPanel, deleteZone, saveZones }) => {
    return (
        <Stack direction={StackDirection.Horizontal} spacing={48} justify="between">
            <Stack direction={StackDirection.Horizontal} spacing={8} justify="start">
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack direction={StackDirection.Vertical} spacing={4} justify={"center"} className="w-max">
                    <Label size={LabelSize.Small}>{zone.name}</Label>
                    <Label size={LabelSize.Small}>
                        {zone.points} {zone.points == 1 ? "point" : "points"}
                    </Label>
                </Stack>
            </Stack>
            <Stack direction={StackDirection.Horizontal} spacing={8} justify="start">
                <Button
                    value={EditIcon}
                    onClick={() => {
                        SelectedZone.zone = zone
                        saveZones()
                        openPanel("zone-config")
                    }}
                    colorOverrideClass="bg-accept-button hover:brightness-90"
                />
                <Button
                    value={DeleteIcon}
                    onClick={() => {
                        deleteZone()
                        saveZones()
                    }}
                    colorOverrideClass="bg-cancel-button hover:brightness-90"
                />
            </Stack>
        </Stack>
    )
}

const ScoringZonesPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel, closePanel } = usePanelControlContext()
    const [zones, setZones] = useState<ScoringZonePreferences[] | undefined>(undefined)

     const fieldPrefs = PreferencesSystem.getAllFieldPreferences()[SynthesisBrain.fieldsSpawned[0]]

     if (zones == undefined && fieldPrefs != undefined) {
        setZones(fieldPrefs.scoringZones)
     }

    const saveZones = () => {
        if (fieldPrefs != undefined && zones != undefined) {
            fieldPrefs.scoringZones = zones
            PreferencesSystem.savePreferences()
        }
    }

    useEffect(() => {
        closePanel("zone-config")
    },[])

    return (
        <Panel
            name="Scoring Zones"
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            cancelEnabled={false}
            acceptName="Close"
            onAccept={() => {
                saveZones()
            }}
        >
            {zones == undefined ? (
                <Label>Spawn a field to configure scoring zones</Label>
            ) : (
                <>
                    <ScrollView className="flex flex-col gap-4">
                        {zones.map((z: ScoringZonePreferences, i: number) => (
                            <ScoringZoneRow
                                key={i}
                                zone={z}
                                openPanel={openPanel}
                                deleteZone={() => {
                                    setZones(zones.filter((_, idx) => idx !== i))
                                }}
                                saveZones={saveZones}
                            />
                        ))}
                    </ScrollView>
                    <Button
                        value={AddIcon}
                        onClick={() => {
                            const newZone: ScoringZonePreferences = {
                                name: "New Scoring Zone",
                                alliance: "blue",
                                parentNode: undefined,
                                points: 0,
                                destroyGamepiece: false,
                                persistentPoints: false,
                                position: [0, 0, 0],
                                rotation: [0, 0, 0, 0],
                                scale: [1, 1, 1],
                            }
                            zones.push(newZone)
                            SelectedZone.zone = newZone
                            saveZones()
                            console.log(SelectedZone.zone.name)
                            openPanel("zone-config")
                        }}
                    />
                </>
            )}
        </Panel>
    )
}

export default ScoringZonesPanel
