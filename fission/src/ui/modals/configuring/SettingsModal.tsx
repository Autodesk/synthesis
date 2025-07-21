import { Box, Button, Checkbox, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import type { ModalImplProps } from "@/ui/components/Modal"
import { Spacer } from "@/ui/components/StyledComponents"
import GraphicsSettingsPanel from "@/ui/panels/GraphicsSettingsPanel"
import { CloseType, useUIContext } from "@/ui/UIProvider"

const StatefulSlider: React.FC<
    Omit<Parameters<typeof Slider>[0], "value" | "onChange"> & {
        label: string
        defaultValue: number
        onChange: (val: number) => void
    }
> = props => {
    const [value, setValue] = useState(props.defaultValue)
    return (
        <FormControlLabel
            label={props.label}
            control={
                <Slider
                    {...props}
                    value={value}
                    onChange={(_, value) => {
                        setValue(value as number)
                        props.onChange?.(value as number)
                    }}
                ></Slider>
            }
        />
    )
}

const SettingsModal: React.FC<ModalImplProps<void>> = ({ modal, parent }) => {
    const { closeModal, openPanel } = useUIContext()
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

        modal!.onAccept.setDefaultFunc(save)
        modal!.onCancel.setDefaultFunc(onCancel)
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
            <Typography variant="h5">Camera Settings</Typography>
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
            <FormControlLabel
                label="Show View Cube"
                control={
                    <Checkbox
                        defaultChecked={PreferencesSystem.getGlobalPreference("ShowViewCube")}
                        onChange={e => {
                            PreferencesSystem.setGlobalPreference("ShowViewCube", e.target.checked)
                        }}
                        // tooltipText="Show the view cube in the top-right corner for quick camera orientation changes."
                    />
                }
            />
            {Spacer(10)}
            <Typography variant="h5">Preferences</Typography>
            <Stack direction="column">
                <FormControlLabel
                    label="Report Analytics"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("ReportAnalytics")}
                            onChange={e => PreferencesSystem.setGlobalPreference("ReportAnalytics", e.target.checked)}
                            // tooltipText="Record user data such as what robots are spawned and how they are configured. No personal data will be collected."
                        />
                    }
                />
                <FormControlLabel
                    label="Realistic Subsystem Gravity"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("SubsystemGravity")}
                            onChange={e => PreferencesSystem.setGlobalPreference("SubsystemGravity", e.target.checked)}
                            // tooltipText="Allows you to set a target torque or force for subsystems and joints. If not properly configured, joints may not be able to resist gravity or may not behave as intended."
                        />
                    }
                />
                <FormControlLabel
                    label="Show Score Zones"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("RenderScoringZones")}
                            onChange={e =>
                                PreferencesSystem.setGlobalPreference("RenderScoringZones", e.target.checked)
                            }
                            // tooltipText="If disabled, scoring zones will not be visible but will continue to function the same."
                        />
                    }
                />
                <FormControlLabel
                    label="Show Protected Zones"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("RenderProtectedZones")}
                            onChange={e => {
                                PreferencesSystem.setGlobalPreference("RenderProtectedZones", e.target.checked)
                            }}
                            // tooltipText="If disabled, protected zones will not be visible but will continue to function the same."
                        />
                    }
                />
                <FormControlLabel
                    label="Show Scene Tags"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("RenderSceneTags")}
                            onChange={e => {
                                PreferencesSystem.setGlobalPreference("RenderSceneTags", e.target.checked)
                            }}
                            // tooltipText="Name tags above robot."
                        />
                    }
                />
                <FormControlLabel
                    label="Show Scoreboard"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("RenderScoreboard")}
                            onChange={e => {
                                const checked = e.target.checked
                                PreferencesSystem.setGlobalPreference("RenderScoreboard", checked)
                                if (checked) {
                                    // TODO: figure out scoreboard - I think it should be its own component and not a panel
                                    // openPanel("scoreboard");
                                }
                            }}
                        />
                    }
                />

                <FormControlLabel
                    label="Show Centers of Mass"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")}
                            onChange={e => {
                                PreferencesSystem.setGlobalPreference("ShowCenterOfMassIndicators", e.target.checked)
                            }}
                            // tooltipText="Show a purple dot to indicate the center of mass of each robot in frame"
                        />
                    }
                />
                <FormControlLabel
                    label="Mute All Sound"
                    control={
                        <Checkbox
                            defaultChecked={PreferencesSystem.getGlobalPreference("MuteAllSound")}
                            onChange={e => PreferencesSystem.setGlobalPreference("MuteAllSound", e.target.checked)}
                        />
                    }
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
