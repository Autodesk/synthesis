import { Box, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import type * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import { componentWorldTransform, DEBUG_SNAP_TO_FACE } from "@/mix-and-match/MixAndMatchPlacement"
import type { ComponentState } from "@/mix-and-match/MixAndMatchTimeline"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import { Button, NegativeButton, Spacer } from "@/ui/components/StyledComponents"
import Tree, { type TreeNode } from "@/ui/components/Tree"
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

function buildNodes(components: ReadonlyMap<ComponentId, ComponentState>): TreeNode[] {
    const childrenByParent = new Map<ComponentId, ComponentId[]>()
    const roots: ComponentId[] = []

    components.forEach(component => {
        if (component.weld) {
            const siblings = childrenByParent.get(component.weld.parentId) ?? []
            siblings.push(component.id)
            childrenByParent.set(component.weld.parentId, siblings)
        } else {
            roots.push(component.id)
        }
    })

    const toNode = (id: ComponentId): TreeNode => {
        const component = components.get(id)!
        return {
            id,
            label: (
                <Stack direction="row" gap={0.5} alignItems="baseline" sx={{ minWidth: 0 }}>
                    <Label size="sm" noWrap>
                        {partName(component.libraryPartRef)}
                    </Label>
                    <Label size="sm" color="text.secondary" noWrap>
                        · {id}
                    </Label>
                </Stack>
            ),
            children: (childrenByParent.get(id) ?? []).map(toNode),
        }
    }

    return roots.map(toNode)
}

/**
 * Floating design-tree HUD for an in-progress mix-and-match build: a scene overlay, not a Panel, so
 * it stays on screen for the whole session the way a CAD browser tree does. Welded components nest
 * under whatever they're welded onto, mirroring `MixAndMatchBuild`'s weld tree directly.
 */
const AssemblyTreeOverlay: React.FC = () => {
    const { openModal } = useUIContext()
    const [, bumpRevision] = useState(0)
    const [selected, setSelected] = useState<ComponentId | undefined>(undefined)
    const [pickStep, setPickStep] = useState<SnapPickStep>("idle")
    const [sourceFace, setSourceFace] = useState<PickedFace | undefined>(undefined)
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", () => bumpRevision(x => x + 1)), [])

    useEffect(() => {
        if (selected && !MixAndMatchMode.build?.state.components.has(selected)) setSelected(undefined)
    })

    const components = MixAndMatchMode.build?.state.components
    const nodes = useMemo(() => (components ? buildNodes(components) : []), [components])
    const scene = MixAndMatchMode.scene
    const selectedComponent = selected ? scene?.get(selected) : undefined

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

            if (DEBUG_SNAP_TO_FACE) {
                console.debug("[MixAndMatch] face pick raycast", {
                    pickStep,
                    selected,
                    componentId,
                    hitPoint: hit?.point.toArray(),
                    hitNormal: hit?.normal.toArray(),
                })
            }

            if (!hit || !componentId) {
                if (DEBUG_SNAP_TO_FACE) console.debug("[MixAndMatch] face pick ignored: no hit or no component")
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

                setSourceFace({ point: hit.point, normal: hit.normal })
                if (DEBUG_SNAP_TO_FACE)
                    console.debug("[MixAndMatch] source face picked", {
                        componentId,
                        point: hit.point.toArray(),
                        normal: hit.normal.toArray(),
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
                    point: hit.point.toArray(),
                    normal: hit.normal.toArray(),
                })

            MixAndMatchMode.mateFaces(selected, componentId, sourceFace.point, sourceFace.normal, hit.point, hit.normal)
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

        openModal(ConfirmModal, { message: `Delete ${selected}? Every weld attached to it goes with it.` }, undefined, {
            title: "Delete Part",
            acceptText: "Delete",
            onAccept: () => {
                MixAndMatchMode.deleteComponent(selected).catch(console.error)
                setSelected(undefined)
            },
        })
    }, [openModal, selected])

    return (
        <Box
            sx={{
                position: "absolute",
                top: "calc(12px + var(--top-bar-height, 0px))",
                left: 12,
                width: 260,
                maxHeight: "50vh",
                bgcolor: "background.paper",
                borderRadius: 2,
                boxShadow: 6,
                pointerEvents: "auto",
                overflow: "hidden",
                display: "flex",
                flexDirection: "column",
            }}
        >
            <Box sx={{ px: 1.5, py: 1, borderBottom: 1, borderColor: "divider" }}>
                <Label size="sm" sx={{ fontWeight: 600 }}>
                    {`Assembly (${components?.size ?? 0})`}
                </Label>
            </Box>
            <Box sx={{ overflowY: "auto", px: 0.5, py: 0.5 }}>
                {nodes.length === 0 ? (
                    <Label size="sm" sx={{ px: 1, py: 1, color: "text.secondary" }}>
                        Add a part to start building
                    </Label>
                ) : (
                    <Tree
                        nodes={nodes}
                        selectedId={selected}
                        onSelect={id => setSelected(prev => (prev === id ? undefined : id))}
                    />
                )}
            </Box>
            {selectedComponent && (
                <Box sx={{ px: 1, py: 1, borderTop: 1, borderColor: "divider" }}>
                    <Label size="sm" sx={{ fontWeight: 600, pb: 0.5 }}>
                        Placement
                    </Label>
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
                </Box>
            )}
            {selected && (
                <Box sx={{ px: 1, py: 1, borderTop: 1, borderColor: "divider" }}>
                    <NegativeButton onClick={confirmDelete} fullWidth>
                        Delete Part
                    </NegativeButton>
                </Box>
            )}
        </Box>
    )
}

export default AssemblyTreeOverlay
