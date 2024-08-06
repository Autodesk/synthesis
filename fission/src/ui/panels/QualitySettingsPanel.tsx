import Box from "@mui/material/Box"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { SectionDivider, SectionLabel, Spacer, SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "../components/Label"

const QualitySettingsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    return (
        <Panel
            name={"Quality Settings"}
            icon={SynthesisIcons.Gear}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                PreferencesSystem.savePreferences()

                window.location.reload()
            }}
        >
            <Box display="flex" flexDirection={"column"}>
                <Checkbox
                    label="Fancy Shadows"
                    defaultState={PreferencesSystem.getQualityPreferences().fancyShadows}
                    onClick={checked => {
                        console.log("Fancy Shadows", checked)
                    }}
                />
                {Spacer(5)}
                <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    Hot-reload Settings
                </SectionLabel>
                {Spacer(5)}
                <SectionDivider />
                {Spacer(5)}
                <Checkbox
                    label="Anti-Aliasing"
                    defaultState={PreferencesSystem.getQualityPreferences().antiAliasing}
                    onClick={checked => {
                        // saving the new preference
                        PreferencesSystem.getQualityPreferences().antiAliasing = checked
                    }}
                />
            </Box>
        </Panel>
    )
}

export default QualitySettingsPanel
