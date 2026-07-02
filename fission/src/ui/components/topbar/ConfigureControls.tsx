import { MenuItem, Stack, Tooltip } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { IoMdArrowDropdown } from "react-icons/io"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import InputSystem from "@/systems/input/InputSystem"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import World from "@/systems/World"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfigurePanel from "@/ui/panels/configuring/assembly-config/ConfigurePanel"
import { IconButton, Select } from "../StyledComponents"
import { ConfigMode, type ConfigurationType } from "../../panels/configuring/assembly-config/ConfigTypes"
import { TOP_BAR_ICON_BUTTON_SX } from "./topBarConfig"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

type ConfigureButton = { name: TopBarIconName; label: string; mode: ConfigMode }

const ROBOT_CONFIGURE_BUTTONS: ConfigureButton[] = [
    { name: "cfg-1", label: "Controls", mode: ConfigMode.CONTROLS },
    { name: "cfg-2", label: "Drivetrain", mode: ConfigMode.DRIVETRAIN },
    { name: "cfg-3", label: "Intake", mode: ConfigMode.INTAKE },
    { name: "cfg-4", label: "Ejector", mode: ConfigMode.EJECTOR },
    { name: "cfg-5", label: "Joints", mode: ConfigMode.JOINTS },
    { name: "cfg-6", label: "Alliance / Station", mode: ConfigMode.ALLIANCE },
]

const FIELD_CONFIGURE_BUTTONS: ConfigureButton[] = [
    { name: "cfg-8", label: "Scoring Zones", mode: ConfigMode.SCORING_ZONES },
    { name: "cfg-7", label: "Protected Zones", mode: ConfigMode.PROTECTED_ZONES },
]

const assemblyLabel = (assembly: MirabufSceneObject): string => {
    if (assembly.miraType !== MiraType.ROBOT) return assembly.assemblyName
    const scheme =
        assembly.multiplayerOwnerName ??
        InputSystem.brainIndexSchemeMap.get((assembly.brain as SynthesisBrain).brainIndex)?.schemeName ??
        "-"
    return `[${scheme}] ${assembly.assemblyName}`
}

const ConfigureControls: React.FC = () => {
    const { selectedConfigAssembly, setSelectedConfigAssembly } = useStateContext()
    const { openPanel, addToast } = useUIContext()
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>([])

    // A field shows its own config buttons; everything else (incl. the default,
    // empty selection) shows the robot buttons.
    const isField = selectedConfigAssembly?.miraType === MiraType.FIELD
    const configurationType: ConfigurationType = isField ? "FIELDS" : "ROBOTS"
    const configureButtons = isField ? FIELD_CONFIGURE_BUTTONS : ROBOT_CONFIGURE_BUTTONS

    const openConfig = (mode: ConfigMode) => {
        if (!selectedConfigAssembly) {
            addToast("warning", "No Assembly Selected", "Select an assembly to configure first.")
            return
        }
        openPanel(ConfigurePanel, {
            selectedAssembly: selectedConfigAssembly,
            configMode: mode,
            configurationType,
        })
    }

    const update = useCallback(() => {
        setAssemblies(World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll() : [])
    }, [])

    useEffect(() => {
        update()
        // On spawn the event carries the new assembly; select it. On disposal it's null.
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

    {
        /* TODO: add a "..." after a long robot name to ensure it isn't rendered underneath the dropdown arrow */
    }
    return (
        <Stack direction="row" alignItems="center" gap={2}>
            <Select
                displayEmpty
                value={selectedValue}
                onChange={e => setSelectedConfigAssembly(assemblies.find(a => a.id.toString() === e.target.value))}
                renderValue={() =>
                    selectedConfigAssembly ? assemblyLabel(selectedConfigAssembly) : "Select an assembly"
                }
                IconComponent={props => <IoMdArrowDropdown {...props} fontSize="2em" />}
                sx={{
                    bgcolor: "surface.main",
                    color: "topBarText.main",
                    borderRadius: 3,
                    height: 46,
                    minWidth: 260,
                    fontSize: 16,
                    cursor: "pointer",
                    alignItems: "stretch",
                    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0, boxSizing: "border-box" },
                    "& .MuiSelect-icon": { color: "topBarText.main", right: 14, pointerEvents: "none" },
                }}
            >
                {assemblies.length === 0 && (
                    <MenuItem value="" disabled>
                        No assemblies spawned
                    </MenuItem>
                )}
                {assemblies.map(assembly => (
                    <MenuItem key={assembly.id} value={assembly.id.toString()}>
                        {assemblyLabel(assembly)}
                    </MenuItem>
                ))}
            </Select>
            {configureButtons.map(({ name, label, mode }) => (
                <Tooltip key={label} title={label}>
                    <IconButton size="large" disableRipple sx={TOP_BAR_ICON_BUTTON_SX} onClick={() => openConfig(mode)}>
                        <TopBarIcon name={name} size={40} />
                    </IconButton>
                </Tooltip>
            ))}
        </Stack>
    )
}

export default ConfigureControls
