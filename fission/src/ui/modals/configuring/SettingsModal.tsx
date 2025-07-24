import { Box, Button, Checkbox, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useCallback, useEffect } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import type { ModalImplProps } from "@/ui/components/Modal"
import { Spacer } from "@/ui/components/StyledComponents"
import GraphicsSettingsPanel from "@/ui/panels/GraphicsSettingsPanel"
import { CloseType, useUIContext } from "@/ui/UIProvider"
import StatefulSlider from "@/ui/components/StatefulSlider"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"
import Label from "@/ui/components/Label"

const SettingsModal: React.FC<ModalImplProps<void>> = ({ modal }) => {
    const { closeModal, openPanel, configureScreen } = useUIContext()
    const save = useCallback(() => {
        SoundPlayer.changeVolume()
        PreferencesSystem.savePreferences()
        globalAddToast("info", "Settings Saved")
    }, [])

    useEffect(() => {
        const onCancel = () => {
            PreferencesSystem.revertPreferences()
            SoundPlayer.changeVolume()
        }

        configureScreen(modal!, { title: "Settings", allowClickAway: false }, { onAccept: save, onCancel })
    }, [modal, save])

    return (
        <Stack
            direction="column"
            gap={2}
            className="overflow-y-auto bg-background-secondary rounded-md p-2 max-h-[60vh] min-w-[20vw]"
        >
            <Box alignSelf={"center"}>
                <Button
                    onClick={() => {
                        openPanel(<GraphicsSettingsPanel />, modal)
                        closeModal(CloseType.Overwrite)
                        save()
                    }}
                >
                    Graphics Settings
                </Button>
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
            <Label size="sm">Camera Settings</Label>
            <StatefulSlider
                min={0.1}
                max={2.0}
                defaultValue={PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")}
                label={"Scene Rotation Sensitivity"}
                // format={{ maximumFractionDigits: 2 }}
                onChange={value => PreferencesSystem.setGlobalPreference("SceneRotationSensitivity", value)}
                step={0.1}
                // TODO: tooltipText="Controls how fast the scene rotates when dragging with the mouse."
            />
            {Spacer(5)}
            <StatefulSlider
                min={0.06}
                max={6.0}
                defaultValue={PreferencesSystem.getGlobalPreference("ViewCubeRotationSensitivity")}
                label={"ViewCube Rotation Sensitivity"}
                // TODO: format={{ maximumFractionDigits: 2 }}
                onChange={value => PreferencesSystem.setGlobalPreference("ViewCubeRotationSensitivity", value)}
                step={0.06}
                // tooltipText="Controls how fast the view changes when dragging on the view cube."
            />
            <StatefulCheckbox
                label="Show View Cube"
                checked={PreferencesSystem.getGlobalPreference("ShowViewCube")}
                onClick={checked => {
                    PreferencesSystem.setGlobalPreference("ShowViewCube", checked)
                }}
                // tooltipText="Show the view cube in the top-right corner for quick camera orientation changes."
            />
            {Spacer(10)}
            <Label size="sm">Preferences</Label>
            <Stack direction="column">
                <StatefulCheckbox
                    label="Report Analytics"
                    checked={PreferencesSystem.getGlobalPreference("ReportAnalytics")}
                    onClick={checked => PreferencesSystem.setGlobalPreference("ReportAnalytics", checked)}
                    // tooltipText="Record user data such as what robots are spawned and how they are configured. No personal data will be collected."
                />
                <StatefulCheckbox
                    label="Realistic Subsystem Gravity"
                    checked={PreferencesSystem.getGlobalPreference("SubsystemGravity")}
                    onClick={checked => PreferencesSystem.setGlobalPreference("SubsystemGravity", checked)}
                    // tooltipText="Allows you to set a target torque or force for subsystems and joints. If not properly configured, joints may not be able to resist gravity or may not behave as intended."
                />
                <StatefulCheckbox
                    label="Show Score Zones"
                    checked={PreferencesSystem.getGlobalPreference("RenderScoringZones")}
                    onClick={checked =>
                        PreferencesSystem.setGlobalPreference("RenderScoringZones", checked)
                    }
                    // tooltipText="If disabled, scoring zones will not be visible but will continue to function the same."
                />
                <StatefulCheckbox
                    label="Show Protected Zones"
                    checked={PreferencesSystem.getGlobalPreference("RenderProtectedZones")}
                    onClick={checked => {
                        PreferencesSystem.setGlobalPreference("RenderProtectedZones", checked)
                    }}
                    // tooltipText="If disabled, protected zones will not be visible but will continue to function the same."
                />
                <StatefulCheckbox
                    label="Show Scene Tags"
                    checked={PreferencesSystem.getGlobalPreference("RenderSceneTags")}
                    onClick={checked => {
                        PreferencesSystem.setGlobalPreference("RenderSceneTags", checked)
                    }}
                    // tooltipText="Name tags above robot."
                />
                <StatefulCheckbox
                    label="Show Scoreboard"
                    checked={PreferencesSystem.getGlobalPreference("RenderScoreboard")}
                    onClick={checked => {
                        PreferencesSystem.setGlobalPreference("RenderScoreboard", checked)
                        if (checked) {
                            // TODO: figure out scoreboard - I think it should be its own component and not a panel
                            // openPanel("scoreboard");
                        }
                    }}
                />
                <StatefulCheckbox
                    label="Show Centers of Mass"
                    checked={PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")}
                    onClick={checked => {
                        PreferencesSystem.setGlobalPreference("ShowCenterOfMassIndicators", checked)
                    }}
                    // tooltipText="Show a purple dot to indicate the center of mass of each robot in frame"
                />
                <StatefulCheckbox
                    label="Mute All Sound"
                    checked={PreferencesSystem.getGlobalPreference("MuteAllSound")}
                    onClick={checked => PreferencesSystem.setGlobalPreference("MuteAllSound", checked)}
                />
                <StatefulSlider
                    min={0}
                    max={100}
                    defaultValue={PreferencesSystem.getGlobalPreference("SFXVolume")}
                    label={"SFX Volume"}
                    // format={{ maximumFractionDigits: 2 }}
                    onChange={value => PreferencesSystem.setGlobalPreference("SFXVolume", value)}
                    // tooltipText="Volume of sound effects (%)."
                />
                {Spacer(8)}
            </Stack>
        </Stack>
    )
}

export default SettingsModal
