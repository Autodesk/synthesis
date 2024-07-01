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

    const [screenMode, setScreenMode] = useState<string>(PreferencesSystem.getPreference<string>("screenMode"))
    const [qualitySettings, setQualitySettings] = useState<string>(PreferencesSystem.getPreference<string>("qualitySettings"))
    const [zoomSensitivity, setZoomSensitivity] = useState<number>(PreferencesSystem.getPreference<number>("zoomSensitivity"))
    const [pitchSensitivity, setPitchSensitivity] = useState<number>(PreferencesSystem.getPreference<number>("pitchSensitivity"))
    const [yawSensitivity, setYawSensitivity] = useState<number>(PreferencesSystem.getPreference<number>("yawSensitivity"))
    const [reportAnalytics, setReportAnalytics] = useState<boolean>(PreferencesSystem.getPreference<boolean>("reportAnalytics"))
    const [useMetric, setUseMetric] = useState<boolean>(PreferencesSystem.getPreference<boolean>("useMetric"))
    const [renderScoringZones, setRenderScoringZones] = useState<boolean>(PreferencesSystem.getPreference<boolean>("renderScoringZones"))

    const saveSettings = () => {
        PreferencesSystem.setPreference<string>("screenMode", screenMode)
        PreferencesSystem.setPreference<string>("qualitySettings", qualitySettings)
        PreferencesSystem.setPreference<number>("zoomSensitivity", zoomSensitivity)
        PreferencesSystem.setPreference<number>("pitchSensitivity", pitchSensitivity)
        PreferencesSystem.setPreference<number>("yawSensitivity", yawSensitivity)
        PreferencesSystem.setPreference<boolean>("reportAnalytics", reportAnalytics)
        PreferencesSystem.setPreference<boolean>("useMetric", useMetric)
        PreferencesSystem.setPreference<boolean>("renderScoringZones", renderScoringZones)
        
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
                options={moveElementToTop(screenModeOptions, PreferencesSystem.getPreference<string>("screenMode"))}
                onSelect={selected => {
                    setScreenMode(selected)
                }}
            />
            <Dropdown
                label="Quality Settings"
                options={moveElementToTop(
                    qualitySettingsOptions,
                    PreferencesSystem.getPreference<string>("qualitySettings")
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
                defaultValue={PreferencesSystem.getPreference<number>("zoomSensitivity")}
                label={"Zoom Sensitivity"}
                format={{ maximumFractionDigits: 2 }}
                onChange={value => setZoomSensitivity(value)}
            />
            <Slider
                min={1}
                max={15}
                defaultValue={PreferencesSystem.getPreference<number>("pitchSensitivity")}
                label={"Pitch Sensitivity"}
                format={{ maximumFractionDigits: 2 }}
                onChange={value => setPitchSensitivity(value)}
            />
            <Slider
                min={1}
                max={15}
                defaultValue={PreferencesSystem.getPreference<number>("yawSensitivity")}
                label={"Yaw Sensitivity"}
                format={{ maximumFractionDigits: 2 }}
                onChange={value => setYawSensitivity(value)}
            />
            <Label size={LabelSize.Medium}>Preferences</Label>
            <Checkbox
                label="Report Analytics"
                defaultState={PreferencesSystem.getPreference<boolean>("reportAnalytics")}
                onClick={checked => {
                    setReportAnalytics(checked)
                }}
            />
            <Checkbox
                label="Use Metric"
                defaultState={PreferencesSystem.getPreference<boolean>("useMetric")}
                onClick={checked => {
                    setUseMetric(checked)
                }}
            />
            <Checkbox
                label="Render Score Zones"
                defaultState={PreferencesSystem.getPreference<boolean>("renderScoringZones")}
                onClick={checked => {
                    setRenderScoringZones(checked)
                }}
            />
        </Modal>
    )
}

export default SettingsModal
