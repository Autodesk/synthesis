import { Box, Button, Stack } from "@mui/material"
import type React from "react"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import { useEffect, useState } from "react"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "../components/Checkbox"
import Label from "../components/Label"
import type { PanelImplProps } from "../components/Panel"
import StatefulSlider from "../components/StatefulSlider"
import { useUIContext } from "../helpers/UIProviderHelpers"

const MIN_LIGHT_INTENSITY = 1
const MAX_LIGHT_INTENSITY = 10

const MIN_MAX_FAR = 10
const MAX_MAX_FAR = 40

const MIN_CASCADES = 3
const MAX_CASCADES = 8

const MIN_SHADOW_MAP_SIZE = 1024

const GraphicsSettingsPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
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
    useEffect(() => {
        const onBeforeAccept = () => {
            PreferencesSystem.getGraphicsPreferences().fancyShadows = fancyShadows
            PreferencesSystem.getGraphicsPreferences().lightIntensity = lightIntensity
            PreferencesSystem.getGraphicsPreferences().maxFar = maxFar
            PreferencesSystem.getGraphicsPreferences().cascades = cascades
            PreferencesSystem.getGraphicsPreferences().shadowMapSize = shadowMapSize
            PreferencesSystem.getGraphicsPreferences().antiAliasing = antiAliasing

            PreferencesSystem.savePreferences()

            if (reload) window.location.reload()
        }
        const onCancel = () => {
            SceneRenderer.changeLighting(PreferencesSystem.getGraphicsPreferences().fancyShadows)
        }

        configureScreen(panel!, { title: "Graphics Settings", position: "center" }, { onBeforeAccept, onCancel })
    }, [fancyShadows, lightIntensity, maxFar, cascades, shadowMapSize, antiAliasing, reload])

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
                    SceneRenderer.setLightIntensity(value as number)
                }}
                step={0.25}
            />
            <Checkbox
                label="Fancy Shadows"
                checked={fancyShadows}
                onClick={checked => {
                    setFancyShadows(checked)
                    SceneRenderer.changeLighting(checked)
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
                            SceneRenderer.changeCSMSettings({
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
                            SceneRenderer.changeCSMSettings({
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
                        max={SceneRenderer.renderer.capabilities.maxTextureSize}
                        defaultValue={shadowMapSize}
                        onChange={value => {
                            setShadowMapSize(value as number)
                            SceneRenderer.changeCSMSettings({
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

                                SceneRenderer.changeCSMSettings({
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
            )}
            <Label size="sm">Requires Browser Refresh</Label>
            <Checkbox
                label="Anti-Aliasing"
                checked={antiAliasing}
                onClick={checked => {
                    setAntiAliasing(checked)
                    setReload(true)
                }}
            />
        </Stack>
    )
}

export default GraphicsSettingsPanel
