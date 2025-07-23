import { useCallback, useEffect, useMemo, useState } from "react"
import Button from "@/components/Button"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { getSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import { mirabufPanelState } from "@/panels/mirabuf/MirabufState.tsx"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import { PAUSE_REF_ASSEMBLY_MOVE } from "@/systems/physics/PhysicsSystem"
import { Alliance, Station } from "@/systems/preferences/PreferenceTypes"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { ConfigurationType, setSelectedConfigurationType } from "../assembly-config/ConfigurationType"
import { setSelectedScheme } from "../assembly-config/interfaces/inputs/ConfigureInputsInterface"
import InputSchemeSelection from "./InputSchemeSelection"

const InitialConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel, openPanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()
    const [alliance, setAlliance] = useState<Alliance>("red")
    const [station, setStation] = useState<Station>(1)

    const targetAssembly = useMemo(() => {
        return getSpotlightAssembly()
    }, [])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_MOVE)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_MOVE)
        }
    }, [])

    // biome-ignore lint: Making closePanel a dep causes maxium depth exceeded errors
    useEffect(() => {
        closePanel("import-mirabuf")
        mirabufPanelState.hasUnconfirmedImport = true

        return () => {
            mirabufPanelState.hasUnconfirmedImport = false
        }
    }, [])

    const closeFinish = useCallback(() => {
        if (targetAssembly?.miraType == MiraType.ROBOT) {
            targetAssembly.alliance = alliance
            targetAssembly.station = station
            SimulationSystem.addPerRobotScore(targetAssembly, 0) // Initialize score for the robot

            setSelectedConfigurationType(ConfigurationType.ROBOT)
            const brainIndex = SynthesisBrain.getBrainIndex(targetAssembly)

            if (brainIndex == undefined) return
            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]
            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)

            setSelectedScheme(scheme)
        } else {
            setSelectedConfigurationType(ConfigurationType.FIELD)
        }

        closePanel(panelId)
    }, [closePanel, panelId, alliance, station, targetAssembly])

    const closeDelete = useCallback(() => {
        if (targetAssembly) {
            World.sceneRenderer.removeSceneObject(targetAssembly.id)
        }

        closePanel(panelId)
    }, [closePanel, panelId, targetAssembly])

    const brainIndex = useMemo(() => {
        return SynthesisBrain.getBrainIndex(targetAssembly)
    }, [targetAssembly])

    return (
        <Panel
            name="Assembly Setup"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={true}
            acceptName="Finish"
            onAccept={() => closeFinish()}
            icon={SynthesisIcons.GAMEPAD}
            cancelEnabled={true}
            cancelName="Remove"
            onCancel={() => closeDelete()}
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                {targetAssembly?.miraType === MiraType.ROBOT ? (
                    <div>
                        <Label>Alliance: </Label>
                        {/** Set the alliance color */}
                        <Button
                            value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                            onClick={() => {
                                setAlliance(alliance == "blue" ? "red" : "blue")
                            }}
                            colorOverrideClass={`bg-match-${alliance}-alliance`}
                        />
                        <div className="mt-4">
                            <Label>Station: </Label>
                            {/** Set the station number */}
                            <div className="flex gap-2">
                                <Button
                                    value="1"
                                    onClick={() => setStation(1)}
                                    colorOverrideClass={station === 1 ? `bg-match-${alliance}-alliance` : ""}
                                />
                                <Button
                                    value="2"
                                    onClick={() => setStation(2)}
                                    colorOverrideClass={station === 2 ? `bg-match-${alliance}-alliance` : ""}
                                />
                                <Button
                                    value="3"
                                    onClick={() => setStation(3)}
                                    colorOverrideClass={station === 3 ? `bg-match-${alliance}-alliance` : ""}
                                />
                            </div>
                        </div>
                        <div className="mb-4"></div>
                    </div>
                ) : (
                    <></>
                )}
                {targetAssembly ? (
                    <TransformGizmoControl
                        key={"init-config-gizmo"}
                        defaultMode="translate"
                        scaleDisabled={true}
                        size={3.0}
                        parent={targetAssembly}
                        onAccept={closeFinish}
                        onCancel={closeDelete}
                    />
                ) : (
                    <></>
                )}
                {brainIndex != undefined ? (
                    <InputSchemeSelection
                        brainIndex={brainIndex}
                        onSelect={() => {}}
                        onEdit={() => openPanel("configure")}
                        onCreateNew={() => openModal("assign-new-scheme")}
                    />
                ) : (
                    <></>
                )}
            </div>
        </Panel>
    )
}

export default InitialConfigPanel
