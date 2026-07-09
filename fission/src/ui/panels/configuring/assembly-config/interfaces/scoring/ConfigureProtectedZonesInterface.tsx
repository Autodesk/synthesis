import { Box, Divider, Stack } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import { Button, SynthesisIcons } from "@/ui/components/StyledComponents"
import ManageProtectedZonesInterface from "./ManageProtectedZonesInterface"
import ProtectedZoneConfigInterface from "./ProtectedZoneConfigInterface"

const saveProtectedZones = (zones: ProtectedZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.protectedZones = zones

    PreferencesSystem.savePreferences()
    field.updateProtectedZones()
}

interface ConfigureZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ProtectedZonePreferences[]
}

const ConfigureProtectedZonesInterface: React.FC<ConfigureZonesProps> = ({ selectedField, initialZones }) => {
    const [selectedZone, setSelectedZone] = useState<ProtectedZonePreferences | undefined>(undefined)

    return (
        <>
            {selectedZone === undefined ? (
                <ManageProtectedZonesInterface
                    selectedField={selectedField}
                    initialZones={initialZones}
                    selectZone={setSelectedZone}
                />
            ) : (
                <>
                    <Stack textAlign="center" minHeight="30px" key="selected-item">
                        <Box width="60px" />

                        {/** Back arrow button when an option is selected */}
                        <Button
                            startIcon={SynthesisIcons.LEFT_ARROW_LARGE}
                            onClick={() => {
                                EventSystem.dispatch("ConfigurationSavedEvent")
                                setSelectedZone(undefined)
                            }}
                        />

                        {/** Label with either the header text, or the name of the selected option if an option is selected */}
                        <Stack alignSelf="center">
                            <Box width="8px" />
                            <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                Configuring Zone
                            </Label>
                        </Stack>
                    </Stack>
                    <Divider />
                    <ProtectedZoneConfigInterface
                        selectedField={selectedField}
                        selectedZone={selectedZone}
                        saveAllZones={() => {
                            saveProtectedZones(selectedField.fieldPreferences?.protectedZones, selectedField)
                        }}
                    />
                </>
            )}
        </>
    )
}

export default ConfigureProtectedZonesInterface
