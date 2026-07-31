import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button, Spacer } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import {
    type CleanupRegisterFunction,
    ConfigMode,
    type ConfigurationSubpanelComponent,
    type ConfigurationType,
    fieldConfigModes,
    synthesisRobotConfigModes,
    wpilibRobotConfigModes,
} from "./ConfigTypes"
import AssemblySelection, { type AssemblySelectionOption } from "./configure/AssemblySelection"
import ConfigModeSelection from "./configure/ConfigModeSelection"
import AllianceSelectionInterface from "./interfaces/AllianceSelectionInterface"
import BrainSelectionInterface from "./interfaces/BrainSelectionInterface"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepieceIntakeInterface.tsx"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureGamepieceEjectorInterface.tsx"
import ConfigureJointsInterface from "./interfaces/ConfigureJointsInterface"
import DrivetrainSelectionInterface from "./interfaces/DrivetrainSelectionInterface"
import ConfigureInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import SimulationInterface from "./interfaces/SimulationInterface"
import ConfigureCameraPointsInterface from "./interfaces/ConfigureCameraPointsInterface"
import ConfigureProtectedZonesInterface from "./interfaces/scoring/ConfigureProtectedZonesInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"
import EventSystem from "@/systems/EventSystem.ts"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import { Tab, Tabs } from "@mui/material"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import CommandRegistry, { type CommandDefinition, type CommandProvider } from "@/ui/components/CommandRegistry"
import { globalAddToast, globalOpenPanel } from "@/ui/components/GlobalUIControls"
import AssemblyExportButton from "@/panels/configuring/assembly-config/configure/AssemblyExport.tsx"
import MetadataConfigInterface from "@/panels/configuring/assembly-config/interfaces/MetadataConfigInterface.tsx"
import { FaArrowsRotate } from "react-icons/fa6"
import MoveInterface from "@/panels/configuring/assembly-config/interfaces/MoveInterface.tsx"
import ControlsConfigInterface from "@/panels/configuring/assembly-config/interfaces/ControlsConfigInterface.tsx"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

// Register command: Configure Assets (module-scope side effect)
CommandRegistry.get().registerCommands([
    {
        id: "configure-assets-robots",
        label: "Configure Assets (Robots)",
        description: "Open the asset configuration panel.",
        keywords: ["configure", "asset", "config", "robot", "robots"],
        perform: () =>
            import("./ConfigurePanel").then(m => globalOpenPanel(m.default, { configurationType: "ROBOTS" })),
    },
    {
        id: "configure-assets-fields",
        label: "Configure Assets (Fields)",
        description: "Open the asset configuration panel.",
        keywords: ["configure", "asset", "config", "field", "fields"],
        perform: () =>
            import("./ConfigurePanel").then(m => globalOpenPanel(m.default, { configurationType: "FIELDS" })),
    },
    {
        id: "configure-assets-inputs",
        label: "Configure Assets (Inputs)",
        description: "Open the asset configuration panel.",
        keywords: ["configure", "asset", "config", "input", "inputs"],
        perform: () =>
            import("./ConfigurePanel").then(m => globalOpenPanel(m.default, { configurationType: "INPUTS" })),
    },
])

// Register dynamic provider: per-assembly configure/remove commands (module-scope)
const provider: CommandProvider = () => {
    if (!World.isAlive || !World.sceneRenderer) return []
    const list: CommandDefinition[] = []

    const robots = World.sceneRenderer.mirabufSceneObjects.getRobots() || []
    for (const r of robots) {
        const name = r.assemblyName || "Robot"
        const nameTokens = String(name)
            .split(/\s+|[-_]/g)
            .filter(Boolean)
        list.push({
            id: `configure-robot-${r.id}`,
            label: `Configure ${r.nameTag?.text()} (${name})`,
            description: `Open configuration for robot ${r.nameTag?.text()} (${name}).`,
            keywords: ["configure", "robot", ...nameTokens.map(t => t.toLowerCase())],
            perform: () =>
                import("./ConfigurePanel").then(m =>
                    globalOpenPanel(m.default, {
                        configurationType: "ROBOTS",
                        selectedAssembly: r,
                    })
                ),
        })
        list.push({
            id: `remove-robot-${r.id}`,
            label: `Remove ${r.nameTag?.text()} (${name})`,
            description: `Remove the robot ${r.nameTag?.text()} (${name}).`,
            keywords: ["remove", "delete", "robot", ...nameTokens.map(t => t.toLowerCase())],
            perform: () => {
                World.sceneRenderer.removeSceneObject(r.id)
            },
        })
    }

    const field = World.sceneRenderer.mirabufSceneObjects.getField()
    if (field) {
        const name = field.assemblyName || "Field"
        const nameTokens = String(name)
            .split(/\s+|[-_]/g)
            .filter(Boolean)
        list.push({
            id: `configure-field-${field.id}`,
            label: `Configure ${name}`,
            description: `Open configuration for field ${name}.`,
            keywords: ["configure", "field", ...nameTokens.map(t => t.toLowerCase())],
            perform: () =>
                import("./ConfigurePanel").then(m =>
                    globalOpenPanel(m.default, {
                        configurationType: "FIELDS",
                        selectedAssembly: field,
                    })
                ),
        })
        list.push({
            id: `remove-field-${field.id}`,
            label: `Remove ${name}`,
            description: `Remove the field ${name}.`,
            keywords: ["remove", "delete", "field", ...nameTokens.map(t => t.toLowerCase())],
            perform: () => {
                World.sceneRenderer.removeSceneObject(field.id)
            },
        })
    }

    return list
}
CommandRegistry.get().registerProvider(provider)

