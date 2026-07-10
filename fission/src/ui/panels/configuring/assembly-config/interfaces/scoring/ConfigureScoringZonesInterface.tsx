import { Divider } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageScoringZonesInterface from "./ManageScoringZonesInterface"
import ScoringZoneConfigInterface from "./ScoringZoneConfigInterface"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"

const saveScoringZones = (zones: ScoringZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones

    field.savePreferences()
    field.updateScoringZones()
}

interface ConfigureZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
}

const ConfigureScoringZonesInterface: React.FC<ConfigureZonesProps> = ({ selectedField, initialZones }) => {
    const [selectedZone, setSelectedZone] = useState<ScoringZonePreferences | undefined>(undefined)

    if (selectedZone === undefined)
        return (
            <ManageScoringZonesInterface
                selectedField={selectedField}
                initialZones={initialZones}
                selectZone={setSelectedZone}
            />
        )

    return (
        <>
            <SelectMenuHeader
                label={`Zone ${selectedZone.name}`}
                showBackButton={true}
                onBackButton={() => {
                    EventSystem.dispatch("ConfigurationSavedEvent")
                    setSelectedZone(undefined)
                }}
            />
            <Divider />
            <ScoringZoneConfigInterface
                selectedField={selectedField}
                selectedZone={selectedZone}
                saveAllZones={() => {
                    saveScoringZones(selectedField.fieldPreferences?.scoringZones, selectedField)
                }}
            />
        </>
    )
}

export default ConfigureScoringZonesInterface
