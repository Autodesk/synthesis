import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import React, { useState } from "react"
import ManageZonesInterface from "./ManageZonesInterface"
import ZoneConfigInterface from "./ZoneConfigInterface"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { Box } from "@mui/material"
import { ButtonIcon, SectionDivider, SectionLabel, SynthesisIcons } from "@/ui/components/StyledComponents"
import { LabelSize } from "@/ui/components/Label"
import { ConfigurationSavedEvent } from "../../ConfigurePanel"

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
                <>
                    <Box display="flex" textAlign={"center"} minHeight={"30px"} key="selected-item">
                        <Box width={`60px`} />

                        {/** Back arrow button when an option is selected */}
                        <ButtonIcon
                            value={SynthesisIcons.LeftArrowLarge}
                            onClick={() => {
                                new ConfigurationSavedEvent()
                                setSelectedZone(undefined)
                                //onOptionSelected(undefined)
                            }}
                        />

                        {/** Label with either the header text, or the name of the selected option if an option is selected */}
                        <Box alignSelf={"center"} display="flex">
                            <Box width="8px" />
                            <SectionLabel size={LabelSize.Small} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                {`Configuring Zone`}
                            </SectionLabel>
                        </Box>
                    </Box>
                    <SectionDivider />
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
