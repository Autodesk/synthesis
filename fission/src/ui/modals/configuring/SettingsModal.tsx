import React, { useState } from "react"
import { useModalControlContext } from "@/ui/ModalContext"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGear } from "react-icons/fa6"
import Label, { LabelSize } from "@/components/Label"
import Dropdown from "@/components/Dropdown"
import Button from "@/components/Button"
import Slider from "@/components/Slider"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SceneOverlayEvent, SceneOverlayEventKey } from "@/ui/components/SceneOverlayEvents"

const moveElementToTop = (arr: string[], element: string | undefined) => {
    if (element == undefined) {
        return arr
    }

    arr = arr.includes(element) ? [element, ...arr.filter(item => item !== element)] : arr
    return arr
}

const screenModeOptions = ["Windowed", "Fullscreen"]
const qualitySettingsOptions = ["Low", "Medium", "High", "Ultra"]

const SettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal } = useModalControlContext()

    const [screenMode, setScreenMode] = useState<string>(PreferencesSystem.getGlobalPreference<string>("ScreenMode"))
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

    const saveSettings = () => {
        PreferencesSystem.setGlobalPreference<string>("ScreenMode", screenMode)
        PreferencesSystem.setGlobalPreference<string>("QualitySettings", qualitySettings)
        PreferencesSystem.setGlobalPreference<number>("ZoomSensitivity", zoomSensitivity)
        PreferencesSystem.setGlobalPreference<number>("PitchSensitivity", pitchSensitivity)
        PreferencesSystem.setGlobalPreference<number>("YawSensitivity", yawSensitivity)
        PreferencesSystem.setGlobalPreference<boolean>("ReportAnalytics", reportAnalytics)
        PreferencesSystem.setGlobalPreference<boolean>("UseMetric", useMetric)
        PreferencesSystem.setGlobalPreference<boolean>("RenderScoringZones", renderScoringZones)
        PreferencesSystem.setGlobalPreference<boolean>("RenderSceneTags", renderSceneTags)

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
            <Label size={LabelSize.Medium}>Screen Settings</Label>
            <Dropdown
                label="Screen Mode"
                options={moveElementToTop(
                    screenModeOptions,
                    PreferencesSystem.getGlobalPreference<string>("ScreenMode")
                )}
                onSelect={selected => {
                    setScreenMode(selected)
                }}
            />
            <Dropdown
                label="Quality Settings"
                options={moveElementToTop(
                    qualitySettingsOptions,
                    PreferencesSystem.getGlobalPreference<string>("QualitySettings")
                )}
                onSelect={selected => {
                    setQualitySettings(selected)
                }}
            />
            <Button value="Theme Editor" onClick={() => openModal("theme-editor")} />
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
            <Label size={LabelSize.Medium}>Preferences</Label>
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
        </Modal>
    )
}

export default SettingsModal
