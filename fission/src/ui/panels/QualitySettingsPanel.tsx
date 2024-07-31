import Box from "@mui/material/Box"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import Dropdown from "@/components/Dropdown"
const QualitySettingsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    return (
        <Panel
            name={"Quality Settings"}
            icon={SynthesisIcons.Gear}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
        >
            <Dropdown
                label="Power Preference"
                options={["High Performance", "Balanced", "Power Saver"]}
                defaultValue="Balanced"
                onSelect={value => {
                    console.log("Power Preference", value)
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
                    defaultState={true}
                    onClick={checked => {
                        console.log("AntiAliasing", checked)
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
