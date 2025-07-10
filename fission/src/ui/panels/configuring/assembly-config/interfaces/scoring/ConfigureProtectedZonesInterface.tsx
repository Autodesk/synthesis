import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import React, { useState } from "react"
import ManageProtectedZonesInterface from "./ManageProtectedZonesInterface"
import ZoneConfigInterface from "./ProtectedZoneConfigInterface"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { Box } from "@mui/material"
import { ButtonIcon, SectionDivider, SectionLabel, SynthesisIcons } from "@/ui/components/StyledComponents"
import { LabelSize } from "@/ui/components/Label"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"

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
            {selectedZone == undefined ? (
                <ManageProtectedZonesInterface
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
                            value={SynthesisIcons.LEFT_ARROW_LARGE}
                            onClick={() => {
                                new ConfigurationSavedEvent()
                                setSelectedZone(undefined)
                                //onOptionSelected(undefined)
                            }}
                        />

                        {/** Label with either the header text, or the name of the selected option if an option is selected */}
                        <Box alignSelf={"center"} display="flex">
                            <Box width="8px" />
                            <SectionLabel size={LabelSize.SMALL} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                                {`Configuring Zone`}
                            </SectionLabel>
                        </Box>
                    </Box>
                    <SectionDivider />
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
