import { MouseEvent, useEffect, useMemo, useReducer, useRef, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import DrivetrainSelectionInterface from "@/panels/configuring/assembly-config/interfaces/DrivetrainSelectionInterface.tsx"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { FieldPreferences, MotorPreferences, RobotPreferences } from "@/systems/preferences/PreferenceTypes"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import World from "@/systems/World"
import Button from "@/ui/components/Button"
import Label from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { ConfigurationSavedEvent } from "./ConfigurationSavedEvent"
import { ConfigurationType, getConfigurationType, setSelectedConfigurationType } from "./ConfigurationType"
import { ConfigMode, popConfigurePanelSettings } from "./ConfigurePanelControls"
import AllianceSelectionInterface from "./interfaces/AllianceSelectionInterface"
import BrainSelectionInterface from "./interfaces/BrainSelectionInterface"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureSubsystemsInterface from "./interfaces/ConfigureSubsystemsInterface"
import ChangeInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import ConfigureSchemeInterface from "./interfaces/inputs/ConfigureSchemeInterface"
import SequentialBehaviorsInterface from "./interfaces/SequentialBehaviorsInterface"
import SimulationInterface from "./interfaces/SimulationInterface"
import ConfigureProtectedZonesInterface from "./interfaces/scoring/ConfigureProtectedZonesInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"

/** Option for selecting a robot of field */
class AssemblySelectionOption extends SelectMenuOption {
    assemblyObject: MirabufSceneObject

    constructor(name: string, assemblyObject: MirabufSceneObject) {
        super(assemblyObject.id.toString(), name)
        this.assemblyObject = assemblyObject
    }
}

interface ConfigurationSelectionProps {
    configurationType: ConfigurationType
    onAssemblySelected: (assembly: MirabufSceneObject | undefined) => void
    selectedAssembly?: MirabufSceneObject
    onStageDelete: (opt: SelectMenuOption) => void
    pendingDeletes: number[]
}

function makeSelectionOption(configurationType: ConfigurationType, assembly: MirabufSceneObject) {
    return new AssemblySelectionOption(
        `${configurationType == ConfigurationType.ROBOT ? `[${InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}]` : ""} ${assembly.assemblyName}`,
        assembly
    )
}

const AssemblySelection: React.FC<ConfigurationSelectionProps> = ({
    configurationType,
    onAssemblySelected,
    selectedAssembly,
    onStageDelete,
    pendingDeletes,
}) => {
    // Update is used when a robot or field is deleted to update the select menu
    const [_u, update] = useReducer(x => !x, false)
    const { openPanel } = usePanelControlContext()

    const robots = useMemo(() => {
        return [...World.sceneRenderer.sceneObjects.values()]
            .filter(x => x instanceof MirabufSceneObject && x.miraType === MiraType.ROBOT)
            .filter(x => !pendingDeletes.includes(x.id))
    }, [pendingDeletes])

    const fields = useMemo(() => {
        return [...World.sceneRenderer.sceneObjects.values()]
            .filter(x => x instanceof MirabufSceneObject && x.miraType === MiraType.FIELD)
            .filter(x => !pendingDeletes.includes(x.id))
    }, [pendingDeletes])

    const options = useMemo(() => {
        const list = configurationType == ConfigurationType.ROBOT ? robots : fields
        return list
            .filter((assembly): assembly is MirabufSceneObject => assembly != null)
            .map(assembly => makeSelectionOption(configurationType, assembly))
    }, [configurationType, robots, fields])

    /** Robot or field select menu */
    return (
        <SelectMenu
            options={options}
            onOptionSelected={val => onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)}
            defaultHeaderText={`Select a ${configurationType == ConfigurationType.ROBOT ? "Robot" : "Field"}`}
            onDelete={val => {
                onStageDelete(val)
                update()
            }}
            onAddClicked={() => {
                openPanel("import-mirabuf")
            }}
            noOptionsText={`No ${configurationType == ConfigurationType.ROBOT ? "robots" : "fields"} spawned!`}
            defaultSelectedOption={
                selectedAssembly ? makeSelectionOption(configurationType, selectedAssembly) : undefined
            }
        />
    )
}

