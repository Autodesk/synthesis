import { Box, Button, Stack, Typography } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { CloseType, useUIContext } from "@/ui/UIProvider"
import { useStateContext } from "@/ui/StateProvider"
import ConfigurePanel from "../assembly-config/ConfigurePanel"
import InputSchemeSelection from "./InputSchemeSelection"
import type { Alliance } from "@/systems/preferences/PreferenceTypes"
import AssignNewSchemeModal from "@/ui/modals/configuring/inputs/AssignNewSchemeModal"
import Label from "@/ui/components/Label"

const InitialConfigPanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { setSelectedScheme, setConfigurationType } = useStateContext()
    const { openModal, closePanel, openPanel } = useUIContext()
    const [alliance, setAlliance] = useState<Alliance>("red")

    const targetAssembly = useMemo(() => getSpotlightAssembly(), [])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    // TODO: unconfirmed import
    const closeFinish = useCallback(() => {
        if (targetAssembly?.miraType === MiraType.ROBOT) {
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

        if (panel) closePanel(panel.id, CloseType.Cancel)
    }, [closePanel, panel, targetAssembly])

    const closeDelete = useCallback(() => {
        if (targetAssembly) World.sceneRenderer.removeSceneObject(targetAssembly.id)

        if (panel) closePanel(panel.id, CloseType.Cancel)
    }, [closePanel, panel, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

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
                        <Stack gap={2}>
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
                    onCreateNew={() => openModal(<AssignNewSchemeModal />, panel)}
                />
            )}
        </Stack>
    )
}

export default InitialConfigPanel
