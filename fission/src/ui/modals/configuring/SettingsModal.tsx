import { Box, Stack, Tab, Tabs, TextField } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useState } from "react"
import { GiPerspectiveDiceSixFacesOne } from "react-icons/gi"
import { globalAddToast, globalOpenModal } from "@/components/GlobalUIControls.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { GlobalPreference, GlobalPreferences } from "@/systems/preferences/PreferenceTypes"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import Label from "@/ui/components/Label"
import type { ModalImplProps } from "@/ui/components/Modal"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Button, Spacer } from "@/ui/components/StyledComponents"
import { useThemeContext } from "@/ui/helpers/ThemeProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { randomColor } from "@/util/Random"
import CommandRegistry from "@/ui/components/CommandRegistry"

// Register command: Open Settings (module-scope side effect)
CommandRegistry.get().registerCommand({
    id: "open-settings",
    label: "Open Settings",
    description: "Open the Settings modal.",
    keywords: ["settings", "preferences", "config"],
    perform: () => import("./SettingsModal").then(m => globalOpenModal(m.default, undefined)),
})

// Graphics settings constants
const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10
const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40
const MIN_CASCADES = 3
const MAX_CASCADES = 8
const MIN_SHADOW_MAP_SIZE = 1024

type GraphicsTabActions = {
    save: () => void
    reset: () => void
    requiresReload: boolean
}

type ThemeEditorTabActions = {
    save: () => void
    reset: () => void
}

type GeneralTabProps = {
    writePreference: <K extends GlobalPreference>(pref: K, value: GlobalPreferences[K]) => void
}

type GraphicsTabProps = {
    onActionsChange?: (actions: GraphicsTabActions) => void
}

type ThemeEditorTabProps = {
    onActionsChange?: (actions: ThemeEditorTabActions) => void
}

interface TabConfigBase {
    key: string
    label: string
}

interface GeneralTabConfig extends TabConfigBase {
    key: "general"
    component: React.ComponentType<GeneralTabProps>
}

interface GraphicsTabConfig extends TabConfigBase {
    key: "graphics"
    component: React.ComponentType<GraphicsTabProps>
}

interface ThemeTabConfig extends TabConfigBase {
    key: "theme"
    component: React.ComponentType<ThemeEditorTabProps>
}

type TabConfig = GeneralTabConfig | GraphicsTabConfig | ThemeTabConfig

const ColorEditor: React.FC<{
    label: string
    color: string
    setColor: (_c: string) => void
}> = ({ label, color, setColor }) => {
    return (
        <Stack direction="row" gap={2}>
            <TextField
                label={label}
                variant="outlined"
                defaultValue={color}
                onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                    setColor(event.target.value)
                }}
            />
            <Box
                sx={{
                    height: 55,
                    aspectRatio: 1,
                    borderRadius: 1,
                    bgcolor: `${color}`,
                }}
            />
        </Stack>
    )
}

