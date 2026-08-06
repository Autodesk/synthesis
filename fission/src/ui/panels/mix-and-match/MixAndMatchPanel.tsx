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
import {
    Accordion,
    AccordionDetails,
    AccordionSummary,
    AddButton,
    Button,
    NegativeButton,
    Spacer,
    SynthesisIcons,
    ToggleButton,
    ToggleButtonGroup,
} from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import { rayCastMesh } from "@/util/RaycastUtils"

const GIZMO_SIZE = 1.5

/** "idle": not picking. "source": next click picks a face on the selected part. "target": next click picks the face to snap against. */
type SnapPickStep = "idle" | "source" | "target"

type PickedFace = { point: THREE.Vector3; normal: THREE.Vector3 }

function partName(libraryPartRef: string): string {
    return PartLibrary.find(libraryPartRef)?.name ?? "Unknown Part"
}

const MixAndMatchPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen, openModal } = useUIContext()

    const [, bumpRevision] = useReducer((x: number) => x + 1, 0)
    const [selected, setSelected] = useState<ComponentId | undefined>(undefined)
    const [pickStep, setPickStep] = useState<SnapPickStep>("idle")
    const [sourceFace, setSourceFace] = useState<PickedFace | undefined>(undefined)
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    useEffect(() => {
        configureScreen(panel!, { title: "Mix and Match", hideAccept: true, cancelText: "Close Build" }, {})
    }, [configureScreen, panel])

    useEffect(() => {
        MixAndMatchMode.enter().then(bumpRevision).catch(console.error)

        return () => MixAndMatchMode.exit()
    }, [])

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", bumpRevision), [])

    const build = MixAndMatchMode.build
    const scene = MixAndMatchMode.scene
    const placed = [...(build?.state.components.values() ?? [])]
    const library = PartLibrary.list()
    const selectedComponent = selected ? scene?.get(selected) : undefined

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
                </AccordionDetails>
            </Accordion>

            {selectedComponent && (
                <Accordion defaultExpanded>
                    <AccordionSummary expandIcon={<SynthesisIcons.EXPAND_MORE_LARGE />}>
                        <Label size="md">Placement</Label>
                    </AccordionSummary>
                    <AccordionDetails>
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
                        <NegativeButton onClick={confirmDelete}>Delete Part</NegativeButton>
                    </AccordionDetails>
                </Accordion>
            )}
        </Stack>
    )
}

export default MixAndMatchPanel
