import { Divider } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import { SelectMenuHeader } from "@/components/SelectMenu.tsx"
import EventSystem from "@/systems/EventSystem.ts"
import type { SensorPreferences } from "@/systems/preferences/PreferenceTypes"
import World from "@/systems/World"
import ManageSensorsInterface from "./ManageSensorsInterface"
import SensorConfigInterface from "./SensorConfigInterface"
import type { ConfigurationSubpanelComponent } from "../../ConfigTypes"

const ConfigureSensorsInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const initialSensors = selectedAssembly.robotPreferences.sensors ?? []
    const [selectedSensor, setSelectedSensor] = useState<SensorPreferences | undefined>(undefined)

    const onBackButton = useCallback(() => {
        EventSystem.dispatch("ConfigurationSavedEvent")
        setSelectedSensor(undefined)
    }, [])
    const saveAllSensors = useCallback(() => {
        selectedAssembly.savePreferences()
        World.simulationSystem.getSimulationLayer(selectedAssembly.mechanism)?.refreshSensors()
    }, [selectedAssembly])

    useEffect(() => {
        const originalSensors = structuredClone(selectedAssembly.robotPreferences.sensors ?? [])
        registerCleanupFunction(undefined, () => {
            selectedAssembly.robotPreferences.sensors = originalSensors
            World.simulationSystem.getSimulationLayer(selectedAssembly.mechanism)?.refreshSensors()
        })
    }, [registerCleanupFunction, selectedAssembly])

    return (
        <>
            {selectedSensor === undefined ? (
                <ManageSensorsInterface
                    selectedRobot={selectedAssembly}
                    initialSensors={initialSensors}
                    selectSensor={setSelectedSensor}
                />
            ) : (
                <>
                    <SelectMenuHeader label={selectedSensor.name} showBackButton={true} onBackButton={onBackButton} />
                    <Divider />
                    <SensorConfigInterface
                        selectedRobot={selectedAssembly}
                        selectedSensor={selectedSensor}
                        saveAllSensors={saveAllSensors}
                    />
                </>
            )}
        </>
    )
}

export default ConfigureSensorsInterface