const GeneralTab: React.FC<GeneralTabProps> = ({ writePreference }) => (
    <Stack direction="column" gap={2}>
        {Spacer(5)}
        <Label size="sm">Camera Settings</Label>
        <StatefulSlider
            min={0.1}
            max={2.0}
            defaultValue={PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")}
            label={"Scene Rotation Sensitivity"}
            onChange={value => writePreference("SceneRotationSensitivity", value)}
            step={0.1}
            tooltip="Controls how fast the scene rotates when dragging with the mouse."
            showValue={false}
        />
        {Spacer(5)}
        <StatefulSlider
            min={0.06}
            max={6.0}
            defaultValue={PreferencesSystem.getGlobalPreference("ViewCubeRotationSensitivity")}
            label={"ViewCube Rotation Sensitivity"}
            onChange={value => writePreference("ViewCubeRotationSensitivity", value)}
            step={0.06}
            tooltip="Controls how fast the view changes when dragging on the view cube."
            showValue={false}
        />
        <Checkbox
            label="Show View Cube"
            checked={PreferencesSystem.getGlobalPreference("ShowViewCube")}
            onClick={checked => writePreference("ShowViewCube", checked)}
            tooltip="Show the view cube in the top-right corner for quick camera orientation changes."
        />
        {Spacer(10)}
        <Label size="md" sx={{ fontWeight: 600 }}>
            Preferences
        </Label>
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
                onClick={checked => writePreference("ShowCenterOfMassIndicators", checked)}
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
                onChange={value => writePreference("SFXVolume", value)}
                tooltip="Volume of sound effects (%)."
            />
        </Stack>
    </Stack>
)

const GraphicsTab: React.FC<GraphicsTabProps> = ({ onActionsChange }) => {
    const [reload, setReload] = useState<boolean>(false)
    const [lightIntensity, setLightIntensity] = useState<number>(
        PreferencesSystem.getGraphicsPreferences().lightIntensity
    )
    const [fancyShadows, setFancyShadows] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().fancyShadows)
    const [maxFar, setMaxFar] = useState<number>(PreferencesSystem.getGraphicsPreferences().maxFar)
    const [cascades, setCascades] = useState<number>(PreferencesSystem.getGraphicsPreferences().cascades)
    const [shadowMapSize, setShadowMapSize] = useState<number>(PreferencesSystem.getGraphicsPreferences().shadowMapSize)
    const [antiAliasing, setAntiAliasing] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().antiAliasing)

    // Create actions object and notify parent
    useEffect(() => {
        const actions: GraphicsTabActions = {
            save: () => {
                PreferencesSystem.getGraphicsPreferences().fancyShadows = fancyShadows
                PreferencesSystem.getGraphicsPreferences().lightIntensity = lightIntensity
                PreferencesSystem.getGraphicsPreferences().maxFar = maxFar
                PreferencesSystem.getGraphicsPreferences().cascades = cascades
                PreferencesSystem.getGraphicsPreferences().shadowMapSize = shadowMapSize
                PreferencesSystem.getGraphicsPreferences().antiAliasing = antiAliasing

                World.analyticsSystem?.event("Graphics Settings", {
                    lightIntensity,
                    fancyShadows,
                    maxFar,
                    cascades,
                    shadowMapSize,
                    antiAliasing,
                })

                if (reload) window.location.reload()
            },
            reset: () => {
                const g = PreferencesSystem.getGraphicsPreferences()
                setLightIntensity(g.lightIntensity)
                setFancyShadows(g.fancyShadows)
                setMaxFar(g.maxFar)
                setCascades(g.cascades)
                setShadowMapSize(g.shadowMapSize)
                setAntiAliasing(g.antiAliasing)
                setReload(false)
                World.sceneRenderer.changeLighting(g.fancyShadows)
            },
            requiresReload: reload,
        }
        onActionsChange?.(actions)
    }, [lightIntensity, fancyShadows, maxFar, cascades, shadowMapSize, antiAliasing, reload, onActionsChange])

    return (
        <Stack gap={2}>
            <StatefulSlider
                label="Light Intensity"
                min={MIN_LIGHT_INTENSITY}
                max={MAX_LIGHT_INTENSITY}
                defaultValue={lightIntensity}
                valueLabelFormat={(val, _idx) => val.toFixed(2)}
                onChange={value => {
                    setLightIntensity(value as number)
                    World.sceneRenderer.setLightIntensity(value as number)
                }}
                step={0.25}
            />
            <Checkbox
                label="Fancy Shadows"
                checked={fancyShadows}
                onClick={checked => {
                    setFancyShadows(checked)
                    World.sceneRenderer.changeLighting(checked)
                }}
            />
            {fancyShadows && (
                <>
                    <StatefulSlider
                        label="Max Far"
                        min={MIN_MAX_FAR}
                        max={MAX_MAX_FAR}
                        defaultValue={maxFar}
                        onChange={value => {
                            setMaxFar(value as number)
                            World.sceneRenderer.changeCSMSettings({
                                maxFar: value as number,
                                lightIntensity,
                                fancyShadows,
                                cascades,
                                shadowMapSize,
                                antiAliasing,
                            })
                        }}
                        step={1}
                    />
                    <StatefulSlider
                        label="Cascade Count"
                        min={MIN_CASCADES}
                        max={MAX_CASCADES}
                        defaultValue={cascades}
                        onChange={value => {
                            setCascades(value as number)
                            World.sceneRenderer.changeCSMSettings({
                                cascades: value as number,
                                maxFar,
                                lightIntensity,
                                fancyShadows,
                                shadowMapSize,
                                antiAliasing,
                            })
                        }}
                        step={1}
                    />
                    <StatefulSlider
                        label="Shadow Map Size"
                        min={MIN_SHADOW_MAP_SIZE}
                        max={World.sceneRenderer.renderer.capabilities.maxTextureSize}
                        defaultValue={shadowMapSize}
                        onChange={value => {
                            setShadowMapSize(value as number)
                            World.sceneRenderer.changeCSMSettings({
                                shadowMapSize: value as number,
                                maxFar,
                                lightIntensity,
                                fancyShadows,
                                cascades,
                                antiAliasing,
                            })
                        }}
                        step={1024}
                    />
                    <Box alignSelf="center">
                        <Button
                            onClick={() => {
                                setShadowMapSize(4096)
                                setMaxFar(30)
                                setLightIntensity(5)
                                setCascades(4)
                                World.sceneRenderer.changeCSMSettings({
                                    shadowMapSize: 4096,
                                    maxFar: 30,
                                    lightIntensity: 5,
                                    fancyShadows,
                                    cascades: 4,
                                    antiAliasing,
                                })
                            }}
                        >
                            Reset Default
                        </Button>
                    </Box>
                </>
            )}
            <Checkbox
                label="Anti-Aliasing"
                checked={antiAliasing}
                onClick={checked => {
                    setAntiAliasing(checked)
                    setReload(true)
                }}
                tooltip={"Requires browser refresh to fully apply"}
            />
        </Stack>
    )
}

