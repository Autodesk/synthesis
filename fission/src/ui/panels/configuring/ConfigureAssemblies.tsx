import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useMemo, useState } from "react"
import { AiOutlinePlus } from "react-icons/ai"

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

interface ConfigModeSelectionProps {
    assemblyType: MiraType
    onModeSelected: (mode: ConfigMode | undefined) => void
}

const robotModes = [
    new ConfigModeSelectionOption("Intake", ConfigMode.INTAKE),
    new ConfigModeSelectionOption("Ejector", ConfigMode.EJECTOR),
    new ConfigModeSelectionOption("Joints", ConfigMode.JOINTS),
    new ConfigModeSelectionOption("Controls", ConfigMode.CONTROLS),
]
const fieldModes = [new ConfigModeSelectionOption("Scoring Zones", ConfigMode.SCORING_ZONES)]

const ConfigureModeSelection: React.FC<ConfigModeSelectionProps> = ({ assemblyType, onModeSelected }) => {
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

const ConfigureAssembliesPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [assemblyType, setAssemblyType] = useState<MiraType>(MiraType.ROBOT)
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)

    console.log(selectedAssembly == undefined)

    return (
        <Panel
            name={"Configure Assemblies"}
            icon={AddIcon}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Back"
            openLocation="right"
        >
            <div className="flex overflow-y-auto flex-col gap-2 bg-background-secondary rounded-md p-2">
                <ToggleButtonGroup
                    value={assemblyType}
                    exclusive
                    onChange={(_, v) => {
                        v != null && setAssemblyType(v)
                        setSelectedAssembly(undefined)
                        console.log("triggered")
                    }}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
                    <ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
                </ToggleButtonGroup>
                <AssemblySelection assemblyType={assemblyType} onAssemblySelected={setSelectedAssembly} />
                {selectedAssembly != undefined ? (
                    <ConfigureModeSelection
                        assemblyType={assemblyType}
                        onModeSelected={mode => {
                            //console.log(mode)
                        }}
                    />
                ) : null}
            </div>
        </Panel>
    )
}

export default ConfigureAssembliesPanel
