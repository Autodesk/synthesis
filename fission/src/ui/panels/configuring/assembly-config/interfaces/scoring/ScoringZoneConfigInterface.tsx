import { TextField } from "@mui/material"
import { useState } from "react"
import Checkbox from "@/ui/components/Checkbox"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ZoneConfigBase from "../zones/ZoneConfigBase"

/**
 * Saves ejector configuration to selected field.
 *
 * Math Explanation:
 * Let W be the world transformation matrix of the gizmo.
 * Let R be the world transformation matrix of the selected field node.
 * Let L be the local transformation matrix of the gizmo, relative to the selected field node.
 *
 * We are given W and R, and want to save L with the field. This way when we create
 * the ejection point afterwards, it will be relative to the selected field node.
 *
 * W = L R
 * L = W R^(-1)
 *
 * ThreeJS sets the standard multiplication operation for matrices to be premultiply. I really
 * don't like this terminology as it's thrown me off multiple times, but I suppose it does go
 * against most other multiplication operations.
 *
 * @param name Name given to the scoring zone by the user.
 * @param alliance Scoring zone alliance.
 * @param points Number of points the zone is worth.
 * @param destroy Destroy gamepiece setting.
 * @param persistent Persistent points setting.
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedNode Selected node that configuration is relative to.
 */
function attachAndPersistZone(zone: ScoringZonePreferences, field: MirabufSceneObject) {
    if (!field?.fieldPreferences) return
    if (!field.fieldPreferences.scoringZones.includes(zone)) field.fieldPreferences.scoringZones.push(zone)
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ScoringZonePreferences
    saveAllZones: () => void
}

const ZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    const [persistent, setPersistent] = useState<boolean>(selectedZone.persistentPoints)

    return (
        <ZoneConfigBase
            selectedField={selectedField}
            selectedZone={selectedZone}
            attachAndPersistZone={attachAndPersistZone}
            applyExtrasOnSave={zone => {
                zone.points = zone.points ?? selectedZone.points
                zone.destroyGamepiece = selectedZone.destroyGamepiece
                zone.persistentPoints = persistent
            }}
            removeZoneObject={(field, zone) => field.removeScoringZoneObject(zone)}
            saveAllZones={saveAllZones}
        >
            <TextField
                inputProps={{ type: "number" }}
                label="Points"
                placeholder="Zone points"
                defaultValue={selectedZone.points}
                onChange={v => {
                    const parsed = parseInt(v.target.value)
                    selectedZone.points = Number.isNaN(parsed) ? 1 : parsed
                }}
            />
            <Checkbox label="Persistent Points" checked={persistent} onClick={checked => setPersistent(checked)} />
        </ZoneConfigBase>
    )
}

export default ZoneConfigInterface