const ThemeEditorTab: React.FC<ThemeEditorTabProps> = ({ onActionsChange }) => {
    const {
        mode,
        setMode,
        primaryColor,
        secondaryColor,
        blueAllianceColor,
        redAllianceColor,
        setPrimaryColor,
        setSecondaryColor,
        setBlueAllianceColor,
        setRedAllianceColor,
    } = useThemeContext()

    const [tempPrimary, setTempPrimary] = useState(primaryColor)
    const [tempSecondary, setTempSecondary] = useState(secondaryColor)
    const [tempBlue, setTempBlue] = useState(blueAllianceColor)
    const [tempRed, setTempRed] = useState(redAllianceColor)

    // Create actions object and notify parent
    useEffect(() => {
        const actions: ThemeEditorTabActions = {
            save: () => {
                setPrimaryColor(tempPrimary)
                setSecondaryColor(tempSecondary)
                setBlueAllianceColor(tempBlue)
                setRedAllianceColor(tempRed)
            },
            reset: () => {
                setTempPrimary(primaryColor)
                setTempSecondary(secondaryColor)
                setTempBlue(blueAllianceColor)
                setTempRed(redAllianceColor)
            },
        }
        onActionsChange?.(actions)
    }, [
        tempPrimary,
        tempSecondary,
        tempBlue,
        tempRed,
        primaryColor,
        secondaryColor,
        blueAllianceColor,
        redAllianceColor,
        onActionsChange,
        setBlueAllianceColor,
        setPrimaryColor,
        setRedAllianceColor,
        setSecondaryColor,
    ])

    return (
        <Stack gap={4}>
            <Label size="md">Theme Editor</Label>
            <ColorEditor label="Primary Color" color={tempPrimary} setColor={setTempPrimary} />
            <ColorEditor label="Secondary Color" color={tempSecondary} setColor={setTempSecondary} />
            <ColorEditor label="Blue Alliance" color={tempBlue} setColor={setTempBlue} />
            <ColorEditor label="Red Alliance" color={tempRed} setColor={setTempRed} />
            <Checkbox
                label="Dark Mode"
                checked={mode === "dark"}
                onClick={checked => setMode(checked ? "dark" : "light")}
            />
            <Button
                startIcon={<GiPerspectiveDiceSixFacesOne />}
                onClick={() => {
                    setTempPrimary(randomColor())
                    setTempSecondary(randomColor())
                }}
            >
                Randomize
            </Button>
            <Button
                onClick={() => {
                    setTempPrimary("#90caf9")
                    setTempSecondary("#ce93d8")
                    setTempBlue("#0066b3")
                    setTempRed("#ed1c24")
                }}
            >
                Reset
            </Button>
            <Button
                onClick={() => {
                    setPrimaryColor(tempPrimary)
                    setSecondaryColor(tempSecondary)
                    setBlueAllianceColor(tempBlue)
                    setRedAllianceColor(tempRed)
                }}
            >
                Apply
            </Button>
        </Stack>
    )
}
interface SettingsModalCustomProps {
    initialTab?: string
}

