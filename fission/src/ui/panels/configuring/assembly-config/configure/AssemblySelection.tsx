import type React from "react"
import { useMemo, useReducer } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import type { ConfigurationType } from "../ConfigTypes"

interface AssemblySelectionProps {
    configurationType: ConfigurationType
    onAssemblySelected: (assembly?: MirabufSceneObject) => void
    selectedAssembly?: MirabufSceneObject
    onStageDelete: (opt: SelectMenuOption) => void
    pendingDeletes: number[]
}

export class AssemblySelectionOption extends SelectMenuOption {
    assemblyObject: MirabufSceneObject

    constructor(name: string, assemblyObject: MirabufSceneObject) {
        super(assemblyObject.id.toString(), name)
        this.assemblyObject = assemblyObject
    }
}

function makeSelectionOption(configurationType: ConfigurationType, assembly: MirabufSceneObject) {
    return new AssemblySelectionOption(
        `${configurationType === "ROBOTS" ? `[${InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}]` : ""} ${assembly.assemblyName}`,
        assembly
    )
}

const AssemblySelection: React.FC<AssemblySelectionProps & PanelImplProps<void>> = ({
    panel,
    configurationType,
    onAssemblySelected,
    selectedAssembly,
    onStageDelete,
    pendingDeletes,
}) => {
    const [u, update] = useReducer(x => !x, false)
    const { openPanel, closePanel } = useUIContext()

    const robots = useMemo(() => {
        return [...World.sceneRenderer.sceneObjects.values()]
            .filter(x => x instanceof MirabufSceneObject && x.miraType === MiraType.ROBOT)
            .filter(x => !pendingDeletes.includes(x.id))
    }, [u, pendingDeletes])

    const fields = useMemo(() => {
        return [...World.sceneRenderer.sceneObjects.values()]
            .filter(x => x instanceof MirabufSceneObject && x.miraType === MiraType.FIELD)
            .filter(x => !pendingDeletes.includes(x.id))
    }, [u, pendingDeletes])

    const options = useMemo(() => {
        const list = configurationType === "ROBOTS" ? robots : fields
        return list
            .filter((assembly): assembly is MirabufSceneObject => assembly != null)
            .map(assembly => makeSelectionOption(configurationType, assembly))
    }, [configurationType, robots, fields])

    return (
        <SelectMenu
            options={options}
            onOptionSelected={val => onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)}
            defaultHeaderText={`Select a ${configurationType === "ROBOTS" ? "Robot" : "Field"}`}
            onDelete={val => {
                onStageDelete(val)
                update()
            }}
            onAddClicked={() => {
                openPanel(<ImportMirabufPanel />, undefined)
                closePanel(panel!.id, CloseType.Overwrite)
            }}
            noOptionsText={`No ${configurationType === "ROBOTS" ? "robots" : "fields"} spawned!`}
            defaultSelectedOption={
                selectedAssembly ? makeSelectionOption(configurationType, selectedAssembly) : undefined
            }
        />
    )
}

export default AssemblySelection
