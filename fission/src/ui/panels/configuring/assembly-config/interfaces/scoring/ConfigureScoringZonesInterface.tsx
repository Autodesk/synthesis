import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import React, { useState } from "react"
import ManageZonesInterface from "./ManageZonesInterface"
import ZoneConfigInterface from "./ZoneConfigInterface"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

const saveZones = (zones: ScoringZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones

    PreferencesSystem.savePreferences()
    field.UpdateScoringZones()
}

interface ConfigureZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
}

const ConfigureScoringZonesInterface: React.FC<ConfigureZonesProps> = ({ selectedField, initialZones }) => {
    const [selectedZone, setSelectedZone] = useState<ScoringZonePreferences | undefined>(undefined)

    return (
        <>
            {selectedZone == undefined ? (
                <ManageZonesInterface
                    selectedField={selectedField}
                    initialZones={initialZones}
                    selectZone={setSelectedZone}
                />
            ) : (
                <ZoneConfigInterface
                    selectedField={selectedField}
                    selectedZone={selectedZone}
                    saveAllZones={() => {
                        saveZones(selectedField.fieldPreferences?.scoringZones, selectedField)
                    }}
                />
            )}
        </>
    )
}

export default ConfigureScoringZonesInterface
