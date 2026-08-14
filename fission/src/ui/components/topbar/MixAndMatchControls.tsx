import { Box, Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import EventSystem from "@/systems/EventSystem.ts"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { TopBarButton } from "@/ui/components/topbar/TopBarButton"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import NameBuildModal from "@/ui/modals/mix-and-match/NameBuildModal"
import SavedBuildsPanel from "@/ui/panels/mix-and-match/SavedBuildsPanel"
import SnapToFacePanel from "@/ui/panels/mix-and-match/SnapToFacePanel"
import WeldPanel from "@/ui/panels/mix-and-match/WeldPanel"

const DEFAULT_BUILD_NAME = "Mix and Match Robot"

const MixAndMatchControls: React.FC = () => {
    const { openModal, togglePanel } = useUIContext()
    const [, bumpRevision] = useState(0)

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", () => bumpRevision(x => x + 1)), [])

    const exportBuild = () =>
        openModal(
            NameBuildModal,
            { title: "Export as Mira", acceptText: "Export", defaultName: DEFAULT_BUILD_NAME },
            undefined,
            { onAccept: (name: string) => MixAndMatchMode.exportBuild(name).catch(console.error) }
        )

    return (
        <Stack direction="row" alignItems="center" gap={1.5}>
            <TopBarButton
                label="Snap to Face"
                icon={
                    <Box sx={TOP_BAR_GLYPH_SX}>
                        <SynthesisIcons.CONNECT />
                    </Box>
                }
                onClick={() => togglePanel(SnapToFacePanel, undefined)}
            />
            <TopBarButton
                label="Weld"
                icon={
                    <Box sx={TOP_BAR_GLYPH_SX}>
                        <SynthesisIcons.SCREWDRIVER_WRENCH />
                    </Box>
                }
                onClick={() => togglePanel(WeldPanel, undefined)}
            />
            <TopBarButton
                label="Saved Builds"
                icon={
                    <Box sx={TOP_BAR_GLYPH_SX}>
                        <SynthesisIcons.IMPORT />
                    </Box>
                }
                onClick={() => togglePanel(SavedBuildsPanel, undefined)}
            />
            <TopBarButton
                label="Export as Mira"
                disabledTooltip={!MixAndMatchMode.build?.state.components.size ? "Add a part first" : undefined}
                icon={
                    <Box sx={TOP_BAR_GLYPH_SX}>
                        <SynthesisIcons.DOWNLOAD_LARGE />
                    </Box>
                }
                onClick={exportBuild}
            />
        </Stack>
    )
}

export default MixAndMatchControls
