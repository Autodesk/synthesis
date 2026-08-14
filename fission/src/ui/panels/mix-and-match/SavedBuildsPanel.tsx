import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer } from "react"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import EventSystem from "@/systems/EventSystem"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

const SavedBuildsPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const savedBuilds = MirabufCachingService.getAll(MiraType.ROBOT).filter(info => info.isMixAndMatchBuild)

    const importBuild = useCallback((hash: string) => {
        MixAndMatchMode.resumeFrom(hash).catch(console.error)
    }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "Saved Builds", hideAccept: true, cancelText: "Close" }, {})
    }, [configureScreen, panel])

    return (
        <Stack direction="column" gap={1} className="overflow-y-auto" minWidth="20rem">
            {savedBuilds.length === 0 && <Label size="sm">No saved builds yet</Label>}
            {savedBuilds.map(saved => (
                <Stack key={saved.hash} direction="row" justifyContent="space-between" alignItems="center">
                    <Label size="sm" className="text-wrap break-all">
                        {saved.name}
                    </Label>
                    <Button onClick={() => importBuild(saved.hash)}>Import</Button>
                </Stack>
            ))}
        </Stack>
    )
}

export default SavedBuildsPanel
