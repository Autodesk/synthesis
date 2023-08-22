import { useState } from "react";
import { usePanelControlContext } from "../../PanelContext";
import Button from "../../components/Button";
import Label, { LabelSize } from "../../components/Label";
import Panel, { PanelPropsImpl } from "../../components/Panel";
import ScrollView from "../../components/ScrollView";
import Stack, { StackDirection } from "../../components/Stack";
import { ScoringZone } from "./ZoneConfigPanel";


type ScoringZoneRowProps = {
    zone: ScoringZone
    openPanel: (id: string) => void
    deleteZone: () => void
}

const ScoringZoneRow: React.FC<ScoringZoneRowProps> = ({ zone, openPanel, deleteZone }) => {
    return (
        <Stack direction={StackDirection.Horizontal} spacing={48} justify='between'>
            <Stack direction={StackDirection.Horizontal} spacing={8} justify='start'>
                <div className={`w-12 h-12 bg-match-${zone.alliance}-alliance rounded-lg`} />
                <Stack direction={StackDirection.Vertical} spacing={4} justify={'center'} className="w-max">
                    <Label size={LabelSize.Small}>{zone.name}</Label>
                    <Label size={LabelSize.Small}>{zone.points} {zone.points == 1 ? 'point' : 'points'}</Label>
                </Stack>
            </Stack>
            <Stack direction={StackDirection.Horizontal} spacing={8} justify='start'>
                <Button value="Edit" onClick={() => openPanel('zone-config')} />
                <Button value="Delete" onClick={deleteZone} />
            </Stack>
        </Stack>
    )

}

const ScoringZonesPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel } = usePanelControlContext();
    const [zones, setZones] = useState<ScoringZone[]>([
        { name: 'Blue 1', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 2', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 3', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 4', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 5', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 6', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 7', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 8', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 9', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 10', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 11', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 12', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 13', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 14', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 15', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 16', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Blue 17', alliance: 'blue', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] },
        { name: 'Red 1', alliance: 'red', parent: 'node_0', points: 1, destroyGamepiece: false, persistentPoints: false, scale: [1, 1, 1] }
    ])

    return (
        <Panel
            name="Scoring Zones"
            panelId={panelId}
            cancelEnabled={false}
            acceptName="Close"
        >
            <ScrollView className="flex flex-col gap-4">
                {zones.map((z: ScoringZone, i: number) => (
                    <ScoringZoneRow key={i} zone={z} openPanel={openPanel} deleteZone={() => {
                        setZones(zones.filter((_, idx) => idx !== i))
                    }} />
                ))}
            </ScrollView>
            <Button value="Add Zone" onClick={() => openPanel("zone-config")} className="px-36 w-full" />
        </Panel>
    )
}

export default ScoringZonesPanel;
