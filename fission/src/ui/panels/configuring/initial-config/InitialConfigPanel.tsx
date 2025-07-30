import { Box, Button, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { InputSchemeUseType } from "@/systems/input/InputTypes"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsTypes"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfigurePanel from "../assembly-config/ConfigurePanel"
import InputSchemeSelection from "./InputSchemeSelection"
import type { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import Label from "@/ui/components/Label"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import NewInputSchemeModal from "@/ui/modals/configuring/inputs/NewInputSchemeModal"

const InitialConfigPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { setSelectedScheme, setUnconfirmedImport, setConfigurationType } = useStateContext()
    const { openModal, closePanel, openPanel, configureScreen } = useUIContext()
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
            SimulationSystem.addPerRobotScore(targetAssembly, 0)

            setConfigurationType("ROBOTS")
            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex === undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            // Find first available scheme
            const scheme = InputSchemeManager.availableInputSchemesByBrain(brainIndex).find(
                scheme => scheme.status == InputSchemeUseType.AVAILABLE
            )?.scheme

            if (scheme) {
                InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
                setSelectedScheme(scheme)
            }
        } else {
            setConfigurationType("FIELDS")
        }
    }, [closePanel, panel, targetAssembly])

    const closeDelete = useCallback(() => {
        if (targetAssembly) World.sceneRenderer.removeSceneObject(targetAssembly.id)
    }, [closePanel, panel, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

    useEffect(() => {
        setUnconfirmedImport(true)

        configureScreen(
            panel!,
            { title: "Assembly Setup", acceptText: "Finish", cancelText: "Remove" },
            {
                onBeforeAccept: closeFinish,
                onCancel: closeDelete,
                onClose: () => {
                    setUnconfirmedImport(false)
                },
            }
        )
    }, [])

    return (
        <Stack gap={2}>
            {targetAssembly?.miraType === MiraType.ROBOT && (
                <Box>
                    <Label size="md">Alliance: </Label>
                    {/** Set the alliance color */}
                    <Button
                        onClick={() => setAlliance(alliance === "blue" ? "red" : "blue")}
                        style={{ background: alliance === "red" ? "#ff0000" : "#0000ff" }}
                    >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
                    <Box>
                        <Label size="md">Station: </Label>
                        {/** Set the station number */}
                        <Stack gap={2} direction="row">
                            <Button
                                onClick={() => setStation(1)}
                                style={station === 1 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                            >
                                1
                            </Button>
                            <Button
                                onClick={() => setStation(2)}
                                style={station === 2 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
                            >
                                2
                            </Button>
                            <Button
                                onClick={() => setStation(3)}
                                style={station === 3 ? { background: alliance === "red" ? "#ff0000" : "#0000ff" } : {}}
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
                    onAccept={closeFinish}
                    onCancel={closeDelete}
                />
            )}
            {brainIndex !== undefined && (
                <InputSchemeSelection
                    brainIndex={brainIndex}
                    onSelect={() => {}}
                    onEdit={() => openPanel(<ConfigurePanel />, panel)}
                    onCreateNew={() => openModal(<NewInputSchemeModal />, panel)}
                />
            )}
        </Stack>
    )
}

export default InitialConfigPanel
