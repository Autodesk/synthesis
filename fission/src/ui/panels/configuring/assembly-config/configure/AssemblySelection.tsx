import type React from "react"
import { useCallback, useEffect, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World.ts"
import type { PanelImplProps } from "@/ui/components/Panel"
import SelectMenu, { SelectMenuOption } from "@/ui/components/SelectMenu"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import LibraryModal from "@/ui/modals/mirabuf/LibraryModal"
import type { ConfigurationType } from "../ConfigTypes"
import type { ConfigurePanelCustomProps } from "../ConfigurePanel"
import EventSystem from "@/systems/EventSystem.ts"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

interface AssemblySelectionProps {
    configurationType: ConfigurationType
    onAssemblySelected: (assembly?: MirabufSceneObject) => void
    selectedAssembly?: MirabufSceneObject
    onStageDelete: (opt: SelectMenuOption) => void
    pendingDeletes: SceneObjectId[]
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

function makeSelectionOption(assembly: MirabufSceneObject) {
    return new AssemblySelectionOption(assembly.descriptiveName, assembly)
}

const AssemblySelection: React.FC<AssemblySelectionProps & PanelImplProps<void, ConfigurePanelCustomProps>> = ({
    panel,
    configurationType,
    onAssemblySelected,
    selectedAssembly,
    onStageDelete,
    pendingDeletes,
}) => {
    const { openModal, closePanel } = useUIContext()

    const getRobots = useCallback(
        () => World.sceneRenderer.mirabufSceneObjects.getRobots().filter(x => !pendingDeletes.includes(x.id)),
        [pendingDeletes]
    )
    const getFields = useCallback(() => {
        const field = World.sceneRenderer.mirabufSceneObjects.getField()
        return !field || pendingDeletes.includes(field.id) ? [] : [field]
    }, [pendingDeletes])

    const computeOptions = useCallback(() => {
        const items: MirabufSceneObject[] = configurationType === "ROBOTS" ? getRobots() : getFields()
        return items.filter(assembly => assembly != null).map(assembly => makeSelectionOption(assembly))
    }, [getRobots, getFields, configurationType])

    const [options, setOptions] = useState<AssemblySelectionOption[]>(computeOptions)

    const update = useCallback(() => {
        setOptions(computeOptions())
    }, [computeOptions])

    useEffect(() => {
        update()
        const mirabufChangeUnsubscribe = EventSystem.listen("MirabufObjectChangeEvent", () => update())
        const configEventUnsubscribe = EventSystem.listen("ConfigurationSavedEvent", () => update())
        return () => {
            mirabufChangeUnsubscribe()
            configEventUnsubscribe()
        }
    }, [update])

    return (
        <SelectMenu
            options={options}
            onOptionSelected={val => onAssemblySelected(val?.assemblyObject)}
            defaultHeaderText={`Select a ${configurationType === "ROBOTS" ? "Robot" : "Field"}`}
            onDelete={val => {
                onStageDelete(val)
                update()
            }}
            onAddClicked={() => {
                // save the config before the library takes over
                closePanel(panel!.id, CloseType.ACCEPT)
                setTimeout(() => openModal(LibraryModal, undefined), 0)
            }}
            noOptionsText={`No ${configurationType === "ROBOTS" ? "robots" : "fields"} spawned!`}
            defaultSelectedOption={selectedAssembly ? makeSelectionOption(selectedAssembly) : undefined}
        />
    )
}

export default AssemblySelection
