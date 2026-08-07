import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    AddButton,
    Button,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const PartLibraryPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const library = PartLibrary.list()
    const savedBuilds = MirabufCachingService.getAll(MiraType.ROBOT).filter(info => info.isMixAndMatchBuild)

    const importBuild = useCallback((hash: string) => {
        MixAndMatchMode.resumeFrom(hash).catch(console.error)
    }, [])

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

            <Accordion>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    <Label size="md">{`Saved Builds (${savedBuilds.length})`}</Label>
                </AccordionSummary>
                <AccordionDetails>
                    {savedBuilds.length === 0 && <Label size="sm">No saved builds yet</Label>}
                    {savedBuilds.map(saved => (
                        <Stack key={saved.hash} direction="row" justifyContent="space-between" alignItems="center">
                            <Label size="sm" className="text-wrap break-all">
                                {saved.name}
                            </Label>
                            <Button onClick={() => importBuild(saved.hash)}>Import</Button>
                        </Stack>
                    ))}
                </AccordionDetails>
            </Accordion>
        </Stack>
    )
}

export default PartLibraryPanel
