import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useRef, useState } from "react"
import type * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import SelectButton from "@/ui/components/SelectButton"
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    AddButton,
    Button,
    EditButton,
    NegativeButton,
    PositiveButton,
    Spacer,
    SynthesisIcons,
    ToggleButton,
    ToggleButtonGroup,
} from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { rayCastMesh } from "@/util/RaycastUtils"
import MixAndMatchTimelinePanel from "./MixAndMatchTimelinePanel"

const GIZMO_SIZE = 1.5

/** "idle": not picking. "source": next click picks a face on the selected part. "target": next click picks the face to snap against. */
type SnapPickStep = "idle" | "source" | "target"

type PickedFace = { point: THREE.Vector3; normal: THREE.Vector3 }

function partName(libraryPartRef: string): string {
    return PartLibrary.find(libraryPartRef)?.name ?? "Unknown Part"
}

const MixAndMatchPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen, closePanel, openModal } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    const [selected, setSelected] = useState<ComponentId | undefined>(undefined)
    const [pickStep, setPickStep] = useState<SnapPickStep>("idle")
    const [sourceFace, setSourceFace] = useState<PickedFace | undefined>(undefined)
    const [weldChild, setWeldChild] = useState<ComponentId | undefined>(undefined)
    const [weldParent, setWeldParent] = useState<ComponentId | undefined>(undefined)
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    useEffect(() => {
        configureScreen(panel!, { title: "Mix and Match", hideAccept: true, cancelText: "Discard Build" }, {})
    }, [configureScreen, panel])

    // Kept in a ref so the mount effect can close the timeline without re-running when the panel list
    // changes underneath it.
    const closePanelRef = useRef(closePanel)
    closePanelRef.current = closePanel

    useEffect(() => {
        MixAndMatchMode.enter().then(bumpRevision).catch(console.error)
        const timelineId = globalOpenPanel(MixAndMatchTimelinePanel, undefined)

        return () => {
            if (timelineId) closePanelRef.current(timelineId, CloseType.CANCEL)
            MixAndMatchMode.exit()
        }
    }, [])

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const build = MixAndMatchMode.build
    const scene = MixAndMatchMode.scene
    const placed = [...(build?.state.components.values() ?? [])]
    const library = PartLibrary.list()
    const selectedComponent = selected ? scene?.get(selected) : undefined
    const selectedState = selected ? build?.state.components.get(selected) : undefined
    const sizes = selectedState ? PartLibrary.sizesFor(selectedState.libraryPartRef) : []

    // Rolled back into history: the timeline shows a past state, so editing is paused until the user
    // explicitly resumes from the playhead. See the Timeline panel.
    const scrubbed = build?.isScrubbed ?? false

    useEffect(() => {
        if (selected && !MixAndMatchMode.build?.state.components.has(selected)) setSelected(undefined)
    })

    // The transform gizmo has no drag-end callback, so the placement is recorded when dragging stops.
    useEffect(() => {
        if (!selected) return

        let wasDragging = false
        let handle = requestAnimationFrame(function tick() {
            const dragging = gizmoRef.current?.isDragging ?? false
            if (wasDragging && !dragging) MixAndMatchMode.commitPlacement(selected).catch(console.error)

            wasDragging = dragging
            handle = requestAnimationFrame(tick)
        })

        return () => cancelAnimationFrame(handle)
    }, [selected])

    const cancelSnapPick = useCallback(() => {
        setPickStep("idle")
        setSourceFace(undefined)
    }, [])

    // Two clicks: first picks a face on the selected part, second picks the face to snap against.
    useEffect(() => {
        if (pickStep === "idle" || !selected) return

        const onClick = (e: MouseEvent) => {
            const components = MixAndMatchMode.scene?.components
            if (!components) return

            // Raycast against the actual render mesh rather than physics colliders: colliders are
            // convex hulls that can approximate a part's shape, so a hull-derived normal can point a
            // different way than the visible surface the user is clicking on.
            const batchToComponent = new Map<THREE.Object3D, ComponentId>()
            const objects: THREE.Object3D[] = []
            components.forEach((component, componentId) => {
                component.mirabufInstance.batches.forEach(batch => {
                    batchToComponent.set(batch, componentId)
                    objects.push(batch)
                })
            })

            const hit = rayCastMesh([e.clientX, e.clientY], objects)
            const componentId = hit && batchToComponent.get(hit.object)

            if (!hit || !componentId) return

            if (pickStep === "source") {
                if (componentId !== selected) return

                setSourceFace({ point: hit.point, normal: hit.normal })
                setPickStep("target")
                return
            }

            if (componentId === selected || !sourceFace) return

            MixAndMatchMode.mateFaces(
                selected,
                componentId,
                sourceFace.point,
                sourceFace.normal,
                hit.point,
                hit.normal
            ).catch(console.error)
            cancelSnapPick()
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)
        return () => World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
    }, [pickStep, selected, sourceFace, cancelSnapPick])

    useEffect(() => {
        cancelSnapPick()
    }, [selected, cancelSnapPick])

    const pickComponent = useCallback((assign: (componentId: ComponentId) => void) => {
        return (body: Jolt.Body) => {
            const componentId = MixAndMatchMode.scene?.componentIdOfBody(body.GetID())
            if (!componentId) return false

            assign(componentId)

            return true
        }
    }, [])

    const applyWeld = useCallback(() => {
        if (!weldChild || !weldParent) return

        MixAndMatchMode.weld(weldParent, weldChild)
            .then(welded => {
                if (!welded) return

                setWeldChild(undefined)
                setWeldParent(undefined)
            })
            .catch(console.error)
    }, [weldChild, weldParent])

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

    const confirmResume = useCallback(
        (libraryPartRef: string) => {
            const resume = () => MixAndMatchMode.resumeFrom(libraryPartRef).catch(console.error)
            if ((MixAndMatchMode.build?.timeline.length ?? 0) === 0) {
                resume()
                return
            }

            openModal(ConfirmModal, { message: "Open this saved build? The build in progress is discarded." }, panel, {
                title: "Open Saved Build",
                acceptText: "Open",
                onAccept: () => {
                    setSelected(undefined)
                    resume()
                },
            })
        },
        [openModal, panel]
    )

    const confirmFinish = useCallback(() => {
        openModal(
            ConfirmModal,
            { message: "Finish the build? Welds become fixed constraints and physics is turned back on." },
            panel,
            {
                title: "Finish Build",
                acceptText: "Finish",
                onAccept: () =>
                    MixAndMatchMode.finish()
                        .then(finished => finished && closePanelRef.current(panel!.id, CloseType.CANCEL))
                        .catch(console.error),
            }
        )
    }, [openModal, panel])

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
                            <Stack direction="row" alignItems="center">
                                <EditButton
                                    title="Open a build saved in this file"
                                    onClick={() => confirmResume(part.ref)}
                                />
                                <AddButton
                                    disabled={scrubbed}
                                    onClick={() =>
                                        MixAndMatchMode.spawnPart(part.ref)
                                            .then(componentId => componentId && setSelected(componentId))
                                            .catch(console.error)
                                    }
                                />
                            </Stack>
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
                                {component.weld ? ` → ${component.weld.parentId}` : ""}
                            </ToggleButton>
                        ))}
                    </ToggleButtonGroup>
                </AccordionDetails>
            </Accordion>

            <Accordion defaultExpanded={placed.length > 1}>
                <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                    <Label size="md">Weld</Label>
                </AccordionSummary>
                <AccordionDetails>
                    <SelectButton
                        labelText="Part to attach"
                        tooltipText="Click any piece of the part you want to attach. It resolves to the whole part, not the piece you clicked."
                        placeholder="Click a part"
                        value={weldChild}
                        onSelect={pickComponent(setWeldChild)}
                    />
                    <SelectButton
                        labelText="Attach to"
                        tooltipText="Click any piece of the part to attach it to."
                        placeholder="Click a part"
                        value={weldParent}
                        onSelect={pickComponent(setWeldParent)}
                    />
                    <Spacer height={10} />
                    <Label size="sm">
                        {weldChild && weldParent && weldChild === weldParent
                            ? "Pick two different parts"
                            : "A part has one weld at a time. Welding it again replaces the old one."}
                    </Label>
                    <Spacer height={10} />
                    <Button
                        disabled={scrubbed || !weldChild || !weldParent || weldChild === weldParent}
                        onClick={applyWeld}
                    >
                        Weld
                    </Button>
                </AccordionDetails>
            </Accordion>

            {selectedComponent && (
                <Accordion defaultExpanded>
                    <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                        <Label size="md">Placement</Label>
                    </AccordionSummary>
                    <AccordionDetails>
                        {scrubbed ? (
                            <Label size="sm">Rolled back — resume from the playhead to keep editing</Label>
                        ) : (
                            <>
                                <TransformGizmoControl
                                    key={`mix-and-match-gizmo-${selected}`}
                                    size={GIZMO_SIZE}
                                    gizmoRef={gizmoRef}
                                    parent={selectedComponent}
                                    defaultMode="translate"
                                    scaleDisabled={true}
                                />
                                <Spacer height={10} />
                                <Stack direction="row" gap={1} alignItems="center">
                                    <Label size="sm">
                                        {pickStep === "idle" && "Snap flush against"}
                                        {pickStep === "source" && "Click a face on this part…"}
                                        {pickStep === "target" && "Click a face on the part to snap against…"}
                                    </Label>
                                    <Button
                                        onClick={() => {
                                            if (pickStep === "idle") {
                                                setPickStep("source")
                                            } else {
                                                cancelSnapPick()
                                            }
                                        }}
                                    >
                                        {pickStep === "idle" ? "Snap to Face" : "Cancel"}
                                    </Button>
                                </Stack>
                                <Spacer height={10} />
                                <Label size="sm">Size</Label>
                                {sizes.length === 0 ? (
                                    <Label size="sm">This part is only made in one size</Label>
                                ) : (
                                    <ToggleButtonGroup
                                        exclusive
                                        value={selectedState?.sizeOption ?? null}
                                        onChange={(_, value) =>
                                            value && MixAndMatchMode.resize(selected!, value).catch(console.error)
                                        }
                                    >
                                        {sizes.map(size => (
                                            <ToggleButton key={size.id} value={size.id}>
                                                {size.label}
                                            </ToggleButton>
                                        ))}
                                    </ToggleButtonGroup>
                                )}
                                <Spacer height={10} />
                                <NegativeButton onClick={confirmDelete}>Delete Part</NegativeButton>
                            </>
                        )}
                    </AccordionDetails>
                </Accordion>
            )}

            <Stack direction="row" gap={1} justifyContent="center">
                <PositiveButton disabled={scrubbed || placed.length === 0} onClick={confirmFinish}>
                    Finish Build
                </PositiveButton>
                <Button
                    disabled={placed.length === 0}
                    onClick={() => MixAndMatchMode.exportBuild().catch(console.error)}
                >
                    Save as .mira
                </Button>
            </Stack>
        </Stack>
    )
}

export default MixAndMatchPanel
