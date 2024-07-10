import { useState } from "react"
import { usePanelControlContext } from "@/ui/PanelContext"
import Button from "@/components/Button"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import ScrollView from "@/components/ScrollView"
import Stack, { StackDirection } from "@/components/Stack"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"

type ScoringZoneRowProps = {
    zone: ScoringZonePreferences
    openPanel: (id: string) => void
    closePanel: (paneId: string) => void
    deleteZone: () => void
    saveZones: () => void
}

export class SelectedZone {
    public static zone: ScoringZonePreferences;
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, openPanel, closePanel, deleteZone, saveZones }) => {
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
                    value="Edit"
                    onClick={() => {
                        SelectedZone.zone = zone
                        saveZones()
                        closePanel("scoring-zones")
                        openPanel("zone-config")
                    }}
                />
                <Button
                    value="Delete"
                    onClick={() => {
                        deleteZone()
                        saveZones()
                    }}
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
                                closePanel={closePanel}
                                deleteZone={() => {
                                    setZones(zones.filter((_, idx) => idx !== i))
                                }}
                                saveZones={saveZones}
                            />
                        ))}
                    </ScrollView>
                    <Button
                        value="Add Zone"
                        onClick={() => {
                            const newZone: ScoringZonePreferences = {
                                name: "New Scoring Zone",
                                alliance: "blue",
                                parent: undefined,
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
                            closePanel(panelId)
                            openPanel("zone-config")
                        }}
                        className="px-36 w-full"
                    />
                </>
            )}
        </Panel>
    )
}

export default ScoringZonesPanel