export interface ConfigurePanelCustomProps {
    selectedAssembly?: MirabufSceneObject
    configMode?: ConfigMode
    configurationType?: ConfigurationType
}
const subConfigPanels: Record<ConfigMode, ConfigurationSubpanelComponent> = {
    [ConfigMode.JOINTS]: ConfigureJointsInterface,
    [ConfigMode.EJECTOR]: ConfigureShotTrajectoryInterface,
    [ConfigMode.INTAKE]: ConfigureGamepiecePickupInterface,
    [ConfigMode.CONTROLS]: ControlsConfigInterface,
    [ConfigMode.SCORING_ZONES]: ConfigureScoringZonesInterface,
    [ConfigMode.PROTECTED_ZONES]: ConfigureProtectedZonesInterface,
    [ConfigMode.CAMERA_POINTS]: ConfigureCameraPointsInterface,
    [ConfigMode.MOVE]: MoveInterface,
    [ConfigMode.SIM]: SimulationInterface,
    [ConfigMode.BRAIN]: BrainSelectionInterface,
    [ConfigMode.DRIVETRAIN]: DrivetrainSelectionInterface,
    [ConfigMode.ALLIANCE]: AllianceSelectionInterface,
    [ConfigMode.METADATA]: MetadataConfigInterface,
}

