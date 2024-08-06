import { useState } from "react"
import Box from "@mui/material/Box"
import Panel, { PanelPropsImpl } from "../components/Panel"
import { SectionDivider, SectionLabel, Spacer, SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "../components/Label"

const QualitySettingsPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [isTooltipVisible, setIsTooltipVisible] = useState(false)

    const handleMouseEnter = () => {
        setIsTooltipVisible(true)
    }

    const handleMouseLeave = () => {
        setIsTooltipVisible(false)
    }

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
                <div className="flex items-center justify-center mt-1 mb-0.5 mx-[5%]">
                    <SectionLabel size={LabelSize.Medium} className="text-center">
                        Hot-reload Settings
                    </SectionLabel>
                    <div
                        className="relative inline-block ml-2"
                        onMouseEnter={handleMouseEnter}
                        onMouseLeave={handleMouseLeave}
                    >
                        {SynthesisIcons.Info}
                        {/* <span className="cursor-pointer">ℹ️</span> */}
                        {isTooltipVisible && (
                            <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-2 w-48 bg-gray-600 bg-opacity-100 text-white text-center rounded-md p-9 px-3 py-2 opacity-90 z-10">
                                If these settings are changed then the game will reload to apply the changes, removing
                                all of your robots and fields from the scene.
                            </div>
                        )}
                    </div>
                </div>
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
