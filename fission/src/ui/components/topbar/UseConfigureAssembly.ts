import { useCallback, useEffect, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { ConfigMode, type ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import type { TopBarIconName } from "./TopBarIcons"

export type ConfigureButton = { name: TopBarIconName; label: string; mode: ConfigMode }

export const ROBOT_CONFIGURE_BUTTONS: ConfigureButton[] = [
    { name: "cfg-1", label: "Controls", mode: ConfigMode.CONTROLS },
    { name: "cfg-2", label: "Drivetrain", mode: ConfigMode.DRIVETRAIN },
    { name: "cfg-3", label: "Intake", mode: ConfigMode.INTAKE },
    { name: "cfg-4", label: "Ejector", mode: ConfigMode.EJECTOR },
    { name: "cfg-5", label: "Joints", mode: ConfigMode.JOINTS },
    { name: "cfg-6", label: "Alliance / Station", mode: ConfigMode.ALLIANCE },
]

export const FIELD_CONFIGURE_BUTTONS: ConfigureButton[] = [
    { name: "cfg-8", label: "Scoring Zones", mode: ConfigMode.SCORING_ZONES },
    { name: "cfg-7", label: "Protected Zones", mode: ConfigMode.PROTECTED_ZONES },
]

export function useConfigureAssembly() {
    const { selectedConfigAssembly, setSelectedConfigAssembly } = useStateContext()
    const { openPanel, addToast } = useUIContext()
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>([])

    // robot configure button set is shown by default
    const isField = selectedConfigAssembly?.miraType === MiraType.FIELD
    const configurationType: ConfigurationType = isField ? "FIELDS" : "ROBOTS"
    const configureButtons = isField ? FIELD_CONFIGURE_BUTTONS : ROBOT_CONFIGURE_BUTTONS

    const openConfig = useCallback(
        (mode: ConfigMode) => {
            if (!selectedConfigAssembly) {
                addToast("warning", "No Assembly Selected", "Select an assembly to configure first.")
                return
            }
            openPanel(ConfigurePanel, {
                selectedAssembly: selectedConfigAssembly,
                configMode: mode,
                configurationType,
            })
        },
        [selectedConfigAssembly, addToast, openPanel, configurationType]
    )

    const update = useCallback(() => {
        setAssemblies(World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll() : [])
    }, [])

    useEffect(() => {
        update()
        // On spawn a new assembly is set as the actively configuring item
        const onChange = (assembly: MirabufSceneObject | null) => {
            update()
            if (assembly) setSelectedConfigAssembly(assembly)
        }
        const unsubChange = EventSystem.listen("MirabufObjectChangeEvent", onChange)
        const unsubSaved = EventSystem.listen("ConfigurationSavedEvent", update)
        return () => {
            unsubChange()
            unsubSaved()
        }
    }, [update, setSelectedConfigAssembly])

    // Drop the selection if its assembly is no longer spawned.
    useEffect(() => {
        if (selectedConfigAssembly && !assemblies.some(a => a.id === selectedConfigAssembly.id)) {
            setSelectedConfigAssembly(undefined)
        }
    }, [assemblies, selectedConfigAssembly, setSelectedConfigAssembly])

    const selectedValue =
        selectedConfigAssembly && assemblies.some(a => a.id === selectedConfigAssembly.id)
            ? selectedConfigAssembly.id.toString()
            : ""

    const selectAssemblyById = useCallback(
        (id: string) => setSelectedConfigAssembly(assemblies.find(a => a.id.toString() === id)),
        [assemblies, setSelectedConfigAssembly]
    )

    return {
        assemblies,
        selectedConfigAssembly,
        isField,
        configurationType,
        configureButtons,
        openConfig,
        selectedValue,
        selectAssemblyById,
    }
}
