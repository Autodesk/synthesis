import { Stack, TextField } from "@mui/material"
import { useEffect, useState } from "react"
import Label from "@/ui/components/Label"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const MetadataConfigInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, registerCleanupFunction }) => {
    const [name, setName] = useState<string>(selectedAssembly.mirabufInstance.parser.assembly.info?.name ?? "Unknown")
    useEffect(() => {
        selectedAssembly.mirabufInstance.parser.assembly.info ??= {}
        selectedAssembly.mirabufInstance.parser.assembly.info.name = name
    }, [name, selectedAssembly])

    useEffect(() => {
        const originalName = selectedAssembly.mirabufInstance.parser.assembly.info?.name
        registerCleanupFunction(undefined, () => {
            selectedAssembly.mirabufInstance.parser.assembly.info!.name = originalName
        })
    }, [registerCleanupFunction, selectedAssembly])
    return (
        <Stack gap={2} direction="column">
            <Label size="sm">Asset Name</Label>
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
