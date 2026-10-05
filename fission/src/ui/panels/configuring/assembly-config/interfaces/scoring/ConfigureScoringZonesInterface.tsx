import { Divider } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import ManageScoringZonesInterface from "./ManageScoringZonesInterface"
import ScoringZoneConfigInterface from "./ScoringZoneConfigInterface"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const ConfigureScoringZonesInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [selectedZone, setSelectedZone] = useState<ScoringZonePreferences | undefined>(undefined)

    const initialZones = useMemo(() => selectedAssembly.fieldPreferences?.scoringZones ?? [], [selectedAssembly])

    useEffect(() => {
        const initial = structuredClone(initialZones)
        registerCleanupFunction(undefined, () => {
            const prefs = selectedAssembly.fieldPreferences
            if (prefs == null) return

            prefs.scoringZones = initial
            selectedAssembly.updateScoringZones()
        })
    }, [registerCleanupFunction, initialZones, selectedAssembly])
    return (
        <>
            {selectedZone === undefined ? (
                <ManageScoringZonesInterface
                    selectedField={selectedAssembly}
                    initialZones={initialZones}
                    selectZone={setSelectedZone}
                />
            ) : (
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
                        selectedField={selectedAssembly}
                        selectedZone={selectedZone}
                        saveAllZones={() => {
                            selectedAssembly.updateScoringZones()
                        }}
                    />
                </>
            )}
        </>
    )
}

export default ConfigureScoringZonesInterface
