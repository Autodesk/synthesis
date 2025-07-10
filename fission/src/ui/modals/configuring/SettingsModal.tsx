import React, { useState } from "react"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import Modal, { ModalPropsImpl } from "@/components/Modal"
import Label, { LabelSize } from "@/components/Label"
import Button from "@/components/Button"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { Box } from "@mui/material"
import { Spacer, SynthesisIcons } from "@/ui/components/StyledComponents"
import Slider from "@/ui/components/Slider"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import { globalAddToast } from "@/components/GlobalUIControls.ts"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & { defaultValue: number; onChange: (val: number) => void }
> = props => {
    const [value, setValue] = useState(props.defaultValue)
    return (
        <Slider
            {...props}
            value={value}
            onChange={(_, value) => {
                setValue(value as number)
                props.onChange?.(value as number)
            }}
        ></Slider>
    )
}
const SettingsModal: React.FC<ModalPropsImpl> = ({ modalId }) => {
    const { closeModal } = useModalControlContext()
    const { openPanel } = usePanelControlContext()
    const save = () => {
        SoundPlayer.changeVolume()
        PreferencesSystem.savePreferences()
        globalAddToast("info", "Settings Saved", "")
    }
    return (
        <Modal
            name="Settings"
            icon={SynthesisIcons.GEAR_LARGE}
            modalId={modalId}
            onAccept={save}
            onClickAway={save}
            onCancel={() => {
                PreferencesSystem.revertPreferences()
                SoundPlayer.changeVolume()
            }}
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 max-h-[60vh] min-w-[20vw]">
                <Box alignSelf={"center"}>
                    <Button
                        value="Graphics Settings"
                        onClick={() => {
                            openPanel("graphics-settings")
                            closeModal()
                            save()
                        }}
                    />
                </Box>

                {/* Disabled until these settings are implemented */}
                {/*   {Spacer(5)}
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
                />*/}
                {Spacer(5)}
                <Label size={LabelSize.MEDIUM}>Camera Settings</Label>
                <StatefulSlider
                    min={0.1}
                    max={2.0}
                    defaultValue={PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")}
                    label={"Scene Rotation Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={value => PreferencesSystem.setGlobalPreference("SceneRotationSensitivity", value)}
                    step={0.1}
                    tooltipText="Controls how fast the scene rotates when dragging with the mouse."
                />
                {Spacer(5)}
                <StatefulSlider
                    min={0.06}
                    max={6.0}
                    defaultValue={PreferencesSystem.getGlobalPreference("ViewCubeRotationSensitivity")}
                    label={"ViewCube Rotation Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={value => PreferencesSystem.setGlobalPreference("ViewCubeRotationSensitivity", value)}
                    step={0.06}
                    tooltipText="Controls how fast the view changes when dragging on the view cube."
                />
                <Checkbox
                    label="Show View Cube"
                    defaultState={PreferencesSystem.getGlobalPreference("ShowViewCube")}
                    onClick={checked => {
                        PreferencesSystem.setGlobalPreference("ShowViewCube", checked)
                    }}
                    tooltipText="Show the view cube in the top-right corner for quick camera orientation changes."
                />
                {Spacer(10)}
                <Label size={LabelSize.MEDIUM}>Preferences</Label>
                <Box display="flex" flexDirection={"column"}>
                    <Checkbox
                        label="Report Analytics"
                        defaultState={PreferencesSystem.getGlobalPreference("ReportAnalytics")}
                        onClick={checked => PreferencesSystem.setGlobalPreference("ReportAnalytics", checked)}
                        tooltipText="Record user data such as what robots are spawned and how they are configured. No personal data will be collected."
                    />
                    <Checkbox
                        label="Realistic Subsystem Gravity"
                        defaultState={PreferencesSystem.getGlobalPreference("SubsystemGravity")}
                        onClick={checked => PreferencesSystem.setGlobalPreference("SubsystemGravity", checked)}
                        tooltipText="Allows you to set a target torque or force for subsystems and joints. If not properly configured, joints may not be able to resist gravity or may not behave as intended."
                    />
                    <Checkbox
                        label="Show Score Zones"
                        defaultState={PreferencesSystem.getGlobalPreference("RenderScoringZones")}
                        onClick={checked => PreferencesSystem.setGlobalPreference("RenderScoringZones", checked)}
                        tooltipText="If disabled, scoring zones will not be visible but will continue to function the same."
                    />
                    <Checkbox
                        label="Show Protected Zones"
                        defaultState={PreferencesSystem.getGlobalPreference("RenderProtectedZones")}
                        onClick={checked => {
                            PreferencesSystem.setGlobalPreference("RenderProtectedZones", checked)
                        }}
                        tooltipText="If disabled, protected zones will not be visible but will continue to function the same."
                    />
                    <Checkbox
                        label="Show Scene Tags"
                        defaultState={PreferencesSystem.getGlobalPreference("RenderSceneTags")}
                        onClick={checked => {
                            PreferencesSystem.setGlobalPreference("RenderSceneTags", checked)
                        }}
                        tooltipText="Name tags above robot."
                    />
                    <Checkbox
                        label="Show Scoreboard"
                        defaultState={PreferencesSystem.getGlobalPreference("RenderScoreboard")}
                        onClick={checked => {
                            PreferencesSystem.setGlobalPreference("RenderScoreboard", checked)
                            if (checked) {
                                openPanel("scoreboard")
                            }
                        }}
                    />

                    <Checkbox
                        label="Show Centers of Mass"
                        defaultState={PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")}
                        onClick={checked => {
                            PreferencesSystem.setGlobalPreference("ShowCenterOfMassIndicators", checked)
                        }}
                        tooltipText={"Show a purple dot to indicate the center of mass of each robot in frame"}
                    />
                    <Checkbox
                        label="Mute All Sound"
                        defaultState={PreferencesSystem.getGlobalPreference("MuteAllSound")}
                        onClick={checked => PreferencesSystem.setGlobalPreference("MuteAllSound", checked)}
                    />
                    <StatefulSlider
                        min={0}
                        max={100}
                        defaultValue={PreferencesSystem.getGlobalPreference("SFXVolume")}
                        label={"SFX Volume"}
                        format={{ maximumFractionDigits: 2 }}
                        onChange={value => PreferencesSystem.setGlobalPreference("SFXVolume", value)}
                        tooltipText="Volume of sound effects (%)."
                    />
                    {Spacer(8)}
                </Box>
            </div>
        </Modal>
    )
}

export default SettingsModal
