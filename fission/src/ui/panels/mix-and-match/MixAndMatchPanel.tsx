import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useState } from "react"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    AddButton,
    NegativeButton,
    SynthesisIcons,
    ToggleButton,
    ToggleButtonGroup,
} from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"

function partName(libraryPartRef: string): string {
    return PartLibrary.find(libraryPartRef)?.name ?? "Unknown Part"
}

const MixAndMatchPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen, openModal } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    const [selected, setSelected] = useState<ComponentId | undefined>(undefined)

    useEffect(() => {
        configureScreen(panel!, { title: "Mix and Match", hideAccept: true, cancelText: "Close Build" }, {})
    }, [configureScreen, panel])

    useEffect(() => {
        MixAndMatchMode.enter().then(bumpRevision).catch(console.error)

        return () => MixAndMatchMode.exit()
    }, [])

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const build = MixAndMatchMode.build
    const placed = [...(build?.state.components.values() ?? [])]
    const library = PartLibrary.list()

    useEffect(() => {
        if (selected && !MixAndMatchMode.build?.state.components.has(selected)) setSelected(undefined)
    })

    const confirmDelete = useCallback(() => {
        if (!selected) return

        openModal(ConfirmModal, { message: `Delete ${selected}? Every weld attached to it goes with it.` }, panel, {
            title: "Delete Part",
            acceptText: "Delete",
            onAccept: () => {
                MixAndMatchMode.deleteComponent(selected).catch(console.error)
                setSelected(undefined)
            },
        })
    }, [openModal, panel, selected])

    return (
        <Stack direction="column" gap={1} className="overflow-y-auto" minWidth="20rem">
            <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    <Label size="md">{`Part Library (${library.length})`}</Label>
                </AccordionSummary>
                <AccordionDetails>
                    {library.length === 0 && <Label size="sm">No parts available</Label>}
                    {library.map(part => (
                        <Stack key={part.ref} direction="row" justifyContent="space-between" alignItems="center">
                            <Label size="sm" className="text-wrap break-all">
                                {part.cached ? part.name : `${part.name} (download)`}
                            </Label>
                            <AddButton
                                onClick={() =>
                                    MixAndMatchMode.spawnPart(part.ref)
                                        .then(componentId => componentId && setSelected(componentId))
                                        .catch(console.error)
                                }
                            />
                        </Stack>
                    ))}
                </AccordionDetails>
            </Accordion>

            <Accordion defaultExpanded>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    <Label size="md">{`Placed Parts (${placed.length})`}</Label>
                </AccordionSummary>
                <AccordionDetails>
                    {placed.length === 0 && <Label size="sm">Add a part to start building</Label>}
                    <ToggleButtonGroup
                        orientation="vertical"
                        exclusive
                        value={selected ?? null}
                        onChange={(_, value) => setSelected((value as ComponentId | null) ?? undefined)}
                        sx={{ width: "100%" }}
                    >
                        {placed.map(component => (
                            <ToggleButton key={component.id} value={component.id}>
                                {`${partName(component.libraryPartRef)} · ${component.id}`}
                            </ToggleButton>
                        ))}
                    </ToggleButtonGroup>
                    {selected && <NegativeButton onClick={confirmDelete}>Delete Part</NegativeButton>}
                </AccordionDetails>
            </Accordion>
        </Stack>
    )
}

export default MixAndMatchPanel
