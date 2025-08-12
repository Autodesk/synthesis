import type React from "react"
import { useMemo, useReducer } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World.ts"
import type { PanelImplProps } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel"
import type { ConfigurationType } from "../ConfigTypes"
import type { ConfigurePanelCustomProps } from "../ConfigurePanel"

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
    console.log("MAKING SELECTION OPTION FOR", configurationType)
    return new AssemblySelectionOption(
        `${configurationType === "ROBOTS" ? `[${InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}] ` : ""}${assembly.assemblyName}`,
        assembly
    )
}

const AssemblySelection: React.FC<AssemblySelectionProps & PanelImplProps<void, ConfigurePanelCustomProps>> = ({
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
        return World.sceneRenderer.mirabufSceneObjects.getRobots().filter(x => !pendingDeletes.includes(x.id))
    }, [u, pendingDeletes])

    const fields = useMemo(() => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        return !field || pendingDeletes.includes(field.id) ? [] : [field]
    }, [u, pendingDeletes])

    const pieces = useMemo(() => {
        return World.sceneRenderer.mirabufSceneObjects.getPieces()
    }, [u, pendingDeletes])

    const options = useMemo(() => {
        const list = configurationType === "ROBOTS" ? robots : configurationType === "FIELDS" ? fields : pieces
        return list
            .filter((assembly): assembly is MirabufSceneObject => assembly != null)
            .map(assembly => makeSelectionOption(configurationType, assembly))
    }, [configurationType, robots, fields, pieces])

    return (
        <SelectMenu
            options={options}
            onOptionSelected={val => onAssemblySelected((val as AssemblySelectionOption)?.assemblyObject)}
            defaultHeaderText={`Select a ${configurationType.slice(0, configurationType.length - 1).toLowerCase()}`}
            onDelete={val => {
                onStageDelete(val)
                update()
            }}
            onAddClicked={() => {
                openPanel(ImportMirabufPanel, { configurationType })
                closePanel(panel!.id, CloseType.Overwrite)
            }}
            noOptionsText={`No ${configurationType.slice(0, configurationType.length - 1).toLowerCase()} spawned!`}
            defaultSelectedOption={
                selectedAssembly ? makeSelectionOption(configurationType, selectedAssembly) : undefined
            }
        />
    )
}

export default AssemblySelection
