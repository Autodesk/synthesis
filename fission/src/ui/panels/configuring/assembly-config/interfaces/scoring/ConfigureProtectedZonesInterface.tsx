import { Divider } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageProtectedZonesInterface from "./ManageProtectedZonesInterface"
import ProtectedZoneConfigInterface from "./ProtectedZoneConfigInterface"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const ConfigureProtectedZonesInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [selectedZone, setSelectedZone] = useState<ProtectedZonePreferences | undefined>(undefined)

    const initialZones = useMemo(() => selectedAssembly.fieldPreferences?.protectedZones ?? [], [selectedAssembly])
    useEffect(() => {
        const initial = structuredClone(initialZones)
        registerCleanupFunction(undefined, () => {
            const prefs = selectedAssembly.fieldPreferences
            if (prefs == null) return

            prefs.protectedZones = initial
            selectedAssembly.updateProtectedZones()
        })
    }, [registerCleanupFunction, initialZones, selectedAssembly])

    if (selectedZone === undefined)
        return (
            <ManageProtectedZonesInterface
                selectedField={selectedAssembly}
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
                selectedField={selectedAssembly}
                selectedZone={selectedZone}
                saveAllZones={() => {
                    selectedAssembly.updateProtectedZones()
                }}
            />
        </>
    )
}

export default ConfigureProtectedZonesInterface
