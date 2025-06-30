import Panel, { PanelPropsImpl } from "../components/Panel"
import { SectionDivider, SectionLabel, Spacer, SynthesisIcons } from "../components/StyledComponents"
import Checkbox from "@/components/Checkbox"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { LabelSize } from "../components/Label"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import { forwardRef, useImperativeHandle, useState } from "react"
import Button from "../components/Button"
import { Box } from "@mui/material"

const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10

const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40

const MIN_CASCADES = 3
const MAX_CASCADES = 8

const MIN_SHADOW_MAP_SIZE = 1024

export interface GraphicsSettingsRef {
    save: () => void
    reset: () => void
}

export const GraphicsSettingsContent = forwardRef<GraphicsSettingsRef>((_, ref) => {
    const [lightIntensity, setLightIntensity] = useState<number>(
        PreferencesSystem.getGraphicsPreferences().lightIntensity
    )
    const [fancyShadows, setFancyShadows] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().fancyShadows)
    const [maxFar, setMaxFar] = useState<number>(PreferencesSystem.getGraphicsPreferences().maxFar)
    const [cascades, setCascades] = useState<number>(PreferencesSystem.getGraphicsPreferences().cascades)
    const [shadowMapSize, setShadowMapSize] = useState<number>(PreferencesSystem.getGraphicsPreferences().shadowMapSize)
    const [antiAliasing, setAntiAliasing] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().antiAliasing)
    const [reload, setReload] = useState<boolean>(false)

    useImperativeHandle(
        ref,
        () => ({
            save: () => {
                const g = PreferencesSystem.getGraphicsPreferences()
                g.lightIntensity = lightIntensity
                g.fancyShadows = fancyShadows
                g.maxFar = maxFar
                g.cascades = cascades
                g.shadowMapSize = shadowMapSize
                g.antiAliasing = antiAliasing
                PreferencesSystem.savePreferences()
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
            },
        }),
        [lightIntensity, fancyShadows, maxFar, cascades, shadowMapSize, antiAliasing, reload]
    )

    return (
        <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 min-w-[22vw]">
            <Slider
                min={MIN_LIGHT_INTENSITY}
                max={MAX_LIGHT_INTENSITY}
                value={lightIntensity}
                label="Light Intensity"
                format={{ maximumFractionDigits: 2 }}
                onChange={(_, v) => {
                    setLightIntensity(v as number)
                    World.SceneRenderer.setLightIntensity(v as number)
                }}
                step={0.25}
            />

            <Checkbox
                label="Fancy Shadows"
                defaultState={fancyShadows}
                onClick={checked => {
                    setFancyShadows(checked)
                    World.SceneRenderer.ChangeLighting(checked)
                }}
                tooltipText="Cascading shadows implementation"
            />

            {fancyShadows && (
                <>
                    <Slider
                        min={MIN_MAX_FAR}
                        max={MAX_MAX_FAR}
                        value={maxFar}
                        label="Max Far"
                        onChange={(_, v) => {
                            setMaxFar(v as number)
                            World.SceneRenderer.changeCSMSettings({
                                maxFar: v as number,
                                lightIntensity,
                                fancyShadows,
                                cascades,
                                shadowMapSize,
                                antiAliasing,
                            })
                        }}
                        step={1}
                    />

                    <Slider
                        min={MIN_CASCADES}
                        max={MAX_CASCADES}
                        value={cascades}
                        label="Cascade Count"
                        onChange={(_, v) => {
                            setCascades(v as number)
                            World.SceneRenderer.changeCSMSettings({
                                cascades: v as number,
                                maxFar,
                                lightIntensity,
                                fancyShadows,
                                shadowMapSize,
                                antiAliasing,
                            })
                        }}
                        step={1}
                    />

                    <Slider
                        min={MIN_SHADOW_MAP_SIZE}
                        max={World.SceneRenderer.renderer.capabilities.maxTextureSize}
                        value={shadowMapSize}
                        label="Shadow Map Size"
                        onChange={(_, v) => {
                            setShadowMapSize(v as number)
                            World.SceneRenderer.changeCSMSettings({
                                shadowMapSize: v as number,
                                maxFar,
                                lightIntensity,
                                fancyShadows,
                                cascades,
                                antiAliasing,
                            })
                        }}
                        step={1024}
                    />

                    {Spacer(10)}

                    <Box alignSelf="center">
                        <Button
                            value="Reset Default"
                            onClick={() => {
                                setShadowMapSize(4096)
                                setMaxFar(30)
                                setLightIntensity(5)
                                setCascades(4)
                                World.SceneRenderer.changeCSMSettings({
                                    shadowMapSize: 4096,
                                    maxFar: 30,
                                    lightIntensity: 5,
                                    fancyShadows,
                                    cascades: 4,
                                    antiAliasing,
                                })
                            }}
                        />
                    </Box>
                </>
            )}

            <div className="flex items-center justify-center mt-1 mb-0.5 mx-[5%]">
                <SectionLabel size={LabelSize.Medium} className="text-center">
                    Requires Browser Refresh
                </SectionLabel>
            </div>

            <SectionDivider />

            <Checkbox
                label="Anti-Aliasing"
                defaultState={antiAliasing}
                onClick={checked => {
                    setAntiAliasing(checked)
                    setReload(true)
                }}
                tooltipText="Will automatically refresh the tab when changed."
            />
        </div>
    )
})

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

    return (
        <Panel
            name={"Graphics Settings"}
            icon={SynthesisIcons.Gear}
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
                World.SceneRenderer.ChangeLighting(PreferencesSystem.getGraphicsPreferences().fancyShadows)
            }}
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 min-w-[22vw]">
                <Slider
                    min={MIN_LIGHT_INTENSITY}
                    max={MAX_LIGHT_INTENSITY}
                    value={lightIntensity}
                    label="Light Intensity"
                    format={{ maximumFractionDigits: 2 }}
                    onChange={(_, value: number | number[]) => {
                        setLightIntensity(value as number)
                        World.SceneRenderer.setLightIntensity(value as number)
                    }}
                    step={0.25}
                />
                <Checkbox
                    label="Fancy Shadows"
                    defaultState={fancyShadows}
                    onClick={checked => {
                        setFancyShadows(checked)
                        World.SceneRenderer.ChangeLighting(checked)
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
                                setMaxFar(value as number)
                                World.SceneRenderer.changeCSMSettings({
                                    maxFar: value as number,

                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    cascades: cascades,
                                    shadowMapSize: shadowMapSize,
                                    antiAliasing: antiAliasing,
                                })
                            }}
                            step={1}
                        />
                        <Slider
                            min={MIN_CASCADES}
                            max={MAX_CASCADES}
                            value={cascades}
                            label="Cascade Count"
                            onChange={(_, value: number | number[]) => {
                                setCascades(value as number)
                                World.SceneRenderer.changeCSMSettings({
                                    cascades: value as number,

                                    maxFar: maxFar,
                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    shadowMapSize: shadowMapSize,
                                    antiAliasing: antiAliasing,
                                })
                            }}
                            step={1}
                        />
                        <Slider
                            min={MIN_SHADOW_MAP_SIZE}
                            max={World.SceneRenderer.renderer.capabilities.maxTextureSize}
                            value={shadowMapSize}
                            label="Shadow Map Size"
                            onChange={(_, value: number | number[]) => {
                                setShadowMapSize(value as number)
                                World.SceneRenderer.changeCSMSettings({
                                    shadowMapSize: value as number,
                                    maxFar: maxFar,
                                    lightIntensity: lightIntensity,
                                    fancyShadows: fancyShadows,
                                    cascades: cascades,
                                    antiAliasing: antiAliasing,
                                })
                            }}
                            step={1024}
                        />
                        {Spacer(10)}
                        <Box alignSelf={"center"}>
                            <Button
                                value="Reset Default"
                                onClick={() => {
                                    setShadowMapSize(4096)
                                    setMaxFar(30)
                                    setLightIntensity(5)
                                    setCascades(4)

                                    World.SceneRenderer.changeCSMSettings({
                                        shadowMapSize: 4096,
                                        maxFar: 30,
                                        lightIntensity: 5,
                                        fancyShadows: fancyShadows,
                                        cascades: 4,
                                        antiAliasing: antiAliasing,
                                    })
                                }}
                            />
                        </Box>
                    </>
                ) : (
                    <></>
                )}
                <div className="flex items-center justify-center mt-1 mb-0.5 mx-[5%]">
                    <SectionLabel size={LabelSize.Medium} className="text-center">
                        Requires Browser Refresh
                    </SectionLabel>
                </div>
                <SectionDivider />
                <Checkbox
                    label="Anti-Aliasing"
                    defaultState={antiAliasing}
                    onClick={checked => {
                        // saving the new preference
                        setAntiAliasing(checked)
                        setReload(true)
                    }}
                    tooltipText="Will automatically refresh the tab when changed, causing all assets to disappear."
                />
            </div>
        </Panel>
    )
}

export default GraphicsSettings
