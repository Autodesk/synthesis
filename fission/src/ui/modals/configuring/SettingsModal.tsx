import React, { useState } from "react"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import { FaGear } from "react-icons/fa6"
import Label, { LabelSize } from "@/components/Label"
import Button from "@/components/Button"
import Slider from "@/components/Slider"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SceneOverlayEvent, SceneOverlayEventKey } from "@/ui/components/SceneOverlayEvents"
import { Box } from "@mui/material"
import { Spacer } from "@/ui/components/StyledComponents"

const SettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { openModal, closeModal } = useModalControlContext()
    const { openPanel } = usePanelControlContext()

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
    const [subsystemGravity, setSubsystemGravity] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("SubsystemGravity")
    )

    const saveSettings = () => {
        PreferencesSystem.setGlobalPreference<number>("ZoomSensitivity", zoomSensitivity)
        PreferencesSystem.setGlobalPreference<number>("PitchSensitivity", pitchSensitivity)
        PreferencesSystem.setGlobalPreference<number>("YawSensitivity", yawSensitivity)
        PreferencesSystem.setGlobalPreference<boolean>("ReportAnalytics", reportAnalytics)
        PreferencesSystem.setGlobalPreference<boolean>("UseMetric", useMetric)
        PreferencesSystem.setGlobalPreference<boolean>("RenderScoringZones", renderScoringZones)
        PreferencesSystem.setGlobalPreference<boolean>("RenderSceneTags", renderSceneTags)
        PreferencesSystem.setGlobalPreference<boolean>("RenderScoreboard", renderScoreboard)
        PreferencesSystem.setGlobalPreference<boolean>("SubsystemGravity", subsystemGravity)

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
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 max-h-[60vh]">
                <Label size={LabelSize.Medium}>Screen Settings</Label>
                <Box alignSelf={"center"}>
                    <Button
                        value="Graphics Settings"
                        onClick={() => {
                            openPanel("graphics-settings")
                            closeModal()
                        }}
                    />
                </Box>
                {Spacer(5)}
                <Label size={LabelSize.Medium}>Camera Settings</Label>
                <Slider
                    min={1}
                    max={15}
                    value={zoomSensitivity}
                    label={"Zoom Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setZoomSensitivity(value as number)}
                />
                {Spacer(2)}
                <Slider
                    min={1}
                    max={15}
                    value={pitchSensitivity}
                    label={"Pitch Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setPitchSensitivity(value as number)}
                    tooltipText="Moving the camera up and down."
                />
                {Spacer(2)}
                <Slider
                    min={1}
                    max={15}
                    value={yawSensitivity}
                    label={"Yaw Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setYawSensitivity(value as number)}
                    tooltipText="Moving the camera left and right."
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
                        tooltipText="Record user data such as what robots are spawned and how they are configured. No personal data will be collected."
                    />
                    <Checkbox
                        label="Use Metric"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("UseMetric")}
                        onClick={checked => {
                            setUseMetric(checked)
                        }}
                    />
                    <Checkbox
                        label="Realistic Subsystem Gravity"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("SubsystemGravity")}
                        onClick={checked => {
                            setSubsystemGravity(checked)
                        }}
                        tooltipText="Allows you to set a target torque or force for subsystems and joints. If not properly configured, joints may not be able to resist gravity or may not behave as intended."
                    />
                    <Checkbox
                        label="Show Score Zones"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("RenderScoringZones")}
                        onClick={checked => {
                            setRenderScoringZones(checked)
                        }}
                        tooltipText="If disabled, scoring zones will not be visible but will continue to function the same."
                    />
                    <Checkbox
                        label="Show Scene Tags"
                        defaultState={PreferencesSystem.getGlobalPreference<boolean>("RenderSceneTags")}
                        onClick={checked => {
                            setRenderSceneTags(checked)
                            if (!checked) new SceneOverlayEvent(SceneOverlayEventKey.DISABLE)
                            else new SceneOverlayEvent(SceneOverlayEventKey.ENABLE)
                        }}
                        tooltipText="Name tags above robot."
                    />
                    <Checkbox
                        label="Show Scoreboard"
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
