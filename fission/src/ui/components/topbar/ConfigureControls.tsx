import { MenuItem, Stack } from "@mui/material"
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
import { IconButton, Select } from "../StyledComponents"
import { TOP_BAR_ICON_BUTTON_SX } from "./topBarConfig"
import { TopBarIcon, type TopBarIconName } from "./TopBarIcons"

const CONFIGURE_BUTTONS: TopBarIconName[] = ["cfg-1", "cfg-2", "cfg-3", "cfg-4", "cfg-5"]

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
    const [assemblies, setAssemblies] = useState<MirabufSceneObject[]>([])

    const update = useCallback(() => {
        setAssemblies(World.isAlive ? World.sceneRenderer.mirabufSceneObjects.getAll() : [])
    }, [])

    useEffect(() => {
        update()
        const unsubChange = EventSystem.listen("MirabufObjectChangeEvent", update)
        const unsubSaved = EventSystem.listen("ConfigurationSavedEvent", update)
        return () => {
            unsubChange()
            unsubSaved()
        }
    }, [update])

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
                    minWidth: 220,
                    fontSize: 16,
                    "& .MuiOutlinedInput-notchedOutline": { border: "none" },
                    "& .MuiSelect-select": { display: "flex", alignItems: "center", py: 0 },
                    "& .MuiSelect-icon": { color: "topBarText.main", right: 14 },
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
            {CONFIGURE_BUTTONS.map(name => (
                <IconButton key={name} size="large" disableRipple sx={TOP_BAR_ICON_BUTTON_SX}>
                    <TopBarIcon name={name} size={26} />
                </IconButton>
            ))}
        </Stack>
    )
}

export default ConfigureControls