class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode

    constructor(name: string, configMode: ConfigMode, tooltip?: string) {
        super(name, name, tooltip)
        this.configMode = configMode
    }
}

function getRobotModes(assembly: MirabufSceneObject): Map<ConfigMode, ConfigModeSelectionOption> {
    const modes = new Map<ConfigMode, ConfigModeSelectionOption>([
        [
            ConfigMode.BRAIN,
            new ConfigModeSelectionOption(
                "Brain",
                ConfigMode.BRAIN,
                "Select and modify what is controlling of the robot."
            ),
        ],
        [
            ConfigMode.MOVE,
            new ConfigModeSelectionOption("Move", ConfigMode.MOVE, "Adjust position of robot relative to field."),
        ],
        [
            ConfigMode.DRIVETRAIN,
            new ConfigModeSelectionOption("Drivetrain", ConfigMode.DRIVETRAIN, "Sets the drivetrain type ."),
        ],
        [
            ConfigMode.INTAKE,
            new ConfigModeSelectionOption(
                "Intake",
                ConfigMode.INTAKE,
                "Configure the robot’s intake position and parent node for picking up game pieces."
            ),
        ],
        [
            ConfigMode.EJECTOR,
            new ConfigModeSelectionOption(
                "Ejector",
                ConfigMode.EJECTOR,
                "Configure the robot’s ejector mechanism, which controls the release or expulsion of game pieces."
            ),
        ],
        [
            ConfigMode.SUBSYSTEMS,
            new ConfigModeSelectionOption(
                "Configure Joints",
                ConfigMode.SUBSYSTEMS,
                "Set the velocities, torques, and accelerations of your robot's motors."
            ),
        ],
        [
            ConfigMode.SEQUENTIAL,
            new ConfigModeSelectionOption(
                "Sequence Joints",
                ConfigMode.SEQUENTIAL,
                "Set which joints follow each other. For example, the second stage of an elevator could follow the first, moving in unison with it."
            ),
        ],
        [
            ConfigMode.ALLIANCE,
            new ConfigModeSelectionOption(
                "Alliance / Station",
                ConfigMode.ALLIANCE,
                "Set the robot's alliance color and station number for matches. (red or blue, 1-3)"
            ),
        ],
    ])

    switch (assembly.brain?.brainType) {
        case "wpilib":
            modes.set(
                ConfigMode.SIM,
                new ConfigModeSelectionOption(
                    "Simulation",
                    ConfigMode.SIM,
                    "Configure the WPILib simulation settings for this robot."
                )
            )
            break
        case "synthesis":
            modes.set(
                ConfigMode.CONTROLS,
                new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS, "Set your controller scheme.")
            )
            break
        default:
            break
    }

    return modes
}

const fieldModes: Map<ConfigMode, ConfigModeSelectionOption> = new Map<ConfigMode, ConfigModeSelectionOption>([
    [
        ConfigMode.MOVE,
        new ConfigModeSelectionOption("Move", ConfigMode.MOVE, "Adjust position of field relative to robot."),
    ],
    [
        ConfigMode.SCORING_ZONES,
        new ConfigModeSelectionOption(
            "Scoring Zones",
            ConfigMode.SCORING_ZONES,
            "Define and manage zones on the field where robots can earn points during simulation."
        ),
    ],
    [
        ConfigMode.PROTECTED_ZONES,
        new ConfigModeSelectionOption(
            "Protected Zones",
            ConfigMode.PROTECTED_ZONES,
            "Define and manage protected zones on the field where robots can not enter."
        ),
    ],
])

interface ConfigModeSelectionProps {
    configurationType: ConfigurationType
    onModeSelected: (mode: ConfigMode | undefined) => void
    selectedMode?: ConfigMode
    assembly: MirabufSceneObject
}

