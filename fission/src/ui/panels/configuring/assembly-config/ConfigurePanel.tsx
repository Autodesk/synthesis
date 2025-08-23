import type React from "react"
import { useEffect, useMemo, useRef, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import type { InputScheme } from "@/systems/input/InputTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { FieldPreferences, MotorPreferences, RobotPreferences } from "@/systems/preferences/PreferenceTypes"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { CloseType, type UIScreen, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ChooseInputSchemePanel from "../ChooseInputSchemePanel"
import { ConfigMode, type ConfigurationType } from "./ConfigTypes"
import AssemblySelection, { type AssemblySelectionOption } from "./configure/AssemblySelection"
import ConfigModeSelection, { ConfigModeSelectionOption } from "./configure/ConfigModeSelection"
import AllianceSelectionInterface from "./interfaces/AllianceSelectionInterface"
import BrainSelectionInterface from "./interfaces/BrainSelectionInterface"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureSubsystemsInterface from "./interfaces/ConfigureSubsystemsInterface"
import DrivetrainSelectionInterface from "./interfaces/DrivetrainSelectionInterface"
import ConfigureInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import ConfigureSchemeInterface from "./interfaces/inputs/ConfigureSchemeInterface"
import SequentialBehaviorsInterface from "./interfaces/SequentialBehaviorsInterface"
import SimulationInterface from "./interfaces/SimulationInterface"
import ConfigureProtectedZonesInterface from "./interfaces/scoring/ConfigureProtectedZonesInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"
import { Tab, Tabs } from "@mui/material"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import CommandRegistry, { type CommandDefinition, type CommandProvider } from "@/ui/components/CommandRegistry"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"

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

interface ConfigInterfaceProps<T, P> {
    panel: UIScreen<T, P>
    configMode: ConfigMode
    assembly: MirabufSceneObject
}

const ConfigInterface: React.FC<ConfigInterfaceProps<void, ConfigurePanelCustomProps>> = ({
    panel,
    configMode,
    assembly,
}) => {
    const { openPanel, closePanel } = useUIContext()

    switch (configMode) {
        case ConfigMode.INTAKE:
            return <ConfigureGamepiecePickupInterface selectedRobot={assembly} />
        case ConfigMode.EJECTOR:
            return <ConfigureShotTrajectoryInterface selectedRobot={assembly} />
        case ConfigMode.SUBSYSTEMS:
            return <ConfigureSubsystemsInterface selectedRobot={assembly} />
        case ConfigMode.CONTROLS: {
            const brainIndex = (assembly.brain as SynthesisBrain).brainIndex
            const scheme = InputSystem.brainIndexSchemeMap.get(brainIndex)

            return (
                <>
                    <Button
                        onClick={() => {
                            setSpotlightAssembly(assembly)
                            openPanel(ChooseInputSchemePanel, undefined, panel)
                            closePanel(panel.id, CloseType.Overwrite)
                        }}
                    >
                        Set Scheme
                    </Button>
                    {scheme && <ConfigureSchemeInterface selectedScheme={scheme} panelId={panel?.id} />}
                </>
            )
        }
        case ConfigMode.SEQUENTIAL:
            return <SequentialBehaviorsInterface selectedRobot={assembly} />
        case ConfigMode.SCORING_ZONES: {
            const zones = assembly.fieldPreferences?.scoringZones ?? []
            if (zones === undefined) {
                console.error("Field does not contain scoring zone preferences!")
                return <Label size="md">ERROR: Field does not contain scoring zone configuration!</Label>
            }
            return <ConfigureScoringZonesInterface selectedField={assembly} initialZones={zones} />
        }
        case ConfigMode.PROTECTED_ZONES: {
            const zones = assembly.fieldPreferences?.protectedZones ?? []
            if (zones === undefined) {
                console.error("Field does not contain protected zone preferences!")
                return <Label size="md">ERROR: Field does not contain protected zone configuration!</Label>
            }
            return <ConfigureProtectedZonesInterface selectedField={assembly} initialZones={zones} />
        }
        case ConfigMode.MOVE:
            return (
                <TransformGizmoControl
                    key="config-move-gizmo"
                    defaultMode="translate"
                    scaleDisabled={true}
                    size={3.0}
                    parent={assembly}
                    onAccept={() => closePanel(panel.id, CloseType.Accept)}
                    onCancel={() => closePanel(panel.id, CloseType.Cancel)}
                />
            )
        case ConfigMode.SIM:
            return <SimulationInterface selectedAssembly={assembly} />
        case ConfigMode.BRAIN:
            return <BrainSelectionInterface selectedAssembly={assembly} />
        case ConfigMode.ALLIANCE:
            return <AllianceSelectionInterface selectedAssembly={assembly} />
        case ConfigMode.DRIVETRAIN:
            return <DrivetrainSelectionInterface selectedAssembly={assembly} />
        default:
            throw new Error(`Config mode ${configMode} has no associated interface`)
    }
}

export interface ConfigurePanelCustomProps {
    selectedAssembly?: MirabufSceneObject
    configMode?: ConfigMode
    configurationType?: ConfigurationType
}

const ConfigurePanel: React.FC<PanelImplProps<void, ConfigurePanelCustomProps>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    const {
        configMode: initialConfigMode,
        selectedAssembly: initialSelectedAssembly,
        configurationType: initialConfigurationType,
    } = panel!.props.custom

    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(initialSelectedAssembly)
    const [configMode, setConfigMode] = useState<ConfigMode | undefined>(initialConfigMode)
    const [configurationType, setConfigurationType] = useState<ConfigurationType>(initialConfigurationType ?? "ROBOTS")
    const [pendingDeletes, setPendingDeletes] = useState<number[]>([])

    const originalRobotPrefs = useRef<RobotPreferences | null>(null)
    const originalFieldPrefs = useRef<FieldPreferences | null>(null)
    const originalMotorPrefs = useRef<MotorPreferences | null>(null)
    const originalInputSchemes = useRef<InputScheme[] | null>(null)

    useEffect(() => {
        const allSchemes: InputScheme[] = PreferencesSystem.getGlobalPreference("InputSchemes") || []
        originalInputSchemes.current = structuredClone(allSchemes)

        if (selectedAssembly) {
            const name = selectedAssembly.assemblyName

            const robotPrefs = PreferencesSystem.getRobotPreferences(name)
            const fieldPrefs = PreferencesSystem.getFieldPreferences(name)
            const motorPrefs = PreferencesSystem.getMotorPreferences(name)

            if (robotPrefs) originalRobotPrefs.current = structuredClone(robotPrefs)
            if (fieldPrefs) originalFieldPrefs.current = structuredClone(fieldPrefs)
            if (motorPrefs) originalMotorPrefs.current = structuredClone(motorPrefs)
        }

        // Listen for input scheme changes from other panels
        const handleExternalSchemeChange = (event: Event) => {
            const customEvent = event as CustomEvent

            if (customEvent.detail?.panelId === panel?.id) return

            const currentSchemes: InputScheme[] = InputSchemeManager.allInputSchemes
            originalInputSchemes.current = structuredClone(currentSchemes)
        }

        window.addEventListener("inputSchemeChanged", handleExternalSchemeChange)

        return () => {
            window.removeEventListener("inputSchemeChanged", handleExternalSchemeChange)
        }
    }, [])

    useEffect(() => {
        const onBeforeAccept = () => {
            pendingDeletes.forEach(id => World.sceneRenderer.removeSceneObject(id))
            setPendingDeletes([])

            InputSchemeManager.saveSchemes(panel?.id)

            originalRobotPrefs.current = null
            originalFieldPrefs.current = null
            originalMotorPrefs.current = null
            originalInputSchemes.current = null

            selectedAssembly?.sendPreferences()
            new ConfigurationSavedEvent()
        }
        const onCancel = () => {
            setPendingDeletes([])

            if (selectedAssembly) {
                const name = selectedAssembly.assemblyName

                if (originalRobotPrefs.current) PreferencesSystem.setRobotPreferences(name, originalRobotPrefs.current)
                if (originalFieldPrefs.current) PreferencesSystem.setFieldPreferences(name, originalFieldPrefs.current)
                if (originalMotorPrefs.current) PreferencesSystem.setMotorPreferences(name, originalMotorPrefs.current)
                selectedAssembly.getPreferences()
            }

            if (originalInputSchemes.current) {
                PreferencesSystem.setGlobalPreference("InputSchemes", originalInputSchemes.current)
                PreferencesSystem.savePreferences()
                InputSchemeManager.resetDefaultSchemes(panel?.id)
            }

            originalRobotPrefs.current = null
            originalFieldPrefs.current = null
            originalMotorPrefs.current = null
            originalInputSchemes.current = null
        }

        configureScreen(
            panel!,
            { title: "Configure Assets", acceptText: "Save", cancelText: "Cancel" },
            { onBeforeAccept, onCancel }
        )
    }, [selectedAssembly, pendingDeletes])

    const modes = useMemo(() => {
        switch (configurationType) {
            case "ROBOTS":
                return [
                    new ConfigModeSelectionOption(
                        "Brain",
                        ConfigMode.BRAIN,
                        "Select and modify what is controlling of the robot."
                    ),

                    new ConfigModeSelectionOption(
                        "Move",
                        ConfigMode.MOVE,
                        "Adjust position of robot relative to field."
                    ),

                    new ConfigModeSelectionOption("Drivetrain", ConfigMode.DRIVETRAIN, "Sets the drivetrain type."),

                    new ConfigModeSelectionOption(
                        "Intake",
                        ConfigMode.INTAKE,
                        "Configure the robot’s intake position and parent node for picking up game pieces."
                    ),

                    new ConfigModeSelectionOption(
                        "Ejector",
                        ConfigMode.EJECTOR,
                        "Configure the robot’s ejector mechanism, which controls the release or expulsion of game pieces."
                    ),

                    new ConfigModeSelectionOption(
                        "Configure Joints",
                        ConfigMode.SUBSYSTEMS,
                        "Set the velocities, torques, and accelerations of your robot's motors."
                    ),

                    new ConfigModeSelectionOption(
                        "Sequence Joints",
                        ConfigMode.SEQUENTIAL,
                        "Set which joints follow each other. For example, the second stage of an elevator could follow the first, moving in unison with it."
                    ),

                    new ConfigModeSelectionOption(
                        "Alliance / Station",
                        ConfigMode.ALLIANCE,
                        "Set the robot's alliance color for matches. (red or blue)"
                    ),
                    selectedAssembly?.brain?.brainType === "wpilib"
                        ? new ConfigModeSelectionOption(
                              "Simulation",
                              ConfigMode.SIM,
                              "Configure the WPILib simulation settings for this robot."
                          )
                        : new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS, "Set your controller scheme."),
                ]
            case "FIELDS":
                return [
                    new ConfigModeSelectionOption(
                        "Move",
                        ConfigMode.MOVE,
                        "Adjust position of field relative to robot."
                    ),
                    new ConfigModeSelectionOption(
                        "Scoring Zones",
                        ConfigMode.SCORING_ZONES,
                        "Define and manage zones on the field where robots can earn points during simulation."
                    ),
                    new ConfigModeSelectionOption(
                        "Protected Zones",
                        ConfigMode.PROTECTED_ZONES,
                        "Define and manage protected zones on the field where robots can not enter."
                    ),
                ]
            default:
                return []
        }
    }, [configurationType, selectedAssembly?.brain?.brainType])

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
            {configurationType === "INPUTS" && <ConfigureInputsInterface />}
            {configurationType !== "INPUTS" && (
                <>
                    <AssemblySelection
                        panel={panel!}
                        configurationType={configurationType}
                        onAssemblySelected={a => {
                            if (configMode !== undefined) new ConfigurationSavedEvent()
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
                                if (configMode !== undefined) new ConfigurationSavedEvent()
                                setConfigMode(mode)
                            }}
                        />
                    )}
                    {configMode !== undefined && selectedAssembly !== undefined && (
                        <ConfigInterface panel={panel!} configMode={configMode} assembly={selectedAssembly} />
                    )}
                </>
            )}
        </>
    )
}

export default ConfigurePanel
