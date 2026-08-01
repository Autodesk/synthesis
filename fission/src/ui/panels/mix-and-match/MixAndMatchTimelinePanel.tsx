import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useReducer } from "react"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { TimelineEntry } from "@/mix-and-match/MixAndMatchTypes"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button, NegativeButton, SynthesisIcons, ToggleButton } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"

function entryIcon(entry: TimelineEntry) {
    switch (entry.type) {
        case "spawn":
            return <SynthesisIcons.ADD />
        case "move":
            return <SynthesisIcons.HAND />
        case "weld":
            return <SynthesisIcons.CONNECT />
        case "resize":
            return <SynthesisIcons.FIT_SCREEN />
        case "delete":
            return <SynthesisIcons.XMARK />
    }
}

function entryLabel(entry: TimelineEntry): string {
    switch (entry.type) {
        case "spawn":
            return `Add ${PartLibrary.find(entry.libraryPartRef)?.name ?? "Part"}`
        case "move":
            return `Move ${entry.componentId}`
        case "weld":
            return `Weld ${entry.componentB} to ${entry.componentA}`
        case "resize":
            return `Resize ${entry.componentId} (${entry.sizeOption})`
        case "delete":
            return `Delete ${entry.componentId}`
    }
}

/**
 * The build timeline, laid out like Fusion's: every step in order, with a playhead the user can roll
 * back to inspect earlier states.
 *
 * Scrubbing is a preview and never edits the timeline. Building while rolled back is blocked until the
 * user explicitly resumes from the playhead, which is the only action that discards steps.
 */
const MixAndMatchTimelinePanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen, openModal } = useUIContext()
    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)

    useEffect(() => {
        configureScreen(panel!, { title: "Timeline", position: "bottom", hideAccept: true, hideCancel: true }, {})
    }, [configureScreen, panel])

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const build = MixAndMatchMode.build
    if (!build) return null

    const timeline = build.timeline
    const marker = build.marker
    const scrub = (next: number) => MixAndMatchMode.scrubTo(next).catch(console.error)

    const resumeHere = () => {
        openModal(
            ConfirmModal,
            {
                message: `Continue building from here? The ${build.discardedByNextEdit} step${
                    build.discardedByNextEdit === 1 ? "" : "s"
                } after the playhead are discarded.`,
            },
            panel,
            {
                title: "Resume Here",
                acceptText: "Discard & Resume",
                onAccept: () => MixAndMatchMode.resumeHere().catch(console.error),
            }
        )
    }

    return (
        <Stack direction="column" gap={1} maxWidth="80vw">
            <Stack direction="row" gap={1} alignItems="center">
                <Button onClick={() => scrub(0)}>Start</Button>
                <Button disabled={marker === 0} onClick={() => scrub(marker - 1)}>
                    Back
                </Button>
                <Button disabled={!build.isScrubbed} onClick={() => scrub(marker + 1)}>
                    Forward
                </Button>
                <Button disabled={!build.isScrubbed} onClick={() => scrub(timeline.length)}>
                    End
                </Button>
                <Label size="sm">{`${marker} / ${timeline.length}`}</Label>
            </Stack>

            <Stack direction="row" gap={0.5} alignItems="center" className="overflow-x-auto" minHeight="3rem">
                {timeline.length === 0 && <Label size="sm">Nothing built yet</Label>}
                {timeline.map((entry, index) => (
                    <ToggleButton
                        // Entries are positional and never reordered, so the index is the identity.
                        key={`${index}-${entry.type}`}
                        value={index}
                        selected={index === marker - 1}
                        onClick={() => scrub(index + 1)}
                        sx={{ opacity: index < marker ? 1 : 0.4, whiteSpace: "nowrap" }}
                    >
                        <Stack direction="row" gap={0.5} alignItems="center">
                            {entryIcon(entry)}
                            {entryLabel(entry)}
                        </Stack>
                    </ToggleButton>
                ))}
            </Stack>

            {build.isScrubbed && (
                <Stack direction="row" gap={1} alignItems="center">
                    <Label size="sm">Rolled back — editing is paused</Label>
                    <NegativeButton onClick={resumeHere}>
                        {`Resume Here (discards ${build.discardedByNextEdit})`}
                    </NegativeButton>
                </Stack>
            )}
        </Stack>
    )
}

export default MixAndMatchTimelinePanel
