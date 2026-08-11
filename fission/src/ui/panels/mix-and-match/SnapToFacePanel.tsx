import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type * as THREE from "three"
import MixAndMatchMode from "@/mix-and-match/MixAndMatchMode"
import type { ComponentId } from "@/mix-and-match/MixAndMatchTypes"
import EventSystem from "@/systems/EventSystem"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { rayCastMesh } from "@/util/RaycastUtils"

/** Which face selector, if any, is waiting on the next click in the viewport. */
type Armed = "moving" | "target" | undefined

type PickedFace = { componentId: ComponentId; point: THREE.Vector3; normal: THREE.Vector3 }

const SnapToFacePanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [, bumpRevision] = useState(0)
    const [armed, setArmed] = useState<Armed>(undefined)
    const [movingFace, setMovingFace] = useState<PickedFace | undefined>(undefined)
    const [targetFace, setTargetFace] = useState<PickedFace | undefined>(undefined)

    useEffect(() => EventSystem.listen("MixAndMatchStateChangedEvent", () => bumpRevision(x => x + 1)), [])

    const selected = MixAndMatchMode.selected

    // The moving part is whatever's selected in the assembly tree; if that changes out from under
    // the panel, any picks and preview made against the old part are meaningless.
    useEffect(() => {
        setArmed(undefined)
        setMovingFace(undefined)
        setTargetFace(undefined)
        MixAndMatchMode.discardPreview().catch(console.error)
    }, [selected])

    // Both faces picked: preview the snap. Re-run from a clean baseline every time either face
    // changes, since `previewMateFaces` moves the part relative to wherever it currently is.
    useEffect(() => {
        if (!movingFace || !targetFace) return

        let cancelled = false
        MixAndMatchMode.discardPreview()
            .then(() => {
                if (cancelled) return
                MixAndMatchMode.previewMateFaces(
                    movingFace.componentId,
                    targetFace.componentId,
                    movingFace.point,
                    movingFace.normal,
                    targetFace.point,
                    targetFace.normal
                )
            })
            .catch(console.error)

        return () => {
            cancelled = true
        }
    }, [movingFace, targetFace])

    useEffect(() => {
        configureScreen(
            panel!,
            { title: "Snap to Face", disableAccept: !movingFace || !targetFace },
            {
                onBeforeAccept: () => {
                    if (selected) MixAndMatchMode.commitPlacement(selected).catch(console.error)
                },
                onClose: closeType => {
                    if (closeType !== CloseType.ACCEPT) MixAndMatchMode.discardPreview().catch(console.error)
                },
            }
        )
    }, [configureScreen, panel, selected, movingFace, targetFace])

    // Undoes any pending preview before (re-)arming a selector, so the next click's raycast reads
    // geometry at its last-committed pose rather than wherever a previous preview left it.
    const arm = (which: Armed) => {
        MixAndMatchMode.discardPreview()
            .then(() => setArmed(prev => (prev === which ? undefined : which)))
            .catch(console.error)
    }

    // A click while a selector is armed picks a face for it: the moving selector only accepts a
    // click on the selected part, the target selector only accepts a click on a different part.
    useEffect(() => {
        if (!armed || !selected) return

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

            if (armed === "moving") {
                if (componentId !== selected) return
                setMovingFace({ componentId, point: hit.point, normal: hit.normal })
            } else {
                if (componentId === selected) return
                setTargetFace({ componentId, point: hit.point, normal: hit.normal })
            }

            setArmed(undefined)
        }

        World.sceneRenderer.renderer.domElement.addEventListener("click", onClick)
        return () => World.sceneRenderer.renderer.domElement.removeEventListener("click", onClick)
    }, [armed, selected])

    if (!selected) {
        return <Label size="sm">Select a part in the assembly tree first.</Label>
    }

    return (
        <Stack direction="column" gap={1.5} minWidth="18rem">
            <Stack direction="row" gap={1} alignItems="center" justifyContent="space-between">
                <Label size="sm">{movingFace ? "Moving face picked" : "Pick the face on this part"}</Label>
                <Button
                    size="small"
                    variant={armed === "moving" ? "contained" : "outlined"}
                    onClick={() => arm("moving")}
                >
                    {armed === "moving" ? "Click a face…" : movingFace ? "Re-pick" : "Select"}
                </Button>
            </Stack>
            <Stack direction="row" gap={1} alignItems="center" justifyContent="space-between">
                <Label size="sm">{targetFace ? "Target face picked" : "Pick the face to snap against"}</Label>
                <Button
                    size="small"
                    variant={armed === "target" ? "contained" : "outlined"}
                    onClick={() => arm("target")}
                >
                    {armed === "target" ? "Click a face…" : targetFace ? "Re-pick" : "Select"}
                </Button>
            </Stack>
        </Stack>
    )
}

export default SnapToFacePanel
