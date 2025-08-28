import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { MirabufObjectChangeEvent } from "@/mirabuf/MirabufSceneObject"
import InputSystem from "@/systems/input/InputSystem.ts"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
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
        const isDisabled = !assemblyObject.isOwnObject
        super(
            assemblyObject.id.toString(),
            name,
            isDisabled ? `Object belongs to ${assemblyObject.multiplayerOwnerName}` : undefined,
            isDisabled
        )
        this.assemblyObject = assemblyObject
    }
}

function makeSelectionOption(configurationType: ConfigurationType, assembly: MirabufSceneObject) {
    return new AssemblySelectionOption(
        `${configurationType === "ROBOTS" ? `[${assembly.multiplayerOwnerName ?? InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ?? "-"}] ` : ""}${assembly.assemblyName}`,
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
    const { openPanel, closePanel } = useUIContext()
    const [options, setOptions] = useState<AssemblySelectionOption[]>([])

    const getRobots = useCallback(
        () => World.sceneRenderer.mirabufSceneObjects.getRobots().filter(x => !pendingDeletes.includes(x.id)),
        [pendingDeletes]
    )
    const getFields = useCallback(() => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        return !field || pendingDeletes.includes(field.id) ? [] : [field]
    }, [pendingDeletes])

    const update = useCallback(() => {
        const items: MirabufSceneObject[] = configurationType === "ROBOTS" ? getRobots() : getFields()
        const newOptions = items
            .filter(assembly => assembly != null)
            .map(assembly => makeSelectionOption(configurationType, assembly))
        setOptions(newOptions)
    }, [getRobots, getFields, configurationType])

    MirabufObjectChangeEvent.addEventListener(() => {
        update()
    })

    ConfigurationSavedEvent.listen(() => {
        update()
    })

    useEffect(() => {
        update()
    }, [update])

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
                // Save current configuration first, then open Spawn panel next tick
                closePanel(panel!.id, CloseType.Accept)
                setTimeout(() => openPanel(ImportMirabufPanel, { configurationType }), 0)
            }}
            noOptionsText={`No ${configurationType === "ROBOTS" ? "robots" : "fields"} spawned!`}
            defaultSelectedOption={
                selectedAssembly ? makeSelectionOption(configurationType, selectedAssembly) : undefined
            }
        />
    )
}

export default AssemblySelection
