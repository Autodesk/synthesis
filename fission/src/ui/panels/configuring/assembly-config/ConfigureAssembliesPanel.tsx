import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useMemo, useState } from "react"
import { AiOutlinePlus } from "react-icons/ai"
import ConfigureGamepiecePickupInterface from "./interfaces/ConfigureGamepiecePickupInterface"
import ConfigureShotTrajectoryInterface from "./interfaces/ConfigureShotTrajectoryInterface"
import ConfigureScoringZonesInterface from "./interfaces/scoring/ConfigureScoringZonesInterface"

const AddIcon = <AiOutlinePlus size={"1.25rem"} />

class AssemblySelectionOption extends SelectMenuOption {
    assemblyObject: MirabufSceneObject

    constructor(name: string, assemblyObject: MirabufSceneObject) {
        super(name)
        this.assemblyObject = assemblyObject
    }
}

interface AssemblySelectionProps {
    assemblyType: MiraType
    onAssemblySelected: (assembly: MirabufSceneObject | undefined) => void
}

const AssemblySelection: React.FC<AssemblySelectionProps> = ({ assemblyType, onAssemblySelected }) => {
    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]

        return assemblies
    }, [])

    const fields = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.FIELD
            }
            return false
        }) as MirabufSceneObject[]

        return assemblies
    }, [])

    return (
        <SelectMenu
            // TODO: Add control scheme name if it's a robot
            options={(assemblyType == MiraType.ROBOT ? robots : fields).map(assembly => {
                return new AssemblySelectionOption(`${assembly.assemblyName} (${assembly.id})`, assembly)
            })}
            onOptionSelected={val => {
                onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)
            }}
            headerText={`Select a ${assemblyType == MiraType.ROBOT ? "Robot" : "Field"}`}
        />
    )
}

enum ConfigMode {
    INTAKE,
    EJECTOR,
    JOINTS,
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
    new ConfigModeSelectionOption("Joints", ConfigMode.JOINTS),
    new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS),
]
const fieldModes = [new ConfigModeSelectionOption("Scoring Zones", ConfigMode.SCORING_ZONES)]

interface ConfigModeSelectionProps {
    assemblyType: MiraType
    onModeSelected: (mode: ConfigMode | undefined) => void
}

const ConfigModeSelection: React.FC<ConfigModeSelectionProps> = ({ assemblyType, onModeSelected }) => {
    return (
        <SelectMenu
            options={assemblyType == MiraType.ROBOT ? robotModes : fieldModes}
            onOptionSelected={val => {
                onModeSelected((val as ConfigModeSelectionOption)?.configMode)
            }}
            headerText="Select a Configuration Mode"
            indentation={1}
        />
    )
}

interface ConfigInterfaceProps {
    configMode: ConfigMode
    assembly: MirabufSceneObject
}

const ConfigInterface: React.FC<ConfigInterfaceProps> = ({ configMode, assembly }) => {
    switch (configMode) {
        case ConfigMode.INTAKE:
            return <ConfigureGamepiecePickupInterface selectedRobot={assembly} />
        case ConfigMode.EJECTOR:
            return <ConfigureShotTrajectoryInterface selectedRobot={assembly} />
        case ConfigMode.JOINTS:
            return <Label>interface not set up</Label>
        case ConfigMode.CONTROLS:
            return <Label>interface not set up</Label>
        case ConfigMode.SCORING_ZONES:
            const zones = assembly.fieldPreferences?.scoringZones
            if (zones == undefined) {
                console.error("Field does not contain scoring zone preferences!")
                return <Label>ERROR: Field does not contain scoring zone configuration!</Label>
            }
            return <ConfigureScoringZonesInterface selectedField={assembly} initialZones={zones} />
        default:
            throw new Error(`Config mode ${configMode} has no associated interface`)
    }
}

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

const ConfigureAssembliesPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [assemblyType, setAssemblyType] = useState<MiraType>(MiraType.ROBOT)
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)
    const [selectedConfigMode, setSelectedConfigMode] = useState<ConfigMode | undefined>(undefined)

    return (
        <Panel
            name={"Configure Assemblies"}
            icon={AddIcon}
            panelId={panelId}
            cancelEnabled={false}
            openLocation="right"
            onAccept={() => {
                new ConfigurationSavedEvent()
            }}
            acceptName="Close"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                <ToggleButtonGroup
                    value={assemblyType}
                    exclusive
                    onChange={(_, v) => {
                        v != null && setAssemblyType(v)
                        setSelectedAssembly(undefined)
                        if (selectedConfigMode != undefined) {
                            new ConfigurationSavedEvent()
                            setSelectedConfigMode(undefined)
                        }
                    }}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
                </ToggleButtonGroup>
                <AssemblySelection
                    assemblyType={assemblyType}
                    onAssemblySelected={a => {
                        if (selectedConfigMode != undefined) {
                            new ConfigurationSavedEvent()
                            setSelectedConfigMode(undefined)
                        }
                        setSelectedAssembly(a)
                    }}
                />
                {selectedAssembly != undefined && (
                    <ConfigModeSelection
                        assemblyType={assemblyType}
                        onModeSelected={mode => {
                            if (selectedConfigMode != undefined) new ConfigurationSavedEvent()
                            setSelectedConfigMode(mode)
                        }}
                    />
                )}
                {selectedConfigMode != undefined && selectedAssembly != undefined && (
                    <ConfigInterface configMode={selectedConfigMode} assembly={selectedAssembly} />
                )}
            </div>
        </Panel>
    )
}

export default ConfigureAssembliesPanel
