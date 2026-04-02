import {
    Checkbox as MuiCheckbox,
    FormControl,
    InputLabel,
    ListItemText,
    MenuItem,
    OutlinedInput,
    TextField,
} from "@mui/material"
import { Select } from "@/ui/components/StyledComponents"
import { useState, useCallback, useMemo } from "react"
import ProtectedZoneSceneObject from "@/mirabuf/ProtectedZoneSceneObject"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ZoneConfigBase from "../zones/ZoneConfigBase"

/**
 * @param penaltyPoints Number of points the zone is worth.
 * @param contactType Contact type of the zone.
 * @param activeDuring Match modes during which the zone is active.
 */

const MATCH_MODE_OPTIONS: MatchModeType[] = [
    MatchModeType.SANDBOX,
    MatchModeType.AUTONOMOUS,
    MatchModeType.TELEOP,
    MatchModeType.ENDGAME,
]

const CONTACT_TYPE_OPTIONS = Object.values(ContactType)

function attachAndPersistZone(zone: ProtectedZonePreferences, field: MirabufSceneObject) {
    if (!field?.fieldPreferences) return
    if (!field.fieldPreferences.protectedZones.includes(zone)) field.fieldPreferences.protectedZones.push(zone)
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ProtectedZonePreferences
    saveAllZones: () => void
}

const ProtectedZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    const [penaltyPoints, setPenaltyPoints] = useState<number>(selectedZone.penaltyPoints)
    const [contactType, setContactType] = useState<ContactType>(selectedZone.contactType || ContactType.ROBOT_ENTERS)
    const [activeDuring, setActiveDuring] = useState<MatchModeType[]>(selectedZone.activeDuring)

    // Use the cloned FIRST materials like before
    const materials = useMemo(
        () => ({
            red: ProtectedZoneSceneObject.redMaterial.clone(),
            blue: ProtectedZoneSceneObject.blueMaterial.clone(),
        }),
        []
    )

    const applyExtrasOnSave = useCallback(
        (zone: ProtectedZonePreferences) => {
            zone.penaltyPoints = penaltyPoints
            zone.contactType = contactType
            zone.activeDuring = activeDuring
        },
        [penaltyPoints, contactType, activeDuring]
    )

    const removeZoneObject = useCallback((field: MirabufSceneObject, zone: ProtectedZonePreferences) => {
        field.removeProtectedZoneObject(zone)
    }, [])

    return (
        <ZoneConfigBase
            selectedField={selectedField}
            selectedZone={selectedZone}
            attachAndPersistZone={attachAndPersistZone}
            applyExtrasOnSave={applyExtrasOnSave}
            removeZoneObject={removeZoneObject}
            saveAllZones={saveAllZones}
            materials={materials}
        >
            <TextField
                inputProps={{ type: "number" }}
                label="Penalty Points"
                placeholder="Zone penalty points"
                defaultValue={selectedZone.penaltyPoints}
                onChange={v => setPenaltyPoints(parseInt(v.target.value) || 0)}
            />
            <FormControl fullWidth>
                <InputLabel id="active-during-label">Active During</InputLabel>
                <Select
                    labelId="active-during-label"
                    label="Active During"
                    onChange={e => {
                        const {
                            target: { value },
                        } = e
                        setActiveDuring(
                            typeof value === "string"
                                ? (value.split(",") as MatchModeType[])
                                : (value as MatchModeType[])
                        )
                    }}
                    value={activeDuring}
                    input={<OutlinedInput label="Contact Type" />}
                    renderValue={selected => (selected as MatchModeType[]).join(", ")}
                    multiple
                >
                    {MATCH_MODE_OPTIONS.map(opt => (
                        <MenuItem key={opt} value={opt}>
                            <MuiCheckbox checked={activeDuring.includes(opt)} />
                            <ListItemText primary={opt} />
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
            <FormControl fullWidth>
                <InputLabel id="contact-type-label">Contact Type</InputLabel>
                <Select
                    labelId="contact-type-label"
                    onChange={e => setContactType(e.target.value as ContactType)}
                    value={contactType}
                >
                    {CONTACT_TYPE_OPTIONS.map(opt => (
                        <MenuItem key={opt} value={opt}>
                            {opt}
                        </MenuItem>
                    ))}
                </Select>
            </FormControl>
        </ZoneConfigBase>
    )
}

export default ProtectedZoneConfigInterface