const SettingsModal: React.FC<ModalImplProps<void, SettingsModalCustomProps | undefined>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [_, refresh] = useReducer(x => !x, false)
    const [activeTab, setActiveTab] = useState<string>(modal?.props.custom?.initialTab || "general")

    const [graphicsActions, setGraphicsActions] = useState<GraphicsTabActions | null>(null)
    const [themeActions, setThemeActions] = useState<ThemeEditorTabActions | null>(null)

    // Tab configuration - easily extensible
    const tabs: TabConfig[] = [
        { key: "general", label: "General", component: GeneralTab },
        { key: "graphics", label: "Graphics", component: GraphicsTab },
        { key: "theme", label: "Theme Editor", component: ThemeEditorTab },
    ]

    const writePreference = <K extends GlobalPreference>(pref: K, value: GlobalPreferences[K]) => {
        PreferencesSystem.setGlobalPreference(pref, value)
        refresh()
    }

    const save = useCallback(() => {
        if (graphicsActions) {
            graphicsActions.save()
        }

        if (themeActions) {
            themeActions.save()
        }

        SoundPlayer.getInstance().changeVolume()
        PreferencesSystem.savePreferences()
        globalAddToast("info", "Settings Saved")
    }, [graphicsActions, themeActions])

    const reset = useCallback(() => {
        if (graphicsActions) {
            graphicsActions.reset()
        }

        if (themeActions) {
            themeActions.reset()
        }

        PreferencesSystem.revertPreferences()
        SoundPlayer.getInstance().changeVolume()
    }, [graphicsActions, themeActions])

    useEffect(() => {
        configureScreen(modal!, { title: "Settings", allowClickAway: false }, { onBeforeAccept: save, onCancel: reset })
    }, [modal, save, reset, configureScreen])

    const renderTabContent = () => {
        const currentTab = tabs.find(tab => tab.key === activeTab)
        if (!currentTab) return null

        switch (currentTab.key) {
            case "general": {
                const GeneralComponent = currentTab.component
                return <GeneralComponent writePreference={writePreference} />
            }
            case "graphics": {
                const GraphicsComponent = currentTab.component
                return <GraphicsComponent onActionsChange={setGraphicsActions} />
            }
            case "theme": {
                const ThemeComponent = currentTab.component
                return <ThemeComponent onActionsChange={setThemeActions} />
            }
            default:
                return null
        }
    }

    return (
        <Stack direction="column" gap={2} className="overflow-y-auto rounded-md p-2 max-h-[60vh] min-w-[20vw]">
            <Tabs
                value={activeTab}
                onChange={(_, newValue) => setActiveTab(newValue)}
                textColor="inherit"
                indicatorColor="primary"
                centered
                {...SoundPlayer.getInstance().buttonSoundEffects()}
            >
                {tabs.map(tab => (
                    <Tab key={tab.key} value={tab.key} label={tab.label} />
                ))}
            </Tabs>

            <Box sx={{ mt: 2 }}>{renderTabContent()}</Box>
        </Stack>
    )
}

export default SettingsModal
