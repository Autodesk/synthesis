import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useReducer } from "react"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { AddButton } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const PartLibraryPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const library = PartLibrary.list()

    useEffect(() => {
        configureScreen(panel!, { title: "Part Library", hideAccept: true, cancelText: "Close" }, {})
    }, [configureScreen, panel])

    return (
        <Stack direction="column" gap={1} className="overflow-y-auto" minWidth="20rem">
            {library.length === 0 && <Label size="sm">No parts available</Label>}
            {library.map(part => (
                <Stack key={part.ref} direction="row" justifyContent="space-between" alignItems="center">
                    <Label size="sm" className="text-wrap break-all">
                        {part.cached ? part.name : `${part.name} (download)`}
                    </Label>
                    <AddButton onClick={() => MixAndMatchMode.spawnPart(part.ref).catch(console.error)} />
                </Stack>
            ))}
        </Stack>
    )
}

export default PartLibraryPanel
