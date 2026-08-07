import { Box, Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { ComponentState } from "@/mix-and-match/MixAndMatchTimeline"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import PartLibrary from "@/mix-and-match/PartLibrary"
import EventSystem from "@/systems/EventSystem"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import Label from "@/ui/components/Label"
import { Button, NegativeButton } from "@/ui/components/StyledComponents"
import Tree, { type TreeNode } from "@/ui/components/Tree"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import ConfirmModal from "@/ui/modals/common/ConfirmModal"
import NameBuildModal from "@/ui/modals/mix-and-match/NameBuildModal"

const DEFAULT_BUILD_NAME = "Mix and Match Robot"

const GIZMO_SIZE = 1.5

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
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", () => bumpRevision(x => x + 1)), [])

    useEffect(() => {
        if (MixAndMatchMode.selected && !MixAndMatchMode.build?.state.components.has(MixAndMatchMode.selected)) {
            MixAndMatchMode.setSelected(undefined)
        }
    })

    const selected = MixAndMatchMode.selected
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

    const confirmDelete = useCallback(() => {
        if (!selected) return

        openModal(ConfirmModal, { message: `Delete ${selected}? Every weld attached to it goes with it.` }, undefined, {
            title: "Delete Part",
            acceptText: "Delete",
            onAccept: () => {
                MixAndMatchMode.deleteComponent(selected).catch(console.error)
                MixAndMatchMode.setSelected(undefined)
            },
        })
    }, [openModal, selected])

    const finishBuild = useCallback(() => {
        openModal(
            NameBuildModal,
            { title: "Finish Build", acceptText: "Finish", defaultName: DEFAULT_BUILD_NAME },
            undefined,
            { onAccept: (name: string) => MixAndMatchMode.finish(name).catch(console.error) }
        )
    }, [openModal])

    const exportBuild = useCallback(() => {
        openModal(
            NameBuildModal,
            { title: "Export as Mira", acceptText: "Export", defaultName: DEFAULT_BUILD_NAME },
            undefined,
            { onAccept: (name: string) => MixAndMatchMode.exportBuild(name).catch(console.error) }
        )
    }, [openModal])

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
                        onSelect={id => MixAndMatchMode.setSelected(selected === id ? undefined : id)}
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
                </Box>
            )}
            {selected && (
                <Box sx={{ px: 1, py: 1, borderTop: 1, borderColor: "divider" }}>
                    <NegativeButton onClick={confirmDelete} fullWidth>
                        Delete Part
                    </NegativeButton>
                </Box>
            )}
            <Box sx={{ px: 1, py: 1, borderTop: 1, borderColor: "divider" }}>
                <Stack direction="row" gap={1}>
                    <Button disabled={!components?.size} onClick={finishBuild} fullWidth>
                        Finish Build
                    </Button>
                    <Button disabled={!components?.size} onClick={exportBuild} fullWidth>
                        Export as Mira
                    </Button>
                </Stack>
            </Box>
        </Box>
    )
}

export default AssemblyTreeOverlay
