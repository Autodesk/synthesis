import Panel, { PanelPropsImpl } from "../components/Panel"
import { SectionDivider, SectionLabel, SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "../components/Label"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import { useState } from "react"
import Dropdown from "@/ui/components/Dropdown"
import { globalAddToast } from "@/ui/components/GlobalUIControls"

const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10

const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40

const MIN_CASCADES = 3
const MAX_CASCADES = 8

const MIN_SHADOW_MAP_SIZE = 1024

type GraphicsPreset = "Fast" | "Balanced" | "Fancy"

const GRAPHICS_PRESETS: Record<
    GraphicsPreset,
    {
        lightIntensity: number
        fancyShadows: boolean
        maxFar: number
        cascades: number
        shadowMapSize: number
        antiAliasing: boolean
    }
> = {
    Fast: {
        lightIntensity: 3,
        fancyShadows: false,
        maxFar: 20,
        cascades: 3,
        shadowMapSize: 0,
        antiAliasing: false,
    },
    Balanced: {
        lightIntensity: 5,
        fancyShadows: true,
        maxFar: 30,
        cascades: 4,
        shadowMapSize: 2048,
        antiAliasing: false,
    },
    Fancy: {
        lightIntensity: 8,
        fancyShadows: true,
        maxFar: 40,
        cascades: 6,
        shadowMapSize: 8192,
        antiAliasing: true,
    },
}

const isMobileDevice = (): boolean => {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
}

const isLowEndDevice = (): boolean => {
    if (isMobileDevice()) return true

    const memory =
        typeof navigator !== "undefined" && "deviceMemory" in navigator
            ? (navigator as Navigator & { deviceMemory?: number }).deviceMemory
            : undefined
    if (memory !== undefined && memory <= 4) return true // 4GB or less RAM

    const cores = navigator.hardwareConcurrency
    if (cores && cores <= 2) return true // 2 cores or less

    const userAgent = navigator.userAgent.toLowerCase()
    if (userAgent.includes("android") && (userAgent.includes("lite") || userAgent.includes("go"))) return true

    return false
}

const shouldUseFastModeByDefault = (): boolean => {
    return isLowEndDevice()
}

const applyInitialGraphicsSettings = (): void => {
    // Check if graphics optimization has already been applied
    const optimizationApplied = PreferencesSystem.getGlobalPreference("GraphicsOptimizationApplied")

    // If optimization hasn't been applied yet and device should use fast mode, apply fast settings
    if (!optimizationApplied && shouldUseFastModeByDefault()) {
        const fastSettings = GRAPHICS_PRESETS.Fast
        PreferencesSystem.getGraphicsPreferences().lightIntensity = fastSettings.lightIntensity
        PreferencesSystem.getGraphicsPreferences().fancyShadows = fastSettings.fancyShadows
        PreferencesSystem.getGraphicsPreferences().maxFar = fastSettings.maxFar
        PreferencesSystem.getGraphicsPreferences().cascades = fastSettings.cascades
        PreferencesSystem.getGraphicsPreferences().shadowMapSize = fastSettings.shadowMapSize
        PreferencesSystem.getGraphicsPreferences().antiAliasing = fastSettings.antiAliasing

        // Mark that optimization has been applied
        PreferencesSystem.setGlobalPreference("GraphicsOptimizationApplied", true)
        PreferencesSystem.savePreferences()

        // Show a toast to let user know we optimized for their device
        globalAddToast?.(
            "info",
            "Graphics Optimized",
            "We've set your graphics to 'Fast' mode for optimal performance on your device. You can change this in Graphics Settings."
        )
    } else if (!optimizationApplied) {
        // Mark that optimization check has been completed (even if no changes were made)
        PreferencesSystem.setGlobalPreference("GraphicsOptimizationApplied", true)
        PreferencesSystem.savePreferences()
    }
}

export { isMobileDevice, isLowEndDevice, shouldUseFastModeByDefault, applyInitialGraphicsSettings }

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

        World.sceneRenderer.setLightIntensity(settings.lightIntensity)
        World.sceneRenderer.changeLighting(settings.fancyShadows)

        if (settings.fancyShadows) {
            World.sceneRenderer.changeCSMSettings({
                lightIntensity: settings.lightIntensity,
                fancyShadows: settings.fancyShadows,
                maxFar: settings.maxFar,
                cascades: settings.cascades,
                shadowMapSize: settings.shadowMapSize,
                antiAliasing: settings.antiAliasing,
            })
        }

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
                World.sceneRenderer.changeLighting(PreferencesSystem.getGraphicsPreferences().fancyShadows)
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
                <Checkbox
                    label="Fancy Shadows"
                    defaultState={fancyShadows}
                    onClick={checked => {
                        setFancyShadows(checked)
                        World.sceneRenderer.changeLighting(checked)
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
                                World.sceneRenderer.changeCSMSettings({
                                    maxFar: newValue,

                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    cascades: cascades,
                                    shadowMapSize: shadowMapSize,
                                    antiAliasing: antiAliasing,
                                })
                                updatePresetFromSettings(
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
                                World.sceneRenderer.changeCSMSettings({
                                    cascades: newValue,

                                    maxFar: maxFar,
                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    shadowMapSize: shadowMapSize,
                                    antiAliasing: antiAliasing,
                                })
                                updatePresetFromSettings(
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
                                World.sceneRenderer.changeCSMSettings({
                                    shadowMapSize: newValue,
                                    maxFar: maxFar,
                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    cascades: cascades,
                                    antiAliasing: antiAliasing,
                                })
                                updatePresetFromSettings(
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
                <Checkbox
                    label="Anti-Aliasing"
                    defaultState={antiAliasing}
                    onClick={handleAntiAliasingChange}
                    tooltipText="Will automatically refresh the tab when changed, causing all assets to disappear."
                />
            </div>
        </Panel>
    )
}

export default GraphicsSettings
