import { Box, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent.ts"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { InputSchemeUseType } from "@/systems/input/InputTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsTypes"
import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import NewInputSchemeModal from "@/ui/modals/configuring/inputs/NewInputSchemeModal"
import ConfigurePanel from "../assembly-config/ConfigurePanel"
import InputSchemeSelection from "./InputSchemeSelection"

const InitialConfigPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    // TODO: can we pass these as custom props?
    const { setSelectedScheme, setUnconfirmedImport } = useStateContext()
    const { openModal, openPanel, configureScreen } = useUIContext()
    const [alliance, setAlliance] = useState<Alliance>("red")
    const [station, setStation] = useState<Station>(1)

    const targetAssembly = useMemo(() => getSpotlightAssembly(), [])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    const closeFinish = useCallback(() => {
        if (targetAssembly?.miraType === MiraType.ROBOT) {
            targetAssembly.alliance = alliance
            targetAssembly.station = station
            ScoreTracker.addPerRobotScore(targetAssembly, 0)

            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex === undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            // Find first available scheme
            const scheme = InputSchemeManager.availableInputSchemesByBrain(brainIndex).find(
                scheme => scheme.status == InputSchemeUseType.AVAILABLE
            )?.scheme

            if (scheme) {
                InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
                setSelectedScheme(scheme)
            }
        }
        new ConfigurationSavedEvent()
    }, [alliance, targetAssembly, station, setSelectedScheme])

    const closeDelete = useCallback(() => {
        if (targetAssembly) World.sceneRenderer.removeSceneObject(targetAssembly.id)
    }, [targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

    useEffect(() => {
        setUnconfirmedImport(true)

        configureScreen(
            panel!,
            { title: "Assembly Setup", acceptText: "Finish", cancelText: "Remove" },
            {
                onBeforeAccept: () => {
                    closeFinish()
                },
                onCancel: () => closeDelete(),
                onClose: () => {
                    setUnconfirmedImport(false)
                },
            }
        )
    }, [closeFinish, closeDelete, configureScreen, panel, setUnconfirmedImport])

    return (
        <Stack gap={2}>
            {targetAssembly?.miraType === MiraType.ROBOT && (
                <Box>
                    <Label size="md">Alliance: </Label>
                    {/** Set the alliance color */}
                    <Button
                        onClick={() => setAlliance(alliance === "blue" ? "red" : "blue")}
                        sx={{ bgcolor: alliance === "red" ? "redAlliance.main" : "blueAlliance.main" }}
                    >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
                    <Box>
                        <Label size="md">Station: </Label>
                        {/** Set the station number */}
                        <Stack gap={2} direction="row">
                            <Button
                                onClick={() => setStation(1)}
                                sx={
                                    station === 1
                                        ? { bgcolor: alliance === "red" ? "redAlliance.main" : "blueAlliance.main" }
                                        : {}
                                }
                            >
                                1
                            </Button>
                            <Button
                                onClick={() => setStation(2)}
                                sx={
                                    station === 2
                                        ? { bgcolor: alliance === "red" ? "redAlliance.main" : "blueAlliance.main" }
                                        : {}
                                }
                            >
                                2
                            </Button>
                            <Button
                                onClick={() => setStation(3)}
                                sx={
                                    station === 3
                                        ? { bgcolor: alliance === "red" ? "redAlliance.main" : "blueAlliance.main" }
                                        : {}
                                }
                            >
                                3
                            </Button>
                        </Stack>
                    </Box>
                </Box>
            )}
            {targetAssembly && (
                <TransformGizmoControl
                    key="init-config-gizmo"
                    defaultMode="translate"
                    scaleDisabled={true}
                    size={3.0}
                    parent={targetAssembly}
                    onAccept={() => closeFinish()}
                    onCancel={closeDelete}
                />
            )}
            {brainIndex !== undefined && (
                <InputSchemeSelection
                    brainIndex={brainIndex}
                    onSelect={() => {}}
                    onEdit={() => openPanel(ConfigurePanel, { configurationType: "INPUTS" }, panel)}
                    onCreateNew={() => openModal(NewInputSchemeModal, undefined, panel)}
                    panelId={panel?.id}
                />
            )}
        </Stack>
    )
}

export default InitialConfigPanel
