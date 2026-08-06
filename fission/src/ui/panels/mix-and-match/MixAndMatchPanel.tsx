import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useReducer, useRef, useState } from "react"
import * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import {
    componentGizmoTransform,
    componentWorldBounds,
    componentWorldTransform,
    debugLog,
    DEBUG_SNAP_TO_FACE,
} from "@/mix-and-match/MixAndMatchPlacement"
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

    // Debug-only: draws the picked point + surface normal in the 3D view so a bad normal (or a point
    // that isn't where the user thinks it is) is visible directly, not just as numbers in the console.
    const debugArrowsRef = useRef<THREE.Object3D[]>([])

    const clearDebugArrows = useCallback(() => {
        debugArrowsRef.current.forEach(arrow => World.sceneRenderer.scene.remove(arrow))
        debugArrowsRef.current = []
    }, [])

    const addDebugArrow = useCallback((point: THREE.Vector3, normal: THREE.Vector3, color: number) => {
        const arrow = new THREE.ArrowHelper(normal.clone().normalize(), point, 0.3, color, 0.08, 0.06)
        World.sceneRenderer.scene.add(arrow)
        debugArrowsRef.current.push(arrow)
    }, [])

    // Marks one of the frames a component is tracked in. Expect these to sit outside the part's own mesh
    // and even inside a neighbouring part: a mira assembly's origin is wherever it was exported, not its
    // geometry, so a mate that rotates the part swings its origin well clear of it. Only relative motion
    // between two consecutive readings of the same frame means anything.
    const addDebugPoint = useCallback((position: THREE.Vector3, color: number) => {
        const marker = new THREE.Mesh(
            new THREE.SphereGeometry(0.05, 12, 12),
            new THREE.MeshBasicMaterial({ color, depthTest: false })
        )
        marker.position.copy(position)
        World.sceneRenderer.scene.add(marker)
        debugArrowsRef.current.push(marker)
    }, [])

    // Wireframe of a component's world-space AABB: unlike the point/normal arrows, this stays visible
    // and to-scale from any camera angle, so "are these two boxes actually touching or overlapping" is
    // answerable by looking, not by trusting a screenshot's foreshortening.
    const addDebugBox = useCallback((box: THREE.Box3, color: number) => {
        const helper = new THREE.Box3Helper(box, color)
        World.sceneRenderer.scene.add(helper)
        debugArrowsRef.current.push(helper)
    }, [])

    // Everything logged straight out of a placement call describes the frame the placement happened on.
    // Anything that moves the part *afterwards* — notably GizmoSceneObject.update() acting on a
    // setTransform force-update — lands a frame later, so a synchronous-only log reads as correct while
    // the render is already wrong. This re-reads the live pose once the dust has settled and reports the
    // drift, which is the difference between "the mate math is wrong" and "something else moved it".
    const debugSettleCheck = useCallback(
        (componentId: ComponentId, expected: THREE.Matrix4, predictedGizmoDrift: THREE.Vector3) => {
            if (!DEBUG_SNAP_TO_FACE) return

            requestAnimationFrame(() =>
                requestAnimationFrame(() => {
                    const component = MixAndMatchMode.scene?.get(componentId)
                    if (!component) return

                    const settled = componentWorldTransform(component)
                    const drift = new THREE.Vector3()
                        .setFromMatrixPosition(settled)
                        .sub(new THREE.Vector3().setFromMatrixPosition(expected))

                    debugLog("[MixAndMatch] settled pose, two frames later", {
                        componentId,
                        expectedTransform: expected.toArray(),
                        settledTransform: settled.toArray(),
                        drift: drift.toArray(),
                        driftLength: drift.length(),
                        // A drift matching this means the gizmo re-drove the bodies through its
                        // COM-relative offsets; the placement math itself was fine.
                        predictedGizmoDrift: predictedGizmoDrift.toArray(),
                        // Divergence from the recorded transform means the scene and the timeline
                        // disagree, so the next sync() will yank the part somewhere else again.
                        recordedTransform: MixAndMatchMode.build?.state.components.get(componentId)?.transform,
                    })

                    // red: where the part actually ended up, against the green snapshot from the mate frame
                    addDebugBox(componentWorldBounds(component), 0xff0000)
                })
            )
        },
        [addDebugBox]
    )

    useEffect(() => clearDebugArrows, [clearDebugArrows])

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

            debugLog("[MixAndMatch] face pick raycast", {
                pickStep,
                selected,
                componentId,
                hitPoint: hit?.point.toArray(),
                hitNormal: hit?.normal.toArray(),
            })

            if (!hit || !componentId) {
                debugLog("[MixAndMatch] face pick ignored", { reason: "no hit or no component" })
                return
            }

            if (pickStep === "source") {
                if (componentId !== selected) {
                    debugLog("[MixAndMatch] source pick ignored: clicked component isn't the selected one", {
                        clicked: componentId,
                        selected,
                    })
                    return
                }

                setSourceFace({ point: hit.point, normal: hit.normal })
                debugLog("[MixAndMatch] source face picked", {
                    componentId,
                    point: hit.point.toArray(),
                    normal: hit.normal.toArray(),
                })
                if (DEBUG_SNAP_TO_FACE) addDebugArrow(hit.point, hit.normal, 0x00ff00) // green: source/moving face
                setPickStep("target")
                return
            }

            if (componentId === selected || !sourceFace) {
                debugLog("[MixAndMatch] target pick ignored: clicked the source part or no source face stored", {
                    clicked: componentId,
                    selected,
                    hasSourceFace: !!sourceFace,
                })
                return
            }

            debugLog("[MixAndMatch] target face picked", {
                componentId,
                point: hit.point.toArray(),
                normal: hit.normal.toArray(),
            })
            if (DEBUG_SNAP_TO_FACE) addDebugArrow(hit.point, hit.normal, 0x0000ff) // blue: target face

            const targetComponent = MixAndMatchMode.scene?.get(componentId)

            MixAndMatchMode.mateFaces(selected, componentId, sourceFace.point, sourceFace.normal, hit.point, hit.normal)
                .then(() => {
                    const component = MixAndMatchMode.scene?.get(selected)

                    // The gizmo has already been re-seated by the placement itself. Logged anyway because
                    // the two frames are metres apart on a real part, so "which one is the gizmo on" is the
                    // first thing worth knowing if a part ever jumps after a snap again.
                    const rootWorldTransform = component ? componentWorldTransform(component) : new THREE.Matrix4()
                    const gizmoTransform = component ? componentGizmoTransform(component) : new THREE.Matrix4()
                    const comOffset = new THREE.Vector3()
                        .setFromMatrixPosition(gizmoTransform)
                        .sub(new THREE.Vector3().setFromMatrixPosition(rootWorldTransform))

                    debugLog("[MixAndMatch] gizmo frames after mate", {
                        componentId: selected,
                        rootWorldTransform: rootWorldTransform.toArray(),
                        gizmoTransform: gizmoTransform.toArray(),
                        // How far a part would be dragged by a gizmo seated on the wrong one of the two.
                        comOffset: comOffset.toArray(),
                        comOffsetLength: comOffset.length(),
                    })

                    if (component) debugSettleCheck(selected, rootWorldTransform, comOffset.clone().negate())

                    if (DEBUG_SNAP_TO_FACE && component && targetComponent) {
                        addDebugBox(componentWorldBounds(component), 0x00ff00) // green: moving part's AABB
                        addDebugBox(componentWorldBounds(targetComponent), 0x0000ff) // blue: target part's AABB
                        // orange: root-body origin, the frame the timeline records. Routinely outside the mesh.
                        addDebugPoint(new THREE.Vector3().setFromMatrixPosition(rootWorldTransform), 0xffa500)
                        // cyan: root-body centre of mass, the frame the gizmo sits at. Should be inside the mesh.
                        addDebugPoint(new THREE.Vector3().setFromMatrixPosition(gizmoTransform), 0x00ffff)

                        // Audit: is there more than one BatchedMesh rendering for this component, e.g. an
                        // orphaned instance from an earlier stage/spawn that never got removed and is
                        // still drawing at its old, stale position while the tracked one is correct?
                        const tracked = new Set<THREE.Object3D>()
                        MixAndMatchMode.scene?.components.forEach(c => c.mirabufInstance.batches.forEach(b => tracked.add(b)))

                        let totalBatchesInScene = 0
                        const orphanBatches: { uuid: string; instancePositions: number[][] }[] = []
                        World.sceneRenderer.scene.traverse(obj => {
                            if (!(obj as THREE.BatchedMesh).isBatchedMesh) return
                            totalBatchesInScene++
                            if (tracked.has(obj)) return

                            const batch = obj as THREE.BatchedMesh
                            const instancePositions: number[][] = []
                            const m = new THREE.Matrix4()
                            for (let i = 0; i < batch.instanceCount; i++) {
                                batch.getMatrixAt(i, m)
                                instancePositions.push(new THREE.Vector3().setFromMatrixPosition(m).toArray())
                            }
                            orphanBatches.push({ uuid: obj.uuid, instancePositions })
                        })

                        debugLog("[MixAndMatch] scene batch audit", {
                            trackedBatchCount: tracked.size,
                            totalBatchesInScene,
                            orphanBatches,
                        })
                    }
                })
                .catch(console.error)
            cancelSnapPick()
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)
        return () => World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
    }, [pickStep, selected, sourceFace, cancelSnapPick, addDebugArrow, addDebugBox, addDebugPoint, debugSettleCheck])

    useEffect(() => {
        cancelSnapPick()
        clearDebugArrows()
    }, [selected, cancelSnapPick, clearDebugArrows])

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
                                        clearDebugArrows()
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
