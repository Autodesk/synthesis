import { Box, Divider, Stack } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import { Button, SynthesisIcons } from "@/ui/components/StyledComponents"
import ManageScoringZonesInterface from "./ManageScoringZonesInterface"
import ZoneConfigInterface from "./ScoringZoneConfigInterface"

const saveZones = (zones: ScoringZonePreferences[] | undefined, field: MirabufSceneObject | undefined) => {
    if (!zones || !field) return

    const fieldPrefs = field.fieldPreferences
    if (fieldPrefs) fieldPrefs.scoringZones = zones

    PreferencesSystem.savePreferences()
    field.updateScoringZones()
}

interface ConfigureZonesProps {
    selectedField: MirabufSceneObject
    initialZones: ScoringZonePreferences[]
}

const ConfigureScoringZonesInterface: React.FC<ConfigureZonesProps> = ({ selectedField, initialZones }) => {
    const [selectedZone, setSelectedZone] = useState<ScoringZonePreferences | undefined>(undefined)

    return (
        <>
            {selectedZone === undefined ? (
                <ManageScoringZonesInterface
                    selectedField={selectedField}
                    initialZones={initialZones}
                    selectZone={setSelectedZone}
                />
            ) : (
                <>
                    <Stack textAlign={"center"} minHeight={"30px"} key="selected-item">
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
                            saveZones(selectedField.fieldPreferences?.scoringZones, selectedField)
                        }}
                    />
                </>
            )}
        </>
    )
}

export default ConfigureScoringZonesInterface
