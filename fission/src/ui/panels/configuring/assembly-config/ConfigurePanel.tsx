import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useEffect, useMemo, useReducer, useState, MouseEvent } from "react"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"
import ChangeInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { usePanelControlContext } from "@/ui/PanelContext"
import Button from "@/ui/components/Button"
import ConfigureSchemeInterface from "./interfaces/inputs/ConfigureSchemeInterface"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import ConfigureSubsystemsInterface from "./interfaces/ConfigureSubsystemsInterface"
import SequentialBehaviorsInterface from "./interfaces/SequentialBehaviorsInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import { ConfigurationSavedEvent } from "./ConfigurationSavedEvent"
import { ConfigurationType, getConfigurationType, setSelectedConfigurationType } from "./ConfigurationType"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { ConfigMode, popConfigurePanelSettings } from "./ConfigurePanelControls"
import BrainSelectionInterface from "./interfaces/BrainSelectionInterface"
import SimulationInterface from "./interfaces/SimulationInterface"

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
}) => {
    // Update is used when a robot or field is deleted to update the select menu
    const [u, update] = useReducer(x => !x, false)
    const { openPanel } = usePanelControlContext()

    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]

        return assemblies
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [u])

    const fields = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.FIELD
            }
            return false
        }) as MirabufSceneObject[]

        return assemblies
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [u])

    const options = useMemo(() => {
        return (configurationType == ConfigurationType.ROBOT ? robots : fields).map(assembly =>
            makeSelectionOption(configurationType, assembly)
        )
    }, [configurationType, fields, robots])

    /** Robot or field select menu */
    return (
        <SelectMenu
            options={options}
            onOptionSelected={val => {
                onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)
            }}
            defaultHeaderText={`Select a ${configurationType == ConfigurationType.ROBOT ? "Robot" : "Field"}`}
            onDelete={val => {
                World.SceneRenderer.RemoveSceneObject((val as AssemblySelectionOption).assemblyObject.id)
                // onAssemblySelected(undefined)
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
        [ConfigMode.MOVE, new ConfigModeSelectionOption("Move", ConfigMode.MOVE)],
        [ConfigMode.INTAKE, new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE)],
        [ConfigMode.EJECTOR, new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR)],
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
            modes.set(ConfigMode.CONTROLS, new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS))
            break
        default:
            break
    }

    return modes
}

const fieldModes: Map<ConfigMode, ConfigModeSelectionOption> = new Map<ConfigMode, ConfigModeSelectionOption>([
    [ConfigMode.MOVE, new ConfigModeSelectionOption("Move", ConfigMode.MOVE)],
    [ConfigMode.SCORING_ZONES, new ConfigModeSelectionOption("Scoring Zones", ConfigMode.SCORING_ZONES)],
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
        default:
            throw new Error(`Config mode ${configMode} has no associated interface`)
    }
}

const ConfigurePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel, closePanel } = usePanelControlContext()
    const [configurationType, setConfigurationType] = useState<ConfigurationType>(getConfigurationType())
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)
    const [configMode, setConfigMode] = useState<ConfigMode | undefined>(undefined)

    useEffect(() => {
        const settings = popConfigurePanelSettings()
        if (settings) {
            setSelectedAssembly(settings.selectedAssembly)
            if (settings.selectedAssembly) setConfigMode(settings.configMode)
        }

        closePanel("choose-scheme")
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    return (
        <Panel
            name={"Configure Assets"}
            icon={SynthesisIcons.Wrench}
            panelId={panelId}
            cancelEnabled={false}
            openLocation="right"
            onAccept={() => {
                // Save the current panel state
                setSelectedConfigurationType(configurationType)

                new ConfigurationSavedEvent()
            }}
            acceptName="Close"
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
