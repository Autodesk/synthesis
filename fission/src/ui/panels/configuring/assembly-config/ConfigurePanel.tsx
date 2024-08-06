import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useMemo, useReducer, useState } from "react"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"
import SequentialBehaviorsInterface from "./interfaces/SequentialBehaviorsInterface"
import ChangeInputsInterface from "./interfaces/inputs/ConfigureInputsInterface"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { usePanelControlContext } from "@/ui/PanelContext"
import Button from "@/ui/components/Button"
import { setSelectedBrainIndexGlobal } from "../ChooseInputSchemePanel"
import ConfigureSchemeInterface from "./interfaces/inputs/ConfigureSchemeInterface"
import { SynthesisIcons } from "@/ui/components/StyledComponents"

/** Option for selecting a robot of field */
class AssemblySelectionOption extends SelectMenuOption {
    assemblyObject: MirabufSceneObject

    constructor(name: string, assemblyObject: MirabufSceneObject) {
        super(name)
        this.assemblyObject = assemblyObject
    }
}

interface ConfigurationSelectionProps {
    configurationType: ConfigurationType
    onAssemblySelected: (assembly: MirabufSceneObject | undefined) => void
}

const AssemblySelection: React.FC<ConfigurationSelectionProps> = ({ configurationType, onAssemblySelected }) => {
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

    /** Robot or field select menu */
    return (
        <SelectMenu
            options={(configurationType == ConfigurationType.ROBOT ? robots : fields).map(assembly => {
                return new AssemblySelectionOption(
                    `${configurationType == ConfigurationType.ROBOT ? `[${InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}]` : ""} ${assembly.assemblyName}`,
                    assembly
                )
            })}
            onOptionSelected={val => {
                onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)
            }}
            defaultHeaderText={`Select a ${configurationType == ConfigurationType.ROBOT ? "Robot" : "Field"}`}
            onDelete={val => {
                World.SceneRenderer.RemoveSceneObject((val as AssemblySelectionOption).assemblyObject.id)
                onAssemblySelected(undefined)
                update()
            }}
            onAddClicked={() => {
                console.log("Open spawn panel")
                openPanel("import-mirabuf")
            }}
            noOptionsText={`No ${configurationType == ConfigurationType.ROBOT ? "robots" : "fields"} spawned!`}
        />
    )
}

enum ConfigMode {
    INTAKE,
    EJECTOR,
    MOTORS,
    CONTROLS,
    SCORING_ZONES,
}

class ConfigModeSelectionOption extends SelectMenuOption {
    configMode: ConfigMode

    constructor(name: string, configMode: ConfigMode) {
        super(name)
        this.configMode = configMode
    }
}

const robotModes = [
    new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE),
    new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR),
    new ConfigModeSelectionOption("Joints", ConfigMode.MOTORS),
    new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS),
]
const fieldModes = [new ConfigModeSelectionOption("Scoring Zones", ConfigMode.SCORING_ZONES)]

interface ConfigModeSelectionProps {
    configurationType: ConfigurationType
    onModeSelected: (mode: ConfigMode | undefined) => void
}

const ConfigModeSelection: React.FC<ConfigModeSelectionProps> = ({ configurationType, onModeSelected }) => {
    return (
        <SelectMenu
            options={configurationType == ConfigurationType.ROBOT ? robotModes : fieldModes}
            onOptionSelected={val => {
                onModeSelected((val as ConfigModeSelectionOption)?.configMode)
            }}
            defaultHeaderText="Select a Configuration Mode"
            indentation={1}
        />
    )
}

interface ConfigInterfaceProps {
    configMode: ConfigMode
    assembly: MirabufSceneObject
    openPanel: (panelId: string) => void
}

/** The interface for the actual configuration */
const ConfigInterface: React.FC<ConfigInterfaceProps> = ({ configMode, assembly, openPanel }) => {
    switch (configMode) {
        case ConfigMode.INTAKE:
            return <ConfigureGamepiecePickupInterface selectedRobot={assembly} />
        case ConfigMode.EJECTOR:
            return <ConfigureShotTrajectoryInterface selectedRobot={assembly} />
        case ConfigMode.MOTORS:
            return <SequentialBehaviorsInterface selectedRobot={assembly} />
        case ConfigMode.CONTROLS: {
            const brainIndex = (assembly.brain as SynthesisBrain).brainIndex
            const scheme = InputSystem.brainIndexSchemeMap.get(brainIndex)

            return (
                <>
                    <Button
                        value="Set Scheme"
                        onClick={() => {
                            setSelectedBrainIndexGlobal(brainIndex)
                            openPanel("choose-scheme")
                        }}
                    />
                    {scheme && <ConfigureSchemeInterface selectedScheme={scheme} />}
                </>
            )
        }
        case ConfigMode.SCORING_ZONES: {
            const zones = assembly.fieldPreferences?.scoringZones
            if (zones == undefined) {
                console.error("Field does not contain scoring zone preferences!")
                return <Label>ERROR: Field does not contain scoring zone configuration!</Label>
            }
            return <ConfigureScoringZonesInterface selectedField={assembly} initialZones={zones} />
        }
        default:
            throw new Error(`Config mode ${configMode} has no associated interface`)
    }
}

/** An event to save whatever configuration interface is open when it is closed */
export class ConfigurationSavedEvent extends Event {
    public constructor() {
        super("ConfigurationSaved")

        window.dispatchEvent(this)
    }

    public static Listen(func: (e: Event) => void) {
        window.addEventListener("ConfigurationSaved", func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener("ConfigurationSaved", func)
    }
}

enum ConfigurationType {
    ROBOT,
    FIELD,
    INPUTS,
}

const ConfigurePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { openPanel } = usePanelControlContext()
    const [configurationType, setConfigurationType] = useState<ConfigurationType>(ConfigurationType.ROBOT)
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)
    const [selectedConfigMode, setSelectedConfigMode] = useState<ConfigMode | undefined>(undefined)

    return (
        <Panel
            name={"Configure Assemblies"}
            icon={SynthesisIcons.Gear}
            panelId={panelId}
            cancelEnabled={false}
            openLocation="right"
            onAccept={() => {
                new ConfigurationSavedEvent()
            }}
            acceptName="Close"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2 max-h-[60vh]">
                {/** Toggle button group for the robot, field, and input buttons */}
                <ToggleButtonGroup
                    value={configurationType}
                    exclusive
                    onChange={(_, v) => {
                        v != null && setConfigurationType(v)
                        setSelectedAssembly(undefined)
                        new ConfigurationSavedEvent()
                        setSelectedConfigMode(undefined)
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
                                if (selectedConfigMode != undefined) {
                                    new ConfigurationSavedEvent()
                                }
                                setSelectedConfigMode(undefined)
                                setSelectedAssembly(a)
                            }}
                        />
                        {/** Nested select menu to pick a configuration mode */}
                        {selectedAssembly != undefined && (
                            <ConfigModeSelection
                                configurationType={configurationType}
                                onModeSelected={mode => {
                                    if (selectedConfigMode != undefined) new ConfigurationSavedEvent()
                                    setSelectedConfigMode(mode)
                                }}
                            />
                        )}
                        {/** The interface for the selected configuration mode */}
                        {selectedConfigMode != undefined && selectedAssembly != undefined && (
                            <ConfigInterface
                                configMode={selectedConfigMode}
                                assembly={selectedAssembly}
                                openPanel={openPanel}
                            />
                        )}
                    </>
                )}
            </div>
        </Panel>
    )
}

export default ConfigurePanel
