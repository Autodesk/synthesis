import Box from "@mui/material/Box"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import Dropdown from "@/components/Dropdown"
import World from "@/systems/World"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

/** converts the easily-readable options to PowerPreference type strings */
function OptionsToPowerPreference(): { [key: string]: WebGLPowerPreference } {
    return {
        "High Performance": "high-performance",
        "Balanced": "default",
        "Power Saver": "low-power",
    }
}

/** recieves the PowerPreference formatted string and converts it into a readable format for users to select */
function PowerPreferenceToOptions(): { [key: string]: string } {
    return {
        "high-performance": "High Performance",
        "default": "Balanced",
        "low-power": "Power Saver",
    }
}

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
            }}
        >
            <Dropdown
                label="Power Preference"
                options={["High Performance", "Balanced", "Power Saver"]}
                defaultValue={
                    PowerPreferenceToOptions()[PreferencesSystem.getGlobalPreference<string>("PowerPreference")]
                }
                onSelect={value => {
                    // converting the options into WebGLPowerPreference values
                    value = OptionsToPowerPreference()[value as string]

                    // recreating the renderer with new preferences
                    World.SceneRenderer.renderer = World.SceneRenderer.CreateRenderer(
                        value as WebGLPowerPreference,
                        PreferencesSystem.getGlobalPreference<boolean>("AntiAliasing")
                    )

                    // saving the new preference
                    PreferencesSystem.setGlobalPreference("PowerPreference", value)
                }}
            />
            <Box display="flex" flexDirection={"column"}>
                <Checkbox
                    label="Fancy Shadows"
                    defaultState={true}
                    onClick={checked => {
                        console.log("Fancy Shadows", checked)
                    }}
                />
                <Checkbox
                    label="Anti-Aliasing"
                    defaultState={PreferencesSystem.getGlobalPreference<boolean>("AntiAliasing")}
                    onClick={checked => {
                        // rupdating anti-aliasing
                        World.SceneRenderer.SetAntiAliasing(checked)

                        // saving the new preference
                        PreferencesSystem.setGlobalPreference("AntiAliasing", checked)
                    }}
                />
                <Checkbox
                    label="Post Processing"
                    defaultState={true}
                    onClick={checked => {
                        console.log("Post Processing", checked)
                    }}
                />
            </Box>
        </Panel>
    )
}

export default QualitySettingsPanel
