import World from "@/systems/World"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { Box, Button, Checkbox, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import type { Panel } from "../UIProvider"

interface GraphicsSettingsPanelProps {
    panel: Panel
}

const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10

const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40

const MIN_CASCADES = 3
const MAX_CASCADES = 8

const MIN_SHADOW_MAP_SIZE = 1024

const GraphicsSettingsPanel: React.FC<GraphicsSettingsPanelProps> = ({ panel }) => {
    const [reload, setReload] = useState<boolean>(false)
    const [lightIntensity, setLightIntensity] = useState<number>(
        PreferencesSystem.getGraphicsPreferences().lightIntensity
    )
    const [fancyShadows, setFancyShadows] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().fancyShadows)
    const [maxFar, setMaxFar] = useState<number>(PreferencesSystem.getGraphicsPreferences().maxFar)
    const [cascades, setCascades] = useState<number>(PreferencesSystem.getGraphicsPreferences().cascades)
    const [shadowMapSize, setShadowMapSize] = useState<number>(PreferencesSystem.getGraphicsPreferences().shadowMapSize)
    const [antiAliasing, setAntiAliasing] = useState<boolean>(PreferencesSystem.getGraphicsPreferences().antiAliasing)

    // TODO: save preferences on accept, reload if needed

    return (
        <Stack gap={2}>
            <FormControlLabel
                label="Light Intensity"
                control={
                    <Slider
                        min={MIN_LIGHT_INTENSITY}
                        max={MAX_LIGHT_INTENSITY}
                        value={lightIntensity}
                        valueLabelFormat={(val, _idx) => val.toFixed(2)}
                        onChange={(_, value: number | number[]) => {
                            setLightIntensity(value as number)
                            World.sceneRenderer.setLightIntensity(value as number)
                        }}
                        step={0.25}
                    />
                }
            />
            <FormControlLabel
                label="Fancy Shadows"
                control={
                    <Checkbox
                        defaultChecked={fancyShadows}
                        onChange={(_, checked) => {
                            setFancyShadows(checked)
                            World.sceneRenderer.ChangeLighting(checked)
                        }}
                    />
                }
            />
            {fancyShadows ? (
                <>
                    <FormControlLabel
                        label="Max Far"
                        control={
                            <Slider
                                min={MIN_MAX_FAR}
                                max={MAX_MAX_FAR}
                                value={maxFar}
                                onChange={(_, value: number | number[]) => {
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
                        }
                    />
                    <FormControlLabel
                        label="Cascade Count"
                        control={
                            <Slider
                                min={MIN_CASCADES}
                                max={MAX_CASCADES}
                                value={cascades}
                                onChange={(_, value: number | number[]) => {
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
                        }
                    />
                    <FormControlLabel
                        label="Shadow Map Size"
                        control={
                            <Slider
                                min={MIN_SHADOW_MAP_SIZE}
                                max={World.sceneRenderer.renderer.capabilities.maxTextureSize}
                                value={shadowMapSize}
                                onChange={(_, value: number | number[]) => {
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
                        }
                    />
                    <Box alignSelf="center">
                        <Button
                            onClick={() => {
                                setShadowMapSize(4096)
                                setMaxFar(30)
                                setLightIntensity(5)
                                setCascades(4)

                                World.sceneRenderer.changeCSMSettings({
                                    shadowMapSize,
                                    maxFar,
                                    lightIntensity,
                                    fancyShadows,
                                    cascades,
                                    antiAliasing,
                                })
                            }}
                        >
                            Reset Default
                        </Button>
                    </Box>
                </>
            ) : (
                <></>
            )}
            <Typography variant="h5">Requires Browser Refresh</Typography>
            <FormControlLabel
                label="Anti-Aliasing"
                control={
                    <Checkbox
                        defaultChecked={antiAliasing}
                        onChange={(_, checked) => {
                            setAntiAliasing(checked)
                            setReload(true)
                        }}
                    />
                }
            />
        </Stack>
    )
}

export default GraphicsSettingsPanel
