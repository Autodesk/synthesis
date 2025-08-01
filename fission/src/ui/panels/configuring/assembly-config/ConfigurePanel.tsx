import { Button, ToggleButton, ToggleButtonGroup } from "@mui/material"
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
import type { PanelImplProps } from "@/ui/components/Panel"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { CloseType, useUIContext, type UIScreen } from "@/ui/helpers/UIProviderHelpers"
import ChooseInputSchemePanel from "../ChooseInputSchemePanel"
import AssemblySelection, { type AssemblySelectionOption } from "./configure/AssemblySelection"
import ConfigModeSelection, { ConfigModeSelectionOption } from "./configure/ConfigModeSelection"
import AllianceSelectionInterface from "./interfaces/AllianceSelectionInterface"
import BrainSelectionInterface from "./interfaces/BrainSelectionInterface"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureSubsystemsInterface from "./interfaces/ConfigureSubsystemsInterface"
import ConfigureInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import ConfigureSchemeInterface from "./interfaces/inputs/ConfigureSchemeInterface"
import SequentialBehaviorsInterface from "./interfaces/SequentialBehaviorsInterface"
import SimulationInterface from "./interfaces/SimulationInterface"
import ConfigureProtectedZonesInterface from "./interfaces/scoring/ConfigureProtectedZonesInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"
import Label from "@/ui/components/Label"
import DrivetrainSelectionInterface from "./interfaces/DrivetrainSelectionInterface"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { ConfigMode, CONFIG_OPTS } from "./ConfigTypes"

export interface ConfigurePanelSettings {
    configMode?: ConfigMode
    selectedAssembly: MirabufSceneObject
}

interface ConfigInterfaceProps<T> {
    panel: UIScreen<T>
    configMode: ConfigMode
    assembly: MirabufSceneObject
}

const ConfigInterface: React.FC<ConfigInterfaceProps<void>> = ({ panel, configMode, assembly }) => {
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
                            openPanel(<ChooseInputSchemePanel />, panel)
                            closePanel(panel.id, CloseType.Overwrite)
                        }}
                    >
                        Set Scheme
                    </Button>
                    {scheme && <ConfigureSchemeInterface selectedScheme={scheme} />}
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

const ConfigurePanel: React.FC<PanelImplProps<void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const { configurationType, setConfigurationType, configurePanelSettings, setConfigurePanelSettings } =
        useStateContext()

    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(
        configurePanelSettings?.selectedAssembly
    )
    const [configMode, setConfigMode] = useState<ConfigMode | undefined>(configurePanelSettings?.configMode)
    const [pendingDeletes, setPendingDeletes] = useState<number[]>([])

    const originalRobotPrefs = useRef<RobotPreferences | null>(null)
    const originalFieldPrefs = useRef<FieldPreferences | null>(null)
    const originalMotorPrefs = useRef<MotorPreferences | null>(null)
    const originalInputSchemes = useRef<InputScheme[] | null>(null)

    useEffect(() => {
        const allSchemes: InputScheme[] = PreferencesSystem.getGlobalPreference("InputSchemes") || []
        originalInputSchemes.current = structuredClone(allSchemes)

        const settings = configurePanelSettings

        if (settings) {
            setConfigMode(settings.configMode)
            if (settings.selectedAssembly) {
                setSelectedAssembly(settings.selectedAssembly)

                const name = settings.selectedAssembly.assemblyName

                const robotPrefs = PreferencesSystem.getRobotPreferences(name)
                const fieldPrefs = PreferencesSystem.getFieldPreferences(name)
                const motorPrefs = PreferencesSystem.getMotorPreferences(name)

                if (robotPrefs) originalRobotPrefs.current = structuredClone(robotPrefs)
                if (fieldPrefs) originalFieldPrefs.current = structuredClone(fieldPrefs)
                if (motorPrefs) originalMotorPrefs.current = structuredClone(motorPrefs)
            }
        }
    }, [])

    useEffect(() => {
        const onBeforeAccept = () => {
            pendingDeletes.forEach(id => World.sceneRenderer.removeSceneObject(id))
            setPendingDeletes([])

            InputSchemeManager.saveSchemes()

            originalRobotPrefs.current = null
            originalFieldPrefs.current = null
            originalMotorPrefs.current = null
            originalInputSchemes.current = null

            setConfigurationType(configurationType)
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
                InputSchemeManager.resetDefaultSchemes()
            }

            originalRobotPrefs.current = null
            originalFieldPrefs.current = null
            originalMotorPrefs.current = null
            originalInputSchemes.current = null
        }

        const onClose = () => {
            setConfigurePanelSettings(undefined)
        }

        configureScreen(
            panel!,
            { title: "Configure Assets", acceptText: "Save", cancelText: "Cancel" },
            { onBeforeAccept, onCancel, onClose }
        )
    }, [configurationType, selectedAssembly, pendingDeletes])

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
            <ToggleButtonGroup
                value={configurationType}
                exclusive
                onChange={(_e, v) => {
                    if (v !== null) {
                        setConfigurationType(v)
                    }

                    setSelectedAssembly(undefined)
                    new ConfigurationSavedEvent()
                    setConfigMode(undefined)
                }}
            >
                {CONFIG_OPTS.map(opt => (
                    <ToggleButton key={opt} value={opt}>
                        {opt}
                    </ToggleButton>
                ))}
            </ToggleButtonGroup>
            {configurationType === "INPUTS" && <ConfigureInputsInterface />}
            {configurationType !== "INPUTS" && (
                <>
                    <AssemblySelection
                        panel={panel!}
                        configurationType={configurationType}
                        onAssemblySelected={a => {
                            if (configMode !== undefined) new ConfigurationSavedEvent()
                            setConfigMode(undefined)
                            setSelectedAssembly(a)
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
