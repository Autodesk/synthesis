import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useRef, useState } from "react"
import type * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import { componentWorldTransform, DEBUG_SNAP_TO_FACE } from "@/mix-and-match/MixAndMatchPlacement"
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
import { rayCastForRigidBody } from "@/util/RaycastUtils"

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
            const hit = rayCastForRigidBody([e.clientX, e.clientY])
            const componentId = hit && MixAndMatchMode.scene?.componentIdOfBody(hit.bodyId)

            if (DEBUG_SNAP_TO_FACE) {
                console.debug("[MixAndMatch] face pick raycast", {
                    pickStep,
                    selected,
                    bodyId: hit?.bodyId?.GetIndex?.(),
                    componentId,
                    hitPoint: hit?.hitPoint.toArray(),
                    hitNormal: hit?.hitNormal?.toArray(),
                })
            }

            if (!hit || !componentId || !hit.hitNormal) {
                if (DEBUG_SNAP_TO_FACE) console.debug("[MixAndMatch] face pick ignored: no hit, no component, or missing normal")
                return
            }

            if (pickStep === "source") {
                if (componentId !== selected) {
                    if (DEBUG_SNAP_TO_FACE)
                        console.debug("[MixAndMatch] source pick ignored: clicked component isn't the selected one", {
                            clicked: componentId,
                            selected,
                        })
                    return
                }

                setSourceFace({ point: hit.hitPoint, normal: hit.hitNormal })
                if (DEBUG_SNAP_TO_FACE)
                    console.debug("[MixAndMatch] source face picked", {
                        componentId,
                        point: hit.hitPoint.toArray(),
                        normal: hit.hitNormal.toArray(),
                    })
                setPickStep("target")
                return
            }

            if (componentId === selected || !sourceFace) {
                if (DEBUG_SNAP_TO_FACE)
                    console.debug("[MixAndMatch] target pick ignored: clicked the source part or no source face stored", {
                        clicked: componentId,
                        selected,
                        hasSourceFace: !!sourceFace,
                    })
                return
            }

            if (DEBUG_SNAP_TO_FACE)
                console.debug("[MixAndMatch] target face picked", {
                    componentId,
                    point: hit.hitPoint.toArray(),
                    normal: hit.hitNormal.toArray(),
                })

            MixAndMatchMode.mateFaces(selected, componentId, sourceFace.point, sourceFace.normal, hit.hitPoint, hit.hitNormal)
                .then(() => {
                    const component = MixAndMatchMode.scene?.get(selected)
                    if (component) gizmoRef.current?.setTransform(componentWorldTransform(component))
                })
                .catch(console.error)
            cancelSnapPick()
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)
        return () => World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
    }, [pickStep, selected, sourceFace, cancelSnapPick])

    useEffect(() => cancelSnapPick(), [selected, cancelSnapPick])

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
                            <Button onClick={() => (pickStep === "idle" ? setPickStep("source") : cancelSnapPick())}>
                                {pickStep === "idle" ? "Snap to Face" : "Cancel"}
                            </Button>
                        </Stack>
                    </AccordionDetails>
                </Accordion>
            )}
        </Stack>
    )
}

export default MixAndMatchPanel
