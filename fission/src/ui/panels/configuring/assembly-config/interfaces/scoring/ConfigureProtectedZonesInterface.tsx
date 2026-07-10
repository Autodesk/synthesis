import { Divider } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageProtectedZonesInterface from "./ManageProtectedZonesInterface"
import ProtectedZoneConfigInterface from "./ProtectedZoneConfigInterface"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"

const saveProtectedZones = (zones: ProtectedZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.protectedZones = zones

    field.savePreferences()
    field.updateProtectedZones()
}

interface ConfigureZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ProtectedZonePreferences[]
}

const ConfigureProtectedZonesInterface: React.FC<ConfigureZonesProps> = ({ selectedField, initialZones }) => {
    const [selectedZone, setSelectedZone] = useState<ProtectedZonePreferences | undefined>(undefined)

    if (selectedZone === undefined)
        return (
            <ManageProtectedZonesInterface
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
            <ProtectedZoneConfigInterface
                selectedField={selectedField}
                selectedZone={selectedZone}
                saveAllZones={() => {
                    saveProtectedZones(selectedField.fieldPreferences?.protectedZones, selectedField)
                }}
            />
        </>
    )
}

export default ConfigureProtectedZonesInterface
