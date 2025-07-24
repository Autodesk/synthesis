import { Box, Button, Divider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import ManageProtectedZonesInterface from "./ManageProtectedZonesInterface"
import ZoneConfigInterface from "./ProtectedZoneConfigInterface"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import Label from "@/ui/components/Label"

const protectedZones = (zones: ProtectedZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
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
                        <Box width={`60px`} />

                        {/** Back arrow button when an option is selected */}
                        <Button
                            startIcon={SynthesisIcons.LEFT_ARROW_LARGE}
                            onClick={() => {
                                new ConfigurationSavedEvent()
                                setSelectedZone(undefined)
                            }}
                        />

                        {/** Label with either the header text, or the name of the selected option if an option is selected */}
                        <Stack alignSelf={"center"}>
                            <Box width="8px" />
                            <Label size="sm" className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                Configuring Zone
                            </Label>
                        </Stack>
                    </Stack>
                    <Divider />
                    <ZoneConfigInterface
                        selectedField={selectedField}
                        selectedZone={selectedZone}
                        saveAllZones={() => {
                            protectedZones(selectedField.fieldPreferences?.protectedZones, selectedField)
                        }}
                    />
                </>
            )}
        </>
    )
}

export default ConfigureProtectedZonesInterface
