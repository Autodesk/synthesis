import { useCallback, useEffect, useState } from "react"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import World from "@/systems/World"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { ConfigMode, type ConfigurationType } from "@/ui/panels/configuring/assembly-config/ConfigTypes"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import type { ConfigureIconSource } from "@/ui/components/topbar/ConfigureIcon"

export type ConfigureButton = { icon: ConfigureIconSource; label: string; mode: ConfigMode }

const MOVE_CONFIGURE_BUTTON: ConfigureButton = {
    icon: { glyph: SynthesisIcons.MOVE },
    label: "Move",
    mode: ConfigMode.MOVE,
}

export const ROBOT_CONFIGURE_BUTTONS: ConfigureButton[] = [
    MOVE_CONFIGURE_BUTTON,
    { icon: { sprite: "cfg-1" }, label: "Controls", mode: ConfigMode.CONTROLS },
    { icon: { sprite: "cfg-2" }, label: "Drivetrain", mode: ConfigMode.DRIVETRAIN },
    { icon: { sprite: "cfg-3" }, label: "Intake", mode: ConfigMode.INTAKE },
    { icon: { sprite: "cfg-4" }, label: "Ejector", mode: ConfigMode.EJECTOR },
    { icon: { sprite: "cfg-5" }, label: "Joints", mode: ConfigMode.JOINTS },
    { icon: { sprite: "cfg-6" }, label: "Alliance / Station", mode: ConfigMode.ALLIANCE },
]

export const FIELD_CONFIGURE_BUTTONS: ConfigureButton[] = [
    MOVE_CONFIGURE_BUTTON,
    { icon: { sprite: "cfg-8" }, label: "Scoring Zones", mode: ConfigMode.SCORING_ZONES },
    { icon: { sprite: "cfg-7" }, label: "Protected Zones", mode: ConfigMode.PROTECTED_ZONES },
]

const readSpawned = (): MirabufSceneObject[] => (World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll() : [])

/** owns spawned assembly list in topbar */
export function useAssemblySelection() {
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>(readSpawned)
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)

    useEffect(() => {
        // `spawned` is the assembly just added / null when one removed
        const sync = (spawned?: MirabufSceneObject | null) => {
            const current = readSpawned()
            setAssemblies(current)

            // a newly spawned assembly becomes the selection
            setSelectedAssembly(prev => spawned ?? (prev && current.some(a => a.id === prev.id) ? prev : undefined))
        }

        const unsubChange = EventSystem.listen("MirabufObjectChangeEvent", sync)
        const unsubSaved = EventSystem.listen("ConfigurationSavedEvent", () => sync())
        return () => {
            unsubChange()
            unsubSaved()
        }
    }, [])

    const selectAssemblyById = useCallback(
        (id: string) => setSelectedAssembly(assemblies.find(a => a.id.toString() === id)),
        [assemblies]
    )

    return { assemblies, selectedAssembly, selectAssemblyById }
}

/** gets configure options avaiable for selected assembly */
export function useConfigureAssembly(selectedAssembly?: MirabufSceneObject) {
    const { togglePanel, addToast } = useUIContext()

    // robot configure button set is shown by default
    const isField = selectedAssembly?.miraType === MiraType.FIELD
    const configurationType: ConfigurationType = isField ? "FIELDS" : "ROBOTS"
    const configureButtons = isField ? FIELD_CONFIGURE_BUTTONS : ROBOT_CONFIGURE_BUTTONS

    // simulation only available when wpilib brain is enabled
    const isWpilibBrain = selectedAssembly?.brain?.isWPILib() ?? false

    const openConfig = useCallback(
        (mode: ConfigMode) => {
            if (!selectedAssembly) {
                addToast("warning", "No Assembly Selected", "Select an assembly to configure first.")
                return
            }
            togglePanel(
                ConfigurePanel,
                { selectedAssembly, configMode: mode, configurationType },
                // only close on a repeat click of the same button - a different config mode re-opens the panel
                open => open.configMode === mode && open.selectedAssembly?.id === selectedAssembly.id
            )
        },
        [selectedAssembly, addToast, togglePanel, configurationType]
    )

    return {
        isField,
        isWpilibBrain,
        configurationType,
        configureButtons,
        openConfig,
    }
}
