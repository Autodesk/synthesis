import { useCallback, useEffect, useMemo, useState } from "react"
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
    { icon: { sprite: "cfg-controls" }, label: "Controls", mode: ConfigMode.CONTROLS },
    { icon: { sprite: "cfg-drivetrain" }, label: "Drivetrain", mode: ConfigMode.DRIVETRAIN },
    { icon: { sprite: "cfg-intake" }, label: "Intake", mode: ConfigMode.INTAKE },
    { icon: { sprite: "cfg-ejector" }, label: "Ejector", mode: ConfigMode.EJECTOR },
    { icon: { sprite: "cfg-joints" }, label: "Joints", mode: ConfigMode.JOINTS },
    { icon: { sprite: "cfg-alliance" }, label: "Alliance / Station", mode: ConfigMode.ALLIANCE },
]

export const FIELD_CONFIGURE_BUTTONS: ConfigureButton[] = [
    MOVE_CONFIGURE_BUTTON,
    { icon: { sprite: "cfg-scoring-zones" }, label: "Scoring Zones", mode: ConfigMode.SCORING_ZONES },
    { icon: { sprite: "cfg-protected-zones" }, label: "Protected Zones", mode: ConfigMode.PROTECTED_ZONES },
    { icon: { sprite: "cfg-camera-positions" }, label: "Camera Positions", mode: ConfigMode.CAMERA_POINTS },
]

const readSpawned = (): MirabufSceneObject[] =>
    World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll().filter(assembly => assembly.isOwnObject) : []

/** owns spawned assembly list in topbar */
export function useAssemblySelection() {
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>(readSpawned)
    const [selectedAssembly, setSelectedAssembly] = useState<MirabufSceneObject | undefined>(undefined)

    useEffect(() => {
        // `spawned` is the assembly just added / null when one removed
        const sync = (spawned?: MirabufSceneObject | null) => {
            if (spawned != null && !spawned.isOwnObject) return

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

    // simulation only available when a codesim-capable brain is enabled
    const isCodesimBrain = (selectedAssembly?.brain?.isWPILib() || selectedAssembly?.brain?.isFTC()) ?? false

    const openConfig = useCallback(
        (mode: ConfigMode) => {
            if (!selectedAssembly) {
                addToast("warning", "No Assembly Selected", "Select an assembly to configure first.")
                return
            }
            if (!selectedAssembly.isOwnObject) {
                addToast("warning", `This assembly belongs to ${selectedAssembly.multiplayerOwnerName}`)
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

    const disabledMessage: string | undefined = useMemo(() => {
        if (selectedAssembly == null) return "Spawn an assembly first"
        if (!selectedAssembly.isOwnObject)
            return `Cannot configure assembly owned by ${selectedAssembly.multiplayerOwnerName}`
    }, [selectedAssembly])

    return {
        isField,
        isCodesimBrain,
        configurationType,
        configureButtons,
        openConfig,
        disabledMessage,
    }
}
