import { Button, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer } from "react"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { GlobalPreference, GlobalPreferences } from "@/systems/preferences/PreferenceTypes"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Spacer } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import GraphicsSettingsPanel from "@/ui/panels/GraphicsSettingsPanel"
import { ThemeEditorPanel } from "@/ui/panels/ThemeEditorPanel"

const SettingsModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { closeModal, openPanel, configureScreen } = useUIContext()
    const [_, refresh] = useReducer(x => !x, false)
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

        configureScreen(modal!, { title: "Settings", allowClickAway: false }, { onBeforeAccept: save, onCancel })
    }, [modal, save, configureScreen])

    const writePreference = <K extends GlobalPreference>(pref: K, value: GlobalPreferences[K]) => {
        PreferencesSystem.setGlobalPreference(pref, value)
        refresh()
    }

    return (
        <Stack direction="column" gap={2} className="overflow-y-auto rounded-md p-2 max-h-[60vh] min-w-[20vw]">
            <Stack alignSelf={"center"} direction="row" gap={2}>
                <Button
                    onClick={() => {
                        openPanel(GraphicsSettingsPanel, undefined, modal)
                        closeModal(CloseType.Overwrite)
                        save()
                    }}
                >
                    Graphics Settings
                </Button>
                <Button
                    onClick={() => {
                        openPanel(ThemeEditorPanel, undefined, modal)
                        closeModal(CloseType.Overwrite)
                        save()
                    }}
                >
                    Theme Editor
                </Button>
            </Stack>

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
                    tooltip="Moving the camera up and down."
                />
                {Spacer(2)}
                <Slider
                    min={1}
                    max={15}
                    value={yawSensitivity}
                    label={"Yaw Sensitivity"}
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value) => setYawSensitivity(value as number)}
                    tooltip="Moving the camera left and right."
                />*/}
            {Spacer(5)}
            <Label size="sm">Camera Settings</Label>
            <StatefulSlider
                min={0.1}
                max={2.0}
                defaultValue={PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")}
                label={"Scene Rotation Sensitivity"}
                // format={{ maximumFractionDigits: 2 }}
                onChange={value => writePreference("SceneRotationSensitivity", value)}
                step={0.1}
                tooltip="Controls how fast the scene rotates when dragging with the mouse."
            />
            {Spacer(5)}
            <StatefulSlider
                min={0.06}
                max={6.0}
                defaultValue={PreferencesSystem.getGlobalPreference("ViewCubeRotationSensitivity")}
                label={"ViewCube Rotation Sensitivity"}
                // TODO: format={{ maximumFractionDigits: 2 }}
                onChange={value => writePreference("ViewCubeRotationSensitivity", value)}
                step={0.06}
                tooltip="Controls how fast the view changes when dragging on the view cube."
            />
            <Checkbox
                label="Show View Cube"
                checked={PreferencesSystem.getGlobalPreference("ShowViewCube")}
                onClick={checked => {
                    writePreference("ShowViewCube", checked)
                }}
                tooltip="Show the view cube in the top-right corner for quick camera orientation changes."
            />
            {Spacer(10)}
            <Label size="sm">Preferences</Label>
            <Stack direction="column">
                <Checkbox
                    label="Report Analytics"
                    checked={PreferencesSystem.getGlobalPreference("ReportAnalytics")}
                    onClick={checked => writePreference("ReportAnalytics", checked)}
                    tooltip="Record user data such as what robots are spawned and how they are configured. No personal data will be collected."
                />
                <Checkbox
                    label="Realistic Subsystem Gravity"
                    checked={PreferencesSystem.getGlobalPreference("SubsystemGravity")}
                    onClick={checked => writePreference("SubsystemGravity", checked)}
                    tooltip="Allows you to set a target torque or force for subsystems and joints. If not properly configured, joints may not be able to resist gravity or may not behave as intended."
                />
                <Checkbox
                    label="Show Score Zones"
                    checked={PreferencesSystem.getGlobalPreference("RenderScoringZones")}
                    onClick={checked => writePreference("RenderScoringZones", checked)}
                    tooltip="If disabled, scoring zones will not be visible but will continue to function the same."
                />
                <Checkbox
                    label="Show Protected Zones"
                    checked={PreferencesSystem.getGlobalPreference("RenderProtectedZones")}
                    onClick={checked => writePreference("RenderProtectedZones", checked)}
                    tooltip="If disabled, protected zones will not be visible but will continue to function the same."
                />
                <Checkbox
                    label="Show Scene Tags"
                    checked={PreferencesSystem.getGlobalPreference("RenderSceneTags")}
                    onClick={checked => writePreference("RenderSceneTags", checked)}
                    tooltip="Name tags above robot."
                />
                <Checkbox
                    label="Show Scoreboard"
                    checked={PreferencesSystem.getGlobalPreference("RenderScoreboard")}
                    onClick={checked => {
                        writePreference("RenderScoreboard", checked)
                        if (checked) {
                            // TODO: figure out scoreboard - I think it should be its own component and not a panel
                            // openPanel("scoreboard");
                        }
                    }}
                />
                <Checkbox
                    label="Show Centers of Mass"
                    checked={PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")}
                    onClick={checked => {
                        writePreference("ShowCenterOfMassIndicators", checked)
                    }}
                    tooltip="Show a purple dot to indicate the center of mass of each robot in frame"
                />
                <Checkbox
                    label="Mute All Sound"
                    checked={PreferencesSystem.getGlobalPreference("MuteAllSound")}
                    onClick={checked => writePreference("MuteAllSound", checked)}
                />
                <StatefulSlider
                    min={0}
                    max={100}
                    defaultValue={PreferencesSystem.getGlobalPreference("SFXVolume")}
                    label={"SFX Volume"}
                    // format={{ maximumFractionDigits: 2 }}
                    onChange={value => writePreference("SFXVolume", value)}
                    tooltip="Volume of sound effects (%)."
                />
                {Spacer(8)}
            </Stack>
        </Stack>
    )
}

export default SettingsModal
