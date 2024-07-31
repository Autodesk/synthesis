import React, { useState } from "react"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGear } from "react-icons/fa6"
import Label, { LabelSize } from "@/components/Label"
import Dropdown from "@/components/Dropdown"
import Button from "@/components/Button"
import Slider from "@/components/Slider"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SceneOverlayEvent, SceneOverlayEventKey } from "@/ui/components/SceneOverlayEvents"
import { QualitySetting } from "@/systems/preferences/PreferenceTypes"
import { Box } from "@mui/material"
import { Spacer } from "@/ui/components/StyledComponents"

const SettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()
    const { openPanel } = usePanelControlContext()

    const [qualitySettings, setQualitySettings] = useState<string>(
        PreferencesSystem.getGlobalPreference<string>("QualitySettings")
    )
    const [zoomSensitivity, setZoomSensitivity] = useState<number>(
        PreferencesSystem.getGlobalPreference<number>("ZoomSensitivity")
    )
    const [pitchSensitivity, setPitchSensitivity] = useState<number>(
        PreferencesSystem.getGlobalPreference<number>("PitchSensitivity")
    )
    const [yawSensitivity, setYawSensitivity] = useState<number>(
        PreferencesSystem.getGlobalPreference<number>("YawSensitivity")
    )
    const [reportAnalytics, setReportAnalytics] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("ReportAnalytics")
    )
    const [useMetric, setUseMetric] = useState<boolean>(PreferencesSystem.getGlobalPreference<boolean>("UseMetric"))
    const [renderScoringZones, setRenderScoringZones] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("RenderScoringZones")
    )
    const [renderSceneTags, setRenderSceneTags] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("RenderSceneTags")
    )
    const [renderScoreboard, setRenderScoreboard] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("RenderScoreboard")
    )

    const saveSettings = () => {
        PreferencesSystem.setGlobalPreference<string>("QualitySettings", qualitySettings)
        PreferencesSystem.setGlobalPreference<number>("ZoomSensitivity", zoomSensitivity)
        PreferencesSystem.setGlobalPreference<number>("PitchSensitivity", pitchSensitivity)
        PreferencesSystem.setGlobalPreference<number>("YawSensitivity", yawSensitivity)
        PreferencesSystem.setGlobalPreference<boolean>("ReportAnalytics", reportAnalytics)
        PreferencesSystem.setGlobalPreference<boolean>("UseMetric", useMetric)
        PreferencesSystem.setGlobalPreference<boolean>("RenderScoringZones", renderScoringZones)
        PreferencesSystem.setGlobalPreference<boolean>("RenderSceneTags", renderSceneTags)
        PreferencesSystem.setGlobalPreference<boolean>("RenderScoreboard", renderScoreboard)

        PreferencesSystem.savePreferences()
    }

    return (
        <Modal
            name="Settings"
            icon={<FaGear />}
            modalId={modalId}
            onAccept={() => {
                saveSettings()
            }}
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 min-w-[22vw]">
                <Label size={LabelSize.Medium}>Screen Settings</Label>
                <Box alignSelf={"center"}>
                    <Button value="Configure Quality Settings" onClick={() => openPanel("quality-settings")} />
                </Box>
                <Dropdown
                    label="Quality Settings"
                    options={["Low", "Medium", "High"] as QualitySetting[]}
                    defaultValue={PreferencesSystem.getGlobalPreference<QualitySetting>("QualitySettings")}
                    onSelect={selected => {
                        setQualitySettings(selected)
                    }}
                />
                {Spacer(5)}
                <Box alignSelf={"center"}>
                    <Button value="Theme Editor" onClick={() => openModal("theme-editor")} />
                </Box>
                {Spacer(5)}
                <Label size={LabelSize.Medium}>Camera Settings</Label>
                <Slider
                    min={1}
                    max={15}
                    value={PreferencesSystem.getGlobalPreference<number>("ZoomSensitivity")}
                    label={"Zoom Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setZoomSensitivity(value as number)}
                />
                <Slider
                    min={1}
                    max={15}
                    value={PreferencesSystem.getGlobalPreference<number>("PitchSensitivity")}
                    label={"Pitch Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setPitchSensitivity(value as number)}
                />
                <Slider
                    min={1}
                    max={15}
                    value={PreferencesSystem.getGlobalPreference<number>("YawSensitivity")}
                    label={"Yaw Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setYawSensitivity(value as number)}
                />
                {Spacer(20)}
                <Label size={LabelSize.Medium}>Preferences</Label>
                <Box display="flex" flexDirection={"column"}>
                    <Checkbox
                        label="Report Analytics"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("ReportAnalytics")}
                        onClick={checked => {
                            setReportAnalytics(checked)
                        }}
                    />
                    <Checkbox
                        label="Use Metric"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("UseMetric")}
                        onClick={checked => {
                            setUseMetric(checked)
                        }}
                    />
                    <Checkbox
                        label="Render Score Zones"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("RenderScoringZones")}
                        onClick={checked => {
                            setRenderScoringZones(checked)
                        }}
                    />
                    <Checkbox
                        label="Render Scene Tags"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("RenderSceneTags")}
                        onClick={checked => {
                            setRenderSceneTags(checked)
                            if (!checked) new SceneOverlayEvent(SceneOverlayEventKey.DISABLE)
                            else new SceneOverlayEvent(SceneOverlayEventKey.ENABLE)
                        }}
                    />
                    <Checkbox
                        label="Render Scoreboard"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("RenderScoreboard")}
                        onClick={checked => {
                            setRenderScoreboard(checked)
                        }}
                    />
                </Box>
            </div>
        </Modal>
    )
}

export default SettingsModal
