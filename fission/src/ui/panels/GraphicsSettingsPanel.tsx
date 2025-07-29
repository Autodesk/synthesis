import { useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "@/systems/World"
import Dropdown from "@/ui/components/Dropdown"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import Slider from "@/ui/components/Slider"
import Button from "../components/Button"
import { LabelSize } from "../components/Label"
import Panel, { type PanelPropsImpl } from "../components/Panel"
import StatefulCheckbox from "../components/StatefulCheckbox"
import { SectionDivider, SectionLabel, SynthesisIcons } from "../components/StyledComponents"
import { autoOptimizeGraphics, GRAPHICS_PRESETS, type GraphicsPreset } from "../helpers/GraphicsSettings"

const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10

const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40

const MIN_CASCADES = 3
const MAX_CASCADES = 8

const MIN_SHADOW_MAP_SIZE = 1024

const detectCurrentPreset = (
    lightIntensity: number,
    fancyShadows: boolean,
    maxFar: number,
    cascades: number,
    shadowMapSize: number,
    antiAliasing: boolean
): string => {
    for (const [presetName, presetSettings] of Object.entries(GRAPHICS_PRESETS)) {
        if (
            presetSettings.lightIntensity === lightIntensity &&
            presetSettings.fancyShadows === fancyShadows &&
            presetSettings.maxFar === maxFar &&
            presetSettings.cascades === cascades &&
            presetSettings.shadowMapSize === shadowMapSize &&
            presetSettings.antiAliasing === antiAliasing
        ) {
            return presetName
        }
    }
    return "Custom"
}

const GraphicsSettings: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [reload, setReload] = useState<boolean>(false)

    const [lightIntensity, setLightIntensity] = useState<number>(
        PreferencesSystem.getGraphicsPreferences().lightIntensity
    )
    const [fancyShadows, setFancyShadows] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().fancyShadows)
    const [maxFar, setMaxFar] = useState<number>(PreferencesSystem.getGraphicsPreferences().maxFar)
    const [cascades, setCascades] = useState<number>(PreferencesSystem.getGraphicsPreferences().cascades)
    const [shadowMapSize, setShadowMapSize] = useState<number>(PreferencesSystem.getGraphicsPreferences().shadowMapSize)
    const [antiAliasing, setAntiAliasing] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().antiAliasing)

    const initialPreset = detectCurrentPreset(
        PreferencesSystem.getGraphicsPreferences().lightIntensity,
        PreferencesSystem.getGraphicsPreferences().fancyShadows,
        PreferencesSystem.getGraphicsPreferences().maxFar,
        PreferencesSystem.getGraphicsPreferences().cascades,
        PreferencesSystem.getGraphicsPreferences().shadowMapSize,
        PreferencesSystem.getGraphicsPreferences().antiAliasing
    )

    const [selectedPreset, setSelectedPreset] = useState<string>(initialPreset)
    const [dropdownKey, setDropdownKey] = useState<number>(0)

    const updatePresetFromSettings = (
        newLightIntensity: number,
        newFancyShadows: boolean,
        newMaxFar: number,
        newCascades: number,
        newShadowMapSize: number,
        newAntiAliasing: boolean
    ) => {
        const detectedPreset = detectCurrentPreset(
            newLightIntensity,
            newFancyShadows,
            newMaxFar,
            newCascades,
            newShadowMapSize,
            newAntiAliasing
        )
        if (detectedPreset !== selectedPreset) {
            setSelectedPreset(detectedPreset)
            setDropdownKey(prev => prev + 1)
        }
    }

    const updateCSMSettingsAndPreset = (
        newLightIntensity: number,
        newFancyShadows: boolean,
        newMaxFar: number,
        newCascades: number,
        newShadowMapSize: number,
        newAntiAliasing: boolean
    ) => {
        World.sceneRenderer.changeCSMSettings({
            lightIntensity: newLightIntensity,
            fancyShadows: newFancyShadows,
            maxFar: newMaxFar,
            cascades: newCascades,
            shadowMapSize: newShadowMapSize,
            antiAliasing: newAntiAliasing,
        })
        updatePresetFromSettings(
            newLightIntensity,
            newFancyShadows,
            newMaxFar,
            newCascades,
            newShadowMapSize,
            newAntiAliasing
        )
    }

    const applyPreset = (preset: GraphicsPreset) => {
        const settings = GRAPHICS_PRESETS[preset]
        const previousAntiAliasing = antiAliasing

        setLightIntensity(settings.lightIntensity)
        setFancyShadows(settings.fancyShadows)
        setMaxFar(settings.maxFar)
        setCascades(settings.cascades)
        setShadowMapSize(settings.shadowMapSize)
        setAntiAliasing(settings.antiAliasing)
        setSelectedPreset(preset)
        setDropdownKey(prev => prev + 1)

        // Apply settings to scene renderer immediately
        World.sceneRenderer.changeLighting(settings.fancyShadows)
        World.sceneRenderer.setLightIntensity(settings.lightIntensity)

        // Only update CSM settings if fancy shadows are enabled

        World.sceneRenderer.changeCSMSettings({
            lightIntensity: settings.lightIntensity,
            fancyShadows: settings.fancyShadows,
            maxFar: settings.maxFar,
            cascades: settings.cascades,
            shadowMapSize: settings.shadowMapSize,
            antiAliasing: settings.antiAliasing,
        })

        if (previousAntiAliasing !== settings.antiAliasing) {
            setReload(true)
            globalAddToast?.(
                "info",
                "Refresh Required",
                "Anti-aliasing has been changed. Please refresh the page to see the effects."
            )
        }
    }

    const handleAntiAliasingChange = (checked: boolean) => {
        setAntiAliasing(checked)
        setReload(true)
        updatePresetFromSettings(lightIntensity, fancyShadows, maxFar, cascades, shadowMapSize, checked)
        globalAddToast?.("info", "Refresh Required", "Please refresh the page to see the anti-aliasing changes.")
    }

    return (
        <Panel
            name={"Graphics Settings"}
            icon={SynthesisIcons.GEAR}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                // Save preferences only - scene renderer updates happen immediately when settings change
                PreferencesSystem.getGraphicsPreferences().fancyShadows = fancyShadows
                PreferencesSystem.getGraphicsPreferences().lightIntensity = lightIntensity
                PreferencesSystem.getGraphicsPreferences().maxFar = maxFar
                PreferencesSystem.getGraphicsPreferences().cascades = cascades
                PreferencesSystem.getGraphicsPreferences().shadowMapSize = shadowMapSize
                PreferencesSystem.getGraphicsPreferences().antiAliasing = antiAliasing

                PreferencesSystem.savePreferences()

                if (reload) window.location.reload()
            }}
            onCancel={() => {
                // Revert all settings to original preferences
                const originalPrefs = PreferencesSystem.getGraphicsPreferences()
                World.sceneRenderer.setLightIntensity(originalPrefs.lightIntensity)
                World.sceneRenderer.changeLighting(originalPrefs.fancyShadows)
                World.sceneRenderer.changeCSMSettings({
                    lightIntensity: originalPrefs.lightIntensity,
                    fancyShadows: originalPrefs.fancyShadows,
                    maxFar: originalPrefs.maxFar,
                    cascades: originalPrefs.cascades,
                    shadowMapSize: originalPrefs.shadowMapSize,
                    antiAliasing: originalPrefs.antiAliasing,
                })
            }}
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 min-w-[22vw]">
                <div className="flex items-center justify-center mt-1 mb-0.5 mx-[5%]">
                    <SectionLabel size={LabelSize.MEDIUM} className="text-center">
                        Graphics Presets
                    </SectionLabel>
                </div>
                <Dropdown
                    key={dropdownKey}
                    label=""
                    options={["Fast", "Balanced", "Fancy", "Custom"]}
                    defaultValue={selectedPreset}
                    onSelect={(value: string) => {
                        if (value === "Fast" || value === "Balanced" || value === "Fancy") {
                            applyPreset(value as GraphicsPreset)
                        } else if (value === "Custom") {
                            setSelectedPreset("Custom")
                        }
                    }}
                    className="mb-2"
                />
                <Button
                    value="Auto Optimize"
                    onClick={() => {
                        const preset = autoOptimizeGraphics("short")
                        applyPreset(preset)
                    }}
                    className="mb-2 w-full"
                />
                <SectionDivider />
                <Slider
                    min={MIN_LIGHT_INTENSITY}
                    max={MAX_LIGHT_INTENSITY}
                    value={lightIntensity}
                    label="Light Intensity"
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value: number | number[]) => {
                        const newValue = value as number
                        setLightIntensity(newValue)
                        World.sceneRenderer.setLightIntensity(newValue)
                        updatePresetFromSettings(newValue, fancyShadows, maxFar, cascades, shadowMapSize, antiAliasing)
                    }}
                    step={0.25}
                />
                <StatefulCheckbox
                    label="Fancy Shadows"
                    checked={fancyShadows}
                    onClick={checked => {
                        setFancyShadows(checked)
                        World.sceneRenderer.changeLighting(checked)
                        World.sceneRenderer.setLightIntensity(lightIntensity)
                        updatePresetFromSettings(lightIntensity, checked, maxFar, cascades, shadowMapSize, antiAliasing)
                    }}
                    tooltipText="Cascading shadows implementation"
                />
                {fancyShadows ? (
                    <>
                        <Slider
                            min={MIN_MAX_FAR}
                            max={MAX_MAX_FAR}
                            value={maxFar}
                            label="Max Far"
                            onChange={(_, value: number | number[]) => {
                                const newValue = value as number
                                setMaxFar(newValue)
                                updateCSMSettingsAndPreset(
                                    lightIntensity,
                                    fancyShadows,
                                    newValue,
                                    cascades,
                                    shadowMapSize,
                                    antiAliasing
                                )
                            }}
                            step={1}
                        />
                        <Slider
                            min={MIN_CASCADES}
                            max={MAX_CASCADES}
                            value={cascades}
                            label="Cascade Count"
                            onChange={(_, value: number | number[]) => {
                                const newValue = value as number
                                setCascades(newValue)
                                updateCSMSettingsAndPreset(
                                    lightIntensity,
                                    fancyShadows,
                                    maxFar,
                                    newValue,
                                    shadowMapSize,
                                    antiAliasing
                                )
                            }}
                            step={1}
                        />
                        <Slider
                            min={MIN_SHADOW_MAP_SIZE}
                            max={World.sceneRenderer.renderer.capabilities.maxTextureSize}
                            value={shadowMapSize}
                            label="Shadow Map Size"
                            onChange={(_, value: number | number[]) => {
                                const newValue = value as number
                                setShadowMapSize(newValue)
                                updateCSMSettingsAndPreset(
                                    lightIntensity,
                                    fancyShadows,
                                    maxFar,
                                    cascades,
                                    newValue,
                                    antiAliasing
                                )
                            }}
                            step={1024}
                        />
                    </>
                ) : (
                    <></>
                )}
                <div className="flex items-center justify-center mt-1 mb-0.5 mx-[5%]">
                    <SectionLabel size={LabelSize.MEDIUM} className="text-center">
                        Requires Browser Refresh
                    </SectionLabel>
                </div>
                <SectionDivider />
                <StatefulCheckbox
                    label="Anti-Aliasing"
                    checked={antiAliasing}
                    onClick={handleAntiAliasingChange}
                    tooltipText="Will automatically refresh the tab when changed, causing all assets to disappear."
                />
            </div>
        </Panel>
    )
}

export default GraphicsSettings