const ConfigModeSelection: React.FC<ConfigModeSelectionProps> = ({
    configurationType,
    onModeSelected,
    selectedMode,
    assembly,
}) => {
    // Not sure about leaving this outside of a hook
    const robotModes = getRobotModes(assembly)

    return (
        <SelectMenu
            options={configurationType == ConfigurationType.ROBOT ? [...robotModes.values()] : [...fieldModes.values()]}
            onOptionSelected={val => {
                onModeSelected((val as ConfigModeSelectionOption)?.configMode)
            }}
            defaultHeaderText="Select a Configuration Mode"
            indentation={1}
            defaultSelectedOption={
                selectedMode
                    ? configurationType == ConfigurationType.ROBOT
                        ? robotModes.get(selectedMode)!
                        : fieldModes.get(selectedMode)!
                    : undefined
            }
        />
    )
}

interface ConfigInterfaceProps {
    configMode: ConfigMode
    assembly: MirabufSceneObject
    openPanel: (panelId: string) => void
    closePanel: (panelId: string) => void
}

/** The interface for the actual configuration */
const ConfigInterface: React.FC<ConfigInterfaceProps> = ({ configMode, assembly, openPanel, closePanel }) => {
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
                        value="Set Scheme"
                        onClick={() => {
                            setSpotlightAssembly(assembly)
                            openPanel("choose-scheme")
                        }}
                    />
                    {scheme && <ConfigureSchemeInterface selectedScheme={scheme} />}
                </>
            )
        }
        case ConfigMode.SEQUENTIAL:
            return <SequentialBehaviorsInterface selectedRobot={assembly} />
        case ConfigMode.SCORING_ZONES: {
            const zones = assembly.fieldPreferences?.scoringZones
            if (zones == undefined) {
                console.error("Field does not contain scoring zone preferences!")
                return <Label>ERROR: Field does not contain scoring zone configuration!</Label>
            }
            return <ConfigureScoringZonesInterface selectedField={assembly} initialZones={zones} />
        }
        case ConfigMode.PROTECTED_ZONES: {
            const zones = assembly.fieldPreferences?.protectedZones ?? []
            if (zones == undefined) {
                console.error("Field does not contain protected zone preferences!")
                return <Label>ERROR: Field does not contain protected zone configuration!</Label>
            }
            return <ConfigureProtectedZonesInterface selectedField={assembly} initialZones={zones} />
        }
        case ConfigMode.MOVE: {
            return (
                <TransformGizmoControl
                    key={"config-move-gizmo"}
                    defaultMode="translate"
                    scaleDisabled={true}
                    size={3.0}
                    parent={assembly}
                    onAccept={() => closePanel("configure")}
                    onCancel={() => closePanel("configure")}
                />
            )
        }
        case ConfigMode.SIM: {
            return <SimulationInterface selectedAssembly={assembly} />
        }
        case ConfigMode.BRAIN: {
            return <BrainSelectionInterface selectedAssembly={assembly} />
        }
        case ConfigMode.ALLIANCE: {
            return <AllianceSelectionInterface selectedAssembly={assembly} />
        }
        case ConfigMode.DRIVETRAIN: {
            return <DrivetrainSelectionInterface selectedAssembly={assembly} />
        }
        default:
            throw new Error(`Config mode ${configMode} has no associated interface`)
    }
}

const ConfigurePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const originalRobotPrefs = useRef<RobotPreferences | null>(null)
    const originalFieldPrefs = useRef<FieldPreferences | null>(null)
    const originalMotorPrefs = useRef<MotorPreferences | null>(null)
    const originalInputSchemes = useRef<InputScheme[] | null>(null)

    const { openPanel, closePanel } = usePanelControlContext()
    const [configurationType, setConfigurationType] = useState<ConfigurationType>(getConfigurationType())
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)
    const [configMode, setConfigMode] = useState<ConfigMode | undefined>(undefined)
    const [pendingDeletes, setPendingDeletes] = useState<number[]>([])

    // biome-ignore lint: Making closePanel a dep causes a depth exceeded error
    useEffect(() => {
        const allSchemes = PreferencesSystem.getGlobalPreference("InputSchemes") || []
        originalInputSchemes.current = structuredClone(allSchemes)

        const settings = popConfigurePanelSettings()
        if (settings) {
            setSelectedAssembly(settings.selectedAssembly)
            if (settings.selectedAssembly) {
                setConfigMode(settings.configMode)

                const name = settings.selectedAssembly.assemblyName

                const robotPrefs = PreferencesSystem.getRobotPreferences(name)
                const fieldPrefs = PreferencesSystem.getFieldPreferences(name)
                const motorPrefs = PreferencesSystem.getMotorPreferences(name)

                if (robotPrefs) originalRobotPrefs.current = structuredClone(robotPrefs)
                if (fieldPrefs) originalFieldPrefs.current = structuredClone(fieldPrefs)
                if (motorPrefs) originalMotorPrefs.current = structuredClone(motorPrefs)
            }
        }

        closePanel("choose-scheme")
    }, [])

    return (
        <Panel
            name={"Configure Assets"}
            icon={SynthesisIcons.WRENCH}
            panelId={panelId}
            acceptEnabled={true}
            cancelEnabled={true}
            openLocation="right"
            onAccept={() => {
                pendingDeletes.forEach(id => World.sceneRenderer.removeSceneObject(id))
                setPendingDeletes([])

                InputSchemeManager.saveSchemes()

                originalRobotPrefs.current = null
                originalFieldPrefs.current = null
                originalMotorPrefs.current = null
                originalInputSchemes.current = null

                // Save the current panel state
                setSelectedConfigurationType(configurationType)
                new ConfigurationSavedEvent()
            }}
            onCancel={() => {
                setPendingDeletes([])

                if (selectedAssembly) {
                    const name = selectedAssembly.assemblyName
                    if (originalRobotPrefs.current)
                        PreferencesSystem.setRobotPreferences(name, originalRobotPrefs.current)
                    if (originalFieldPrefs.current)
                        PreferencesSystem.setFieldPreferences(name, originalFieldPrefs.current)
                    if (originalMotorPrefs.current)
                        PreferencesSystem.setMotorPreferences(name, originalMotorPrefs.current)
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
            }}
            acceptName="Save"
            cancelName="Cancel"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 max-h-[60vh]">
                {/** Toggle button group for the robot, field, and input buttons */}
                <ToggleButtonGroup
                    value={configurationType}
                    exclusive
                    onChange={(_: MouseEvent<HTMLElement>, v: ConfigurationType) => {
                        if (v != null) {
                            setConfigurationType(v)
                            setSelectedConfigurationType(v)
                        }
                        setSelectedAssembly(undefined)
                        new ConfigurationSavedEvent()
                        setConfigMode(undefined)
                    }}
                    {...SoundPlayer.buttonSoundEffects()}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={ConfigurationType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={ConfigurationType.FIELD}>Fields</ToggleButton>
                    <ToggleButton value={ConfigurationType.INPUTS}>Inputs</ToggleButton>
                </ToggleButtonGroup>
                {configurationType == ConfigurationType.INPUTS ? (
                    <ChangeInputsInterface />
                ) : (
                    <>
                        {/** Select menu to pick a robot or field */}
                        <AssemblySelection
                            configurationType={configurationType}
                            onAssemblySelected={a => {
                                if (configMode != undefined) {
                                    new ConfigurationSavedEvent()
                                }
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
                        {/** Nested select menu to pick a configuration mode */}
                        {selectedAssembly != undefined && (
                            <ConfigModeSelection
                                configurationType={configurationType}
                                onModeSelected={mode => {
                                    if (configMode != undefined) new ConfigurationSavedEvent()
                                    setConfigMode(mode)
                                }}
                                selectedMode={configMode}
                                assembly={selectedAssembly}
                            />
                        )}
                        {/** The interface for the selected configuration mode */}
                        {configMode != undefined && selectedAssembly != undefined && (
                            <ConfigInterface
                                configMode={configMode}
                                assembly={selectedAssembly}
                                openPanel={openPanel}
                                closePanel={closePanel}
                            />
                        )}
                    </>
                )}
            </div>
        </Panel>
    )
}

export default ConfigurePanel
