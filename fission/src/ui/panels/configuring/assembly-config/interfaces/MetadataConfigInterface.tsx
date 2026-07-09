import { Stack, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import Label from "@/ui/components/Label"

type MetadataConfigInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

const MetadataConfigInterface: React.FC<MetadataConfigInterfaceProps> = ({ selectedAssembly }) => {
    const [name, setName] = useState<string>(selectedAssembly.mirabufInstance.parser.assembly.info?.name ?? "Unknown")
    useEffect(() => {
        selectedAssembly.mirabufInstance.parser.assembly.info ??= {}
        selectedAssembly.mirabufInstance.parser.assembly.info.name = name
    }, [name, selectedAssembly])
    return (
        <Stack gap={2} direction="column">
            <Label size={"sm"}>Asset Name</Label>
            <TextField
                placeholder={selectedAssembly.mirabufInstance.parser.assembly.info?.name ?? "Unknown"}
                className="w-full"
                onChange={e => setName(e.target.value)}
                value={name}
            />
        </Stack>
    )
}

export default MetadataConfigInterface