const ConfigurePanel: React.FC<PanelImplProps<void, ConfigurePanelCustomProps>> = ({ panel }) => {
    const { configureScreen, closePanel, addToast } = useUIContext()
    const {
        configMode: initialConfigMode,
        selectedAssembly: initialSelectedAssembly,
        configurationType: initialConfigurationType,
    } = panel!.props.custom

    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(initialSelectedAssembly)
    const [configMode, setConfigMode] = useState<ConfigMode | undefined>(initialConfigMode)
    const [configurationType, setConfigurationType] = useState<ConfigurationType>(initialConfigurationType ?? "ROBOTS")
    const [pendingDeletes, setPendingDeletes] = useState<SceneObjectId[]>([])

    const [confirmCallbacks, setConfirmCallbacks] = useState<(() => void | Promise<void>)[]>([])
    const [cancelCallbacks, setCancelCallbacks] = useState<(() => void | Promise<void>)[]>([])
    const [accessedAssemblies, setAccessedAssemblies] = useState<MirabufSceneObject[]>([])

    const registerCleanupFunctions: CleanupRegisterFunction = useCallback((applyFunc?, revertFunc?) => {
        if (applyFunc) {
            setConfirmCallbacks(old => [...old, applyFunc])
        }
        if (revertFunc) {
            setCancelCallbacks(old => [...old, revertFunc])
        }
    }, [])

    useEffect(() => {
        if (selectedAssembly != null) {
            setAccessedAssemblies(v => [...v, selectedAssembly])
        }
    }, [selectedAssembly])

    useEffect(() => {
        return EventSystem.listen("MatchStateChangedEvent", ({ mode }) => {
            if (mode === MatchModeType.AUTONOMOUS) {
                closePanel(panel!.id, CloseType.OVERWRITE)
            }
        })
    }, [closePanel, panel])

    const onBeforeAccept = useCallback(async () => {
        for (const callback of confirmCallbacks) {
            await callback()
        }
        new Set(accessedAssemblies).forEach(assembly => {
            assembly.savePreferences()
        })

        pendingDeletes.forEach(item => {
            World.sceneRenderer.removeSceneObject(item)
        })

        InputSchemeManager.saveSchemes()

        EventSystem.dispatch("ConfigurationSavedEvent")
        setConfirmCallbacks([])
        setCancelCallbacks([])
        setAccessedAssemblies([])
    }, [confirmCallbacks, accessedAssemblies, pendingDeletes])

    const onCancel = useCallback(async () => {
        for (const callback of [...cancelCallbacks].reverse()) {
            // If the same subpanel is opened twice, you want to revert in reverse order
            await callback()
        }

        new Set(accessedAssemblies).forEach(assembly => {
            assembly.savePreferences()
        })

        setConfirmCallbacks([])
        setCancelCallbacks([])
        setAccessedAssemblies([])
        addToast("info", "Configuration reverted")
    }, [cancelCallbacks, accessedAssemblies, addToast])

    const hasMadeChanges = useMemo(
        () => confirmCallbacks.length > 0 || cancelCallbacks.length > 0,
        [confirmCallbacks, cancelCallbacks]
    )

    const onClose = useCallback(
        async (closeType: CloseType) => {
            if (closeType == CloseType.OVERWRITE && hasMadeChanges) {
                await onBeforeAccept()
            }
        },
        [hasMadeChanges, onBeforeAccept]
    )

    useEffect(() => {
        configureScreen(
            panel!,
            { title: "Configure Assets", acceptText: "Save", cancelText: hasMadeChanges ? "Revert" : "Cancel" },
            { onBeforeAccept, onCancel, onClose }
        )
    }, [onBeforeAccept, onCancel, onClose, configureScreen, panel, hasMadeChanges])

    const modes = useMemo(() => {
        if (configurationType == "FIELDS") {
            return fieldConfigModes
        }
        if (configurationType == "ROBOTS") {
            if (selectedAssembly?.brain?.isSynthesis()) {
                return synthesisRobotConfigModes
            }
            return wpilibRobotConfigModes
        }
        return []
    }, [configurationType, selectedAssembly?.brain])

    const ConfigSubPanel: ConfigurationSubpanelComponent | null = useMemo(() => {
        if (configMode == null || selectedAssembly == null) {
            return null
        }
        return subConfigPanels[configMode]
    }, [configMode, selectedAssembly])

    return (
        <>
            <Tabs
                value={configurationType}
                onChange={(_, newValue) => setConfigurationType(newValue)}
                textColor="inherit"
                indicatorColor="primary"
                centered
                {...SoundPlayer.getInstance().buttonSoundEffects()}
            >
                <Tab key="robots" value="ROBOTS" label="ROBOTS" />
                <Tab key="fields" value="FIELDS" label="FIELDS" />
                <Tab key="inputs" value="INPUTS" label="INPUTS" />
            </Tabs>
            {configurationType === "INPUTS" && (
                <ConfigureInputsInterface panel={panel!} registerCleanupFunction={registerCleanupFunctions} />
            )}
            {configurationType !== "INPUTS" && (
                <>
                    <AssemblySelection
                        panel={panel!}
                        configurationType={configurationType}
                        onAssemblySelected={a => {
                            if (configMode !== undefined) EventSystem.dispatch("ConfigurationSavedEvent")
                            setConfigMode(undefined)
                            setSelectedAssembly(a as MirabufSceneObject)
                        }}
                        selectedAssembly={selectedAssembly}
                        onStageDelete={opt => {
                            const id = (opt as AssemblySelectionOption).assemblyObject.id
                            setPendingDeletes(prev => [...prev, id])
                        }}
                        pendingDeletes={pendingDeletes}
                    />
                    {selectedAssembly !== undefined && (
                        <ConfigModeSelection
                            modes={modes}
                            configMode={configMode}
                            onModeSelected={mode => {
                                if (configMode !== undefined) EventSystem.dispatch("ConfigurationSavedEvent")
                                setConfigMode(mode)
                            }}
                        />
                    )}
                    {ConfigSubPanel != null && (
                        <ConfigSubPanel
                            panel={panel!}
                            selectedAssembly={selectedAssembly!}
                            hasMadeChanges={hasMadeChanges}
                            registerCleanupFunction={registerCleanupFunctions}
                        />
                    )}
                    {configMode === undefined && selectedAssembly !== undefined && (
                        <>
                            <Spacer height={16} />
                            <AssemblyExportButton selectedAssembly={selectedAssembly} />
                            <Spacer height={16} />
                            <Button
                                className={"w-full"}
                                color={"warning"}
                                onClick={() => {
                                    closePanel(panel!.id, CloseType.ACCEPT)
                                    selectedAssembly.resetPreferences()
                                    globalAddToast(
                                        "info",
                                        "Preferences for " + selectedAssembly.descriptiveName + " reset"
                                    )
                                }}
                            >
                                Reset
                                <Spacer width={5} />
                                <FaArrowsRotate />
                            </Button>
                        </>
                    )}
                </>
            )}
        </>
    )
}

export default ConfigurePanel
